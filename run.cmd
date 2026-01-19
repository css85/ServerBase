@echo off
cmd /C start "battle" /D "%CD%\output\battle" dotnet .\BattleServer.dll --ENVIRONMENT=LocalPublishKgh
cmd /C start "chat" /D "%CD%\output\chat" dotnet .\ChatServer.dll --ENVIRONMENT=LocalPublishKgh
cmd /C start "front" /D "%CD%\output\front" dotnet .\FrontEndWeb.dll --ENVIRONMENT=LocalPublishKgh
cmd /C start "webtool" /D "%CD%\output\webtool" dotnet .\WebTool.dll --ENVIRONMENT=LocalPublishKgh
cmd /C start "gate" /D "%CD%\output\gate" dotnet .\GateServer.dll --ENVIRONMENT=LocalPublishKgh
