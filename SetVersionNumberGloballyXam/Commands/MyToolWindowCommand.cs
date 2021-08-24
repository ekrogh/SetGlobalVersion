using SolutionEvents = Microsoft.VisualStudio.Shell.Events.SolutionEvents;


namespace SetVersionNumberGloballyXam
{
	[Command(PackageIds.MyCommand)]
	internal sealed class MyToolWindowCommand : BaseCommand<MyToolWindowCommand>
	{
		protected override void BeforeQueryStatus(EventArgs e)
		{
			_ = SetVisibilityDependIfXamForProjAsync();
			base.BeforeQueryStatus(e);
		}
		protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			SolutionEvents.OnAfterCloseSolution += SolutionEvents_OnAfterCloseSolution;
			SolutionEvents.OnAfterBackgroundSolutionLoadComplete += SolutionEvents_OnAfterBackgroundSolutionLoadComplete;

			return MyToolWindow.ShowAsync();
		}

		private void SolutionEvents_OnAfterCloseSolution(object sender, EventArgs e)
		{
			Command.Visible = false;
		}

		private void SolutionEvents_OnAfterBackgroundSolutionLoadComplete(object sender, EventArgs e)
		{
			_ = SetVisibilityDependIfXamForProjAsync();
		}

		private async Task SetVisibilityDependIfXamForProjAsync()
		{
			if (await CheckSolutionType.ThisIsXamarinAsync().ConfigureAwait(true))
			{
				Command.Visible = true;
			}
			else
			{
				Command.Visible = false;
			}
		}
	}
}
