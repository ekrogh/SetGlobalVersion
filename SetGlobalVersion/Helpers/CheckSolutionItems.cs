﻿using EnvDTE;
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

		public static List<VersionFilePathAndProj> infoplistFiles = new();
		public static List<VersionFilePathAndProj> appxmanifestFiles = new();
		public static List<VersionFilePathAndProj> manifestxmlFiles = new();
		public static List<VersionFilePathAndProj> AssemblyInfo_csFiles = new();
		public static List<VersionFilePathAndProj> notsupportedFiles = new();
		public static string PathToSolutionFolder { get; set; } = "";
		public static string PathToMajorMinorBuildRevisionNumbersXmlFile = "";
		public static Community.VisualStudio.Toolkit.Project MajorMinorBuildRevisionNumbersxmlContainingProject = null;
		public static bool MajorMinorBuildRevisionNumbersxmlExistsInProject = false;
		public static string NameOfMajorMinorBuildRevisionNumbersXmlFile { get; set; } = "";
		public static string PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile = "";
		public static bool MajorMinorBuildRevisionNumbersXmlFileExistedAtStart { get; set; } = false;
		public static bool MajorMinorBuildRevisionNumbersXmlFileJustCreated { get; set; } = false;

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
			AssemblyInfo_cs
	,
			notsupported
		}


		private const string splist = $".plist";
		private const string sappxmanifest = $"appxmanifest";
		private const string smanifestxml = $"manifest.xml";
		private const string sgtk = $".gtk";

		private static readonly string[] stringsToSearchFor = { splist, sappxmanifest, smanifestxml, sgtk };

		public static void CleanUpHelpers()
		{
			TheSolution = null;

			infoplistFiles.Clear();
			appxmanifestFiles.Clear();
			manifestxmlFiles.Clear();
			AssemblyInfo_csFiles.Clear();
			notsupportedFiles.Clear();

			PathToSolutionFolder = "";
			PathToMajorMinorBuildRevisionNumbersXmlFile = "";
			MajorMinorBuildRevisionNumbersxmlContainingProject = null;
			MajorMinorBuildRevisionNumbersxmlExistsInProject = false;
			NameOfMajorMinorBuildRevisionNumbersXmlFile = "";
			PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile = "";
			MajorMinorBuildRevisionNumbersXmlFileExistedAtStart = false;
			MajorMinorBuildRevisionNumbersXmlFileJustCreated = false;

		}

		public static async Task<bool> SearchProjectFilesContainingVersionAsync(IEnumerable<SolutionItem> Projs)
		{
			bool VersionContainingProjectFileFound = false;

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
					VersionContainingProjectFileFound = true;

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
						case FilesContainingVersionTypes.AssemblyInfo_cs:
							{
								AssemblyInfo_csFiles.Add(vfpp);
								break;
							}
						case FilesContainingVersionTypes.notsupported:
							{
								break;
							}
						default:
							break;
					}
					if
					(
						!
						await CheckOutFromSourceControlAsync
						(
							pathAndFile
						)
					)
					{
						VersionContainingProjectFileFound = false;
						return false;
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
								case sgtk:
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