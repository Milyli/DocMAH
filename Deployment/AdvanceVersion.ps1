# Called from master build script BuildAndPack.ps1
# Advances the version number in _version.txt based on the type of build being made.

param(
    [ValidateSet("Major", "Feature", "Bug", "Revision")]
    [String] $BuildType
)

# Set script file paths.
$deploymentDirectoryPath = $PSScriptRoot
$versionFilePath = $deploymentDirectoryPath + "\_version.txt"

# Read last build number from file.
$version = Get-Content $versionFilePath

# Split into components and advance as instructed from command line parameter if present.
$versionComponents = $version.Split(".")
$major = [System.Int32]::Parse($versionComponents[0])
$minor = [System.Int32]::Parse($versionComponents[1]) 
$build = [System.Int32]::Parse($versionComponents[2])
$revision = [System.Int32]::Parse($versionComponents[3])

# Advance build number as specified.
if ($BuildType.ToLower() -eq "major") { 
    $major = $major + 1 
    $minor = 0
    $build = 0
    $revision = 0
}
elseif ($BuildType.ToLower() -eq "feature") { 
    $minor = $minor + 1
    $build = 0
    $revision = 0
}
elseif ($BuildType.ToLower() -eq "bug") { 
    $build = $build + 1
    $revision = $revision = 0
}
elseif ([String]::IsNullOrEmpty($BuildType) -or $BuildType.ToLower() -eq "revision") { 
    $revision = $revision + 1 
}
else {
    "Invalid BuildType value. Valid values are: Major, Feature, Bug, Revision (default)"
    Exit
}

# Compose next build number
$version = [System.String]::Format("{0}.{1}.{2}.{3}", $major, $minor, $build, $revision)

"Writing next version number to file -> " + $version
$version | Out-File $versionFilePath