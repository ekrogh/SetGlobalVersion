using EnvDTE;
using EnvDTE100;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VSLangProj;

namespace SetGlobalVersion.Helpers
{
	public class CheckSolutionItems
	{
		public static DTE2 _dte;

		public static Community.VisualStudio.Toolkit.Solution TheSolution;

		public static SortedList<string, List<VersionFilePathAndType>> ProjsWithVersionFiles = new();

		public static string PathToSolutionFolder { get; set; } = "";
		public static string PathToMajorMinorBuildRevisionNumbersXmlFile = "";
		public static dynamic MajorMinorBuildRevisionNumbersxmlContainingProject = null;
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
			Assemblyinfo_cs
			,
			projcsproj
			,
			notsupported
			,
			appmanifest
			,
			vsixmanifest
		}


		public enum OsType
		{
			android
			,
			gtk
			,
			ios
			,
			macos
			,
			tizen
			,
			uwp
			,
			windows
			,
			unknown
		}


		private const string splist = $".plist";
		private const string sappxmanifest = $"appxmanifest";
		private const string smanifestxml = $"manifest.xml";
		private const string sAssemblyinfo_cs = $"assemblyinfo.cs";
		private const string svsixmanifest = $".vsixmanifest";
		private const string scsproj = $"csproj";
		private const string sappmanifest = $"app.manifest";

		private static readonly string[] stringsToSearchFor =
			{
				splist
				, sappxmanifest
				, smanifestxml
				, sAssemblyinfo_cs
				, svsixmanifest
				, scsproj
				, sappmanifest
			};


		public static void CleanUpHelpers()
		{
			TheSolution = null;

			ProjsWithVersionFiles.Clear();

			PathToSolutionFolder = "";
			PathToMajorMinorBuildRevisionNumbersXmlFile = "";
			MajorMinorBuildRevisionNumbersxmlExistsInProject = false;
			NameOfMajorMinorBuildRevisionNumbersXmlFile = "";
			PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile = "";
			MajorMinorBuildRevisionNumbersXmlFileExistedAtStart = false;
			MajorMinorBuildRevisionNumbersXmlFileJustCreated = false;

		}


		public static async Task SearchProjectItemsNotSupportedAsync(ProjectItems projectItems)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			VersionFilePathAndType VFPT = new()
			{
				FilePathAndName = "-"
		,
				FileType = FilesContainingVersionTypes.notsupported
			};

			foreach (ProjectItem projectItem in projectItems)
			{
				// Check the file extension of the project item
				string fileExtension = System.IO.Path.GetExtension(projectItem.Name);

				if
				(
					(projectItem.Name != null)
					&&
					(projectItem.Kind == PrjKind.prjKindCSharpProject)
				)
				{
					if (ProjsWithVersionFiles.IndexOfKey(projectItem.Name) < 0)
					{
						List<VersionFilePathAndType> VFPTList = new();
						VFPTList.Add(VFPT);
						ProjsWithVersionFiles.Add(projectItem.Name, VFPTList);
					}
				}

				// If the project item has child items, recursively search through them
				if (projectItem.ProjectItems != null && projectItem.ProjectItems.Count > 0)
				{
					// Call the function recursively
					await SearchProjectItemsNotSupportedAsync(projectItem.ProjectItems);
				}
			}
		}

		static async Task CheckForProjTypesNotSupportedAsync(Solution4 mySln)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			try
			{
				foreach (EnvDTE.Project project in mySln.Projects)
				{
					await SearchProjectItemsNotSupportedAsync(project.ProjectItems);
				}
			}
			catch (Exception e)
			{
				_ = await VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK).ConfigureAwait(true);
			}

		}

		static async Task AddToProjsWithVersionFilesAsync(EnvDTE.Project proj, string PathAndFile, FilesContainingVersionTypes FileTypeIn)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			try
			{
				VersionFilePathAndType VFPT = new()
				{
					FilePathAndName = PathAndFile
				,
					FileType = FileTypeIn
				};

				//var proj = proj.FindParent(SolutionItemType.Project);

				if (ProjsWithVersionFiles.IndexOfKey(proj.Name) < 0)
				{
					List<VersionFilePathAndType> VFPTList = new();
					VFPTList.Add(VFPT);
					ProjsWithVersionFiles.Add(proj.Name, VFPTList);
				}
				else
				{
					ProjsWithVersionFiles[proj.Name].Add(VFPT);
				}
			}
			catch (Exception e)
			{
				_ = await VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK).ConfigureAwait(true);
			}

		}

		static async Task AddToProjsWithVersionFilesAsync(ProjectItem projItem, string PathAndFile, FilesContainingVersionTypes FileTypeIn)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			try
			{
				VersionFilePathAndType VFPT = new()
				{
					FilePathAndName = PathAndFile
				,
					FileType = FileTypeIn
				};

				var proj = projItem.ContainingProject;
				//var proj = proj.FindParent(SolutionItemType.Project);

				if (ProjsWithVersionFiles.IndexOfKey(proj.Name) < 0)
				{
					List<VersionFilePathAndType> VFPTList = new();
					VFPTList.Add(VFPT);
					ProjsWithVersionFiles.Add(proj.Name, VFPTList);
				}
				else
				{
					ProjsWithVersionFiles[proj.Name].Add(VFPT);
				}
			}
			catch (Exception e)
			{
				_ = await VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK).ConfigureAwait(true);
			}

		}

		static async Task<bool> CheckOutFromSourceControlAddToProjsWithVersionFilesAsync
		(
			EnvDTE.Project proj
			,
			string pathAndFile
			,
			FilesContainingVersionTypes fileType
		)
		{
			bool VersionContainingProjectFileFound = false;

			try
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

					await AddToProjsWithVersionFilesAsync(proj, pathAndFile, fileType);
				}
				else
				{
					VersionContainingProjectFileFound = false;
				}
			}
			catch (Exception e)
			{
				_ = await VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK).ConfigureAwait(true);
			}

			return VersionContainingProjectFileFound;
		}

		static async Task<bool> CheckOutFromSourceControlAddToProjsWithVersionFilesAsync
		(
			ProjectItem projItem
			,
			string pathAndFile
			,
			FilesContainingVersionTypes fileType
		)
		{
			bool VersionContainingProjectFileFound = false;

			try
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

					await AddToProjsWithVersionFilesAsync(projItem, pathAndFile, fileType);
				}
				else
				{
					VersionContainingProjectFileFound = false;
				}
			}
			catch (Exception e)
			{
				_ = await VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK).ConfigureAwait(true);
			}

			return VersionContainingProjectFileFound;
		}

		public static Task<OsType> FindOsTypeAsync(string FullPath)
		{
			string tstContnt = File.ReadAllText(FullPath).ToLower();

			if (tstContnt.Contains("android"))
			{
				return System.Threading.Tasks.Task.FromResult(OsType.android);
			}
			else
			{
				if (tstContnt.Contains("gtk"))
				{
					return System.Threading.Tasks.Task.FromResult(OsType.gtk);
				}
				else
				{
					if (tstContnt.Contains("ios"))
					{
						return System.Threading.Tasks.Task.FromResult(OsType.ios);
					}
					else
					{
						if (tstContnt.Contains("macos"))
						{
							return System.Threading.Tasks.Task.FromResult(OsType.macos);
						}
						else
						{
							if (tstContnt.Contains("windows.universal") || tstContnt.Contains("uwp"))
							{
								return System.Threading.Tasks.Task.FromResult(OsType.uwp);
							}
							else
							{
								if (tstContnt.Contains("windows"))
								{
									return System.Threading.Tasks.Task.FromResult(OsType.windows);
								}
								else
								{
									if (tstContnt.Contains("tizen"))
									{
										return System.Threading.Tasks.Task.FromResult(OsType.tizen);
									}
									else
									{
										return System.Threading.Tasks.Task.FromResult(OsType.unknown);
									}
								}
							}
						}
					}
				}
			}
		}

		static bool SearchProjectItemsAsyncFoundVersionContainingFileInProject = false;
		static bool FoundVersionContainingFileInProject = false;
		static bool VersionContainingProjectFileFound = false;

		// Recursive function to search through project items
		public static async Task<bool> SearchProjectItemsAsync(ProjectItems projectItems)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			FilesContainingVersionTypes fileType = FilesContainingVersionTypes.notsupported;
			string pathAndFile = String.Empty;
			bool FoundInThisSolitm = false;

			foreach (ProjectItem projectItem in projectItems)
			{
				// Check the file extension of the project item
				string fileExtension = System.IO.Path.GetExtension(projectItem.Name);

				foreach (string str in stringsToSearchFor)
				{
					if (projectItem.Name.EndsWith(str, StringComparison.OrdinalIgnoreCase))
					{
						pathAndFile =
							projectItem.Properties.Item("FullPath").Value.ToString();

						switch (str)
						{
							case splist:
								{
									if
									(
										File.ReadAllText(pathAndFile).Contains
											(
												"CFBundleShortVersionString"
											)
									)
									{
										fileType = FilesContainingVersionTypes.infoplist;
										SearchProjectItemsAsyncFoundVersionContainingFileInProject |= FoundInThisSolitm =
											await CheckOutFromSourceControlAddToProjsWithVersionFilesAsync
												(
													projectItem
													,
													pathAndFile
													,
													fileType
												);

									}

									break;
								}
							case sappxmanifest:
								{
									if
									(
										File.ReadAllText(pathAndFile).Contains
											(
												"Identity"
											)
									)
									{
										fileType = FilesContainingVersionTypes.appxmanifest;
										SearchProjectItemsAsyncFoundVersionContainingFileInProject |= FoundInThisSolitm =
											await CheckOutFromSourceControlAddToProjsWithVersionFilesAsync
												(
													projectItem
													,
													pathAndFile
													,
													fileType
												);
									}

									break;
								}
							case smanifestxml:
								{
									var fileTxt = File.ReadAllText(pathAndFile).ToLower();

									bool isManifestWVer = fileTxt.Contains
												(
													"manifest"
												);

									if (projectItem.Name == "AndroidManifest.xml")
									{
										isManifestWVer =
											isManifestWVer && fileTxt.Contains
												(
													"android:versionName"
												);
									}
									if (isManifestWVer)
									{
										fileType = FilesContainingVersionTypes.manifestxml;
										SearchProjectItemsAsyncFoundVersionContainingFileInProject |= FoundInThisSolitm =
											await CheckOutFromSourceControlAddToProjsWithVersionFilesAsync
												(
													projectItem
													,
													pathAndFile
													,
													fileType
												);
									}

									break;
								}
							case sAssemblyinfo_cs:
								{
									fileType = FilesContainingVersionTypes.Assemblyinfo_cs;

									SearchProjectItemsAsyncFoundVersionContainingFileInProject |= FoundInThisSolitm =
										await CheckOutFromSourceControlAddToProjsWithVersionFilesAsync
											(
													projectItem
													,
													pathAndFile
													,
													fileType
											);

									break;
								}
							case svsixmanifest:
								{
									if
									(
										File.ReadAllText(pathAndFile).Contains
											(
												"Identity"
											)
									)
									{
										fileType = FilesContainingVersionTypes.vsixmanifest;
										SearchProjectItemsAsyncFoundVersionContainingFileInProject |= FoundInThisSolitm =
											await CheckOutFromSourceControlAddToProjsWithVersionFilesAsync
												(
													projectItem
													,
													pathAndFile
													,
													fileType
												);
									}

									break;
								}
							case scsproj:
								{
									if
									(
										File.ReadAllText(pathAndFile).Contains
											(
												"ApplicationDisplayVersion"
											)
									)
									{

										fileType = FilesContainingVersionTypes.projcsproj;

										SearchProjectItemsAsyncFoundVersionContainingFileInProject |= FoundInThisSolitm =
											await CheckOutFromSourceControlAddToProjsWithVersionFilesAsync
												(
													projectItem
														,
														pathAndFile
														,
														fileType
												);
									}
									break;
								}
							case sappmanifest:
								{
									if
									(
										File.ReadAllText(pathAndFile).ToLower().Contains
											(
												"assemblyidentity"
											)
									)
									{

										fileType = FilesContainingVersionTypes.appmanifest;

										SearchProjectItemsAsyncFoundVersionContainingFileInProject |=
											FoundInThisSolitm =
											await CheckOutFromSourceControlAddToProjsWithVersionFilesAsync
												(
													projectItem
														,
														pathAndFile
														,
														fileType
												);
									}
									break;
								}
							default:
								break;
						}

						if (FoundInThisSolitm)
						{
							break; // Break foreach (string str in stringsToSearchFor)
						}
					}
				}

				// If the project item has child items, recursively search through them
				if (projectItem.ProjectItems != null && projectItem.ProjectItems.Count > 0)
				{
					// Call the function recursively
					await SearchProjectItemsAsync(projectItem.ProjectItems);
				}
			}

			return SearchProjectItemsAsyncFoundVersionContainingFileInProject;
		}

		public static async Task<bool> SearchProjectFilesContainingVersionAsync(Solution4 mySln)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			try
			{
				SearchProjectItemsAsyncFoundVersionContainingFileInProject = false;
				FoundVersionContainingFileInProject = false;

				foreach (EnvDTE.Project project in mySln.Projects)
				{
					// Check if the project is a C# project
					//if (project.Kind == ProjectKinds.vsProjectKindSolutionFolder)
					if (project.Kind == PrjKind.prjKindCSharpProject)
					{
						if
						(
							File.ReadAllText(project.FullName).Contains
								(
									"ApplicationDisplayVersion"
								)
						)
						{
							string pathAndFile = project.FullName;
							bool FoundInThisSolitm = false;

							FilesContainingVersionTypes fileType =
								FilesContainingVersionTypes.projcsproj;

							FoundVersionContainingFileInProject |=
								FoundInThisSolitm =
								await CheckOutFromSourceControlAddToProjsWithVersionFilesAsync
									(
										project
										,
										pathAndFile
										,
										fileType
									);
						}

						FoundVersionContainingFileInProject |=
							await SearchProjectItemsAsync(project.ProjectItems);
					}

					VersionContainingProjectFileFound |= FoundVersionContainingFileInProject;

				}
			}
			catch (Exception e)
			{
				_ = await VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK).ConfigureAwait(true);
			}

			return VersionContainingProjectFileFound;
		}

		//private static List<string> GetSolutionFolderProjectFilePaths(EnvDTE.Project solutionFolder)
		//{
		//	ThreadHelper.ThrowIfNotOnUIThread();
		//	List<string> projectFilePaths = new List<string>();

		//	foreach (ProjectItem projectItem in solutionFolder.ProjectItems)
		//	{
		//		if (projectItem.SubProject != null)
		//		{
		//			if (projectItem.SubProject.Kind == ProjectKinds.vsProjectKindSolutionFolder)
		//			{
		//				// If the project item is a solution folder, recursively search for projects inside it
		//				projectFilePaths.AddRange(GetSolutionFolderProjectFilePaths(projectItem.SubProject));
		//			}
		//			else if (projectItem.SubProject.FileName.EndsWith(".csproj"))
		//			{
		//				// If the project item is a C# project, add its file path to the list
		//				projectFilePaths.Add(projectItem.SubProject.FileName);
		//			}
		//		}
		//	}

		//	return projectFilePaths;
		//}


		public static async Task<bool> GetVersionContainingFilesInSolutionAsync()
		{

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			CleanUpHelpers();

			bool ResultToReturn = false;

			try
			{
				if (await VS.Solutions.IsOpenAsync())
				{
					TheSolution =
						await VS.Solutions.GetCurrentSolutionAsync().ConfigureAwait(true);

					if (TheSolution != null)
					{
						//_dte = await VS.GetServiceAsync<DTE, DTE2>().ConfigureAwait(true);
						_dte = (DTE2)ServiceProvider.GlobalProvider.GetService(typeof(DTE));

						Solution4 mySln = (Solution4)_dte.Solution;

						// Sln folder
						PathToSolutionFolder = Path.GetDirectoryName(mySln.FullName);
						//PathToSolutionFolder = mySln.FullName;


						// MajorMinorBuildRevisionNumbers.xml

						// Does it exist in a projectItem ?
						ProjectItem docItem =
							mySln.FindProjectItem
								(
									"MajorMinorBuildRevisionNumbers.xml"
								);
						if (docItem != null)
						{ // It exists
							string flpth = String.Empty;
							for (Int16 idx = 1; idx <= docItem.FileCount; idx++)
							{
								flpth = docItem.FileNames[idx];
								if
								(
									flpth.IndexOf("MajorMinorBuildRevisionNumbers.xml"
									, StringComparison.OrdinalIgnoreCase) != -1
								)
								{
									PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile = flpth;
									MajorMinorBuildRevisionNumbersxmlContainingProject = docItem.ContainingProject;
									MajorMinorBuildRevisionNumbersXmlFileExistedAtStart = true;
									MajorMinorBuildRevisionNumbersxmlExistsInProject = true;
									break;
								}
							}
							if (!MajorMinorBuildRevisionNumbersXmlFileExistedAtStart
								|| !MajorMinorBuildRevisionNumbersxmlExistsInProject)
							{
								//find Community.VisualStudio.Toolkit.Project
								(
									bool success
									, PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile
								)
								= await SearchFileInProjectAsync
									(
										TheSolution.Children
										,
										"MajorMinorBuildRevisionNumbers.xml"
									);
								if
								(
									success
									&&
									(
										PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile !=
										String.Empty
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
						}

						// Search in projects
						FoundVersionContainingFileInProject = false;
						VersionContainingProjectFileFound = false;

						ResultToReturn =
							await SearchProjectFilesContainingVersionAsync(mySln);

						if (ResultToReturn)
						{
							//await CheckForProjTypesNotSupportedAsync(mySln);
							ResultToReturn =
								await AddMajorMinorBuildRevisionNumbersXmlFileProjectAsync();
						}
					}
				}
			}
			catch (Exception e)
			{
				_ = await VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK).ConfigureAwait(true);
			}

			return ResultToReturn;

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

				MajorMinorBuildRevisionNumbersxmlExistsInProject = true;

			}

			return true;
		}


		private static async Task<bool> CheckOutFromSourceControlAsync(string FileNameAndPath)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			bool fileIsCheckedOut = false;
			while (!fileIsCheckedOut)
			{
				try
				{
					if (_dte.SourceControl is SourceControl2 sourceControl)
					{
						//if
						//(
						//	sourceControl.IsItemUnderSCC
						//	(
						//		FileNameAndPath
						//	)
						//)
						//{
						if
						(
							!
							sourceControl.IsItemCheckedOut
							(
								FileNameAndPath
							)
						)
						{
							if
							(
								!
								sourceControl.CheckOutItem
								(
									FileNameAndPath
								)
							)
							{
								_ =
								await VS.MessageBox.ShowAsync
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
								).ConfigureAwait(true);

								return false;
							}
							else
							{
								fileIsCheckedOut = true;
							}
						}
						else
						{
							fileIsCheckedOut = true;
						}
						//}
					}
				}
				catch (System.NotImplementedException)
				{
					// Wait for the sourceControl system to get ready
					// Sleep for 1 second without blocking the rest of the system
					await Task.Delay(1000);

				}
			}

			return true;
		}


		public static async Task<(bool success, string filePathAndName)>
			SearchFileInProjectAsync
			(
				IEnumerable<SolutionItem> projChldrn
				,
				string fileName

			)
		{
			string filePathAndName = "";

			try
			{
				foreach (SolutionItem SLI in projChldrn)
				{
					if
					(
						(SLI.Name != null)
						&& (SLI.Name.EndsWith
								(
									fileName, StringComparison.OrdinalIgnoreCase
								)
							)
					)
					{
						filePathAndName = SLI.FullPath;
						return (true, filePathAndName);
					}

					// Not found ..Try children
					try
					{
						if (SLI.Children.Count() > 0)
						{
							(bool success, filePathAndName) =
									await SearchFileInProjectAsync
									(
										SLI.Children
										,
										fileName
									);
							if (success)
							{
								return (true, filePathAndName);
							}
						}
					}
					catch (Exception)
					{
						if (SLI.Name != null)
						{
							_ = await VS.MessageBox.ShowAsync
								(
									SLI.Name
									, "Is Broken"
									, OLEMSGICON.OLEMSGICON_WARNING
									, OLEMSGBUTTON.OLEMSGBUTTON_OK
								);
						}
					}

				}
			}
			catch (Exception e)
			{
				_ = await VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK).ConfigureAwait(true);
			}

			// Not found
			filePathAndName = "";
			return (false, filePathAndName);
		}
	}
}
