$ErrorActionPreference = "Stop"

$projectDirectory = "$PSScriptRoot\Community.PowerToys.Run.Plugin.Kinopio"
$projectFile = "$projectDirectory\Community.PowerToys.Run.Plugin.Kinopio.csproj"
[xml]$xml = Get-Content -Path $projectFile
$version = $xml.Project.PropertyGroup.Version
$version = "$version".Trim()

foreach ($platform in "x64", "ARM64")
{
    if (Test-Path -Path "$projectDirectory\bin")
    {
        Remove-Item -Path "$projectDirectory\bin\*" -Recurse
    }

    if (Test-Path -Path "$projectDirectory\obj")
    {
        Remove-Item -Path "$projectDirectory\obj\*" -Recurse
    }

    Write-Output "Building project for platform: $platform"
    dotnet build $projectFile -c Release -p:Platform=$platform

    $buildOutputPath = "$projectDirectory\bin\$platform\Release"
    if (-Not (Test-Path -Path $buildOutputPath))
    {
        Write-Error "Build output directory not found: $buildOutputPath"
        exit 1
    }

    Remove-Item -Path "$projectDirectory\bin\*" -Recurse -Include *.xml, *.pdb, PowerToys.*, Wox.*
    Rename-Item -Path $buildOutputPath -NewName "Kinopio"

    Compress-Archive -Path "$projectDirectory\bin\$platform\Kinopio" -DestinationPath "$PSScriptRoot\Kinopio-$version-$platform.zip" -Force
}