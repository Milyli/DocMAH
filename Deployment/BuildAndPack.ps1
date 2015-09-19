# Grand unified build and package script.
# 1. Advances the version number as needed.
#      Advances Revision value by default. (Major.Feature.Bug.Revision)
#      Advance different values using the -BuildType parameters Major, Feature, Bug or Revision
# 2. Builds the DocMAH project.
# 3. Packages the NuGetPackage.
# 4. Moves the package to a local server folder for testing before publish.
# 5. Publish handled manually. 
#      It's not that we don't trust all y'all with the keys to
#      the DocMAH NuGet kingdom; we just don't trust everyone else.

param(
    [ValidateSet("Major", "Feature", "Bug", "Revision")]
    [String] $BuildType
)


# Errors should all be handled. If they're not, we want to see them.
$ErrorActionPreference = "Stop"


$deploymentDirectoryPath = $PSScriptRoot
$advanceVersion = $deploymentDirectoryPath + '\AdvanceVersion.ps1'
$buildProject = $deploymentDirectoryPath + '\BuildProject.ps1'
$packNuGet = $deploymentDirectoryPath + '\PackNuGet.ps1'
$publishLocal = $deploymentDirectoryPath + '\PublishNuGetLocal.ps1'

Invoke-Expression -Command ($advanceVersion + ' -BuildType $BuildType')
Try {
    Invoke-Expression -Command $buildProject
}
Catch [System.Management.Automation.CommandNotFoundException]{
    "MSBUILD not found. MSBUILD must exist in your PATH environment variable."
    Exit
}
Invoke-Expression -Command $packNuGet
Invoke-Expression -Command $publishLocal
