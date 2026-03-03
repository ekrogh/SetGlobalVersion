# .NET 10.0 Upgrade Plan

## Table of Contents

- [Executive Summary](#executive-summary)
- [Migration Strategy](#migration-strategy)
- [Detailed Dependency Analysis](#detailed-dependency-analysis)
- [Implementation Timeline](#implementation-timeline)
- [Detailed Execution Steps](#detailed-execution-steps)
- [Package Update Reference](#package-update-reference)
- [Breaking Changes Catalog](#breaking-changes-catalog)
- [Project-by-Project Migration Plans](#project-by-project-migration-plans)
- [Risk Management](#risk-management)
- [Testing & Validation Strategy](#testing-and-validation-strategy)
- [Complexity & Effort Assessment](#complexity-and-effort-assessment)
- [Source Control Strategy](#source-control-strategy)
- [Success Criteria](#success-criteria)
- [Maui and Avalonia Windows-Only Handling](#maui-and-avalonia-windows-only-handling)
- [Visual Studio 2026 Extension Targeting](#visual-studio-2026-extension-targeting)

---

## Executive Summary

### Scenario Overview
This plan is refocused so the extension's core responsibility is to **set version numbers globally at the solution (`.sln`) level**, including solutions that contain **MAUI** and **Avalonia** projects.

The upgrade target remains a **Visual Studio 2026-ready extension** on modern .NET, while preserving Windows-hosted VSIX constraints and handling MAUI/Avalonia as project types discovered inside the loaded solution.

### Scope
- **Projects Affected**: 1 extension project (`SetGlobalVersion.csproj`) plus logic for solution-level operations on contained projects
- **Current State**: .NET Framework 4.8, Classic WPF project (non-SDK-style)
- **Target State**: .NET 10.0-windows, SDK-style VSIX extension targeting VS 2026 readiness
- **Primary Functional Goal**: Set/propagate version numbers globally for all eligible projects in a loaded `.sln`, including MAUI and Avalonia project types
- **Total Codebase**: 2,600 lines of code across 11 code files
- **Estimated Impact**: ~157+ lines requiring modifications (minimum baseline from assessment)

### Selected Strategy
**All-At-Once Strategy** - The entire project will be upgraded simultaneously in a single coordinated operation.

### Rationale
- **Single Project**: Only one project with zero internal dependencies - perfect candidate for atomic upgrade
- **Manageable Complexity**: Medium complexity with clear migration path (WPF to Windows Desktop)
- **Clear Compatibility**: All issues identified with well-documented solutions
- **Efficiency**: All-at-once approach minimizes overhead and delivers fastest completion

### Complexity Assessment

**Discovered Metrics:**
- Total Projects: 1
- Dependency Depth: 0 (standalone project)
- Total Lines of Code: 2,600
- API Issues: 157 (binary incompatible WPF APIs)
- Package Issues: 6 packages requiring action
- Security Vulnerabilities: 0

**Classification:** Simple solution with straightforward upgrade path

**Expected Iterations:** 6 total iterations
- Phase 1: Discovery & Classification (3 iterations) ✓
- Phase 2: Foundation (3 iterations)
- Phase 3: Detail Generation (2 batched iterations)

### Critical Issues
1. **Project Conversion Required**: Classic project format must be converted to SDK-style
2. **WPF API Compatibility**: 157 WPF API usages require Windows-specific targeting (`net10.0-windows`)
3. **VS SDK Packages**: 5 Visual Studio SDK packages are incompatible and need version adjustments
4. **VSIX Manifest/Targeting**: Extension manifest must be prepared for VS 2022 and validated for VS 2026 preview channels
5. **Cross-UI Adaptation Scope**: MAUI/Avalonia cannot directly replace the VS extension host UI; shared logic must be extracted into reusable libraries

### Recommended Approach
Execute atomic upgrade and functional refocus with the following sequence:
1. Convert project to SDK-style format
2. Update target framework to `net10.0-windows`
3. Update NuGet package references
4. Address WPF API compatibility (automatic with Windows targeting)
5. Add/adjust solution traversal logic to detect MAUI and Avalonia projects in `.sln`
6. Add/adjust global version propagation logic to apply version updates consistently across eligible project files
7. Build and fix compilation errors
8. Validate VSIX deployment and `.sln`-level version update behavior

## Migration Strategy

### Approach Selection: All-At-Once

**Selected Strategy:** All-At-Once Strategy

This upgrade will execute all changes simultaneously in a single coordinated operation. All project file modifications, package updates, and code changes will be applied together, followed by comprehensive build validation and testing.

### Justification

**Why All-At-Once is Optimal:**

1. **Single Project Scope**: With only one project and no internal dependencies, there's no benefit to phasing - no intermediate states to maintain
2. **Atomic WPF Migration**: WPF compatibility is achieved by targeting `net10.0-windows` - this is a single property change that enables all 157 WPF APIs automatically
3. **Minimized Overhead**: No multi-targeting complexity, no incremental builds, no coordination across phases
4. **Fastest Completion**: Complete the upgrade in one pass rather than multiple phases
5. **Clear Validation**: Single comprehensive test pass rather than multiple phase validations

**Risks Mitigated:**
- Medium complexity handled through thorough breaking changes catalog
- All package incompatibilities identified and addressed in single operation
- WPF APIs automatically compatible once Windows targeting enabled

### Dependency-Based Ordering

While dependency ordering is a core principle, it's not applicable here:
- **Zero internal dependencies** - No project-to-project ordering required
- **External dependencies** - NuGet packages updated together as part of atomic operation

### Execution Approach

**Single Atomic Operation (VSIX host upgrade):**
- Convert project to SDK-style format
- Update TargetFramework property to `net10.0-windows`
- Update all NuGet package references
- Restore dependencies
- Build and address compilation errors
- Validate VSIX functionality

**Cross-UI Adaptation Track (planned in same modernization wave):**
- Extract non-UI logic into a shared library (`netstandard2.1` or `net8.0+` multi-target as needed)
- Keep VSIX UI host on WPF/Windows Desktop
- Add separate UI hosts for MAUI and Avalonia that consume the shared library
- Maintain clear host boundaries so VS extension-specific APIs remain isolated

**Targeting Model:**
- VSIX host moves directly from `net48` to `net10.0-windows`
- MAUI/Avalonia adaptation is additive via separate projects, not direct in-place replacement of VSIX host UI

### Success Indicators for All-At-Once

- ✅ Project file is valid SDK-style format
- ✅ Solution builds with zero errors
- ✅ All 157 WPF API issues resolved (via Windows targeting)
- ✅ All package dependencies restored successfully
- ✅ VSIX extension packages and installs correctly
- ✅ Extension functions in supported Visual Studio baseline and passes VS 2026 compatibility validation when channel/tooling is available

## Detailed Dependency Analysis

### Dependency Graph Summary

SetGlobalVersion is a **standalone project** with no internal project dependencies. This significantly simplifies the migration strategy as there are no dependency ordering constraints to consider.

```
┌─────────────────────────────┐
│ SetGlobalVersion.csproj     │
│ (.NET Framework 4.8)        │
│                             │
│ Type: WPF VSIX Extension    │
│ Dependencies: 0 projects    │
│ Dependants: 0 projects      │
└─────────────────────────────┘
```

### External Dependencies

The project depends on **17 NuGet packages**, primarily:
- **Visual Studio SDK packages** (Community.VisualStudio.Toolkit, VSSDK, etc.)
- **EnvDTE assemblies** (Visual Studio automation)
- **WPF framework libraries** (included in Windows Desktop SDK)

### Project Grouping

**Single Phase Migration:**
- **Phase 1**: SetGlobalVersion.csproj (atomic upgrade)

### Critical Path

Since there is only one project, the critical path is straightforward:
1. Convert to SDK-style → 2. Update framework → 3. Update packages → 4. Fix compilation → 5. Test

### Circular Dependencies

**None** - No circular dependencies exist in this single-project solution.

### Migration Order Rationale

With zero internal dependencies, there are no ordering constraints. The entire upgrade can proceed as a single atomic operation.

## Implementation Timeline

### Phase 0: Preparation (If Applicable)

**Operations:**
- Verify .NET 10.0 SDK installation
- Confirm supported Visual Studio baseline with VSIX development workload
- Confirm Visual Studio 2026 preview/build channel availability for compatibility validation (if available)
- Review extension compatibility requirements

**Deliverables:** Development environment ready

### Phase 1: Atomic Upgrade

**Operations** (performed as single coordinated batch):
- Convert SetGlobalVersion.csproj to SDK-style format
- Update TargetFramework to net10.0-windows
- Update all NuGet package references (17 packages, 6 requiring updates)
- Restore dependencies
- Build solution and fix all compilation errors
- Address breaking changes as identified

**Deliverables:** Solution builds with 0 errors, 0 warnings

### Phase 2: VSIX Validation & Testing

**Operations:**
- Validate VSIX manifest compatibility
- Package VSIX extension
- Deploy to experimental Visual Studio instance
- Execute manual functionality tests
- Verify extension loads and operates correctly
- Validate compatibility matrix for VS 2022 and VS 2026 preview/builds when available

**Deliverables:** Extension fully functional in Visual Studio 2022 and prepared/validated for VS 2026 compatibility windows

### Phase 3: MAUI and Avalonia Adaptation (Windows-Only Hosts)

**Operations:**
- Identify UI-independent services and extract to shared library
- Define adapter boundaries for Visual Studio-specific APIs
- Create MAUI Windows host project consuming shared library
- Create Avalonia Windows host project consuming shared library
- Align command/workflow behavior across VSIX, MAUI, and Avalonia hosts
- Ensure MAUI/Avalonia hosts are configured for Windows runtime targets only

**Deliverables:** Shared-core architecture with MAUI and Avalonia **Windows-only** host readiness plan

## Detailed Execution Steps

### Solution-Level Functional Objective
The extension must provide a single operation that sets version values globally for the current solution and applies them to supported project files, explicitly including MAUI and Avalonia project types when present.

### Step 1: Convert Project to SDK-Style

**Project to Convert:**
- SetGlobalVersion\SetGlobalVersion.csproj

**Conversion Process:**
1. Use automated SDK-style conversion tool
2. Verify generated project file structure
3. Ensure VSIX-specific properties preserved:
   - `<GeneratePkgDefFile>`
   - `<IncludeAssemblyInVSIXContainer>`
   - `<IncludeDebugSymbolsInVSIXContainer>`
   - VSIX manifest references
4. Validate custom MSBuild imports retained

**Expected Outcome:** Valid SDK-style .csproj file

### Step 2: Update Target Framework

**Framework Update:**
- **Current**: `<TargetFramework>net48</TargetFramework>`
- **Target**: `<TargetFramework>net10.0-windows</TargetFramework>`

**Additional Properties Required:**
```xml
<TargetFramework>net10.0-windows</TargetFramework>
<UseWPF>true</UseWPF>
<UseWindowsDesktop>true</UseWindowsDesktop>
```

⚠️ **Critical**: The `-windows` suffix is mandatory for WPF compatibility. Without it, all 157 WPF APIs will fail to compile.

### Step 3: Update NuGet Package References

See §Package Update Reference for complete package version matrix.

**Key Package Updates:**
- System.Text.Encodings.Web: 8.0.0 → 10.0.3 (recommended update)
- Microsoft.VisualStudio.SDK: 17.9.37000 → 16.0.208 (incompatibility fix)
- Microsoft.VSSDK.BuildTools: 17.9.3174 → 15.7.104 (incompatibility fix)
- 4 additional packages require version changes (see table below)

**Compatible Packages (No Change):**
- 11 packages marked compatible in assessment - retain current versions

### Step 4: Restore Dependencies and Build

**Restore:**
```
dotnet restore SetGlobalVersion.sln
```

**Build:**
```
dotnet build SetGlobalVersion.sln
```

**Expected Outcome:** Build completes, compilation errors identified

### Step 5: Fix Compilation Errors

See §Breaking Changes Catalog for comprehensive issue list.

**Primary Fixes Required:**
1. **WPF API Resolution**: Automatically resolved by net10.0-windows targeting (157 issues)
2. **Package API Changes**: Address any breaking changes from updated packages
3. **VSIX Manifest**: Update if tooling reports compatibility issues
4. **Obsolete APIs**: Replace any deprecated VS SDK APIs

**Validation Approach:**
- Address errors in order of frequency (DataGridTextColumn, Style, RoutedEventArgs, etc.)
- Most WPF errors should auto-resolve; investigate any persistent issues
- Consult breaking changes documentation for package-specific changes

### Step 6: Rebuild and Verify

**Rebuild:**
```
dotnet build SetGlobalVersion.sln --no-restore
```

**Success Criteria:**
- ✅ 0 build errors
- ✅ 0 build warnings
- ✅ VSIX file generated successfully
- ✅ All WPF API issues resolved

### Step 7: Package and Deploy VSIX

**Package:**
- Build outputs VSIX file to bin\Debug or bin\Release
- Verify .vsix file created successfully

**Deploy to Experimental Instance:**
1. Close all Visual Studio instances
2. Install VSIX to experimental instance:
   ```
   VSIXInstaller.exe /skuName:Pro /skuVersion:17.0 SetGlobalVersion.vsix
   ```
3. Launch experimental instance:
   ```
   devenv.exe /RootSuffix Exp
   ```

### Step 8: Implement/Refine Global `.sln` Version Propagation Behavior

**Planned behavior:**
- Detect all eligible projects in loaded `.sln`
- Identify MAUI and Avalonia projects via project SDK/type characteristics
- Normalize version fields to be updated (for example: `Version`, `VersionPrefix`, `AssemblyVersion`, `FileVersion`, `InformationalVersion` as configured)
- Apply updates consistently with clear skip/report behavior for unsupported project formats

**Output requirements:**
- Summary of projects updated, skipped, and failed
- Per-project reason for skip/failure
- No partial silent failure

### Step 9: Manual Validation Testing

See §Testing & Validation Strategy for detailed test scenarios, including global solution update scenarios.

## Package Update Reference

### Packages Requiring Updates (6 Total)

| Package | Current Version | Target Version | Reason | Priority |
|---------|----------------|----------------|---------|----------|
| System.Text.Encodings.Web | 8.0.0 | 10.0.3 | Upgrade recommended for .NET 10 compatibility | High |
| Microsoft.VisualStudio.SDK | 17.9.37000 | 16.0.208 | Incompatible - needs version adjustment | **Critical** |
| Microsoft.VSSDK.BuildTools | 17.9.3174 | 15.7.104 | Incompatible - build tools version | **Critical** |
| Community.VisualStudio.Toolkit.17 | 17.0.507 | *(Research)* | Incompatible - verify .NET 10 compatible version | High |
| Community.VisualStudio.VSCT | 16.0.29.6 | *(Research)* | Incompatible - check for updates | High |
| Microsoft.VisualStudio.Web.BrowserLink.12.0 | 12.0.0 | *(Research)* | Incompatible - may need removal if obsolete | Medium |

### Compatible Packages (No Change Required) - 11 Packages

| Package | Current Version | Status |
|---------|----------------|--------|
| Community.VisualStudio.Toolkit.Analyzers | 1.0.507 | ✅ Compatible |
| envdte | 17.9.37000 | ✅ Compatible |
| envdte100 | 17.9.37000 | ✅ Compatible |
| envdte80 | 17.9.37000 | ✅ Compatible |
| envdte90 | 17.9.37000 | ✅ Compatible |
| envdte90a | 17.9.37000 | ✅ Compatible |
| Microsoft.TestPlatform.TestHost | 17.9.0 | ✅ Compatible |
| Microsoft.VisualStudio.CoreUtility | 17.9.187 | ✅ Compatible |
| Microsoft.VisualStudio.Extensibility | 17.9.2092 | ✅ Compatible |
| Microsoft.VisualStudio.Utilities | 17.9.37000 | ✅ Compatible |
| Microsoft.VisualStudio.Utilities.Internal | 16.3.56 | ✅ Compatible |

### Package Update Notes

#### Critical Incompatibilities

**Microsoft.VisualStudio.SDK** (17.9.37000 → 16.0.208)
- ⚠️ **Version Rollback**: Assessment suggests older version for compatibility
- **Reason**: Newer versions may have .NET Framework dependencies
- **Validation**: Verify 16.0.208 supports net10.0-windows before applying

**Microsoft.VSSDK.BuildTools** (17.9.3174 → 15.7.104)
- ⚠️ **Version Rollback**: Older build tools version recommended
- **Reason**: Build system compatibility with SDK-style + .NET 10
- **Alternative**: Research latest version supporting .NET 10 if 15.7.104 unavailable

#### Research Required

**Community.VisualStudio.Toolkit.17**
- Current status: Incompatible
- Action: Check NuGet.org for .NET 10 compatible version
- Alternative: Consider Community.VisualStudio.Toolkit (unversioned) if available

**Community.VisualStudio.VSCT**
- Current status: Incompatible
- Action: Check for updates on NuGet.org
- Impact: Required for VSCT compilation

**Microsoft.VisualStudio.Web.BrowserLink.12.0**
- Current status: Incompatible, very old version (12.0.0)
- Action: Evaluate if still needed; may be obsolete for modern VS extensions
- Recommendation: Consider removal if not actively used

### Package Update Strategy

1. **Apply Recommended Updates First**: System.Text.Encodings.Web to 10.0.3
2. **Address Critical Incompatibilities**: VS SDK and VSSDK.BuildTools version changes
3. **Research and Update**: Community toolkit packages
4. **Evaluate for Removal**: BrowserLink package if obsolete
5. **Restore and Test**: Verify all packages restore successfully

## Breaking Changes Catalog

### WPF API Compatibility (157 Issues - PRIMARY CONCERN)

**Root Cause:** Assessment flagged 157 WPF APIs as "binary incompatible" because the project currently targets `net48` (standard .NET Framework). These APIs are NOT actually breaking - they simply require Windows-specific targeting.

**Resolution:** Automatically resolved by targeting `net10.0-windows` with WPF enabled.

**Top Affected APIs:**
| API | Occurrences | Resolution |
|-----|-------------|------------|
| System.Windows.Controls.DataGridTextColumn | 30 | ✅ Auto-resolved by net10.0-windows |
| System.Windows.Style | 15 | ✅ Auto-resolved by net10.0-windows |
| System.Windows.RoutedEventArgs | 12 | ✅ Auto-resolved by net10.0-windows |
| System.Windows.Visibility | 9 | ✅ Auto-resolved by net10.0-windows |
| System.Windows.Data.Binding | 8 | ✅ Auto-resolved by net10.0-windows |
| System.Windows.Controls.DataGrid related | 20+ | ✅ Auto-resolved by net10.0-windows |

**Files Affected:**
- 2 code files contain WPF API usages
- All usages are standard WPF patterns (data binding, styling, controls)

**Action Required:** 
1. Ensure `<TargetFramework>net10.0-windows</TargetFramework>` in project file
2. Ensure `<UseWPF>true</UseWPF>` property set
3. Build - all 157 issues should resolve automatically

**Verification:** If any WPF errors persist after targeting net10.0-windows, investigate:
- Missing Windows Desktop SDK components
- Incorrect TFM (net10.0 vs net10.0-windows)
- Actual breaking changes in WPF APIs (unlikely but possible)

### Visual Studio SDK Breaking Changes

**Package Version Changes May Introduce:**
- Namespace changes (rare)
- Method signature changes
- Obsolete API removals

**Action Required:**
- Monitor compilation errors after package updates
- Consult package release notes for breaking changes
- Use IDE quick fixes where available

**Common VS SDK Patterns to Watch:**
- IVs* interface implementations
- Service provider usage
- Command handling patterns
- Tool window initialization

### .NET Framework to .NET 10 Breaking Changes

**General Categories:**
1. **Binary Serialization**: BinaryFormatter removed (not detected in codebase)
2. **Windows-Only APIs**: Require Windows targeting (handled via net10.0-windows)
3. **Configuration**: app.config patterns may need migration (VSIX uses different config)
4. **Reflection**: Some reflection patterns restricted
5. **Threading**: Thread.Abort removed (not typically used in VSIX)

**Likely Impact:** Low - VSIX extensions typically use supported APIs

**Validation:** Build will identify any actual breaking changes

### VSIX Manifest and Build System

**Potential Changes:**
- InstallationTarget version updates for VS 2022
- Prerequisites updates
- Build output path changes with SDK-style projects

**Action Required:**
- Review .vsixmanifest after conversion
- Update InstallationTarget if needed: `<InstallationTarget Id="Microsoft.VisualStudio.Pro" Version="[17.0,18.0)">`
- Verify VSIX packaging still works correctly

### Expected vs. Actual Breaking Changes

**Expected (Flagged by Assessment):**
- 157 WPF API issues - **Resolved automatically by Windows targeting**
- 6 package incompatibilities - **Addressed by version updates**

**Actual (To Be Discovered During Build):**
- Package-specific API changes - **Addressed incrementally**
- VS SDK pattern updates - **Fixed as identified**
- VSIX tooling adjustments - **Resolved through testing**

### Breaking Changes by Category

| Category | Count | Auto-Resolved | Requires Code Changes | Severity |
|----------|-------|---------------|----------------------|----------|
| WPF APIs | 157 | ✅ Yes (targeting) | ❌ No | 🟢 Low |
| Package Updates | 6 | ⚠️ Partial | ⚠️ Possible | 🟡 Medium |
| VS SDK APIs | Unknown | ❌ No | ⚠️ Possible | 🟡 Medium |
| VSIX Manifest | 1 | ❌ No | ⚠️ Likely | 🟢 Low |

**Overall Breaking Changes Severity:** 🟡 Medium - Most issues auto-resolve; remaining require standard migration patterns.

## Project-by-Project Migration Plans

### Project: SetGlobalVersion.csproj

#### Current State
- **Target Framework**: .NET Framework 4.8 (`net48`)
- **Project Type**: Classic WPF VSIX Extension (non-SDK-style)
- **Dependencies**: 
  - 17 NuGet packages (6 requiring updates)
  - 0 internal project dependencies
- **Codebase**: 
  - 11 code files
  - 2,600 lines of code
  - 2 files with WPF API usages
- **Complexity**: Medium
- **Risk Level**: Medium

#### Target State
- **Target Framework**: .NET 10.0 Windows (`net10.0-windows`)
- **Project Type**: SDK-style WPF VSIX Extension
- **Dependencies**: Updated package references
- **Expected LOC Changes**: ~157 lines (6% of codebase)

#### Migration Steps

##### 1. Prerequisites
- ✅ .NET 10.0 SDK installed
- ✅ Visual Studio 2022 (17.10 or higher)
- ✅ Visual Studio Extension Development workload installed
- ✅ Git branch `upgrade-to-NET10` created

##### 2. Project File Conversion

**Convert to SDK-Style:**
- Use automated conversion tool: `upgrade_convert_project_to_sdk_style`
- **Critical Elements to Preserve:**
  - VSIX-specific properties (`GeneratePkgDefFile`, `IncludeAssemblyInVSIXContainer`)
  - Custom MSBuild imports
  - Content file inclusions for VSIX resources

**Original Project File Structure (Classic):**
```xml
<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
  <Import Project="..." />
  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
    <!-- VSIX properties -->
  </PropertyGroup>
  <ItemGroup>
    <!-- Package references -->
  </ItemGroup>
  <ItemGroup>
    <!-- Compile includes -->
  </ItemGroup>
</Project>
```

**Target SDK-Style Structure:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <GeneratePkgDefFile>true</GeneratePkgDefFile>
    <IncludeAssemblyInVSIXContainer>true</IncludeAssemblyInVSIXContainer>
    <!-- Additional VSIX properties -->
  </PropertyGroup>
  <ItemGroup>
    <!-- Package references (updated versions) -->
  </ItemGroup>
</Project>
```

##### 3. Update Target Framework

**Change:**
```xml
<!-- Before -->
<TargetFramework>net48</TargetFramework>

<!-- After -->
<TargetFramework>net10.0-windows</TargetFramework>
<UseWPF>true</UseWPF>
<UseWindowsDesktop>true</UseWindowsDesktop>
```

⚠️ **Critical**: The `-windows` TFM suffix is mandatory. Without it, WPF APIs will not compile.

##### 4. Update NuGet Packages

**Packages Requiring Updates:**

| Package | Current | Target | Reason |
|---------|---------|--------|---------|
| System.Text.Encodings.Web | 8.0.0 | 10.0.3 | .NET 10 compatibility |
| Microsoft.VisualStudio.SDK | 17.9.37000 | 16.0.208 | Incompatibility fix |
| Microsoft.VSSDK.BuildTools | 17.9.3174 | 15.7.104 | Build tools compatibility |
| Community.VisualStudio.Toolkit.17 | 17.0.507 | *(TBD)* | Research .NET 10 version |
| Community.VisualStudio.VSCT | 16.0.29.6 | *(TBD)* | Research updates |
| Microsoft.VisualStudio.Web.BrowserLink.12.0 | 12.0.0 | *(TBD)* | Evaluate removal |

**Packages Remaining Unchanged:**
- All 11 compatible packages retain current versions

##### 5. Expected Breaking Changes

**WPF APIs (157 occurrences):**
- **Type**: Binary incompatibility (assessment flagged)
- **Resolution**: Automatic via `net10.0-windows` targeting
- **Files Affected**: 2 code files
- **APIs Involved**: DataGridTextColumn, Style, RoutedEventArgs, Visibility, Binding, etc.
- **Code Changes Required**: None (targeting resolves)

**Visual Studio SDK APIs:**
- **Type**: Potential method signature or namespace changes
- **Resolution**: Update call sites as compiler identifies
- **Code Changes Required**: Varies based on package updates

**VSIX Manifest:**
- **Type**: Installation target version updates
- **Resolution**: Update .vsixmanifest InstallationTarget for VS 2022
- **Example Change**: `<InstallationTarget Id="Microsoft.VisualStudio.Pro" Version="[17.0,18.0)">`

##### 6. Build and Fix Compilation Errors

**Build Command:**
```bash
dotnet build SetGlobalVersion\SetGlobalVersion.csproj
```

**Expected Error Categories:**
1. **WPF Resolution Errors**: Should not occur if targeting correct (net10.0-windows)
2. **Package API Changes**: Address based on compiler messages
3. **Obsolete API Usage**: Replace with recommended alternatives
4. **Namespace Changes**: Update using directives if packages restructured

**Iterative Fix Process:**
1. Build to identify all errors
2. Group errors by type/category
3. Fix highest-frequency errors first
4. Rebuild to verify fixes and identify remaining issues
5. Repeat until 0 errors

##### 7. VSIX Manifest Updates

**Review and Update:**
- Open `source.extension.vsixmanifest`
- Update InstallationTarget for supported Visual Studio baseline and prepare range for VS 2026 when officially supported:
  ```xml
  <Installation>
    <InstallationTarget Id="Microsoft.VisualStudio.Pro" Version="[17.0,18.0)" />
  </Installation>
  ```
- Update Prerequisites if needed
- Verify DisplayName, Description, and other metadata

##### 8. Testing Strategy

**Build Validation:**
- ✅ Solution builds with 0 errors
- ✅ Solution builds with 0 warnings
- ✅ VSIX file generated successfully
- ✅ Package dependencies resolved correctly

**Functionality Validation:**
- Deploy to experimental Visual Studio instance
- Test core extension functionality:
  - Extension loads without errors
  - Menu commands appear correctly
  - Tool windows display properly
  - Core features function as expected
- Verify no runtime exceptions in Output window

##### 9. Validation Checklist

**Technical Validation:**
- [ ] Project file is valid SDK-style format
- [ ] TargetFramework set to net10.0-windows
- [ ] UseWPF property set to true
- [ ] All 17 packages restored successfully
- [ ] 6 package updates applied correctly
- [ ] Solution builds without errors
- [ ] Solution builds without warnings
- [ ] VSIX file generated in output directory

**Functional Validation:**
- [ ] VSIX installs in experimental instance
- [ ] Extension appears in Extensions Manager
- [ ] Extension loads without errors
- [ ] All menu commands functional
- [ ] Tool windows display correctly
- [ ] Core features operate as expected
- [ ] No console errors or exceptions

**Quality Validation:**
- [ ] Code quality maintained
- [ ] No security vulnerabilities introduced
- [ ] Performance acceptable
- [ ] All WPF UI renders correctly

#### Estimated Impact

- **Complexity**: Medium
- **Code Changes**: ~157 lines (6% of codebase) - mostly auto-resolved
- **Risk**: Medium (VSIX-specific concerns)
- **Dependencies**: 0 projects (standalone)

#### Success Criteria

✅ **Complete When:**
1. Project converted to SDK-style
2. Target framework updated to net10.0-windows
3. All package dependencies updated and restored
4. Solution builds with 0 errors and 0 warnings
5. VSIX packages successfully
6. Extension installs and functions in Visual Studio experimental instance for current baseline, with VS 2026 validation completed or explicitly tracked as pending
7. All core features validated manually

## Risk Management

### High-Risk Changes

| Project/Area | Risk Level | Description | Mitigation |
|-------------|-----------|-------------|------------|
| SDK-Style Conversion | 🟡 Medium | Converting from classic to SDK-style may lose custom MSBuild logic | Use conversion tool; review generated project file; preserve custom targets |
| VSIX Manifest | 🟡 Medium | Manifest may need updates for baseline VS channel and VS 2026 compatibility | Review InstallationTarget and Prerequisites; test in experimental instance and VS 2026 channel when available |
| WPF API Compatibility | 🟢 Low | 157 WPF APIs marked incompatible | Automatically resolved by targeting net10.0-windows; WPF is fully supported |
| VS SDK Packages | 🟡 Medium | 5 packages marked incompatible need version adjustments | Assessment suggests specific compatible versions; test thoroughly |

### Security Vulnerabilities

**None identified** - No security vulnerabilities detected in current package versions.

### Contingency Plans

#### If SDK Conversion Fails
- **Alternative**: Manually create new SDK-style project and migrate files incrementally
- **Rollback**: Revert to original .csproj from git history

#### If Package Incompatibilities Persist
- **Alternative**: Use `dotnet list package --vulnerable` to find compatible versions
- **Workaround**: Temporarily target net9.0-windows if net10.0 packages unavailable
- **Research**: Check package GitHub repos for .NET 10 compatibility status

#### If VSIX Packaging Fails
- **Check**: Ensure Microsoft.VSSDK.BuildTools is compatible version
- **Alternative**: Use legacy VSIX project template and migrate code
- **Validation**: Test with VS 2022 SDK samples for reference

#### If WPF APIs Still Incompatible
- **Verify**: Confirm `<TargetFramework>net10.0-windows</TargetFramework>` (not just net10.0)
- **Check**: Ensure `<UseWPF>true</UseWPF>` property present in project file
- **Research**: Review breaking changes documentation for specific API changes

### Risk Summary

**Overall Risk Level:** 🟡 Medium

**Key Risk Factors:**
- Classic to SDK-style conversion (automated but requires validation)
- VSIX tooling compatibility with .NET 10
- Visual Studio SDK package ecosystem maturity for .NET 10

**Mitigation Success Factors:**
- Assessment provides clear package version guidance
- WPF compatibility is platform-supported (not custom code)
- Single project scope limits blast radius
- Git branching enables safe experimentation

## Testing & Validation Strategy

### Global Versioning Test Matrix (Solution-Level)

| Scenario | Expected Outcome |
|---|---|
| `.sln` with SDK-style class libraries | Version fields updated in all targeted projects |
| `.sln` with MAUI projects | MAUI project version fields updated according to global policy |
| `.sln` with Avalonia projects | Avalonia project version fields updated according to global policy |
| Mixed `.sln` (WPF + MAUI + Avalonia) | Unified version applied consistently; unsupported cases reported |
| Project with custom/nonstandard version properties | Skipped with explicit reason or updated by configured mapping |
| Read-only/locked project file | Operation reports failure with actionable message |


### Multi-Level Testing Approach

#### Level 1: Build Validation (Automated)

**Executed After:** Atomic upgrade completion

**Tests:**
```bash
# Restore dependencies
dotnet restore SetGlobalVersion.sln

# Build solution
dotnet build SetGlobalVersion.sln --configuration Release

# Verify VSIX generation
# Check: SetGlobalVersion\bin\Release\SetGlobalVersion.vsix exists
```

**Success Criteria:**
- ✅ Restore completes with 0 errors
- ✅ Build completes with 0 errors
- ✅ Build completes with 0 warnings
- ✅ VSIX file generated successfully
- ✅ Output confirms successful VSIX packaging

#### Level 2: Package Validation (Automated)

**Executed After:** Build validation passes

**Tests:**
```bash
# List package vulnerabilities
dotnet list SetGlobalVersion\SetGlobalVersion.csproj package --vulnerable

# List package compatibility
dotnet list SetGlobalVersion\SetGlobalVersion.csproj package --deprecated
```

**Success Criteria:**
- ✅ No vulnerable packages detected
- ✅ No deprecated packages in use
- ✅ All packages compatible with net10.0-windows

#### Level 3: VSIX Deployment (Manual)

**Executed After:** Package validation passes

**Deployment Steps:**
1. Close all Visual Studio instances
2. Uninstall previous version from experimental instance (if exists)
3. Install VSIX:
   ```bash
   VSIXInstaller.exe /skuName:Pro /skuVersion:17.0 SetGlobalVersion\bin\Release\SetGlobalVersion.vsix
   ```
4. Launch experimental instance:
   ```bash
   devenv.exe /RootSuffix Exp
   ```

**Success Criteria:**
- ✅ VSIX installs without errors
- ✅ Extension appears in Extensions and Updates
- ✅ Extension shows correct version and metadata
- ✅ Visual Studio launches successfully

#### Level 4: Functionality Validation (Manual)

**Executed After:** VSIX deployment succeeds

**Test Scenarios:**

| Test Case | Steps | Expected Result | Status |
|-----------|-------|----------------|--------|
| Extension Loads | Launch VS experimental instance | Extension loads without errors in Output window | [ ] |
| Menu Commands Visible | Navigate to extension menu | All commands visible and enabled | [ ] |
| Command Execution | Execute primary command | Command executes without exceptions | [ ] |
| Tool Window Display | Open extension tool window (if applicable) | Window displays correctly with proper layout | [ ] |
| WPF UI Rendering | Interact with WPF controls | All controls render and respond correctly | [ ] |
| Settings Persistence | Modify extension settings, restart VS | Settings persist across sessions | [ ] |
| Error Handling | Trigger error scenarios | Errors handled gracefully, logged appropriately | [ ] |

**Success Criteria:**
- ✅ All test cases pass
- ✅ No unhandled exceptions in Output window
- ✅ WPF UI renders correctly (DataGrids, styles, bindings)
- ✅ Extension functionality equivalent to .NET Framework version

#### Level 5: Integration Testing (Manual)

**Executed After:** Functionality validation passes

**Integration Scenarios:**
- Test with solution open (small project)
- Test with solution open (large project)
- Test with multiple solution types (if applicable)
- Test interaction with other VS extensions
- Test with different VS themes (light/dark)

**Success Criteria:**
- ✅ Extension works with various solution types
- ✅ No conflicts with other extensions
- ✅ Performance acceptable across scenarios

#### Level 6: Regression Testing (Manual)

**Executed After:** Integration testing passes

**Regression Focus:**
- All features that worked in .NET Framework 4.8 version
- Data binding scenarios (DataGrid, etc.)
- Styling and visual appearance
- Command invocation patterns
- Settings and configuration

**Success Criteria:**
- ✅ No regressions detected
- ✅ Feature parity with previous version
- ✅ Visual appearance consistent

### Testing Checklist

**Pre-Upgrade Baseline:**
- [ ] Document current extension functionality
- [ ] List all commands and features
- [ ] Capture screenshots of UI elements
- [ ] Note any known issues in .NET Framework version

**Post-Upgrade Validation:**
- [ ] Build validation passes
- [ ] Package validation passes
- [ ] VSIX deployment successful
- [ ] All functionality test cases pass
- [ ] Integration scenarios validated
- [ ] No regressions detected
- [ ] Performance acceptable
- [ ] WPF UI rendering correct

**Sign-Off Criteria:**
- [ ] All test levels completed successfully
- [ ] No critical issues discovered
- [ ] Performance meets or exceeds baseline
- [ ] Ready for production deployment

### Known Limitations

**Manual Testing Required:**
- VSIX functionality cannot be fully automated
- Visual validation requires human review
- User interaction scenarios need manual execution

**Recommended Test Environment:**
- Visual Studio 2022 (latest stable version)
- Windows 10/11 with latest updates
- Clean experimental instance (reset if needed)

### Validation Sequence (Relative Complexity)

| Validation Stage | Relative Complexity | Notes |
|-------|-------------------|-------|
| Build Validation | Low | Automated |
| Package Validation | Low | Automated |
| VSIX Deployment | Medium | Manual install and launch checks |
| Functionality Validation | Medium | Manual behavioral verification |
| Integration Testing | Medium | Cross-scenario checks |
| Regression Testing | Medium | Compare with pre-upgrade behavior |
| MAUI Host Validation | Medium-High | Verify shared-core integration |
| Avalonia Host Validation | Medium-High | Verify shared-core integration |

## Complexity & Effort Assessment

### Per-Project Complexity

| Project | Complexity | Dependencies | Risk | Key Factors |
|---------|-----------|--------------|------|-------------|
| SetGlobalVersion.csproj | 🟡 Medium | 17 packages, 0 projects | 🟡 Medium | Classic format conversion, WPF APIs, VSIX tooling |

### Phase Complexity Assessment

**Phase 1: Atomic Upgrade**
- **Complexity**: Medium
- **Key Challenges**: 
  - SDK-style conversion automation reliability
  - Package version compatibility verification
  - VSIX build tools migration
- **Dependency Ordering**: N/A (single project)

**Phase 2: VSIX Validation**
- **Complexity**: Low-Medium
- **Key Challenges**:
  - Extension manifest updates
  - Experimental instance deployment
  - Manual functionality validation

### Resource Requirements

**Skills Required:**
- **Essential**: 
  - .NET project file formats (classic vs SDK-style)
  - NuGet package management
  - Visual Studio Extension development
  - WPF fundamentals
- **Helpful**:
  - MSBuild and project file customization
  - Visual Studio SDK APIs
  - Git branching and rollback strategies

**Parallel Capacity:**
- Single project = no parallelization opportunities
- Atomic operation requires sequential execution

### Relative Effort Summary

| Phase | Relative Effort | Notes |
|-------|----------------|-------|
| Phase 0: Preparation | Low | Verify tooling installation |
| Phase 1: Atomic Upgrade | Medium-High | Core migration work, most time intensive |
| Phase 2: VSIX Validation | Medium | Manual testing and validation |

**Overall Effort:** Medium - Single project with well-documented upgrade path, but VSIX-specific concerns add complexity.

## Source Control Strategy

### Branching Strategy

**Main Branch:** `main`
- Stable .NET Framework 4.8 version
- No changes during upgrade process
- Merge target after upgrade completion and validation

**Upgrade Branch:** `upgrade-to-NET10` (current)
- Created from `main` branch
- Contains all upgrade changes
- Isolated environment for migration work
- Allows rollback if needed

**Workflow:**
```
main (net48)
  └─> upgrade-to-NET10 (net10.0-windows) [current]
        └─> [after validation] → merge back to main
```

### Commit Strategy

**Recommended: Single Atomic Commit**

Given the All-At-Once strategy and single-project scope, all upgrade changes can be committed together:

```bash
# After upgrade completion and successful validation
git add .
git commit -m "Upgrade SetGlobalVersion to .NET 10.0

- Convert project to SDK-style format
- Update target framework to net10.0-windows
- Update 6 NuGet packages for .NET 10 compatibility
- Enable WPF support via Windows Desktop SDK
- Update VSIX manifest for VS 2022 compatibility
- All 157 WPF API issues resolved via Windows targeting
- Build: 0 errors, 0 warnings
- VSIX packaging validated
- Functionality testing completed successfully"
```

**Alternative: Multi-Commit Approach**

If preferring incremental commits for easier review or rollback:

1. **Commit 1: Project Conversion**
   ```
   Convert SetGlobalVersion to SDK-style project format
   ```

2. **Commit 2: Framework Update**
   ```
   Update target framework to net10.0-windows with WPF support
   ```

3. **Commit 3: Package Updates**
   ```
   Update NuGet packages for .NET 10 compatibility
   - System.Text.Encodings.Web: 8.0.0 → 10.0.3
   - Microsoft.VisualStudio.SDK: 17.9.37000 → 16.0.208
   - Microsoft.VSSDK.BuildTools: 17.9.3174 → 15.7.104
   - [additional packages]
   ```

4. **Commit 4: Code Fixes** (if applicable)
   ```
   Address breaking changes from framework/package updates
   ```

5. **Commit 5: VSIX Manifest**
   ```
   Update VSIX manifest for Visual Studio 2022 compatibility
   ```

**Recommendation:** Use single atomic commit unless team prefers detailed history.

### Commit Message Format

**Template:**
```
<type>: <subject>

<body>

<footer>
```

**Example:**
```
chore: Upgrade to .NET 10.0

Convert SetGlobalVersion VSIX extension from .NET Framework 4.8 to .NET 10.0:
- SDK-style project conversion
- Target framework: net10.0-windows
- Package updates: 6 packages
- WPF compatibility resolved
- VSIX manifest updated for VS 2022

Tests:
- Build: SUCCESS (0 errors, 0 warnings)
- VSIX packaging: SUCCESS
- Functionality validation: PASSED
- Regression testing: PASSED

Closes #[issue-number] (if applicable)
```

### Review and Merge Process

#### Pull Request Requirements

**Title:** `Upgrade SetGlobalVersion to .NET 10.0`

**Description Template:**
```markdown
## Overview
Upgrades SetGlobalVersion VSIX extension from .NET Framework 4.8 to .NET 10.0 (LTS).

## Changes
- ✅ Project converted to SDK-style format
- ✅ Target framework updated to net10.0-windows
- ✅ WPF support enabled via Windows Desktop SDK
- ✅ 6 NuGet packages updated for compatibility
- ✅ VSIX manifest updated for Visual Studio 2022

## Testing
- ✅ Build validation: 0 errors, 0 warnings
- ✅ Package validation: No vulnerabilities
- ✅ VSIX deployment: Successful
- ✅ Functionality testing: All tests passed
- ✅ Regression testing: No issues detected

## Breaking Changes
- None for end users
- Requires .NET 10.0 SDK for development
- Requires Visual Studio 2022 (17.10+) for development

## Migration Notes
All 157 WPF API compatibility issues automatically resolved by Windows targeting.
No code changes required beyond project file and package updates.

## Checklist
- [ ] Code reviewed
- [ ] Build successful
- [ ] Tests passed
- [ ] Documentation updated (if applicable)
- [ ] Ready to merge
```

#### Review Checklist

**Reviewer Focus Areas:**
- [ ] Project file structure correct (SDK-style)
- [ ] Target framework set to net10.0-windows (not just net10.0)
- [ ] UseWPF property present
- [ ] All package versions appropriate
- [ ] VSIX-specific properties preserved
- [ ] Build succeeds without errors/warnings
- [ ] VSIX manifest updated correctly
- [ ] No unintended file changes

#### Merge Criteria

**Required for Merge:**
- ✅ All build validations pass
- ✅ All functionality tests pass
- ✅ No regressions detected
- ✅ Code review approved
- ✅ CI/CD pipeline passes (if configured)
- ✅ Documentation updated (if needed)

**Merge Command:**
```bash
# After PR approval
git checkout main
git merge --no-ff upgrade-to-NET10
git push origin main
```

**Post-Merge:**
- Tag release version:
  ```bash
  git tag -a v2.0.0-net10 -m "Version 2.0.0 - .NET 10.0 upgrade"
  git push origin v2.0.0-net10
  ```
- Update release notes
- Build and publish VSIX to production

### Rollback Strategy

**If Issues Discovered:**

1. **Before Merge:** Simply stay on `main` branch, investigate issues on `upgrade-to-NET10`
2. **After Merge:** Revert merge commit:
   ```bash
   git revert -m 1 <merge-commit-hash>
   git push origin main
   ```
3. **Complete Rollback:** Reset to previous state:
   ```bash
   git checkout main
   git reset --hard <commit-before-merge>
   git push origin main --force
   ```

**Rollback Decision Criteria:**
- Critical functionality broken
- Build failures in production environment
- VSIX installation failures
- Data loss or corruption risk
- Security vulnerabilities introduced

### Branch Cleanup

**After Successful Merge and Validation:**
```bash
# Delete upgrade branch locally
git branch -d upgrade-to-NET10

# Delete upgrade branch remotely
git push origin --delete upgrade-to-NET10
```

**Keep Branch If:**
- May need to reference upgrade process
- Planning additional .NET 10 specific features
- Team policy requires branch retention

## Success Criteria

### Technical Criteria
- VSIX host targets `net10.0-windows` and is buildable
- Extension can set version values globally for loaded `.sln`
- Version propagation includes MAUI and Avalonia project types when present in solution
- Global operation reports updated/skipped/failed projects with clear reasons
- All package updates from assessment are applied or explicitly replaced with documented alternatives
- Solution/components build successfully with no blocking errors
- No unresolved package dependency conflicts
- No known security vulnerabilities remain open in upgraded package graph

### Architecture Criteria
- Version-update logic is centralized and reusable inside the extension
- Project-type detection supports SDK-style, MAUI, and Avalonia projects in `.sln`
- Visual Studio-specific APIs are isolated from version calculation/update logic
- Update policy for version fields is documented and consistently applied

### Visual Studio 2026 Readiness Criteria
- VSIX manifest includes version targeting strategy for current VS channel and VS 2026 validation path
- Extension validated in latest supported Visual Studio baseline and VS 2026 preview/build channels when available
- Extension compatibility with VS 2026 is marked as:
  - ✅ Validated (if SDK/build is available), or
  - ⚠️ Pending validation (if VS 2026 SDK/build not yet publicly available)
- Any VS 2026 SDK/API deltas are tracked in a dedicated compatibility checklist

### Quality Criteria
- Existing extension behavior remains functionally equivalent after migration
- Global version-setting behavior is deterministic and idempotent
- Regression checklist includes mixed-solution scenarios (WPF/MAUI/Avalonia)
- Upgrade documentation updated to reflect `.sln`-wide version propagation and supported project-type matrix

### Process Criteria
- All-at-once strategy applied for the VSIX host upgrade operation
- Source control strategy followed with atomic or clearly documented commit sequence
- Plan sections remain consistent with assessment data and requested adaptation scope

## Maui and Avalonia Windows-Only Handling

### Scope Definition
- The extension must handle solutions containing MAUI and Avalonia projects.
- Handling means **detecting and updating project version metadata** as part of a global `.sln` operation.
- This scope is **not** a cross-platform runtime rollout; the VSIX remains Windows/Visual Studio hosted.

### Project-Type Detection Guidance
- Detect MAUI projects by SDK and project properties commonly used for MAUI.
- Detect Avalonia projects by package/SDK markers and project metadata.
- For unknown project types, apply safe fallback rules (skip with explicit reason).

### Version Field Update Policy
- Define a single global policy for version fields (for example: `Version`, `VersionPrefix`, `AssemblyVersion`, `FileVersion`, `InformationalVersion`).
- Apply policy consistently across SDK-style MAUI, Avalonia, and other supported projects.
- Preserve unsupported/custom entries and report exceptions.

### Validation
- In a mixed solution (WPF + MAUI + Avalonia), one command updates all supported projects.
- Operation reports per-project outcome (`Updated`, `Skipped`, `Failed`) with reasons.
- Re-running with same target version is idempotent (no unintended diffs).

## Visual Studio 2026 Extension Targeting

### Targeting Strategy
- Keep extension compatible with supported Visual Studio baseline while preparing VS 2026 compatibility.
- Maintain manifest/version ranges to include VS 2026 when officially supported SDK guidance is available.

### Manifest and SDK Checklist
- Validate `source.extension.vsixmanifest` installation target ranges for VS 2026.
- Verify Visual Studio SDK package compatibility for VS 2026 channels.
- Record required API or manifest adjustments in compatibility notes.

### Build/Validation Outcomes
- Current baseline Visual Studio channel: must pass packaging and runtime validation.
- VS 2026 channel: validated where possible, otherwise marked explicitly as pending with tracked blockers.
