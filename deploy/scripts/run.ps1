[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$RESOURCEGROUP_NAME,

     [Parameter(Mandatory = $true)]
    [string]$STORAGE_ACCOUNT_NAME,

    [Parameter(Mandatory = $true)]
    [string]$STORAGE_SHARE_NAME,

     [Parameter(Mandatory = $true)]
    [string]$otelScriptBase64
)

# Ensure the az module is installed
try {
    if (-not (Get-Module -ListAvailable -Name Az)) {
        Write-Output "Installing az module..."
        Install-Module -Name Az  -Force -AllowClobber -Scope CurrentUser
    }
} catch {
    Write-Error "Error installing az module: $_"
    exit 1
}



# Decode the Base64-encoded otel script
$otelScript = [System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($otelScriptBase64))

# Determine the correct temporary storage path within Azure Deployment Scripts
$tempFolder = if ($Env:AZ_SCRIPTS_TEMP) { $Env:AZ_SCRIPTS_TEMP } else { "/mnt/azscripts/azscriptinput" }

# Ensure the temp folder exists
if (!(Test-Path $tempFolder)) {
    Write-Output "Creating temp folder: $tempFolder"
    New-Item -ItemType Directory -Path $tempFolder -Force
}

# Save the decoded otel script to a temporary file
$tempotelFile = Join-Path $tempFolder "config.yaml"
Set-Content -Path $tempotelFile -Value $otelScript

Write-Output "config.yaml => : [$tempotelFile] ..."


      Write-Output "Getting storage account key..."
      $accountKey = (Get-AzStorageAccountKey -ResourceGroupName $RESOURCEGROUP_NAME -Name $STORAGE_ACCOUNT_NAME)[0].Value

      $ctx = New-AzStorageContext -StorageAccountName $STORAGE_ACCOUNT_NAME -StorageAccountKey $accountKey

      Write-Output "Uploading file to file share..."
      
$result = Set-AzStorageFileContent `
  -ShareName $STORAGE_SHARE_NAME `
  -Source $tempotelFile `
  -Path "/" `
  -Context $ctx `
  -Force `
  -Confirm:$false
  

  write-output "Set-AzStorageFileContent  => : $result"

  write-output "Verifying file: $STORAGE_SHARE_NAME in account: $STORAGE_ACCOUNT_NAME"

$file = Get-AzStorageFile `
  -ShareName $STORAGE_SHARE_NAME `
  -Path "config.yaml" `
  -Context $ctx `
  -ErrorAction SilentlyContinue

if ($file) {
    Write-Output "✅ Uploaded file: [$($file.Name)] successfully to file share '$STORAGE_SHARE_NAME'."
} else {
    Write-Error "❌ Upload failed: File 'config.yaml' not found in file share '$STORAGE_SHARE_NAME'."
    exit 1
}


Write-Output "File uploaded to Azure Storage Share: $STORAGE_SHARE_NAME in account: $STORAGE_ACCOUNT_NAME"