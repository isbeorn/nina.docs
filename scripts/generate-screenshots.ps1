[CmdletBinding()]
param(
    [Parameter()]
    [string] $NinaSource = (Join-Path $PSScriptRoot '..\..\nina'),

    [Parameter()]
    [string] $Id,

    [Parameter()]
    [string] $Area,

    [Parameter()]
    [switch] $Preview,

    [Parameter()]
    [switch] $Restore
)

$ErrorActionPreference = 'Stop'

$documentationRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$sourceRoot = Resolve-Path -LiteralPath $NinaSource -ErrorAction SilentlyContinue
if (-not $sourceRoot) {
    throw "NINA source directory was not found: $NinaSource"
}

$solution = Join-Path $sourceRoot.Path 'NINA.sln'
if (-not (Test-Path -LiteralPath $solution -PathType Leaf)) {
    throw "-NinaSource must point to the NINA solution directory containing NINA.sln: $($sourceRoot.Path)"
}

$project = Join-Path $documentationRoot 'tools\NINA.DocumentationScreenshots\NINA.DocumentationScreenshots.csproj'
if (-not (Test-Path -LiteralPath $project -PathType Leaf)) {
    throw "The documentation screenshot renderer was not found: $project"
}

$catalog = Join-Path $documentationRoot 'screenshots\manifest.json'
if (-not (Test-Path -LiteralPath $catalog -PathType Leaf)) {
    throw "The screenshot catalog was not found: $catalog"
}

if ($Id -and $Area) {
    throw 'Use either -Id or -Area, not both.'
}

$arguments = @(
    'run',
    '--project', $project,
    '--configuration', 'Debug',
    "-p:NinaSource=$($sourceRoot.Path)"
)
if (-not $Restore) {
    $arguments += '--no-restore'
}
$arguments += @(
    '--',
    '--catalog', $catalog,
    '--docs-root', $documentationRoot
)
if ($Id) {
    $arguments += @('--id', $Id)
}
if ($Area) {
    $arguments += @('--area', $Area)
}
if ($Preview) {
    $arguments += '--preview'
}

& dotnet @arguments
if ($LASTEXITCODE -ne 0) {
    throw "Screenshot generation failed with exit code $LASTEXITCODE. Existing documentation images were left unchanged."
}
