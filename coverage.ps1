nuget install OpenCover -Version 4.6.519 -OutputDirectory coverage
nuget install ReportGenerator -Version 2.5.10 -OutputDirectory coverage
.\coverage\OpenCover.4.6.519\tools\OpenCover.Console.exe -output:"coverage\results.xml" -target:"C:\Program Files\dotnet\dotnet.exe" -targetargs:"test .\Automa.Opc.Ua.Client.Tests\Automa.Opc.Ua.Client.Tests.csproj" -filter:"+[*]* -[*.Tests]*" -register:user
.\coverage\ReportGenerator.2.5.10\tools\ReportGenerator.exe -targetdir:"coverage\report" -reporttypes:"Html;Badges" -reports:"coverage\results.xml" -verbosity:Error
.\coverage\report\index.htm