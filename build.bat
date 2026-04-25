@echo off
set CSC="C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

if not exist %CSC% (
    echo Error: Could not find C# compiler at %CSC%
    echo Are you using a very old version of Windows?
    pause
    exit /b 1
)

echo Compiling LidController...
%CSC% /nologo /target:winexe /out:LidController.exe /win32manifest:app.manifest Program.cs MainForm.cs PowerInterop.cs
if %errorlevel% neq 0 (
    echo Compilation failed!
    pause
    exit /b %errorlevel%
)

echo Compilation succeeded: LidController.exe
pause
