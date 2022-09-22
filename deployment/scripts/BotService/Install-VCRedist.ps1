function Install-VCRedist {
    param (
    $vmName,
    $vmResourceGroup,
    $vcRedistUri
    )

    Write-Host "(VM) Downloading and installing VCRedist.." -ForegroundColor green
    $VM_SCRIPT = "Invoke-WebRequest -UseBasicParsing -Uri " + $vcRedistUri + " -OutFile c:\\vcRedist.exe;Start-Process 'c:\\vcRedist.exe' -ArgumentList '/quiet'"
    az vm run-command invoke --command-id RunPowerShellScript --name $vmName -g $vmResourceGroup --scripts $VM_SCRIPT

}