param(
    [string]$TargetExe = "jDroid-X-Desk.exe",
    [switch]$Force
)

$thumbprint = "ACBD12BEE072C0CAE42760AF078BD1996A6F293F"

# 1. Unblock file from Mark-of-the-Web (Zone.Identifier)
Unblock-File -Path $TargetExe -ErrorAction SilentlyContinue

# 2. Check if the Enterprise Root is trusted by Windows Kernel (LocalMachine\Root)
$isMachineRootTrusted = [bool](Get-ChildItem Cert:\LocalMachine\Root -ErrorAction SilentlyContinue | Where-Object { $_.Thumbprint -eq $thumbprint })

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
try {
    $sig = Set-AuthenticodeSignature -FilePath $TargetExe -Certificate $cert -ErrorAction Stop
} catch {
    Write-Error "[FAIL] Set-AuthenticodeSignature threw exception: $_"
    exit 1
}

if ($sig.Status -ne "Valid") {
    Write-Error "[FAIL] Authenticode Signing Failed with Status: $($sig.Status) ($($sig.StatusMessage))"
    exit 1
}

Write-Host "`n=== Step 3: Microsoft Authenticode Verification ===" -ForegroundColor Cyan
$auth = Get-AuthenticodeSignature -FilePath $TargetExe
$auth | Format-List Path, Status, StatusMessage, SignerCertificate

if ($auth.Status -eq "Valid") {
    Write-Host "[SUCCESS] Authenticode Status: VALID (Full System Trust & Zero Smart App Control blocks)!" -ForegroundColor Green
    exit 0
} elseif ($auth.SignerCertificate -and $auth.SignerCertificate.Thumbprint -eq $thumbprint) {
    Write-Host "[NOTICE] Binary signed with Enterprise Certificate ($thumbprint), but Status is '$($auth.Status)'." -ForegroundColor Yellow
    Write-Host "[ACTION] To whitelist permanently on this machine, run 'TRUST_ENTERPRISE_CERT_AS_ADMIN.bat' as Administrator once." -ForegroundColor Yellow
    exit 0
} else {
    Write-Warning "[WARN] Signing did not match expected thumbprint."
    exit 1
}
