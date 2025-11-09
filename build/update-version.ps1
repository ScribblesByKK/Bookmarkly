# PowerShell script to update version for CI builds
# Format: major.yyww.build.revision
# - major: manually set by developers (default: 1)
# - yyww: last 2 digits of year + week number (yyyww for year >= 2100)
# - build: auto-incremented on each job
# - revision: manually set by developers (default: 0)

param(
    [Parameter(Mandatory=$false)]
    [string]$VersionFilePath = "build/version.json"
)

# Read the version file
if (-not (Test-Path $VersionFilePath)) {
    Write-Error "Version file not found: $VersionFilePath"
    exit 1
}

$versionData = Get-Content $VersionFilePath | ConvertFrom-Json

# Get current date
$currentDate = Get-Date

# Calculate week number (ISO 8601 week)
$culture = [System.Globalization.CultureInfo]::InvariantCulture
$calendar = $culture.Calendar
$weekRule = [System.Globalization.CalendarWeekRule]::FirstFourDayWeek
$firstDayOfWeek = [System.DayOfWeek]::Monday
$weekNumber = $calendar.GetWeekOfYear($currentDate, $weekRule, $firstDayOfWeek)

# Format week number with leading zero
$weekString = $weekNumber.ToString("00")

# Calculate year format
$year = $currentDate.Year
if ($year -ge 2100) {
    # Use yyyww format for year >= 2100
    $yearString = $year.ToString().Substring(1, 3)  # Gets last 3 digits (e.g., 2100 -> 100)
    $yyww = "$yearString$weekString"
} else {
    # Use yyww format for year < 2100
    $yearString = $year.ToString().Substring(2, 2)  # Gets last 2 digits (e.g., 2025 -> 25)
    $yyww = "$yearString$weekString"
}

# Increment build number
$newBuild = $versionData.build + 1

# Construct the full version
$fullVersion = "$($versionData.major).$yyww.$newBuild.$($versionData.revision)"

Write-Host "Current date: $($currentDate.ToString('yyyy-MM-dd'))"
Write-Host "Week number: $weekNumber"
Write-Host "Year/Week component: $yyww"
Write-Host "New build number: $newBuild"
Write-Host "Full version: $fullVersion"

# Update version data
$versionData.build = $newBuild

# Save updated version file
$versionData | ConvertTo-Json | Set-Content $VersionFilePath

# Update Package.appxmanifest
$manifestPath = "Bookmarkly.App/Package.appxmanifest"
if (Test-Path $manifestPath) {
    $manifest = Get-Content $manifestPath -Raw
    
    # Replace the version in the Identity element only
    # The version must be in format x.y.z.w where each component is 0-65535
    # Use a more specific regex to target only the Identity element's Version attribute
    $manifest = $manifest -replace '(<Identity[^>]*Version=")[^"]+(")', "`${1}$fullVersion`${2}"
    
    Set-Content $manifestPath $manifest
    Write-Host "Updated $manifestPath with version $fullVersion"
} else {
    Write-Warning "Manifest file not found: $manifestPath"
}

# Output the version for GitHub Actions
if ($env:GITHUB_OUTPUT) {
    Write-Output "version=$fullVersion" >> $env:GITHUB_OUTPUT
    Write-Host "Version output to GitHub Actions: $fullVersion"
} else {
    Write-Host "Not running in GitHub Actions context"
}

Write-Host "Version update completed successfully"
