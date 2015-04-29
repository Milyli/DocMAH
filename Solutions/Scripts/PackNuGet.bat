SET /P DocMAH_NuGet_Version= < _version.txt
cd ..\..\DocMAH.nuget
copy ..\DocMAH\bin\Debug\DocMAH.* .\lib\net40
copy ..\DocMAH\readme.txt .\
nuget pack -version %DocMAH_NuGet_Version%

PAUSE