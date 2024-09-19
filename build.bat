@echo off

set "PATH=C:\Windows\System32\;C:\Windows\Microsoft.NET\Framework64\v4.0.30319\;"

if not exist build mkdir build

csc.exe /nologo /out:build\ModbusTCPServer.exe ModbusTCPServer.cs

if %ERRORLEVEL% EQU 0 (
    echo Compilation successful. Running the program...
    build\ModbusTCPServer.exe
) else (
    echo Compilation failed.
)

pause