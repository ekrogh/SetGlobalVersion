using Microsoft.VisualStudio.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace SetGlobalVersion
{
	public class MyToolWindow : BaseToolWindow<MyToolWindow>
	{
		public override string GetTitle(int toolWindowId) => "My Tool Window";

		public override Type PaneType => typeof(Pane);

		public override Task<FrameworkElement> CreateAsync(int toolWindowId, CancellationToken cancellationToken)
		{
			return Task.FromResult<FrameworkElement>(new MyToolWindowControl());
		}

		[Guid("a6806b75-2075-474c-875f-7fbc9c95bf31")]
		internal class Pane : ToolkitToolWindowPane
		{
			public Pane()
			{
				BitmapImageMoniker = KnownMonikers.ToolWindow;
			}
		}
	}
}