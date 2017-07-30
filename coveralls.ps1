nuget install OpenCover -Version 4.6.519 -OutputDirectory coverage
nuget install coveralls.net -Version 0.7.0 -OutputDirectory coverage
.\coverage\OpenCover.4.6.519\tools\OpenCover.Console.exe `
-oldStyle `
-output:"coverage\results.xml" `
-target:"C:\Program Files\dotnet\dotnet.exe" `
-targetargs:"test .\Automa.Opc.Ua.Client.Tests\Automa.Opc.Ua.Client.Tests.csproj" `
-filter:"+[Automa.Opc.Ua.Client*]* -[Automa.Opc.Ua.Client.Tests*]* -[Automa.Opc.Ua.Client.Sample*]*" `
-register:user
.\coverage\coveralls.net.0.7.0\tools\csmacnz.Coveralls.exe --opencover -i "coverage\results.xml"
