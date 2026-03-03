# SetGlobalVersion .NET 10.0 Upgrade Tasks

## Overview

This document tracks the execution of the SetGlobalVersion VSIX extension upgrade from .NET Framework 4.8 to .NET 10.0. The single project will be upgraded using an atomic all-at-once approach, converting to SDK-style format and updating all dependencies simultaneously.

**Progress**: 2/2 tasks complete (100%) ![0%](https://progress-bar.xyz/100)

---

## Tasks

### [✓] TASK-001: Verify prerequisites *(Completed: 2026-03-03 00:24)*
**References**: Plan §Phase 0

- [✓] (1) Verify .NET 10.0 SDK installed per Plan §Prerequisites
- [✓] (2) .NET 10.0 SDK meets minimum requirements (**Verify**)
- [✓] (3) Verify Visual Studio baseline (17.10+) with VSIX development workload installed
- [✓] (4) VSIX development workload available (**Verify**)

### [✓] TASK-002: Atomic framework and package upgrade with build validation *(Completed: 2026-03-03 00:51)*
**References**: Plan §Phase 1, Plan §Step 1-6, Plan §Package Update Reference, Plan §Breaking Changes Catalog

- [✓] (1) Convert SetGlobalVersion.csproj to SDK-style format per Plan §Step 1 (preserve VSIX-specific properties: GeneratePkgDefFile, IncludeAssemblyInVSIXContainer, IncludeDebugSymbolsInVSIXContainer, manifest references)
- [✓] (2) Project file is valid SDK-style format (**Verify**)
- [✓] (3) Update target framework to net10.0-windows per Plan §Step 2 (add UseWPF=true and UseWindowsDesktop=true properties)
- [✓] (4) Target framework set to net10.0-windows with WPF enabled (**Verify**)
- [✓] (5) Update package references per Plan §Step 3 and Plan §Package Update Reference (6 packages: System.Text.Encodings.Web 8.0.0→10.0.3, Microsoft.VisualStudio.SDK 17.9.37000→16.0.208, Microsoft.VSSDK.BuildTools 17.9.3174→15.7.104, plus 3 Community toolkit packages per assessment guidance)
- [✓] (6) All package references updated (**Verify**)
- [✓] (7) Restore dependencies per Plan §Step 4
- [✓] (8) All dependencies restored successfully (**Verify**)
- [✓] (9) Build solution and fix all compilation errors per Plan §Step 5 and Plan §Breaking Changes Catalog (157 WPF APIs auto-resolve via net10.0-windows targeting; address package API changes or VSIX manifest compatibility issues as identified)
- [✓] (10) Solution builds with 0 errors and 0 warnings (**Verify**)
- [✓] (11) Verify VSIX file generated successfully in bin\Release or bin\Debug per Plan §Step 6
- [✓] (12) VSIX file exists in build output directory (**Verify**)
- [✓] (13) Commit all changes with message: "Complete .NET 10.0 upgrade (SDK-style conversion, framework update, package updates, build validation)"





