# Errors should all be handled. If they're not, we want to see them.
$ErrorActionPreference = "Stop"

#Set root script directory.
$deploymentDirectoryPath = $PSScriptRoot
$versionFilePath = $deploymentDirectoryPath + "\_version.txt"
$packageDirectoryPath = $deploymentDirectoryPath + "\Package"
$contentDirectoryPath = $packageDirectoryPath + "\content"
$libDirectoryPath = $packageDirectoryPath + "\lib"
$net40DirectoryPath = $libDirectoryPath + "\net40"

$solutionDirectoryPath = $deploymentDirectoryPath + "\.."
$projectOutputDirectoryPath = ($deploymentDirectoryPath + "..\..\..\DocMAh\bin\Debug")
$nugetExePath = $solutionDirectoryPath + "\.nuget\NuGet.exe"

#Get next build number.
$version = Get-Content $versionFilePath
$versionComponents = $version.Split(".")
$version = [System.String]::Format("{0}.{1}.{2}.{3}", $versionComponents[0], $versionComponents[1], $versionComponents[2], [System.Int32]::Parse($versionComponents[3]) + 1)

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
Copy-Item ($deploymentDirectoryPath + "\web.config.uninstall.xdt") $contentDirectoryPath

Invoke-Expression "$($nugetExePath) pack $($packageDirectoryPath + "\DocMAH.nuspec") -version $($version) -OutputDirectory $($deploymentDirectoryPath)"

Remove-Item $packageDirectoryPath -Recurse
"Package directory deleted."

"Writing last build number to version file."
$version | Out-File $versionFilePath