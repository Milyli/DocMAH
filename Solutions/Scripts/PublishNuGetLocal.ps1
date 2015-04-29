# This script copies the NuGet package to DocMAH\Solutions\Scripts\NuGet.DocMAH.Local
# Add this location to NuGet Package Manager in Visual Studio to test the package before uploading.
# Follow Step 10 here -> https://blogs.endjin.com/2014/07/how-to-test-nuget-packages-locally/

$existingLocation = get-item .\NuGet.DocMAH.Local
if ($existingLocation -eq $null) {
	new-item .\NuGet.DocMAH.Local -itemtype directory
}
copy-item ..\..\DocMAH.nuget\DocMAH.*.nupkg .\NuGet.DocMAH.Local