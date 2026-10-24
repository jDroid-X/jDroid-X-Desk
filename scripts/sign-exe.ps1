param(
    [string]$TargetExe = "jDroid-X-Desk.exe"
)

$thumbprint = "ACBD12BEE072C0CAE42760AF078BD1996A6F293F"

Write-Host "=== Step 1: Whitelist in Trusted Publisher Store ===" -ForegroundColor Cyan
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$cerPath = Join-Path $rootDir "jDroid-X-Enterprise.cer"
if (Test-Path $cerPath) {
    & certutil -user -addstore TrustedPublisher $cerPath | Out-Null
    Write-Host "[+] Registered in CurrentUser\TrustedPublisher" -ForegroundColor Green
}

Write-Host "`n=== Step 2: Sign Binary with Authenticode ===" -ForegroundColor Cyan
$cert = Get-ChildItem Cert:\CurrentUser\My | Where-Object { $_.Thumbprint -eq $thumbprint -and $_.HasPrivateKey }
if (-not $cert) {
    $cert = Get-ChildItem Cert:\CurrentUser\My | Where-Object { $_.Subject -like "*jDroid-X Studio Enterprise*" -and $_.HasPrivateKey } | Select-Object -First 1
}

if (-not $cert) {
    Write-Error "Code-signing certificate not found in Cert:\CurrentUser\My!"
    exit 1
}

Write-Host "[+] Signer: $($cert.Subject)"
$sig = Set-AuthenticodeSignature -FilePath $TargetExe -Certificate $cert

Write-Host "`n=== Step 3: Microsoft Authenticode Verification ===" -ForegroundColor Cyan
$auth = Get-AuthenticodeSignature -FilePath $TargetExe
$auth | Format-List Path, Status, StatusMessage, SignerCertificate

if ($auth.SignerCertificate -and $auth.SignerCertificate.Thumbprint -eq $thumbprint) {
    Write-Host "[SUCCESS] Binary successfully signed by Enterprise Whitelisted Certificate ($thumbprint)!" -ForegroundColor Green
    exit 0
} else {
    Write-Warning "[WARN] Signing did not match expected thumbprint."
    exit 1
}
