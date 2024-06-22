# PowerToys Run Plugin Template

## Usage

1. Create repository with this template.
1. Clone.
1. Execute following command.
    ```
    .\New-Solution.ps1
    ```
    1. You can specify options.
        ```
        .\New-Solution.ps1 -ActionKeyword $ActionKeyword -Author $AuthorName -ProjectName $ProjectName -DotnetVersion $DotnetVersion
        ```
1. Edit $ProjectName\Main.cs
1. Add icon image (path: `$ProjectName\images\icon.png`)
1. Build (zip file will be generated in repository root directory)
    ```
    .\Build-Solution.ps1
    ```

## Installation

1. Unzip and copy to `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins`

## Debug

Error log location: `%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Logs\$Version`
