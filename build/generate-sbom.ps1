# Generate SBOM for Bookmarkly - Direct NuGet Dependencies Only
# This script generates an SBOM containing only direct NuGet package dependencies

param(
    [string]$OutputPath = ".",
    [switch]$IncludeTransitive
)

# Read version from version.json
$versionFile = Join-Path $PSScriptRoot "version.json"
$version = Get-Content $versionFile | ConvertFrom-Json
$versionString = "$($version.major).$($version.build).$($version.revision)"

Write-Host "Generating SBOM for Bookmarkly v$versionString" -ForegroundColor Cyan

# Create temp directory for empty BuildDropPath
$tempDir = Join-Path $PSScriptRoot "..\temp_sbom"
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

# Clean up any existing manifest
$manifestPath = Join-Path $OutputPath "_manifest"
if (Test-Path $manifestPath) {
    Remove-Item -Path $manifestPath -Recurse -Force
}

# Find sbom.exe
$sbomTool = "$env:LOCALAPPDATA\Microsoft\WinGet\Packages\Microsoft.SBOMTool_Microsoft.Winget.Source_8wekyb3d8bbwe\sbom.exe"
if (-not (Test-Path $sbomTool)) {
    Write-Error "SBOM tool not found. Please install it using: winget install Microsoft.SBOMTool"
    exit 1
}

# Generate SBOM
Write-Host "Running SBOM generation..." -ForegroundColor Yellow
$componentPath = Split-Path $PSScriptRoot -Parent

& $sbomTool generate `
    -BuildDropPath $tempDir `
    -BuildComponentPath $componentPath `
    -PackageName "Bookmarkly" `
    -PackageVersion $versionString `
    -PackageSupplier "ScribblesByKK" `
    -NamespaceUriBase "https://github.com/ScribblesByKK/Bookmarkly" `
    -AdditionalComponentDetectorArgs "--DetectorCategories NuGet" `
    -ManifestDirPath $OutputPath `
    -DeleteManifestDirIfPresent true `
    -Verbosity Information

# Clean up temp directory
Remove-Item -Path $tempDir -Recurse -Force

if (-not $IncludeTransitive) {
    Write-Host "Filtering to direct dependencies only..." -ForegroundColor Yellow
    
    # Read the generated SBOM
    $sbomFile = Join-Path $OutputPath "_manifest\spdx_2.2\manifest.spdx.json"
    $sbom = Get-Content $sbomFile | ConvertFrom-Json
    
    # Get all direct dependencies from project files
    $projectFiles = Get-ChildItem -Path $componentPath -Recurse -Filter "*.csproj"
    $directPackages = @{}
    
    foreach ($projectFile in $projectFiles) {
        [xml]$project = Get-Content $projectFile.FullName
        $packageRefs = $project.Project.ItemGroup.PackageReference
        foreach ($ref in $packageRefs) {
            $pkgName = $ref.Include
            if ($pkgName) {
                $directPackages[$pkgName] = $true
            }
        }
    }
    
    # Also include packages from Directory.Packages.Props
    $dirPackagesFile = Join-Path $componentPath "Directory.Packages.Props"
    if (Test-Path $dirPackagesFile) {
        [xml]$dirPackages = Get-Content $dirPackagesFile
        $packageVersions = $dirPackages.Project.ItemGroup.PackageVersion
        foreach ($pkg in $packageVersions) {
            $pkgName = $pkg.Include
            if ($pkgName) {
                $directPackages[$pkgName] = $true
            }
        }
    }
    
    Write-Host "Found $($directPackages.Count) direct package references" -ForegroundColor Green
    
    # Filter packages to only include direct dependencies
    $filteredPackages = $sbom.packages | Where-Object { 
        $directPackages.ContainsKey($_.name) 
    }
    
    Write-Host "Filtered from $($sbom.packages.Count) to $($filteredPackages.Count) packages" -ForegroundColor Green
    
    # Update the SBOM
    $sbom.packages = $filteredPackages
    
    # Save the filtered SBOM
    $sbom | ConvertTo-Json -Depth 100 | Set-Content $sbomFile -Encoding UTF8
    
    # Regenerate the SHA256 hash
    $hash = Get-FileHash -Path $sbomFile -Algorithm SHA256
    $hash.Hash | Set-Content "$sbomFile.sha256" -Encoding ASCII
}

Write-Host "`nSBOM generated successfully!" -ForegroundColor Green
Write-Host "Location: $manifestPath" -ForegroundColor Cyan
Write-Host "Version: $versionString" -ForegroundColor Cyan

# Display summary
$sbomFile = Join-Path $OutputPath "_manifest\spdx_2.2\manifest.spdx.json"
$finalSbom = Get-Content $sbomFile | ConvertFrom-Json
Write-Host "`nPackages in SBOM: $($finalSbom.packages.Count)" -ForegroundColor Cyan
Write-Host "Files in SBOM: $($finalSbom.files.Count)" -ForegroundColor Cyan
