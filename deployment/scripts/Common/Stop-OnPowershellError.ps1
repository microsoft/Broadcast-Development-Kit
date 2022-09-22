function Stop-OnPowershellError {
    if ($error.count -gt 0) {
        Write-Host "Something Wrong happened" -ForegroundColor red
        Exit
    }
}