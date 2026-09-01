[CmdletBinding()]
param(
    [Parameter()]
    [string] $NinaSource = (Join-Path $PSScriptRoot '..\..\nina'),

    [Parameter()]
    [string] $DocumentationRoot = (Join-Path $PSScriptRoot '..\docs'),

    [Parameter()]
    [string] $MappingPath = (Join-Path $PSScriptRoot '..\documentation-map.json'),

    [Parameter()]
    [string] $OutputPath
)

$ErrorActionPreference = 'Stop'

function Resolve-ExistingDirectory {
    param([string] $Path, [string] $Description)

    $resolved = Resolve-Path -LiteralPath $Path -ErrorAction SilentlyContinue
    if (-not $resolved -or -not (Test-Path -LiteralPath $resolved.Path -PathType Container)) {
        throw "$Description was not found: $Path"
    }
    return $resolved.Path
}

function Get-LocalizationTable {
    param([string] $SourceRoot)

    $localePath = Join-Path $SourceRoot 'NINA.Core\Locale\Locale.resx'
    [xml] $locale = Get-Content -LiteralPath $localePath -Raw
    $table = @{}
    foreach ($entry in $locale.root.data) {
        if ($entry.name -and $entry.value) {
            $table[[string] $entry.name] = [string] $entry.value
        }
    }
    return $table
}

function Resolve-Label {
    param([string] $Value, [hashtable] $Labels)

    if ($Value -and $Labels.ContainsKey($Value)) {
        return $Labels[$Value]
    }
    return $Value
}

function Get-SequencerItems {
    param([string] $SourceRoot, [hashtable] $Labels)

    $sequencerRoot = Join-Path $SourceRoot 'NINA.Sequencer'
    $files = Get-ChildItem -LiteralPath $sequencerRoot -Recurse -Filter '*.cs' -File |
        Where-Object { $_.FullName -match '\\(SequenceItem|Conditions|Trigger)\\' }

    foreach ($file in $files) {
        $attributes = [System.Collections.Generic.List[string]]::new()
        foreach ($line in Get-Content -LiteralPath $file.FullName) {
            $trimmed = $line.Trim()
            if ($trimmed.StartsWith('[')) {
                $attributes.Add($trimmed)
                continue
            }

            $classMatch = [regex]::Match(
                $trimmed,
                '^(?:public\s+)?(?:sealed\s+|abstract\s+|partial\s+)*class\s+(?<class>[A-Za-z_][A-Za-z0-9_]*)')
            if (-not $classMatch.Success) {
                if ($trimmed -and -not $trimmed.StartsWith('//')) {
                    $attributes.Clear()
                }
                continue
            }

            $attributeText = $attributes -join [Environment]::NewLine
            $attributes.Clear()
            $kindMatch = [regex]::Match($attributeText, 'Export\(typeof\(ISequence(?<kind>Item|Condition|Trigger)\)\)')
            if (-not $kindMatch.Success) {
                continue
            }

            $nameMatch = [regex]::Match($attributeText, 'ExportMetadata\("Name",\s*"(?<value>[^"]+)"\)')
            $categoryMatch = [regex]::Match($attributeText, 'ExportMetadata\("Category",\s*"(?<value>[^"]+)"\)')
            $descriptionMatch = [regex]::Match($attributeText, 'ExportMetadata\("Description",\s*"(?<value>[^"]+)"\)')
            $relativePath = [System.IO.Path]::GetRelativePath($SourceRoot, $file.FullName).Replace('\', '/')

            [pscustomobject]@{
                Kind             = "Sequence$($kindMatch.Groups['kind'].Value)"
                Identifier       = $classMatch.Groups['class'].Value
                DisplayName      = Resolve-Label $nameMatch.Groups['value'].Value $Labels
                Category         = Resolve-Label $categoryMatch.Groups['value'].Value $Labels
                Description      = Resolve-Label $descriptionMatch.Groups['value'].Value $Labels
                Source           = $relativePath
                Documentation    = $null
                DocumentationMap = $null
            }
        }
    }
}

function Get-ProfileSettings {
    param([string] $SourceRoot)

    $interfacesRoot = Join-Path $SourceRoot 'NINA.Profile\Interfaces'
    $propertyPattern = [regex]::new(
        '(?m)^\s*(?:IList<[^>]+>|IReadOnlyList<[^>]+>|ObservableCollection<[^>]+>|[A-Za-z_][A-Za-z0-9_?.<>\[\], ]+)\s+(?<name>[A-Za-z_][A-Za-z0-9_]*)\s*\{\s*get;',
        [System.Text.RegularExpressions.RegexOptions]::Compiled)

    foreach ($file in Get-ChildItem -LiteralPath $interfacesRoot -Recurse -Filter 'I*Settings.cs' -File) {
        $content = Get-Content -LiteralPath $file.FullName -Raw
        $settingGroup = [System.IO.Path]::GetFileNameWithoutExtension($file.Name) -replace '^I', '' -replace 'Settings$', ''
        foreach ($match in $propertyPattern.Matches($content)) {
            [pscustomobject]@{
                Kind             = 'Setting'
                Identifier       = "$settingGroup.$($match.Groups['name'].Value)"
                DisplayName      = $match.Groups['name'].Value
                Category         = $settingGroup
                Description      = ''
                Source           = [System.IO.Path]::GetRelativePath($SourceRoot, $file.FullName).Replace('\', '/')
                Documentation    = $null
                DocumentationMap = $null
            }
        }
    }
}

function Get-Views {
    param([string] $SourceRoot)

    $viewRoot = Join-Path $SourceRoot 'NINA\View'
    foreach ($file in Get-ChildItem -LiteralPath $viewRoot -Recurse -Filter '*.xaml' -File) {
        $relativeToView = [System.IO.Path]::GetRelativePath($viewRoot, $file.FullName).Replace('\', '/')
        $segments = $relativeToView.Split('/')
        $isDocumentable = $segments[0] -in @('Options', 'Equipment', 'Imaging', 'FlatWizard', 'Sequencer') -or
            $file.BaseName -in @('FramingAssistantView', 'SkyAtlasView', 'PlateSolveView', 'FrameFocusView')
        if (-not $isDocumentable) {
            continue
        }

        [pscustomobject]@{
            Kind             = 'WorkspaceOrPanel'
            Identifier       = $file.BaseName
            DisplayName      = ($file.BaseName -replace 'View$', '')
            Category         = $segments[0]
            Description      = ''
            Source           = [System.IO.Path]::GetRelativePath($SourceRoot, $file.FullName).Replace('\', '/')
            Documentation    = $null
            DocumentationMap = $null
        }
    }
}

function Get-FileFormats {
    param([string] $SourceRoot)

    $patterns = @(
        @{ Identifier = 'FITS'; DisplayName = 'FITS'; Source = 'NINA.Image/FileFormat/FITS' },
        @{ Identifier = 'XISF'; DisplayName = 'XISF'; Source = 'NINA.Image/FileFormat/XISF' },
        @{ Identifier = 'TIFF'; DisplayName = 'TIFF'; Source = 'NINA.Image/ImageData/BaseImageData.cs' }
    )
    foreach ($pattern in $patterns) {
        $candidate = Join-Path $SourceRoot $pattern.Source.Replace('/', '\')
        if (Test-Path -LiteralPath $candidate) {
            [pscustomobject]@{
                Kind             = 'FileFormat'
                Identifier       = $pattern.Identifier
                DisplayName      = $pattern.DisplayName
                Category         = 'Image file format'
                Description      = ''
                Source           = $pattern.Source
                Documentation    = $null
                DocumentationMap = $null
            }
        }
    }
}

function Get-DocumentationText {
    param([string] $DocsRoot)

    $documents = @{}
    foreach ($file in Get-ChildItem -LiteralPath $DocsRoot -Recurse -Filter '*.md' -File) {
        $documents[[System.IO.Path]::GetRelativePath($DocsRoot, $file.FullName).Replace('\', '/')] =
            Get-Content -LiteralPath $file.FullName -Raw
    }
    return $documents
}

function Find-LikelyDocumentation {
    param($Item, [hashtable] $Documents)

    $needles = @($Item.DisplayName, $Item.Identifier) |
        Where-Object { $_ -and $_.Length -ge 4 } |
        Select-Object -Unique

    $matches = foreach ($document in $Documents.GetEnumerator()) {
        foreach ($needle in $needles) {
            if ($document.Value.IndexOf($needle, [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
                $document.Key
                break
            }
        }
    }
    return @($matches | Sort-Object -Unique)
}

$sourceRoot = Resolve-ExistingDirectory $NinaSource 'NINA source directory'
$docsRoot = Resolve-ExistingDirectory $DocumentationRoot 'Documentation directory'
$labels = Get-LocalizationTable $sourceRoot
$documents = Get-DocumentationText $docsRoot

$mapping = @{}
if (Test-Path -LiteralPath $MappingPath) {
    $mappingDocument = Get-Content -LiteralPath $MappingPath -Raw | ConvertFrom-Json
    foreach ($entry in $mappingDocument.items) {
        $mapping[[string] $entry.identifier] = @($entry.documentation)
    }
}

$items = @(
    Get-SequencerItems $sourceRoot $labels
    Get-ProfileSettings $sourceRoot
    Get-Views $sourceRoot
    Get-FileFormats $sourceRoot
) | Sort-Object Kind, Category, DisplayName, Identifier

foreach ($item in $items) {
    if ($mapping.ContainsKey($item.Identifier)) {
        $item.DocumentationMap = $mapping[$item.Identifier]
    }
    $item.Documentation = Find-LikelyDocumentation $item $documents
}

$lines = [System.Collections.Generic.List[string]]::new()
$lines.Add('# NINA documentation catch-up report')
$lines.Add('')
$lines.Add("Generated: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss zzz')")
$lines.Add("Source: $sourceRoot")
$lines.Add("Documentation: $docsRoot")
$lines.Add('')
$lines.Add('This is a heuristic maintenance report. A missing match is a review prompt, not proof that documentation is absent.')
$lines.Add('')

foreach ($group in $items | Group-Object Kind) {
    $lines.Add("## $($group.Name)")
    $lines.Add('')
    $lines.Add('| Identifier | Display name | Category | Documentation candidates | Source |')
    $lines.Add('| --- | --- | --- | --- | --- |')
    foreach ($item in $group.Group) {
        $mapped = @($item.DocumentationMap) | Where-Object { -not [string]::IsNullOrWhiteSpace([string] $_) }
        $candidates = if ($mapped.Count -gt 0) { $mapped } else { @($item.Documentation) }
        $candidateText = if ($candidates.Count -gt 0) { $candidates -join '<br>' } else { '**Review: no likely match**' }
        $displayName = ([string] $item.DisplayName).Replace('|', '\|')
        $category = ([string] $item.Category).Replace('|', '\|')
        $lines.Add("| $($item.Identifier) | $displayName | $category | $candidateText | $($item.Source) |")
    }
    $lines.Add('')
}

$report = $lines -join [Environment]::NewLine
if ($OutputPath) {
    $parent = Split-Path -Parent $OutputPath
    if ($parent -and -not (Test-Path -LiteralPath $parent)) {
        New-Item -ItemType Directory -Path $parent -Force | Out-Null
    }
    Set-Content -LiteralPath $OutputPath -Value $report -Encoding utf8
    Write-Host "Wrote $($items.Count) inventory entries to $OutputPath"
} else {
    $report
}
