Param(
  [string] $GStreamerPath = "c:\gstreamer\1.0\mingw_x86_64\bin\"
)

if (-not (Test-Path -Path $GStreamerPath))
{
    throw [System.IO.FileNotFoundException] "The GStreamer bin folder ""$GStreamerPath"" does not exist. Verify that GStreamer is installed and use -GStreamerPath parameter to specify the GStream bin path location (default $GStreamerPath)"
}

$RegexAddPath = [regex]::Escape($GStreamerPath)
$UserPath = [Environment]::GetEnvironmentVariable("PATH", [EnvironmentVariableTarget]::User)
$ArrPath = $UserPath -split ';' | Where-Object {$_ -notMatch "^$RegexAddPath\\?"}
$NewPath = ($ArrPath + $GStreamerPath) -join ';'

[Environment]::SetEnvironmentVariable("PATH", $NewPath, [EnvironmentVariableTarget]::User)

Write-Host "Added '$GStreamerPath' to PATH environment variable"
