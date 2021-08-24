global using Community.VisualStudio.Toolkit;
global using Microsoft.VisualStudio.Shell;
global using System;
global using Task = System.Threading.Tasks.Task;
using System.Runtime.InteropServices;
using System.Threading;
using static Microsoft.VisualStudio.VSConstants;

namespace SetVersionNumberGloballyXam
{
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
	[ProvideToolWindow(typeof(MyToolWindow.Pane), Style = VsDockStyle.Tabbed, Window = WindowGuids.SolutionExplorer)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[Guid(PackageGuids.SetVersionNumberGloballyXamString)]
	[ProvideAutoLoad
		(
			UICONTEXT.SolutionHasSingleProject_string
			,
			flags: PackageAutoLoadFlags.BackgroundLoad
		)
	]
	[ProvideAutoLoad
		(
			UICONTEXT.SolutionHasMultipleProjects_string
			,
			flags: PackageAutoLoadFlags.BackgroundLoad
		)
	]
	//[ProvideUIContextRule
	//	(
	//		PackageGuids.uiContextSupportedFlavorsString,
	//		name: "Auto show",
	//		expression: "(single | multiple) & (UWP | XamarinAndroid | XamariniOS | XamarinmacOS)",
	//		termNames: new[]
	//			{
	//				"single"
	//				,
	//				"multiple"
	//				,
	//				"UWP"
	//				,
	//				"XamarinAndroid"
	//				,
	//				"XamariniOS"
	//				,
	//				"XamarinmacOS"
	//			},
	//		termValues: new[]
	//			{
	//				UICONTEXT.SolutionHasSingleProject_string
	//				,
	//				UICONTEXT.SolutionHasMultipleProjects_string
	//				,
	//				"SolutionHasProjectFlavor:A5A43C5B-DE2A-4C0C-9213-0A381AF9435A"
	//				,
	//				"SolutionHasProjectFlavor:EFBA0AD7-5A72-4C68-AF49-83D382785DCF"
	//				,
	//				"SolutionHasProjectFlavor:6BC8ED88-2882-458C-8E55-DFD12B67127B"
	//				,
	//				"SolutionHasProjectFlavor:A3F8F2AB-B479-4A4A-A458-A89E7DC349F1"
	//			}
	//	)
	//]
	public sealed class SetVersionNumberGloballyXamPackage : ToolkitPackage
	{
		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await this.RegisterCommandsAsync();

			this.RegisterToolWindows();
		}
	}
}