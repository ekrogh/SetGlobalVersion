using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SetVersionNumberGloballyXam
{
	public class CheckSolutionItems
	{
		public static List<Project> XamarinFormsProjectsList = new List<Project>();
		public static Solution TheSolution { get; set; } = null;

		public static async Task<bool> ThisIsXamarinAsync()
		{
			if (XamarinFormsProjectsList.Count != 0)
			{
				return true;
			}
			else
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

				try
				{
					TheSolution = await VS.Solutions.GetCurrentSolutionAsync().ConfigureAwait(true);

					if (TheSolution != null)
					{
						IEnumerable<SolutionItem> Projs = TheSolution.Children.Where(x => x.Type == SolutionItemType.Project);

						if (Projs.Any())
						{
							foreach (Project proj in Projs)
							{
								if (ThisIsXamarinFormsProject(proj.Children))
								{
									XamarinFormsProjectsList.Add(proj);
								}
							}
						}
					}
				}
				catch (Exception e)
				{
					_ = VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
				}

				if (XamarinFormsProjectsList.Count != 0)
				{
					return true;
				}
				else
				{
					return false;
				}

			}
		}

		public static bool ThisIsXamarinFormsProject(in IEnumerable<SolutionItem> chldrn)
		{
			try
			{
				foreach (VirtualFolder vFldr in chldrn.Where(x => x.Type == SolutionItemType.VirtualFolder))
				{
					foreach (SolutionItem item in vFldr.Children)
					{
						if (item.Name.ToLower().Contains("xamarin.forms"))
						{
							return true;
						}
					}
				}
				// Not found ..Try children
				foreach (SolutionItem chld in chldrn)
				{
					if (ThisIsXamarinFormsProject(chld.Children))
					{
						return true;
					}
				}
			}
			catch (Exception e)
			{
				_ = VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			}
			// Not found
			return false;
		}

		public static bool SearchFileInProject(in IEnumerable<SolutionItem> chldrn, in string stringToSearchFor, out string pathAndFile)
		{
			try
			{
				foreach (PhysicalFile aFile in chldrn.Where(x => x.Type == SolutionItemType.PhysicalFile))
				{
					if (aFile.Name.ToLower().Contains(stringToSearchFor))
					{
						pathAndFile = aFile.FullPath;
						return true;
					}
				}
				// Not found ..Try children
				foreach (SolutionItem chld in chldrn)
				{
					if (CheckSolutionItems.SearchFileInProject(chld.Children, stringToSearchFor, out pathAndFile))
					{
						return true;
					}
				}
			}
			catch (Exception e)
			{
				_ = VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			}

			// Not found
			pathAndFile = "";
			return false;
		}

	}
}
