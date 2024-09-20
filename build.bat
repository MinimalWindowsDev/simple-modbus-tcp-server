@echo off

set "MSBUILD=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe"

echo Downloading NuGet packages...
call download-packages.bat

echo Building ModbusTCPServer...
%MSBUILD% /nologo /verbosity:quiet ModbusTCPServer.csproj /p:Configuration=Release

if %ERRORLEVEL% NEQ 0 (
    echo Server build failed.
    pause
    exit /b 1
)

echo Building ModbusTCPClient...
%MSBUILD% /nologo /verbosity:quiet ModbusTCPClient.csproj /p:Configuration=Release

if %ERRORLEVEL% NEQ 0 (
    echo Client build failed.
    pause
    exit /b 1
)

echo Building ServerDashboard...
%MSBUILD% /nologo /verbosity:quiet ServerDashboard\ServerDashboard.csproj /p:Configuration=Release /tv:4.0 /p:TargetFrameworkVersion=v4.8

if %ERRORLEVEL% NEQ 0 (
    echo Server Dashboard build failed.
    pause
    exit /b 1
)

echo All projects built successfully.

echo Starting ModbusTCPServer...
start "Modbus TCP Server" cmd /c bin\Release\ModbusTCPServer.exe

echo Starting ServerDashboard...
start "Server Dashboard" cmd /c ServerDashboard\bin\Release\ServerDashboard.exe

echo Waiting for server to initialize...
timeout /t 5 /nobreak

echo Starting ModbusTCPClient...
bin\Release\ModbusTCPClient.exe

echo Client execution finished. Server and Dashboard are still running in the background.
echo Press any key to exit...
pause