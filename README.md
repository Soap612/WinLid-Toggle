# Laptop Lid Behavior Controller

A very lightweight, natively compiled Windows application for cleanly controlling laptop lid-close behavior (i.e., whether the PC goes to sleep or stays awake) without bloating your system. This tool targets Windows APIs directly, minimizing memory and CPU footprints.

## Features

- **Stay Awake Mode**: Intercepts lid close events and overrides the Windows power plan so the PC stays awake.
- **Smart Default Restore**: Automatically backs up your exact old power settings before modifying them. When restoring defaults, it gracefully reverts to your individual configuration instead of blindly assigning "Sleep".
- **Completely Background Friendly**: Closes minimally to your System Tray causing zero system drag.
- **Native Efficiency**: Built on the native Win32 `powrprof.dll` API so it doesn’t poll your system endlessly like some scripting solutions do.

## System Requirements
- Windows 10 or 11 (Requires Administrator Privileges to reliably configure Power Plans).
- .NET Framework (built right into Windows).

## Building from Source

No large SDKs or Visual Studio environments are required to build this project—only tools that are already built into your Windows operating system.

Run the attached `build.bat` file to automatically invoke the C# Compiler (`csc.exe`) bundled locally with the OS:

```cmd
build.bat
```

This merges the source configurations (`Program.cs`, `MainForm.cs`, `PowerInterop.cs`), attaches the UAC Manifest (`app.manifest`), and produces a standalone `LidController.exe`.

## Start on Boot

To have this controller silently run in your System Tray each time you log into your PC:
1. Press `Win + R` and type `shell:startup`. Press Enter.
2. Inside that directory, Right-click to create a `Shortcut`.
3. Target the `LidController.exe` file, but append `-minimized` to the end of the shortcut target. It should look like this:
   `"C:\path\to\LidController.exe" -minimized`

## Technical Details

This app overrides typical power settings manually manipulating the properties via Windows API P/Invoke calls:
- `GUID_BUTTON_SUBGROUP ("4f971e89-eebd-4455-a8de-9e59040e7347")`
- `GUID_LIDCLOSE_ACTION ("5ca83367-6e45-459f-a27b-476b1d01c936")`

State values are stored per user locally at `HKCU\Software\LidBehaviorController`.
