@echo off

set "PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319;%PATH%"

echo Building ModbusTCPServer...
msbuild.exe /nologo /verbosity:quiet ModbusTCPServer.csproj /p:Configuration=Release

if %ERRORLEVEL% NEQ 0 (
    echo Server build failed.
    pause
    exit /b 1
)

echo Building ModbusTCPClient...
msbuild.exe /nologo /verbosity:quiet ModbusTCPClient.csproj /p:Configuration=Release

if %ERRORLEVEL% NEQ 0 (
    echo Client build failed.
    pause
    exit /b 1
)

echo Both server and client built successfully.

echo Starting ModbusTCPServer...
start "Modbus TCP Server" cmd /c bin\Release\ModbusTCPServer.exe

echo Waiting for server to initialize...
timeout /t 5 /nobreak

echo Starting ModbusTCPClient...
bin\Release\ModbusTCPClient.exe

echo Client execution finished. Server is still running in the background.
echo Press any key to exit...
pause