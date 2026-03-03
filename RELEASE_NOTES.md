# Release Notes

## 6.0

### Added
- Restored and aligned extension command registration for reliable solution context-menu visibility.
- Restored full tool window implementation and helper support files.

### Changed
- Command/menu registration now uses the standard `Menus.ctmenu` flow.
- Project version-file detection expanded to include MAUI and Avalonia-style project properties.
- `.csproj` XML node selection made namespace-agnostic for broader compatibility.

### Fixed
- `Set Global Version` command visibility on solution right-click menu.
- Tool window fallback/template UI issue.
- Avalonia project discovery in mixed MAUI + Avalonia solutions.
- Avalonia project version updates for:
  - `Version`
  - `AssemblyVersion`
  - `FileVersion`
  - `InformationalVersion`
- Design-time `VsctGenerator` warning by removing obsolete custom-tool metadata from project settings.

### Non-functional cleanup
- Trimmed non-essential VSCT boilerplate.
- Kept build/runtime behavior unchanged.

