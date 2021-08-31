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

		public static IVsTrackProjectDocuments2 _VsTrackProjectDocuments =
			SetVersionNumberGloballyXamPackage._VsTrackProjectDocuments;
		public static IVsQueryEditQuerySave2 _VsQueryEditQuerySave =
			SetVersionNumberGloballyXamPackage._VsQueryEditQuerySave;

		public static Community.VisualStudio.Toolkit.Solution TheSolution;

		public static List<VersionFilePathAndProj> infoplistFiles = new();
		public static List<VersionFilePathAndProj> appxmanifestFiles = new();
		public static List<VersionFilePathAndProj> manifestxmlFiles = new();
		public static List<VersionFilePathAndProj> notsupportedFiles = new();
		public static string PathToSolutionFolder { get; set; } = "";
		public static string PathToMajorMinorBuildRevisionNumbersXmlFile = "";
		public static Community.VisualStudio.Toolkit.Project MajorMinorBuildRevisionNumbersxmlContainingProject;
		public static bool MajorMinorBuildRevisionNumbersxmlExistsInProject = false;
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
			notsupported
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
			notsupportedFiles.Clear();

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

					MajorMinorBuildRevisionNumbersxmlExistsInProject =
						SearchFileInProject
						(
							TheSolution.Children
							,
							"MajorMinorBuildRevisionNumbers.xml"
							,
							out PathToMajorMinorBuildRevisionNumbersXmlFile
							,
							out MajorMinorBuildRevisionNumbersxmlContainingProject
						);

					if (!MajorMinorBuildRevisionNumbersxmlExistsInProject)
					{
						// Not found in project. Search directory
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

									string newLocation =
									   PathToSolutionFolder
									   +
									   Path.DirectorySeparatorChar
									   +
									   NameOfMajorMinorBuildRevisionNumbersXmlFile;

									//if
									//(
									//	_VsTrackProjectDocuments.OnQueryRenameFile
									//	(
									//		(IVsProject)TheSolution
									//		,
									//		PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile
									//		,
									//		newLocation
									//		,
									//		VSRENAMEFILEFLAGS.VSRENAMEFILEFLAGS_NoFlags
									//		,
									//		out int pfRenameCanContinue
									//	) == 0
									//)
									//{
									//	if (pfRenameCanContinue != 0)
									//	{
									//		Directory.Move
									//				(
									//					PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile
									//					,
									//					newLocation
									//				);
									//		PathToMajorMinorBuildRevisionNumbersXmlFile = PathToSolutionFolder;
									//		PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile = newLocation;
									//	}
									//}

									Directory.Move
									(
										PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile
										,
										newLocation
									);
									PathToMajorMinorBuildRevisionNumbersXmlFile = PathToSolutionFolder;
									PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile = newLocation;

								}
							}
							else
							{
								_ = (await VS.GetServiceAsync<DTE, DTE2>().ConfigureAwait(true)).SourceControl.CheckOutItem
										(PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile);

								File.Delete(PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile);
							}
						}
					}

					IEnumerable<SolutionItem> Projs = TheSolution.Children.Where
						(x => x.Type == SolutionItemType.Project);

					if (Projs.Any())
					{
						VersionFilePathAndProj vfpp;
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

								vfpp = new VersionFilePathAndProj()
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
									case FilesContainingVersionTypes.notsupported:
										{
											break;
										}
									default:
										break;
								}
							}
							else
							{
								vfpp = new VersionFilePathAndProj()
								{
									project = proj.Name
										,
									filePath = "Not supported type"
								};

								notsupportedFiles.Add(vfpp);
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
			fileType = FilesContainingVersionTypes.notsupported;
			pathAndFile = "";
			return false;
		}


		public static bool SearchFileInProject
			(
				in IEnumerable<SolutionItem> projChldrn
				,
				in string fileName
				,
				out string filePathAndName
				,
				out Community.VisualStudio.Toolkit.Project containingProject
			)
		{
			try
			{
				foreach (PhysicalFile pf in projChldrn.Where(x => x.Type == SolutionItemType.PhysicalFile))
				{
					if (pf.Name.ToLower().Contains(fileName.ToLower()))
					{
						filePathAndName = pf.FullPath;
						containingProject = pf.ContainingProject;
						return true;
					}
				}
				// Not found ..Try children
				foreach (SolutionItem chld in projChldrn)
				{
					if
					(
						SearchFileInProject
						(
							chld.Children
							,
							fileName
							,
							out filePathAndName
							,
							out containingProject
						)
					)
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
			filePathAndName = "";
			containingProject = null;
			return false;
		}
	}
}
