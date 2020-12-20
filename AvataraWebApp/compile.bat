@ECHO OFF
dotnet publish --self-contained -c Release -r ubuntu-x64 /p:PublishSingleFile=true /p:PublishTrimmed=true
RET dotnet publish --self-contained -c Release -r win-x64 /p:PublishSingleFile=true /p:PublishTrimmed=true
