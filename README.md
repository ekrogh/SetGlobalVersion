# SetGlobalVersion

`SetGlobalVersion` is a Visual Studio extension that adds a **Set Global Version** command on the solution context menu and opens a tool window for updating version values across projects in a solution.

## What it does

- Shows a solution-level tool window from Solution Explorer (`Right-click solution -> Set Global Version`)
- Scans solution projects for version-bearing files
- Lets you set:
  - `Version Major`
  - `Version Minor`
  - `Build Number`
  - `Revision Number`
- Applies updates to supported files (for example project files and manifests)
- Stores last-used values in `MajorMinorBuildRevisionNumbers.xml`

## Current supported project/version patterns

The extension currently detects and updates common version properties used by MAUI and Avalonia-style projects, including:

- `ApplicationDisplayVersion`
- `ApplicationVersion`
- `Version`
- `AssemblyVersion`
- `FileVersion`
- `InformationalVersion`

It also handles several manifest/version file types used across solution projects.

## Requirements

- Visual Studio 2022 (`17.x`)
- .NET Framework `4.8` build environment for this extension project

## Build

From the solution root:

1. Restore/build in Visual Studio, or
2. Build the `SetGlobalVersion` project directly

A `.vsix` is produced in `SetGlobalVersion\bin\Release`.

## Notes

If you are troubleshooting command visibility:

- Ensure command registration uses `Menus.ctmenu`
- Re-launch the Experimental instance after changes

## Quick Start

See `GETTING_STARTED.md`.

