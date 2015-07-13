# Called from master build script BuildAndPack.ps1
# Creates the NuGet package folder structure.
# Places files within the structure.
# Builds the NuGet package.

# Set script file paths.
$deploymentDirectoryPath = $PSScriptRoot
$versionFilePath = $deploymentDirectoryPath + "\_version.txt"
$packageDirectoryPath = $deploymentDirectoryPath + "\Package"
$contentDirectoryPath = $packageDirectoryPath + "\content"
$libDirectoryPath = $packageDirectoryPath + "\lib"
$net40DirectoryPath = $libDirectoryPath + "\net40"
$solutionDirectoryPath = $deploymentDirectoryPath + "\..\Solutions"
$projectOutputDirectoryPath = $deploymentDirectoryPath + "\..\DocMAh\bin\Debug"
$nugetExePath = $solutionDirectoryPath + "\.nuget\NuGet.exe"

# Get build number.
$version = Get-Content $versionFilePath

Try {
	Remove-Item $packageDirectoryPath -Recurse -ErrorAction Stop | Out-Null
	"Package directory deleted."
}
Catch {
	"Package directory does not exist."
}

"Creating package directory structure."
New-Item -ItemType directory -Path $packageDirectoryPath | Out-Null
New-Item -ItemType directory -Path $contentDirectoryPath | Out-Null
New-Item -ItemType directory -Path $libDirectoryPath | Out-Null
New-Item -ItemType directory -Path $net40DirectoryPath | Out-Null

"Moving package content to directories."
Copy-Item ($projectOutputDirectoryPath + "\*") $net40DirectoryPath
Copy-Item ($deploymentDirectoryPath + "\DocMAH.nuspec") $packageDirectoryPath
Copy-Item ($deploymentDirectoryPath + "\readme.txt") $packageDirectoryPath
Copy-Item ($deploymentDirectoryPath + "\web.config.install.xdt") $contentDirectoryPath
# Copy-Item ($deploymentDirectoryPath + "\web.config.uninstall.xdt") $contentDirectoryPath

Invoke-Expression "$($nugetExePath) pack $($packageDirectoryPath + "\DocMAH.nuspec") -version $($version) -OutputDirectory $($deploymentDirectoryPath)"

Remove-Item $packageDirectoryPath -Recurse
"Package directory deleted."