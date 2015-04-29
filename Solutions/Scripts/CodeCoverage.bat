RMDIR /s /q ..\TestResults

MKDIR ..\TestResults\Coverage
MKDIR ..\TestResults\UnitTests

..\packages\OpenCover.4.5.3723\OpenCover.Console.exe -register:user "-filter:+[DocMAH]* -[*UnitTest]*" "-target:..\packages\NUnit.Runners.2.6.4\tools\nunit-console-x86.exe" "-output:..\TestResults\Coverage\DocMAH.Coverage.xml" "-targetargs:/noshadow /xml=..\TestResults\UnitTests\DocMAH.UnitTests.xml ..\..\DocMAH.UnitTests\bin\Debug\DocMAH.UnitTests.dll"
..\packages\ReportGenerator.2.1.4.0\ReportGenerator.exe "-reports:..\TestResults\Coverage\DocMAH.Coverage.xml" "-targetdir:..\TestResults\Coverage\DocMAHCoverage"
