@echo off

set "PATH=C:\Windows\System32\;C:\Windows\Microsoft.NET\Framework64\v4.0.30319\;"

if not exist build mkdir build

echo Compiling ModbusTCPServer...
csc.exe /nologo /out:build\ModbusTCPServer.exe ModbusTCPServer.cs

if %ERRORLEVEL% NEQ 0 (
    echo Server compilation failed.
    pause
    exit /b 1
)

echo Compiling ModbusTCPClient...
csc.exe /nologo /out:build\ModbusTCPClient.exe ModbusTCPClient.cs

if %ERRORLEVEL% NEQ 0 (
    echo Client compilation failed.
    pause
    exit /b 1
)

echo Both server and client compiled successfully.

echo Starting ModbusTCPServer...
start "Modbus TCP Server" cmd /c build\ModbusTCPServer.exe

echo Waiting for server to initialize...
timeout /t 5 /nobreak

echo Starting ModbusTCPClient...
build\ModbusTCPClient.exe

echo Client execution finished. Server is still running in the background.
echo Press any key to exit...
pause