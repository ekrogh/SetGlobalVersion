using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SetVersionNumberGloballyXam.Helpers
{
	public class CheckSolutionItems
	{
		public static Community.VisualStudio.Toolkit.Solution TheSolution;

		public static List<VersionFilePathAndProj> infoplistFiles = new();
		public static List<VersionFilePathAndProj> appxmanifestFiles = new();
		public static List<VersionFilePathAndProj> manifestxmlFiles = new();
		public static string PathToSolutionFolder { get; set; } = "";
		public static string PathToMajorMinorBuildRevisionNumbersXmlFile { get; set; } = "";
		public static string NameOfMajorMinorBuildRevisionNumbersXmlFile { get; set; } = "";
		public static string PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile { get; set; } = "";
		public static bool MajorMinorBuildRevisionNumbersXmlFileExistedAtStart { get; set; } = false;

		public struct VersionFilePathAndProj
		{
			public string project;
			public string filePath;
		};
		public enum FilesContainingVersionTypes
		{
			infoplist
	,
			appxmanifest
	,
			manifestxml
	,
			none
		}


		private const string splist = $".plist";
		private const string sappxmanifest = $"appxmanifest";
		private const string smanifestxml = $"manifest.xml";

		private static readonly string[] stringsToSearchFor = { splist, sappxmanifest, smanifestxml };

		public static void CleanUpHelpers()
		{
			TheSolution = null;

			infoplistFiles.Clear();
			appxmanifestFiles.Clear();
			manifestxmlFiles.Clear();
			PathToSolutionFolder = "";
			PathToMajorMinorBuildRevisionNumbersXmlFile = "";
			NameOfMajorMinorBuildRevisionNumbersXmlFile = "";
			PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile = "";
			MajorMinorBuildRevisionNumbersXmlFileExistedAtStart = false;
		}

		public static async Task<bool> FindVersionContainingFilesInSolutionAsync()
		{

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			CleanUpHelpers();

			bool versionContainingFileFound = false;

			try
			{
				TheSolution = await VS.Solutions.GetCurrentSolutionAsync().ConfigureAwait(true);

				if (TheSolution != null)
				{

					// Sln folder
					PathToSolutionFolder = Path.GetDirectoryName(TheSolution.FullPath);

					// MajorMinorBuildRevisionNumbers.xml
					PathToMajorMinorBuildRevisionNumbersXmlFile = PathToSolutionFolder;
					NameOfMajorMinorBuildRevisionNumbersXmlFile = "MajorMinorBuildRevisionNumbers.xml";
					PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile =
						PathToMajorMinorBuildRevisionNumbersXmlFile
						+
						Path.DirectorySeparatorChar
						+
						NameOfMajorMinorBuildRevisionNumbersXmlFile;

					string[] MMRNXml =
						Directory.GetFiles
							(
								PathToSolutionFolder
								,
								NameOfMajorMinorBuildRevisionNumbersXmlFile
								,
								SearchOption.AllDirectories
							);
					if (MMRNXml.Length != 0)
					{
						PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile = MMRNXml[0];

						if (!File.ReadAllText(PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile).ToLower().Contains("justcreated"))
						{
							MajorMinorBuildRevisionNumbersXmlFileExistedAtStart = true;

							PathToMajorMinorBuildRevisionNumbersXmlFile =
								Path.GetDirectoryName(PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile);

							if (PathToMajorMinorBuildRevisionNumbersXmlFile != PathToSolutionFolder)
							{
								_ = (await VS.GetServiceAsync<DTE, DTE2>().ConfigureAwait(true)).SourceControl.CheckOutItem
										(PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile);

								Directory.Move
										(
											PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile
											,
											(
												PathToSolutionFolder
												+
												Path.DirectorySeparatorChar
												+
												NameOfMajorMinorBuildRevisionNumbersXmlFile
											)
										);
								PathToMajorMinorBuildRevisionNumbersXmlFile = PathToSolutionFolder;
								PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile =
											PathToSolutionFolder
											+
											Path.DirectorySeparatorChar
											+
											NameOfMajorMinorBuildRevisionNumbersXmlFile;

							}
						}
						else
						{
							_ = (await VS.GetServiceAsync<DTE, DTE2>().ConfigureAwait(true)).SourceControl.CheckOutItem
									(PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile);

							File.Delete(PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile);
						}
					}

					IEnumerable<SolutionItem> Projs = TheSolution.Children.Where
						(x => x.Type == SolutionItemType.Project);

					if (Projs.Any())
					{
						foreach (Community.VisualStudio.Toolkit.Project proj in Projs)
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

							//// not found
							//fileType = FilesContainingVersionTypes.none;
							//return false;
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
