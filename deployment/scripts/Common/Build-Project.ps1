function Build-Project {
    param(
        $projectSrc,
        $publishOutputSrc
    )

    $currentPath = Convert-Path .
    cd $projectSrc

    if(Test-Path -Path $publishOutputSrc){
        Write-Host "Removing Previous Artifact..." -ForegroundColor green
        Remove-Item $publishOutputSrc -Force -Recurse
    }

    Write-Host "Publishing project..." -ForegroundColor green
    dotnet publish -o $publishOutputSrc
    Write-Host "Project Published in: ${projectSrc}\\${publishOutputSrc}" -ForegroundColor green

    cd $currentPath
}
