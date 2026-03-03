global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;
using static Microsoft.VisualStudio.VSConstants;

namespace SetGlobalVersion
{
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
	[ProvideToolWindow(typeof(MyToolWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(PackageGuids.SetGlobalVersionString)]
	[ProvideUIContextRule(
		PackageGuids.uiContextSupportedFlavorsString,
		name: "Auto show",
		expression: "single | multiple",
		termNames: new[] { "single", "multiple" },
		termValues: new[] { UICONTEXT.SolutionHasSingleProject_string, UICONTEXT.SolutionHasMultipleProjects_string }
	)]
	public sealed class SetGlobalVersionPackage : ToolkitPackage
	{
		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

			await this.RegisterCommandsAsync();

			this.RegisterToolWindows();
		}
	}
}