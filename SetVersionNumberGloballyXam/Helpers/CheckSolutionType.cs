using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SetVersionNumberGloballyXam
{
	public class CheckSolutionType
	{
		public static async Task<bool> ThisIsXamarinAsync()
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			try
			{

				Solution Sln = await VS.Solutions.GetCurrentSolutionAsync().ConfigureAwait(true);

				if (Sln != null)
				{

					IEnumerable<SolutionItem> Projs = Sln.Children.Where(x => x.Type == SolutionItemType.Project);

					if (Projs.Any())
					{
						foreach (SolutionItem si in Projs)
						{
							ProjectType ProjType = GetProjectType(si);

							if (ProjType == ProjectType.XamarinForms)
							{
								return true;
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				_ = VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			}
			finally
			{
			}

			return false;
		}

		private static ProjectType GetProjectType(SolutionItem SlnItem)
		{
			string ProjPath = SlnItem.FullPath.Substring(0, SlnItem.FullPath.LastIndexOf(Path.DirectorySeparatorChar));
			foreach (string FilePath in Directory.GetFiles(ProjPath, @"*.*proj", SearchOption.AllDirectories))
			{
				string FilePathLower = FilePath.ToLower();
				if
				(
					!FilePathLower.Contains("obj")
					&&
					!FilePathLower.Contains("debug")
					&&
					!FilePathLower.Contains("release")
					&&
					!FilePathLower.Contains("bin")
				)
				{
					if (System.IO.File.ReadAllText(FilePath).ToLower().Contains("xamarin"))
					{
						return ProjectType.XamarinForms;
					}
				}
			}
			return ProjectType.Unknown;
		}
	}
}
