# This script copies the NuGet package to DocMAH\Solutions\Scripts\NuGet.DocMAH.Local
# Add this location to NuGet Package Manager in Visual Studio to test the package before uploading.
# Follow Step 10 here -> https://blogs.endjin.com/2014/07/how-to-test-nuget-packages-locally/

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