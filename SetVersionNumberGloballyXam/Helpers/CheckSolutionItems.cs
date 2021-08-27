using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SetVersionNumberGloballyXam.Helpers
{
	public class CheckSolutionItems
	{
		private const string splist = $".plist";
		private const string sappxmanifest = $"appxmanifest";
		private const string smanifestxml = $"manifest.xml";
		private static string[] stringsToSearchFor = { splist, sappxmanifest, smanifestxml };
		private enum FilesContainingVersionTypes
		{
			infoplist
			,
			appxmanifest
			,
			manifestxml
			,
			none
		}

		public struct VersionFilePathAndProj
		{
			private string project;
			private string filePath;
		};

		public static List<VersionFilePathAndProj> infoplistFiles = new();
		public static List<VersionFilePathAndProj> appxmanifestFiles = new();
		public static List<VersionFilePathAndProj> manifestxmlFiles = new();

		public static async Task<bool> FindVersionContainingFilesInSolutionAsync()
		{
			bool versionContainingFileFound = false;

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			try
			{
				Solution TheSolution = await VS.Solutions.GetCurrentSolutionAsync().ConfigureAwait(true);

				if (TheSolution != null)
				{
					IEnumerable<SolutionItem> Projs = TheSolution.Children.Where(x => x.Type == SolutionItemType.Project);

					if (Projs.Any())
					{
						foreach (Project proj in Projs)
						{
							if
							(
								SearchVersionContainingFileInProject
								(
									proj.Children
									,
									out FilesContainingVersionTypes fileType
									,
									out string pathAndFile
								)
							)
							{
								versionContainingFileFound = true;

								VersionFilePathAndProj vfpp = new()
								{
									project = proj.Name
									,
									filePath = pathAndFile
								};

								switch (fileType)
								{
									case FilesContainingVersionTypes.infoplist:
										{
											infoplistFiles.Add(vfpp);
											break;
										}
									case FilesContainingVersionTypes.appxmanifest:
										{
											appxmanifestFiles.Add(vfpp);
											break;
										}
									case FilesContainingVersionTypes.manifestxml:
										{
											manifestxmlFiles.Add(vfpp);
											break;
										}
									case FilesContainingVersionTypes.none:
										{
											break;
										}
									default:
										break;
								}
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				_ = VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			}

			return versionContainingFileFound;

		}

		public static bool SearchVersionContainingFileInProject
			(
				in IEnumerable<SolutionItem> projChldrn
				,
				out FilesContainingVersionTypes fileType
				,
				out string pathAndFile
			)
		{
			try
			{
				foreach (PhysicalFile pf in projChldrn.Where(x => x.Type == SolutionItemType.PhysicalFile))
				{
					foreach (string str in stringsToSearchFor)
					{
						if (pf.Name.ToLower().Contains(str))
						{
							pathAndFile = pf.FullPath;
							switch (str)
							{
								case splist:
									{
										if
										(
											File.ReadAllText(pf.FullPath).Contains
												(
													"CFBundleShortVersionString"
												)
										)
										{
											fileType = FilesContainingVersionTypes.infoplist;
											return true;
										}

										break;
									}
								case sappxmanifest:
									{
										if
										(
											File.ReadAllText(pf.FullPath).Contains
												(
													"Identity"
												)
										)
										{
											fileType = FilesContainingVersionTypes.appxmanifest;
											return true;
										}

										break;
									}
								case smanifestxml:
									{
										if
										(
											File.ReadAllText(pf.FullPath).Contains
												(
													"manifest"
												)
)
										{
											fileType = FilesContainingVersionTypes.manifestxml;
											return true;
										}

										break;
									}
								default:
									break;
							}

							// not found
							fileType = FilesContainingVersionTypes.none;
							return false;
						}
					}
				}
				// Not found ..Try children
				foreach (SolutionItem chld in projChldrn)
				{
					if (SearchVersionContainingFileInProject(chld.Children, out fileType, out pathAndFile))
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
			fileType = FilesContainingVersionTypes.none;
			pathAndFile = "";
			return false;
		}

	}
}
