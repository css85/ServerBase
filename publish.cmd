dotnet publish .\src\FrontEndWeb\FrontEndWeb.csproj -o ./output/front
dotnet publish .\src\BattleServer\BattleServer.csproj -o ./output/battle
dotnet publish .\src\ChatServer\ChatServer.csproj -o ./output/chat
dotnet publish .\src\GateServer\GateServer.csproj -o ./output/gate
dotnet publish .\src\WebTool\WebTool.csproj -o ./output/webtool
dotnet publish .\src\SponsorshipServer\SponsorshipServer.csproj -o ./output/sponsorship
dotnet publish .\test\TestConsoleApp\TestConsoleApp.csproj -o ./output/stress

xcopy .\src\csvdata .\output\csvdata /Y