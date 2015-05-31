# Called from master build script BuildAndPack.ps1
# This script copies the NuGet package to DocMAH\Solutions\Scripts\NuGet.DocMAH.Local
# Add this location to NuGet Package Manager in Visual Studio to test the package before uploading.
# Follow instructions here https://docs.nuget.org/create/hosting-your-own-nuget-feeds

$deploymentDirectoryPath = $PSScriptRoot
$nugetDirectoryPath = $deploymentDirectoryPath + '\NuGetLocal'
$packagePath = $deploymentDirectoryPath + '\DocMAH.*.nupkg'

Try 
{
    Get-Item $nugetDirectoryPath -ErrorAction Stop | Out-Null
}
Catch [System.Management.Automation.ItemNotFoundException] {
    
	New-Item $nugetDirectoryPath -ItemType Directory
}
Move-Item $packagePath $nugetDirectoryPath -Force
"Moved package to " + $nugetDirectoryPath
"Include directory above in your NuGet Package Sources to test the package."
"    See https://docs.nuget.org/create/hosting-your-own-nuget-feeds for instructions."