using Microsoft.VisualStudio.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SetGlobalVersion
{
	public class MyToolWindow : BaseToolWindow<MyToolWindow>
	{
		public override string GetTitle(int toolWindowId) => "Set Global Version Number";

		public override Type PaneType => typeof(Pane);

		public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
		{
			return Task.FromResult<FrameworkElement>(new MyToolWindowControl());
		}

		[Guid("d047abcc-9a6a-4507-81a1-9a6d4608410d")]
		internal class Pane : ToolWindowPane
		{
			public Pane()
			{
				BitmapImageMoniker = KnownMonikers.ToolWindow;
			}
		}
	}
}