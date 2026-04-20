@echo off
cd /d %~dp0\..\src\OracleAdminWinForms
dotnet restore
dotnet build
dotnet run
pause
