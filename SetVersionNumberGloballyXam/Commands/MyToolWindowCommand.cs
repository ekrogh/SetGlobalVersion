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
			VS.Events.SolutionEvents.OnAfterCloseSolution += SolutionEvents_OnAfterCloseSolution;
			VS.Events.SolutionEvents.OnAfterBackgroundSolutionLoadComplete += SolutionEvents_OnAfterBackgroundSolutionLoadComplete;

			return MyToolWindow.ShowAsync();
		}

		private void SolutionEvents_OnAfterBackgroundSolutionLoadComplete()
		{
			_ = SetVisibilityDependIfXamForProjAsync();
		}

		private void SolutionEvents_OnAfterCloseSolution()
		{
			Command.Visible = false;
			CheckSolutionItems.XamarinFormsProjectsList.Clear();
		}

		private async Task SetVisibilityDependIfXamForProjAsync()
		{
			if (await CheckSolutionItems.ThisIsXamarinAsync().ConfigureAwait(true))
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
