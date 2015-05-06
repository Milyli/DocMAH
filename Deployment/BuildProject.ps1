# Replaces the version numbers in DocMAH AssemblyInfo.cs, 
#   builds the DocMAH project, and reverts the version numbers.

# MSBUILD.EXE must exist in your PATH environment variable.

# set script file paths
$deploymentDirectoryPath = $PSScriptRoot
$projectDirectoryPath = $deploymentDirectoryPath + "\..\DocMAH"
$projectPath = $projectDirectoryPath + "\DocMAH.csproj"
$assemblyInfoPath = $projectDirectoryPath + "\Properties\AssemblyInfo.cs"
$versionFilePath = $deploymentDirectoryPath + "\_version.txt"

# Get next build number.
$version = Get-Content $versionFilePath

"Replace assembly version attribute values with build version -> " + $version
(Get-Content $assemblyInfoPath) |
ForEach-Object { $_ -replace '\("(\d+\.\d+\.\d+\.\d+)"\)', ('("' + $version + '")') } |
Set-Content $assemblyInfoPath

# Build solution.
Invoke-Expression "msbuild $($projectPath) /target:Rebuild"

"Replace assembly version attribute values with development version -> 0.0.0.1"
(Get-Content $assemblyInfoPath) |
ForEach-Object { $_ -replace '\("(\d+\.\d+\.\d+\.\d+)"\)', '("0.0.0.1")' } |
Set-Content $assemblyInfoPath
