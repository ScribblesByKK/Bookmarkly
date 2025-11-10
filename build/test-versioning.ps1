# Test script for versioning logic
# This script validates that the build number resets when the week changes

param(
    [switch]$Cleanup = $false
)

$ErrorActionPreference = "Stop"

# Save original version.json
$versionPath = "build/version.json"
$backupPath = "build/version.json.backup"

Write-Host "=== Testing Version Update Logic ===" -ForegroundColor Cyan

# Backup original file
Copy-Item $versionPath $backupPath -Force
Write-Host "✓ Created backup of version.json" -ForegroundColor Green

try {
    # Test 1: Same week - should increment
    Write-Host "`n--- Test 1: Same week increment ---" -ForegroundColor Yellow
    $testData1 = @{
        major = 1
        build = 3
        revision = 0
        lastYyww = "2546"
    } | ConvertTo-Json
    Set-Content $versionPath $testData1
    
    & ./build/update-version.ps1
    $result1 = Get-Content $versionPath | ConvertFrom-Json
    
    if ($result1.build -eq 4 -and $result1.lastYyww -eq "2546") {
        Write-Host "✓ PASS: Build incremented from 3 to 4 in same week" -ForegroundColor Green
    } else {
        Write-Host "✗ FAIL: Expected build=4, lastYyww=2546, got build=$($result1.build), lastYyww=$($result1.lastYyww)" -ForegroundColor Red
        exit 1
    }
    
    # Test 2: Week change - should reset to 1
    Write-Host "`n--- Test 2: Week change reset ---" -ForegroundColor Yellow
    $testData2 = @{
        major = 1
        build = 10
        revision = 0
        lastYyww = "2545"
    } | ConvertTo-Json
    Set-Content $versionPath $testData2
    
    & ./build/update-version.ps1
    $result2 = Get-Content $versionPath | ConvertFrom-Json
    
    if ($result2.build -eq 1 -and $result2.lastYyww -eq "2546") {
        Write-Host "✓ PASS: Build reset to 1 when week changed from 2545 to 2546" -ForegroundColor Green
    } else {
        Write-Host "✗ FAIL: Expected build=1, lastYyww=2546, got build=$($result2.build), lastYyww=$($result2.lastYyww)" -ForegroundColor Red
        exit 1
    }
    
    # Test 3: Multiple runs in same week
    Write-Host "`n--- Test 3: Multiple runs in same week ---" -ForegroundColor Yellow
    $testData3 = @{
        major = 1
        build = 1
        revision = 0
        lastYyww = "2546"
    } | ConvertTo-Json
    Set-Content $versionPath $testData3
    
    & ./build/update-version.ps1 | Out-Null
    $result3a = Get-Content $versionPath | ConvertFrom-Json
    
    & ./build/update-version.ps1 | Out-Null
    $result3b = Get-Content $versionPath | ConvertFrom-Json
    
    if ($result3a.build -eq 2 -and $result3b.build -eq 3 -and $result3b.lastYyww -eq "2546") {
        Write-Host "✓ PASS: Build correctly incremented 1→2→3 in same week" -ForegroundColor Green
    } else {
        Write-Host "✗ FAIL: Expected build sequence 2, 3, got $($result3a.build), $($result3b.build)" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "`n=== All Tests Passed! ===" -ForegroundColor Green
    
} finally {
    # Restore original file
    Move-Item $backupPath $versionPath -Force
    Write-Host "`n✓ Restored original version.json" -ForegroundColor Green
    
    if ($Cleanup) {
        # Clean up any test artifacts
        Write-Host "✓ Cleanup completed" -ForegroundColor Green
    }
}
