using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
namespace SetGlobalVersion.Helpers
{
	public class CheckSolutionItems
	{
		public static DTE2 _dte;

		public static Community.VisualStudio.Toolkit.Solution TheSolution;

		public static SortedList<string, List<VersionFilePathAndType>> ProjsWithVersionFiles = new();

		public static string PathToSolutionFolder { get; set; } = "";
		public static string PathToMajorMinorBuildRevisionNumbersXmlFile = "";
		public static Community.VisualStudio.Toolkit.Project MajorMinorBuildRevisionNumbersxmlContainingProject = null;
		public static bool MajorMinorBuildRevisionNumbersxmlExistsInProject = false;
		public static string NameOfMajorMinorBuildRevisionNumbersXmlFile { get; set; } = "";
		public static string PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile = "";

		public static bool MajorMinorBuildRevisionNumbersXmlFileExistedAtStart { get; set; } = false;
		public static bool MajorMinorBuildRevisionNumbersXmlFileJustCreated { get; set; } = false;

		public struct VersionFilePathAndType
		{
			public string FilePathAndName;
			public FilesContainingVersionTypes FileType;
		};

		public enum FilesContainingVersionTypes
		{
			infoplist
	,
			appxmanifest
	,
			manifestxml
		 ,
			AssemblyInfo_cs
	,
			notsupported
		}


		private const string splist = $".plist";
		private const string sappxmanifest = $"appxmanifest";
		private const string smanifestxml = $"manifest.xml";
		private const string sAssemblyInfo_cs = $"AssemblyInfo.cs";

		private static readonly string[] stringsToSearchFor = { splist, sappxmanifest, smanifestxml, sAssemblyInfo_cs };

		public static void CleanUpHelpers()
		{
			TheSolution = null;

			ProjsWithVersionFiles.Clear();

			PathToSolutionFolder = "";
			PathToMajorMinorBuildRevisionNumbersXmlFile = "";
			MajorMinorBuildRevisionNumbersxmlContainingProject = null;
			MajorMinorBuildRevisionNumbersxmlExistsInProject = false;
			NameOfMajorMinorBuildRevisionNumbersXmlFile = "";
			PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile = "";
			MajorMinorBuildRevisionNumbersXmlFileExistedAtStart = false;
			MajorMinorBuildRevisionNumbersXmlFileJustCreated = false;

		}

		public static bool SearchAssemblyInfo_csFiles
		(
			  in SolutionItem Proj
			, out string pathAndFile
		)
		{

			string ThePath = Path.GetDirectoryName(Proj.FullPath);
			string[] AssemblyInfo_cs_files = Directory.GetFiles(ThePath, "AssemblyInfo.cs", SearchOption.AllDirectories);
			if (AssemblyInfo_cs_files.Length > 0)
			{
				pathAndFile = AssemblyInfo_cs_files[0];
				return true;
			}

			pathAndFile = "";
			return false;
		}

		static void AddToProjsWithVersionFiles(Community.VisualStudio.Toolkit.Project Proj, string PathAndFile, FilesContainingVersionTypes FileTypeIn)
		{
			VersionFilePathAndType VFPT = new()
			{
				FilePathAndName = PathAndFile
				,
				FileType = FileTypeIn
			};

			if (ProjsWithVersionFiles.IndexOfKey(Proj.Name) < 0)
			{
				List<VersionFilePathAndType> VFPTList = new();
				VFPTList.Add(VFPT);
				ProjsWithVersionFiles.Add(Proj.Name, VFPTList);
			}
			else
			{
				ProjsWithVersionFiles[Proj.Name].Add(VFPT);
			}

		}

		public static async Task<bool> SearchProjectFilesContainingVersionAsync(IEnumerable<SolutionItem> Projs)
		{
			bool VersionContainingProjectFileFound = false;

			foreach (Community.VisualStudio.Toolkit.Project proj in Projs)
			{
				// Contain AssemblyInfo.cs ?
				if
				(
					SearchAssemblyInfo_csFiles
					(
						  proj
						, out string AssemblyInfo_csPathAndFile
					)
				)
				{
					if
					(
						await CheckOutFromSourceControlAsync
						(
							AssemblyInfo_csPathAndFile
						)
					)
					{
						VersionContainingProjectFileFound = true;

						AddToProjsWithVersionFiles(proj, AssemblyInfo_csPathAndFile, FilesContainingVersionTypes.AssemblyInfo_cs);
					}
					else
					{
						VersionContainingProjectFileFound = false;
					}
				}

				// Search for other
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
					if
					(
						await CheckOutFromSourceControlAsync
						(
							pathAndFile
						)
					)
					{
						VersionContainingProjectFileFound = true;

						AddToProjsWithVersionFiles(proj, pathAndFile, fileType);
					}
					else
					{
						VersionContainingProjectFileFound = false;
					}


				}

				if (!VersionContainingProjectFileFound)
				{
					AddToProjsWithVersionFiles(proj, "Not supported type", FilesContainingVersionTypes.notsupported);

				}
			}

			return VersionContainingProjectFileFound;
		}

		public static async Task<bool> AddMajorMinorBuildRevisionNumbersXmlFileProjectAsync()
		{
			if (!MajorMinorBuildRevisionNumbersxmlExistsInProject)
			{
				// Add to solution (To ensure source control check in)
				Community.VisualStudio.Toolkit.SolutionFolder SolutionFolderMajorMinorBuildRevisionNumbersXmlFile =
					await TheSolution.AddSolutionFolderAsync
					(
						"MajorMinorBuildRevisionNumbersXmlFile"
					).ConfigureAwait(true);

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

				if (MMRNXml.Length == 0)
				{
					// Not found. Create it
					File.WriteAllText
						(
							PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile
							,
							"justcreated"
						);
				}
				else
				{
					PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile = MMRNXml[0];

					if
					(
						!
						await CheckOutFromSourceControlAsync
						(
							PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile
						)
					)
					{
						return false;
					}

					MajorMinorBuildRevisionNumbersXmlFileExistedAtStart = true;
				}

				PathToMajorMinorBuildRevisionNumbersXmlFile =
					Path.GetDirectoryName
					(
						PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile
					);


				PhysicalFile[] refMajorMinorBuildRevisionNumbersXmlFile =
						(await SolutionFolderMajorMinorBuildRevisionNumbersXmlFile.AddExistingFilesAsync
						(
							PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile
						)).ToArray();

				PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile = refMajorMinorBuildRevisionNumbersXmlFile[0].FullPath;
				MajorMinorBuildRevisionNumbersxmlContainingProject = refMajorMinorBuildRevisionNumbersXmlFile[0].ContainingProject;

				MajorMinorBuildRevisionNumbersxmlExistsInProject = true;

			}

			return true;
		}

		public static async Task<bool> GetVersionContainingFilesInSolutionAsync()
		{

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			CleanUpHelpers();

			bool ResultToReturn = false;

			try
			{
				TheSolution = await VS.Solutions.GetCurrentSolutionAsync().ConfigureAwait(true);

				if (TheSolution != null)
				{
					_dte = await VS.GetServiceAsync<DTE, DTE2>().ConfigureAwait(true);

					// Sln folder
					PathToSolutionFolder = Path.GetDirectoryName(TheSolution.FullPath);


					// MajorMinorBuildRevisionNumbers.xml

					// Does it exist in a projectItem ?
					ProjectItem docItem =
						_dte.Solution.FindProjectItem
							(
								"MajorMinorBuildRevisionNumbers.xml"
							);
					if (docItem != null)
					{ // It exists
						if
						(
							SearchFileInProject
							(
								TheSolution.Children
								,
								"MajorMinorBuildRevisionNumbers.xml"
								,
								out PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile
								,
								out MajorMinorBuildRevisionNumbersxmlContainingProject
							)
						)
						{
							MajorMinorBuildRevisionNumbersXmlFileExistedAtStart = true;
							MajorMinorBuildRevisionNumbersxmlExistsInProject = true;

							if (!await CheckOutFromSourceControlAsync(PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile))
							{
								return false;
							}
						}
					}

					// Search in projects
					IEnumerable<SolutionItem> Projs = TheSolution.Children.Where
						(x => x.Type == SolutionItemType.Project);

					if (Projs.Any())
					{
						ResultToReturn =
							await SearchProjectFilesContainingVersionAsync(Projs);

						if (ResultToReturn)
						{
							ResultToReturn = await AddMajorMinorBuildRevisionNumbersXmlFileProjectAsync();
						}
					}
				}
			}
			catch (Exception e)
			{
				_ = VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			}

			return ResultToReturn;

		}

		private static async Task<bool> CheckOutFromSourceControlAsync(string FileNameAndPath)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			if
			(
				!
				_dte.SourceControl.CheckOutItem
				(
					FileNameAndPath
				)
			)
			{
				_ =
				VS.MessageBox.ShowAsync
				(
					"Please check out\n"
					,
					FileNameAndPath
					+
					"\nbefore continuing"
					,
					OLEMSGICON.OLEMSGICON_CRITICAL
					,
					OLEMSGBUTTON.OLEMSGBUTTON_OK
				);

				return false;
			}

			return true;
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
								case sAssemblyInfo_cs:
									{
										string ThePath = Path.GetDirectoryName(pf.FullPath);
										string[] AssemblyInfo_cs_files = Directory.GetFiles(ThePath, "AssemblyInfo.cs", SearchOption.AllDirectories);
										if (AssemblyInfo_cs_files.Length > 0)
										{
											pathAndFile = AssemblyInfo_cs_files[0];
											fileType = FilesContainingVersionTypes.AssemblyInfo_cs;
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
				foreach
					(
						PhysicalFile pf in projChldrn.Where
						(
							x => x.Type == SolutionItemType.PhysicalFile
						)
					)
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
