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
			//,
			//vsixmanifest
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
			unknown
		}


		private const string splist = $".plist";
		private const string sappxmanifest = $"appxmanifest";
		private const string smanifestxml = $"manifest.xml";
		private const string sAssemblyinfo_cs = $"assemblyinfo.cs";
		private const string scsproj = $"csproj";
		//private const string svsixmanifest = $"vsixmanifest";

		private static readonly string[] stringsToSearchFor =
			{
				splist
				, sappxmanifest
				, smanifestxml
				, sAssemblyinfo_cs
				, scsproj
				//, svsixmanifest
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


		static async Task CheckForProjTypesNotSupportedAsync(IEnumerable<SolutionItem> SLNItems)
		{
			try
			{
				VersionFilePathAndType VFPT = new()
				{
					FilePathAndName = "-"
					,
					FileType = FilesContainingVersionTypes.notsupported
				};

				foreach (var SLNItem in SLNItems)
				{
					if ((SLNItem.Name != null) && (SLNItem.Type.ToString() == "Project"))
					{
						if (ProjsWithVersionFiles.IndexOfKey(SLNItem.Name) < 0)
						{
							List<VersionFilePathAndType> VFPTList = new();
							VFPTList.Add(VFPT);
							ProjsWithVersionFiles.Add(SLNItem.Name, VFPTList);
						}
					}

					try
					{
						// Search children
						if (SLNItem.Children.Count() > 0)
						{
							await CheckForProjTypesNotSupportedAsync(SLNItem.Children);
						}
					}
					catch (Exception)
					{
						if (SLNItem.Name != null)
						{
							_ = await VS.MessageBox.ShowAsync
								(
									SLNItem.Name
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

		}


		static async Task AddToProjsWithVersionFilesAsync(SolutionItem SLNItem, string PathAndFile, FilesContainingVersionTypes FileTypeIn)
		{
			try
			{
				VersionFilePathAndType VFPT = new()
				{
					FilePathAndName = PathAndFile
				,
					FileType = FileTypeIn
				};

				var proj = SLNItem.FindParent(SolutionItemType.Project);

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
			SolutionItem proj
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

		static bool FoundVersionContainingFileInProject = false;
		static bool VersionContainingProjectFileFound = false;

		public static async Task<bool> SearchProjectFilesContainingVersionAsync(IEnumerable<SolutionItem> SLNItems)
		{
			string pathAndFile = String.Empty;
			FilesContainingVersionTypes fileType = FilesContainingVersionTypes.notsupported;
			bool FoundInThisSolitm = false;

			try
			{
				FoundVersionContainingFileInProject = false;

				foreach (SolutionItem SLNItem in SLNItems)
				{
					FoundInThisSolitm = false;

					if ((SLNItem.Name != null) && (SLNItem.Type.ToString() == "PhysicalFile"))
					{
						foreach (string str in stringsToSearchFor)
						{
							if (SLNItem.Name.EndsWith(str, StringComparison.OrdinalIgnoreCase))
							{
								pathAndFile = SLNItem.FullPath;
								switch (str)
								{
									case splist:
										{
											if
											(
												File.ReadAllText(SLNItem.FullPath).Contains
													(
														"CFBundleShortVersionString"
													)
											)
											{
												fileType = FilesContainingVersionTypes.infoplist;
												FoundVersionContainingFileInProject |= FoundInThisSolitm =
													await CheckOutFromSourceControlAddToProjsWithVersionFilesAsync
														(
															SLNItem
															, pathAndFile
															, fileType
														);

											}

											break;
										}
									case sappxmanifest:
										{
											if
											(
												File.ReadAllText(SLNItem.FullPath).Contains
													(
														"Identity"
													)
											)
											{
												fileType = FilesContainingVersionTypes.appxmanifest;
												FoundVersionContainingFileInProject |= FoundInThisSolitm =
													await CheckOutFromSourceControlAddToProjsWithVersionFilesAsync
														(
															SLNItem
															, pathAndFile
															, fileType
														);
											}

											break;
										}
									case smanifestxml:
										{
											if
											(
												File.ReadAllText(SLNItem.FullPath).Contains
													(
														"manifest"
													)
											)
											{
												fileType = FilesContainingVersionTypes.manifestxml;
												FoundVersionContainingFileInProject |= FoundInThisSolitm =
													await CheckOutFromSourceControlAddToProjsWithVersionFilesAsync
														(
															SLNItem
															, pathAndFile
															, fileType
														);
											}

											break;
										}
									case sAssemblyinfo_cs:
										{
											fileType = FilesContainingVersionTypes.Assemblyinfo_cs;

											FoundVersionContainingFileInProject |= FoundInThisSolitm =
												await CheckOutFromSourceControlAddToProjsWithVersionFilesAsync
													(
														SLNItem
														, SLNItem.FullPath
														, fileType
													);

											break;
										}
									case scsproj:
										{
											fileType = FilesContainingVersionTypes.projcsproj;

											FoundVersionContainingFileInProject |= FoundInThisSolitm =
												await CheckOutFromSourceControlAddToProjsWithVersionFilesAsync
													(
														SLNItem
														,
														SLNItem.FullPath
														,
														fileType
													);

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
					}

					VersionContainingProjectFileFound |= FoundVersionContainingFileInProject;


					try
					{
						// Search children
						if (SLNItem.Children.Count() > 0)
						{
							_ = SearchProjectFilesContainingVersionAsync(SLNItem.Children);
						}
					}
					catch (Exception)
					{
						if (SLNItem.Name != null)
						{
							_ = await VS.MessageBox.ShowAsync
								(
									SLNItem.Name
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

			return VersionContainingProjectFileFound;
		}


		public static async Task<bool> GetVersionContainingFilesInSolutionAsync()
		{

			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			CleanUpHelpers();

			bool ResultToReturn = false;

			try
			{
				if (await VS.Solutions.IsOpenAsync())
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
						IEnumerable<SolutionItem> SLNItems = TheSolution.Children;

						if (SLNItems.Any())
						{
							FoundVersionContainingFileInProject = false;
							VersionContainingProjectFileFound = false;

							ResultToReturn =
								await SearchProjectFilesContainingVersionAsync(SLNItems);

							if (ResultToReturn)
							{
								await CheckForProjTypesNotSupportedAsync(SLNItems);
								ResultToReturn = await AddMajorMinorBuildRevisionNumbersXmlFileProjectAsync();
							}
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
