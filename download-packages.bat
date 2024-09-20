@echo off
if not exist nuget.exe powershell -Command "Invoke-WebRequest https://dist.nuget.org/win-x86-commandline/latest/nuget.exe -OutFile nuget.exe"
if not exist packages mkdir packages
nuget.exe install LiveCharts -Version 0.9.7 -OutputDirectory packages
nuget.exe install LiveCharts.Wpf -Version 0.9.7 -OutputDirectory packages