[CmdletBinding()]
param(
    [Parameter()]
    [string] $NinaSource = (Join-Path $PSScriptRoot '..\..\nina'),

    [Parameter()]
    [string] $DocumentationRoot = (Join-Path $PSScriptRoot '..'),

    [Parameter()]
    [string] $CatalogPath = (Join-Path $PSScriptRoot '..\screenshots\manifest.json'),

    [Parameter()]
    [switch] $Force
)

$ErrorActionPreference = 'Stop'

if ((Test-Path -LiteralPath $CatalogPath) -and -not $Force) {
    throw "The catalog already exists. Pass -Force only when intentionally rebuilding the one-time baseline: $CatalogPath"
}

# A forced rebuild may carry forward mappings that maintainers already reviewed. New
# files deliberately start excluded. This script must never infer a production view,
# sequencer type or icon from a filename.
$reviewedAssetByOutput = @{}
if (Test-Path -LiteralPath $CatalogPath) {
    $reviewedCatalog = Get-Content -Raw -LiteralPath $CatalogPath | ConvertFrom-Json
    foreach ($reviewedAsset in $reviewedCatalog.assets) {
        $reviewedAssetByOutput[$reviewedAsset.output.ToLowerInvariant()] = $reviewedAsset
    }
}

$root = (Resolve-Path -LiteralPath $DocumentationRoot).Path
$docsRoot = Join-Path $root 'docs'
if (-not (Test-Path -LiteralPath $docsRoot -PathType Container)) {
    throw "Documentation directory was not found: $docsRoot"
}

Add-Type -AssemblyName PresentationCore

function Get-ImageDimensions {
    param([string] $Path)

    if ((Get-Item -LiteralPath $Path).Length -eq 0) {
        return @{ Width = 0; Height = 0 }
    }

    $stream = [System.IO.File]::OpenRead($Path)
    try {
        try {
            $decoder = [System.Windows.Media.Imaging.BitmapDecoder]::Create(
                $stream,
                [System.Windows.Media.Imaging.BitmapCreateOptions]::PreservePixelFormat,
                [System.Windows.Media.Imaging.BitmapCacheOption]::OnLoad)
        } catch {
            throw "Could not read image dimensions for '$Path': $($_.Exception.Message)"
        }
        return @{ Width = $decoder.Frames[0].PixelWidth; Height = $decoder.Frames[0].PixelHeight }
    } finally {
        $stream.Dispose()
    }
}

function Get-CurrentUiDimensions {
    param([string] $Path, [int] $Width, [int] $Height)

    $normalized = $Path.Replace('\', '/').ToLowerInvariant()
    if ($normalized -match '/sequencer/instructions/camera_manyexposures\.png$') {
        return @{ Width = 920; Height = [Math]::Max(34, $Height) }
    }
    if ($normalized -match '/sequencer/instructions/flat_trained(dark|flat)\.png$') {
        return @{ Width = 1000; Height = [Math]::Max(35, $Height) }
    }
    if ($normalized -match '/sequencer/sequencer_issues\.png$') {
        return @{ Width = 698; Height = 834 }
    }
    if ($normalized -match '/sequencer/sequencer_define(variable|constant)\.png$') {
        return @{ Width = 1100; Height = 120 }
    }
    if ($normalized -match '/sequencer/sequencer_instructionsdetails\.png$') {
        return @{ Width = 1220; Height = 160 }
    }
    return @{ Width = $Width; Height = $Height }
}

function Get-Classification {
    param([string] $Path, [int] $Width, [int] $Height)

    $normalized = $Path.Replace('\', '/').ToLowerInvariant()
    if ([System.IO.Path]::GetExtension($normalized) -match '^\.jpe?g$') {
        return @{ Name = 'brand-or-static'; Reason = 'Legacy JPEG retained as a source asset only. All generated screenshots use a separate PNG output path.' }
    }
    if ($Width -eq 0 -or $Height -eq 0) {
        return @{ Name = 'brand-or-static'; Reason = 'This is an empty legacy placeholder rather than a usable screenshot.' }
    }
    if ($Width -lt 16 -or $Height -lt 16) {
        return @{ Name = 'brand-or-static'; Reason = 'A tiny icon or marker is maintained as a static documentation asset.' }
    }
    if ($normalized -match '/images/(nina-icon|nina-logo)' -or $normalized -match '/contributing/ecosystem\.png$') {
        return @{ Name = 'brand-or-static'; Reason = 'NINA branding or a maintained documentation diagram is not an application screenshot.' }
    }
    if ($normalized -match '/advanced/dialgauge\.(png|jpg|jpeg)$') {
        return @{ Name = 'brand-or-static'; Reason = 'This is a hardware photograph rather than a NINA application screenshot.' }
    }
    if ($normalized -match '/advanced/meridianflip/(minequalsmax|minmaxrange|pausebefore)\.png$') {
        return @{ Name = 'brand-or-static'; Reason = 'This is an intentionally maintained explanatory timing diagram assembled from multiple sources.' }
    }
    if ($normalized -match '/images/tabs/plugins_(available|installed)\.png$') {
        return @{ Name = 'brand-or-static'; Reason = 'This NINA plugin-manager capture depends on remote or separately installed third-party plugin content and is kept as an original asset.' }
    }
    if ($normalized -match '/images/setup/' -or $normalized -match '/images/contributing/crowdin-') {
        return @{ Name = 'external-ui'; Reason = 'This image belongs to GitHub, Visual Studio or Crowdin rather than the NINA application.' }
    }
    if ($normalized -match '/troubleshooting/(disablenahimic|eventviewer)' -or
        $normalized -match '/troubleshooting/planetarium/' -or
        $normalized -match '/troubleshooting/qhy/(astroimaging|device_manager|hidden_devices|outdated_driver)' -or
        $normalized -match '/troubleshooting/eqmod/(eqmod_settings|eqmod_setup)' -or
        $normalized -match '/troubleshooting/ioptron/(connection_settings|mount_settings)' -or
        $normalized -match '/advanced/(dithering1|metaguide_scope_setup|phd2scale)\.') {
        return @{ Name = 'external-ui'; Reason = 'This screenshot belongs to Windows or connected third-party astronomy software.' }
    }
    if ($normalized -match '/troubleshooting/renderissues\.png$') {
        return @{ Name = 'brand-or-static'; Reason = 'This intentionally records a hardware-specific broken-rendering state that a healthy deterministic render cannot reproduce.' }
    }
    if ($normalized -match '(autofocuscurve|goodaf|backlash\.png|focuserbacklash|altitudechartwithhorizon|hfr2|hfrhistory)') {
        return @{ Name = 'nina-generated-visual'; Reason = $null }
    }
    return @{ Name = 'nina-ui'; Reason = $null }
}

$imageFiles = Get-ChildItem -LiteralPath $docsRoot -Recurse -File |
    Where-Object { $_.Extension -match '^\.(png|jpg|jpeg|webp|gif)$' } |
    Sort-Object FullName

$usedIds = @{}
$assets = foreach ($file in $imageFiles) {
    $relative = [System.IO.Path]::GetRelativePath($root, $file.FullName).Replace('\', '/')
    $dimensions = Get-ImageDimensions $file.FullName
    $dimensions = Get-CurrentUiDimensions $relative $dimensions.Width $dimensions.Height
    $classification = Get-Classification $relative $dimensions.Width $dimensions.Height
    $reviewedAsset = $reviewedAssetByOutput[$relative.ToLowerInvariant()]
    $id = (($relative.ToLowerInvariant() -replace '\.[^.]+$', '') -replace '[^a-z0-9]+', '-').Trim('-')
    $extension = $file.Extension.TrimStart('.').ToLowerInvariant()
    $baseId = "$id-$extension"
    if ($usedIds.ContainsKey($baseId)) {
        $usedIds[$baseId]++
        $id = "$baseId-$($usedIds[$baseId])"
    } else {
        $usedIds[$baseId] = 1
        $id = $baseId
    }

    $asset = [ordered]@{
        id = $id
        classification = $classification.Name
        output = $relative
        width = $dimensions.Width
        height = $dimensions.Height
    }
    $reviewedManaged = $reviewedAsset -and
        $reviewedAsset.classification -in @('nina-ui', 'nina-generated-visual') -and
        $reviewedAsset.fixture
    if ($reviewedManaged) {
        $asset.classification = $reviewedAsset.classification
        foreach ($propertyName in @(
            'fixture', 'state', 'viewType', 'renderWidth', 'renderHeight',
            'cropTarget', 'crop', 'callouts', 'icon', 'displayName', 'sourceIdentifier'
        )) {
            $property = $reviewedAsset.PSObject.Properties[$propertyName]
            if ($property) {
                $asset[$propertyName] = $property.Value
            }
        }
    } elseif ($reviewedAsset) {
        $asset.classification = $reviewedAsset.classification
        $asset.exclusionReason = $reviewedAsset.exclusionReason
    } elseif ($classification.Name -in @('nina-ui', 'nina-generated-visual')) {
        $asset.classification = 'brand-or-static'
        $asset.exclusionReason = 'No maintainer-verified deterministic fixture mapping exists yet. Keep the original capture until its real production view and state are verified.'
    } else {
        $asset.exclusionReason = $classification.Reason
    }
    $asset
}

$catalog = [ordered]@{
    schemaVersion = 2
    assets = @($assets)
}

$parent = Split-Path -Parent $CatalogPath
New-Item -ItemType Directory -Path $parent -Force | Out-Null
$catalog | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath $CatalogPath -Encoding utf8
Write-Host "Cataloged $($assets.Count) source images in $CatalogPath"
