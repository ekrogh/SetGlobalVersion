﻿using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;
using System.Xml.Linq;
using static SetVersionNumberGloballyXam.Helpers.CheckSolutionItems;

namespace SetVersionNumberGloballyXam
{
	public partial class MyToolWindowControl : UserControl
	{
		private bool HasBeenSetInvisible { get; set; } = false;

		public MyToolWindowControl()
		{
			InitializeComponent();

			_ = GetShowPathsToAndroidiOSmacOSUWPAsync();

			IsVisibleChanged += SetVersionNumberControl_IsVisibleChanged;

			VS.Events.SolutionEvents.OnAfterCloseSolution += SolutionEvents_OnAfterCloseSolution;
			VS.Events.SolutionEvents.OnAfterBackgroundSolutionLoadComplete += SolutionEvents_OnAfterBackgroundSolutionLoadComplete;


		}

		private void SolutionEvents_OnAfterBackgroundSolutionLoadComplete()
		{
			_ = GetShowPathsToAndroidiOSmacOSUWPAsync();

			Visibility = Visibility.Visible;
		}

		private void SolutionEvents_OnAfterCloseSolution()
		{
			GetShowPathsToAndroidiOSmacOSUWPAsynHasBeenRun = false;

			HasBeenSetInvisible = true;
			Visibility = Visibility.Hidden;
		}


		private void SetVersionNumberControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (!(bool)e.NewValue && (bool)e.OldValue)
			{
				HasBeenSetInvisible = true;

			}
			else
			{
				if (HasBeenSetInvisible && ((bool)e.NewValue))
				{
					Visibility = Visibility.Visible;
					_ = GetShowPathsToAndroidiOSmacOSUWPAsync();
				}
			}
		}


		private int VersionMajor = int.MinValue;
		private int VersionMinor = int.MinValue;
		private int BuildNumber = int.MinValue;
		private int RevisionNumber = int.MinValue;

		// Solution
		public struct MySolutionData
		{
			public string thisSolutionName { set; get; }
			public string thisSolutionPath { set; get; }
		}

		// Project
		public struct MyProjectsData
		{
			public string thisSolutionProject { set; get; }
			public string thisSolutionProjectPath { set; get; }
		}

		private bool GetShowPathsToAndroidiOSmacOSUWPAsynHasBeenRun { get; set; } = false;
		private async Task GetShowPathsToAndroidiOSmacOSUWPAsync()
		{
			if (!GetShowPathsToAndroidiOSmacOSUWPAsynHasBeenRun)
			{
				GetShowPathsToAndroidiOSmacOSUWPAsynHasBeenRun = true;

				try
				{
					await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

					// Clear datagrids
					mySolutionDataGrid.Items.Clear();
					myProjectsDataGrid.Items.Clear();

					if (await FindVersionContainingFilesInSolutionAsync())
					{
						Style DataGridTextColumnElementStyle = new(typeof(TextBlock));
						DataGridTextColumnElementStyle.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));


						// Show solution and its path

						DataGridTextColumn col0 = new DataGridTextColumn();
						DataGridTextColumn col1 = new DataGridTextColumn();
						mySolutionDataGrid.Columns.Add(col0);
						mySolutionDataGrid.Columns.Add(col1);
						col0.Binding = new Binding("thisSolutionName");
						col1.Binding = new Binding("thisSolutionPath");
						col0.Header = "Solution";
						col1.Header = "Path";
						col1.ElementStyle = DataGridTextColumnElementStyle;

						mySolutionDataGrid.Items.Add(new MySolutionData { thisSolutionName = TheSolution.Name, thisSolutionPath = TheSolution.FullPath });


						// Show paths to files containing version

						col0 = new DataGridTextColumn();
						col1 = new DataGridTextColumn();
						myProjectsDataGrid.Columns.Add(col0);
						myProjectsDataGrid.Columns.Add(col1);
						col0.Binding = new Binding("thisSolutionProject");
						col1.Binding = new Binding("thisSolutionProjectPath");
						col0.Header = "Project";
						col1.Header = "File containing version";
						col1.ElementStyle = DataGridTextColumnElementStyle;

						foreach (VersionFilePathAndProj vfpp in infoplistFiles)
						{
							myProjectsDataGrid.Items.Add(new MyProjectsData { thisSolutionProject = vfpp.project, thisSolutionProjectPath = vfpp.filePath });
						}
						foreach (VersionFilePathAndProj vfpp in appxmanifestFiles)
						{
							myProjectsDataGrid.Items.Add(new MyProjectsData { thisSolutionProject = vfpp.project, thisSolutionProjectPath = vfpp.filePath });
						}
						foreach (VersionFilePathAndProj vfpp in manifestxmlFiles)
						{
							myProjectsDataGrid.Items.Add(new MyProjectsData { thisSolutionProject = vfpp.project, thisSolutionProjectPath = vfpp.filePath });
						}

						if (MajorMinorBuildRevisionNumbersXmlFileExistedAtStart)
						{
							XDocument TheXDocument = ReadFromXmlFile(PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile);
							if (TheXDocument != null)
							{
								IEnumerable<XElement> XElementList = TheXDocument.Elements();
								foreach (XElement theXElement in XElementList)
								{
									if (theXElement.Name.LocalName == "MAJORMINORBUILDNUMBERS")
									{
										IEnumerable<XAttribute> XAttributesList = theXElement.Attributes();
										foreach (XAttribute theXAttribute in XAttributesList)
										{
											switch (theXAttribute.Name.LocalName)
											{
												case "VersionMajor":
													{
														if (int.TryParse(theXAttribute.Value, out VersionMajor))
														{
															VersionMajorEntryName.Text = theXAttribute.Value;
														}
														else
														{
															_ = VS.MessageBox.ShowAsync
																(
																	"Invalid \"VersionMajor\" in file"
																	, PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile
																	, OLEMSGICON.OLEMSGICON_CRITICAL
																	, OLEMSGBUTTON.OLEMSGBUTTON_OK
																);
															VersionMajor = int.MinValue;
															VersionMajorEntryName.Text = "";
														}
														break;
													}
												case "VersionMinor":
													{
														if (int.TryParse(theXAttribute.Value, out VersionMinor))
														{
															VersionMinorEntryName.Text = theXAttribute.Value;
														}
														else
														{
															_ = VS.MessageBox.ShowAsync("Invalid \"VersionMinor\" in file"
																, PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile
																, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK
																);
															VersionMinor = int.MinValue;
															VersionMinorEntryName.Text = "";
														}
														break;
													}
												case "BuildNumber":
													{
														if (int.TryParse(theXAttribute.Value, out BuildNumber))
														{
															BuildNumberEntryName.Text = theXAttribute.Value;
														}
														else
														{
															_ = VS.MessageBox.ShowAsync("Invalid \"BuildNumber\" in file"
																, PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile
																, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK
																);
															BuildNumber = int.MinValue;
															BuildNumberEntryName.Text = "";
														}
														break;
													}
												case "RevisionNumber":
													{
														if (int.TryParse(theXAttribute.Value, out RevisionNumber))
														{
															RevisionNumberEntryName.Text = theXAttribute.Value;
														}
														else
														{
															_ = VS.MessageBox.ShowAsync("Invalid \"RevisionNumber\" in file"
																, PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile
																, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK
																);
															RevisionNumber = int.MinValue;
															RevisionNumberEntryName.Text = "";
														}
														break;
													}
											}
										}
									}
								}
							}
							else
							{
								_ = VS.MessageBox.ShowAsync("Invalid \".xml\" file", PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
							}
						}
					}
					//if (await ThisIsXamarinAsync().ConfigureAwait(true))
					//{

					//	if (TheSolution != null)
					//	{

					//		PathToSolutionFolderEntry.Text = PathToSolutionFolder;

					//		foreach (Community.VisualStudio.Toolkit.Project proj in XamarinFormsProjectsList)
					//		{
					//			if (ThisIsXamarinFormsProject(proj.Children))
					//			{
					//				if (proj.Name.ToLower().Contains("droid"))
					//				{
					//					if (SearchFileInProject
					//						(
					//							proj.Children
					//							,
					//							out FilesContainingVersionTypes fileType
					//							,
					//							out string pathAndFile
					//						))
					//					{
					//						if (File.ReadAllText(pathAndFile).Contains("manifest"))
					//						{
					//							PathToAndNameOfAndroidManifestFile = pathAndFile;
					//						}
					//					}
					//				}
					//				else
					//				{
					//					if (proj.Name.ToLower().Contains("uwp"))
					//					{
					//						if (SearchFileInProject
					//							(
					//								proj.Children
					//								,
					//								out FilesContainingVersionTypes fileType
					//								,
					//								out string pathAndFile
					//							))
					//						{
					//							if (File.ReadAllText(pathAndFile).Contains("Identity"))
					//							{
					//								PathToAndNameOfUWPPackageAppxmanifest = pathAndFile;
					//							}
					//						}
					//					}
					//					else
					//					{
					//						if (proj.FullPath.ToLower().Contains("ios"))
					//						{
					//							if (SearchFileInProject
					//								(
					//									proj.Children
					//									,
					//									out FilesContainingVersionTypes fileType
					//									,
					//									out string pathAndFile
					//								))
					//							{
					//								if (File.ReadAllText(pathAndFile).Contains("CFBundleShortVersionString"))
					//								{
					//									PathToAndNameOfiOSInfoPlist = pathAndFile;
					//								}
					//							}
					//						}
					//						else
					//						{
					//							if (proj.FullPath.ToLower().Contains("mac"))
					//							{
					//								if (SearchFileInProject
					//									(
					//										proj.Children
					//										,
					//										out FilesContainingVersionTypes fileType
					//										,
					//										out string pathAndFile
					//									))
					//								{
					//									if (File.ReadAllText(pathAndFile).Contains("CFBundleShortVersionString"))
					//									{
					//										PathToAndNameOfmacOSInfoPlist = pathAndFile;
					//									}
					//								}
					//							}
					//						}
					//					}
					//				}
					//			}
					//		}


					//		// Android
					//		if (System.IO.File.Exists(PathToAndNameOfAndroidManifestFile))
					//		{
					//			AndroidManifestFileLabel.Content = PathToAndNameOfAndroidManifestFile;
					//		}
					//		else
					//		{
					//			PathToAndNameOfAndroidManifestFile = "";
					//			AndroidManifestFileLabel.Content = "-";
					//		}

					//		// iOS
					//		if (System.IO.File.Exists(PathToAndNameOfiOSInfoPlist))
					//		{
					//			PathToiOSInfoPlistLabel.Content = PathToAndNameOfiOSInfoPlist;
					//		}
					//		else
					//		{
					//			PathToAndNameOfiOSInfoPlist = "";
					//			PathToiOSInfoPlistLabel.Content = "-";
					//		}

					//		//	macOS
					//		if (System.IO.File.Exists(PathToAndNameOfmacOSInfoPlist))
					//		{
					//			PathTomacOSInfoPlistLabel.Content = PathToAndNameOfmacOSInfoPlist;
					//		}
					//		else
					//		{
					//			PathToAndNameOfmacOSInfoPlist = "";
					//			PathTomacOSInfoPlistLabel.Content = "-";
					//		}
					//		// UWP
					//		if (System.IO.File.Exists(PathToAndNameOfUWPPackageAppxmanifest))
					//		{
					//			PathToUWPPackageAppxmanifestLabel.Content = PathToAndNameOfUWPPackageAppxmanifest;
					//		}
					//		else
					//		{
					//			PathToAndNameOfUWPPackageAppxmanifest = "";
					//			PathToUWPPackageAppxmanifestLabel.Content = "-";
					//		}

					//		if
					//		(
					//			PathToAndNameOfAndroidManifestFile == ""
					//			&&
					//			PathToAndNameOfiOSInfoPlist == ""
					//			&&
					//			PathToAndNameOfmacOSInfoPlist == ""
					//			&&
					//			PathToAndNameOfUWPPackageAppxmanifest == ""
					//		)
					//		{
					//			SetNumbersButton.IsEnabled = false;
					//			_ = VS.MessageBox.ShowAsync("Error: ", "No project folders found !", OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);

					//			if (MajorMinorBuildRevisionNumbersXmlFilejustCreated)
					//			{
					//				System.IO.File.Delete(PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile);
					//			}
					//		}
					//		else
					//		{
					//			SetNumbersButton.IsEnabled = true;
					//		}
					//	}
					//}
				}
				catch (Exception e)
				{
					System.Diagnostics.Debug.WriteLine("String processing failed: {0}", e.ToString());
					_ = VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
				}
				finally { }
			}
		}

		private void OnVersionMajorEntryCompleted(object sender, System.Windows.RoutedEventArgs e)
		{
			int LocalVersionMajor = 0;
			if (((VersionMajorEntryName.Text != null)
				&& (VersionMajorEntryName.Text.Length != 0)
				&& !int.TryParse(VersionMajorEntryName.Text, out LocalVersionMajor))
				|| (LocalVersionMajor < 0)
				)
			{
				VersionMajor = int.MinValue;
				string TextHolder = VersionMajorEntryName.Text;
				VersionMajorEntryName.Text = "";
				_ = VS.MessageBox.ShowAsync("Invalid \"Version Major\" ", TextHolder, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
				VersionMajorEntryName.Focus();
			}
			else
			{
				VersionMajor = LocalVersionMajor;
			}
		}

		private void OnVersionMinorEntryCompleted(object sender, System.Windows.RoutedEventArgs e)
		{
			int LocalVersionMinor = 0;
			if ((VersionMinorEntryName.Text != null)
				&& (VersionMinorEntryName.Text.Length != 0)
				&& !int.TryParse(VersionMinorEntryName.Text, out LocalVersionMinor)
				|| (LocalVersionMinor < 0)
				)
			{
				VersionMinor = int.MinValue;
				string TextHolder = VersionMinorEntryName.Text;
				VersionMinorEntryName.Text = "";
				_ = VS.MessageBox.ShowAsync("Invalid \"Version Minor\" ", TextHolder, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
				VersionMinorEntryName.Focus();
			}
			else
			{
				VersionMinor = LocalVersionMinor;
			}
		}

		private void OnBuildNumberEntryCompleted(object sender, System.Windows.RoutedEventArgs e)
		{
			int LocalBuildNumber = 0;
			if ((BuildNumberEntryName.Text != null)
				&& (BuildNumberEntryName.Text.Length != 0)
				&& !int.TryParse(BuildNumberEntryName.Text, out LocalBuildNumber)
				|| (LocalBuildNumber < 0)
				)
			{
				BuildNumber = int.MinValue;
				string TextHolder = BuildNumberEntryName.Text;
				BuildNumberEntryName.Text = "";
				_ = VS.MessageBox.ShowAsync("Invalid \"Build Number\" ", TextHolder, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
				BuildNumberEntryName.Focus();
			}
			else
			{
				BuildNumber = LocalBuildNumber;
			}
		}

		private void OnRevisionNumberEntryCompleted(object sender, System.Windows.RoutedEventArgs e)
		{
			int LocalRevisionNumber = 0;
			if ((RevisionNumberEntryName.Text != null)
				&& (RevisionNumberEntryName.Text.Length != 0)
				&& !int.TryParse(RevisionNumberEntryName.Text, out LocalRevisionNumber)
				|| (LocalRevisionNumber < 0)
				)
			{
				RevisionNumber = int.MinValue;
				string TextHolder = RevisionNumberEntryName.Text;
				RevisionNumberEntryName.Text = "";
				_ = VS.MessageBox.ShowAsync("Invalid \"Revision Number\" ", TextHolder, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
				RevisionNumberEntryName.Focus();
			}
			else
			{
				RevisionNumber = LocalRevisionNumber;
			}
		}

		public XDocument ReadFromXmlFile(string XmlFilePathAndName)
		{

			try
			{
				XmlReaderSettings settings = new XmlReaderSettings
				{
					Async = true
				};
				settings.DtdProcessing = DtdProcessing.Parse;
				//settings.ValidationType = ValidationType.DTD;
				//settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);
				using (XmlReader reader = XmlReader.Create(XmlFilePathAndName, settings))
				{
					XDocument TheLoadedXDocument = XDocument.Load(reader);
					return TheLoadedXDocument;
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("The ReadFromXmlFile failed: {0}", e.ToString());
				return null;
			}
			finally
			{
			}
		}

		public bool WriteToXmlFile(string XmlFilePathAndFileName, XDocument TheXDocument)
		{
			try
			{
				XmlWriterSettings settings = new XmlWriterSettings
				{
					//Async = true
					//,
					Indent = true
					,
					WriteEndDocumentOnClose = false
				};
				using (XmlWriter writer = XmlWriter.Create(XmlFilePathAndFileName, settings))
				{
					TheXDocument.WriteTo(writer);
					//TheXDocument.Save(writer);
					writer.Flush();
					//await writer.FlushAsync( );
					writer.Close();
				}
				return true;
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("SaveToTextFile failed: {0}", e.ToString());
				return false;
			}
			finally
			{
			}
		}


		private void OnRefreshButtonClicked(object sender, System.Windows.RoutedEventArgs e)
		{
			VersionMajor = int.MinValue;
			VersionMajorEntryName.Text = "";
			VersionMinor = int.MinValue;
			VersionMinorEntryName.Text = "";
			BuildNumber = int.MinValue;
			BuildNumberEntryName.Text = "";
			RevisionNumber = int.MinValue;
			RevisionNumberEntryName.Text = "";

			PathToSolutionFolder = "";

			SetNumbersButton.IsEnabled = false;

			_ = GetShowPathsToAndroidiOSmacOSUWPAsync();
		}


		private bool SetAndroidVersionNumbers()
		{
			return false;
			//	//string PathToAndNameOfTheFile = PathToAndNameOfAndroidManifestFile;

			//	bool RetVal = true;

			//	if (PathToAndNameOfAndroidManifestFile != "")
			//	{
			//		try
			//		{
			//			int LastIndexOfFolderSeperator = PathToAndNameOfTheFile.LastIndexOf(Path.DirectorySeparatorChar);
			//			string PathToTheFile = PathToAndNameOfTheFile.Substring(0, LastIndexOfFolderSeperator);
			//			string NameOfTheFile = PathToAndNameOfTheFile.Substring(LastIndexOfFolderSeperator + 1);

			//			if (System.IO.File.Exists(PathToAndNameOfTheFile))
			//			{
			//				// Read the version numbers for use next time
			//				XDocument TheXDocument =
			//					ReadFromXmlFile(PathToAndNameOfTheFile);

			//				if (TheXDocument != null)
			//				{
			//					bool versionCodeFound = false;
			//					bool versionNameFound = false;

			//					XElement element = TheXDocument.Descendants("manifest").Single();

			//					IEnumerable<XAttribute> XAttributesList = TheXDocument.Descendants("manifest").Single().Attributes();

			//					using (IEnumerator<XAttribute> sequenceEnum = XAttributesList.GetEnumerator())
			//					{
			//						while (sequenceEnum.MoveNext())
			//						{
			//							// Do something with sequenceEnum.Current.
			//							switch (sequenceEnum.Current.Name.LocalName)
			//							{
			//								case "versionCode":
			//									{
			//										sequenceEnum.Current.Value = BuildNumber.ToString();
			//										versionCodeFound = true;
			//										break;
			//									}
			//								case "versionName":
			//									{
			//										sequenceEnum.Current.Value = VersionMajor.ToString() + '.' + VersionMinor.ToString();
			//										versionNameFound = true;
			//										break;
			//									}
			//							}
			//						}
			//					}
			//					if (versionCodeFound && versionNameFound)
			//					{
			//						if (!WriteToXmlFile(PathToAndNameOfTheFile, TheXDocument))
			//						{
			//							RetVal = false;
			//							_ = VS.MessageBox.ShowAsync("Error writing to", PathToAndNameOfTheFile, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			//						}
			//					}
			//					else
			//					{
			//						RetVal = false;
			//						_ = VS.MessageBox.ShowAsync("Could not find \"versionCode\" and/or \"versionName\" in file ", PathToAndNameOfTheFile, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			//					}
			//				}
			//				else
			//				{
			//					RetVal = false;
			//					_ = VS.MessageBox.ShowAsync("Invalid \".xml\" file", PathToAndNameOfTheFile, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			//				}
			//			}
			//			else
			//			{
			//				RetVal = false;
			//				_ = VS.MessageBox.ShowAsync("File not found: ", PathToAndNameOfTheFile, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			//			}
			//		}
			//		catch (Exception e)
			//		{
			//			System.Diagnostics.Debug.WriteLine("String processing failed: {0}", e.ToString());
			//			RetVal = false;
			//			_ = VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			//		}
			//		finally { }
			//	}

			//	return RetVal;
		}


		private bool SetiOSVersionNumbers()
		{
			return false;
		}
		//{
		//	bool RetVal = true;

		//	//string PathToAndNameOfTheFile = PathToAndNameOfiOSInfoPlist;

		//	if (PathToAndNameOfTheFile != "")
		//	{
		//		try
		//		{
		//			int LastIndexOfFolderSeperator = PathToAndNameOfTheFile.LastIndexOf(Path.DirectorySeparatorChar);
		//			string PathToTheFile = PathToAndNameOfTheFile.Substring(0, LastIndexOfFolderSeperator);
		//			string NameOfTheFile = PathToAndNameOfTheFile.Substring(LastIndexOfFolderSeperator + 1);

		//			if (System.IO.File.Exists(PathToAndNameOfTheFile))
		//			{
		//				// Read the version numbers for use next time
		//				XDocument TheXDocument =
		//					ReadFromXmlFile(PathToAndNameOfTheFile);
		//				if (TheXDocument != null)
		//				{
		//					IEnumerable<XElement> keyValues = TheXDocument.Descendants("dict");

		//					// Check if CFBundleVersion exists
		//					IEnumerable<XElement> CFBundleVersion_keyValue = keyValues
		//								 .SelectMany(d => d.Elements("key")
		//										.Where(e => e.Value == "CFBundleVersion"));
		//					// Check if CFBundleShortVersionString exists
		//					IEnumerable<XElement> CFBundleShortVersionString_keyValue = keyValues
		//								 .SelectMany(d => d.Elements("key")
		//										.Where(e => e.Value == "CFBundleShortVersionString"));
		//					// Add missing elements
		//					XElement dictEntry = keyValues.FirstOrDefault();
		//					if ((!CFBundleVersion_keyValue.Any() || !CFBundleShortVersionString_keyValue.Any()) && (dictEntry != null))
		//					{

		//						if (!CFBundleVersion_keyValue.Any())
		//						{
		//							// Add CFBundleVersion
		//							dictEntry.Add(new XElement("key", "CFBundleVersion"));
		//							dictEntry.Add(new XElement("string", "1.1"));
		//						}

		//						if (!CFBundleShortVersionString_keyValue.Any())
		//						{
		//							// Add CFBundleShortVersionString
		//							dictEntry.Add(new XElement("key", "CFBundleShortVersionString"));
		//							dictEntry.Add(new XElement("string", "1.1"));
		//						}

		//						_ = WriteToXmlFile(PathToAndNameOfTheFile, TheXDocument);
		//					}

		//					string thePlist = System.IO.File.ReadAllText(PathToAndNameOfTheFile);
		//					if (thePlist != "Error")
		//					{
		//						// CFBundleVersion
		//						int indexOfVersionKey = thePlist.IndexOf("CFBundleVersion");
		//						int indexOfVersion = thePlist.IndexOf("string>", indexOfVersionKey) + 7;
		//						int indexOfVersionEnd = thePlist.IndexOf("</", indexOfVersion);
		//						if ((indexOfVersionKey >= 0) && (indexOfVersion >= 0) && (indexOfVersionEnd >= 0))
		//						{
		//							int VersionLength = indexOfVersionEnd - indexOfVersion;
		//							string newVersion = BuildNumber.ToString() + '.' + RevisionNumber.ToString();
		//							thePlist = thePlist.Remove(indexOfVersion, VersionLength).Insert(indexOfVersion, newVersion);

		//							// CFBundleShortVersionString
		//							if ((indexOfVersionKey = thePlist.IndexOf("CFBundleShortVersionString")) < 0)
		//							{
		//							}
		//							indexOfVersionKey = thePlist.IndexOf("CFBundleShortVersionString");
		//							indexOfVersion = thePlist.IndexOf("string>", indexOfVersionKey) + 7;
		//							indexOfVersionEnd = thePlist.IndexOf("</", indexOfVersion);
		//							if ((indexOfVersionKey >= 0) && (indexOfVersion >= 0) && (indexOfVersionEnd >= 0))
		//							{
		//								VersionLength = indexOfVersionEnd - indexOfVersion;
		//								newVersion = VersionMajor.ToString() + '.' + VersionMinor.ToString();
		//								thePlist = thePlist.Remove(indexOfVersion, VersionLength).Insert(indexOfVersion, newVersion);

		//								System.IO.File.WriteAllText(PathToAndNameOfTheFile, thePlist);
		//							}
		//							else
		//							{
		//								RetVal = false;
		//								_ = VS.MessageBox.ShowAsync("Error finding \"CFBundleShortVersionString\" in file", PathToAndNameOfTheFile, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
		//							}
		//						}
		//						else
		//						{
		//							RetVal = false;
		//							_ = VS.MessageBox.ShowAsync("Error finding \"CFBundleVersion\" in file", PathToAndNameOfTheFile, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
		//						}
		//					}
		//				}
		//				else
		//				{
		//					RetVal = false;
		//					_ = VS.MessageBox.ShowAsync("Error reading file", PathToAndNameOfTheFile, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
		//				}
		//			}
		//		}
		//		catch (Exception e)
		//		{
		//			System.Diagnostics.Debug.WriteLine("String processing failed: {0}", e.ToString());
		//			RetVal = false;
		//			_ = VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
		//		}
		//		finally { }
		//	}

		//	return RetVal;
		//}

		private bool SetmacOSVersionNumbers()
		{
			return false;
		}
		//{
		//	bool RetVal = true;

		//	string PathToAndNameOfTheFile = PathToAndNameOfmacOSInfoPlist;

		//	if (PathToAndNameOfTheFile != "")
		//	{
		//		try
		//		{
		//			int LastIndexOfFolderSeperator = PathToAndNameOfTheFile.LastIndexOf(Path.DirectorySeparatorChar);
		//			string PathToTheFile = PathToAndNameOfTheFile.Substring(0, LastIndexOfFolderSeperator);
		//			string NameOfTheFile = PathToAndNameOfTheFile.Substring(LastIndexOfFolderSeperator + 1);

		//			if (System.IO.File.Exists(PathToAndNameOfTheFile))
		//			{
		//				string thePlist = System.IO.File.ReadAllText(PathToAndNameOfTheFile);
		//				if (thePlist != "Error")
		//				{
		//					// CFBundleVersion
		//					int indexOfVersionKey = thePlist.IndexOf("CFBundleVersion");
		//					int indexOfVersion = thePlist.IndexOf("string>", indexOfVersionKey) + 7;
		//					int indexOfVersionEnd = thePlist.IndexOf("</", indexOfVersion);
		//					if ((indexOfVersionKey >= 0) && (indexOfVersion >= 0) && (indexOfVersionEnd >= 0))
		//					{
		//						int VersionLength = indexOfVersionEnd - indexOfVersion;
		//						string newVersion = BuildNumber.ToString() + '.' + RevisionNumber.ToString();
		//						thePlist = thePlist.Remove(indexOfVersion, VersionLength).Insert(indexOfVersion, newVersion);

		//						// CFBundleShortVersionString
		//						indexOfVersionKey = thePlist.IndexOf("CFBundleShortVersionString");
		//						indexOfVersion = thePlist.IndexOf("string>", indexOfVersionKey) + 7;
		//						indexOfVersionEnd = thePlist.IndexOf("</", indexOfVersion);
		//						if ((indexOfVersionKey >= 0) && (indexOfVersion >= 0) && (indexOfVersionEnd >= 0))
		//						{
		//							VersionLength = indexOfVersionEnd - indexOfVersion;
		//							newVersion = VersionMajor.ToString() + '.' + VersionMinor.ToString();
		//							thePlist = thePlist.Remove(indexOfVersion, VersionLength).Insert(indexOfVersion, newVersion);

		//							System.IO.File.WriteAllText(PathToAndNameOfTheFile, thePlist);
		//						}
		//						else
		//						{
		//							RetVal = false;
		//							_ = VS.MessageBox.ShowAsync("Error finding \"CFBundleShortVersionString\" in file", PathToAndNameOfTheFile, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
		//						}
		//					}
		//					else
		//					{
		//						RetVal = false;
		//						_ = VS.MessageBox.ShowAsync("Error finding \"CFBundleVersion\" in file", PathToAndNameOfTheFile, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
		//					}
		//				}
		//				else
		//				{
		//					RetVal = false;
		//					_ = VS.MessageBox.ShowAsync("Error reading file", PathToAndNameOfTheFile, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
		//				}
		//			}
		//		}
		//		catch (Exception e)
		//		{
		//			System.Diagnostics.Debug.WriteLine("String processing failed: {0}", e.ToString());
		//			RetVal = false;
		//			_ = VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
		//		}
		//		finally { }
		//	}

		//	return RetVal;
		//}

		private bool SetUWPVersionNumbers()
		{
			return false;
		}
		//{
		//	bool RetVal = true;

		//	string PathToAndNameOfTheFile = PathToAndNameOfUWPPackageAppxmanifest;

		//	if (PathToAndNameOfTheFile != "")
		//	{
		//		try
		//		{
		//			int LastIndexOfFolderSeperator = PathToAndNameOfTheFile.LastIndexOf(Path.DirectorySeparatorChar);
		//			string PathToTheFile = PathToAndNameOfTheFile.Substring(0, LastIndexOfFolderSeperator);
		//			string NameOfTheFile = PathToAndNameOfTheFile.Substring(LastIndexOfFolderSeperator + 1);

		//			if (System.IO.File.Exists(PathToAndNameOfTheFile))
		//			{
		//				string theAppxmanifest = System.IO.File.ReadAllText(PathToAndNameOfTheFile);
		//				if (theAppxmanifest != "Error")
		//				{
		//					// Version
		//					int indexOfPublisherAttribute = theAppxmanifest.IndexOf("Publisher");
		//					int indexOfVersionAttribute = theAppxmanifest.IndexOf("Version", indexOfPublisherAttribute);
		//					int indexOfVersion = theAppxmanifest.IndexOf('"', indexOfVersionAttribute) + 1;
		//					int indexOfVersionEnd = theAppxmanifest.IndexOf('"', indexOfVersion);
		//					if ((indexOfPublisherAttribute >= 0) && (indexOfVersionAttribute >= 0) && (indexOfVersion >= 0) && (indexOfVersionEnd >= 0))
		//					{
		//						int VersionLength = indexOfVersionEnd - indexOfVersion;
		//						string newVersion = VersionMajor.ToString() + '.' + VersionMinor.ToString() + '.' + BuildNumber.ToString() + ".0"; // Revision must be 0 (used be Microsoft store)
		//						theAppxmanifest = theAppxmanifest.Remove(indexOfVersion, VersionLength).Insert(indexOfVersion, newVersion);

		//						// Publisher
		//						int indexOfPublisher = theAppxmanifest.IndexOf('"', indexOfPublisherAttribute) + 1;
		//						int indexOfPublisherEnd = theAppxmanifest.IndexOf('"', indexOfPublisher);
		//						if ((indexOfPublisher >= 0) && (indexOfPublisherEnd >= 0))
		//						{
		//							int PublisherLength = indexOfPublisherEnd - indexOfPublisher;
		//							theAppxmanifest = theAppxmanifest.Remove(indexOfPublisher, PublisherLength).Insert(indexOfPublisher, "CN=Eigil Krogh Sorensen");

		//							System.IO.File.WriteAllText(PathToAndNameOfTheFile, theAppxmanifest);
		//						}
		//						else
		//						{
		//							RetVal = false;
		//							_ = VS.MessageBox.ShowAsync("Error finding \"Index Of Publisher\" in file", PathToAndNameOfTheFile, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
		//						}
		//					}
		//					else
		//					{
		//						RetVal = false;
		//						_ = VS.MessageBox.ShowAsync("Error finding \"Index Of Version\" in file", PathToAndNameOfTheFile, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
		//					}
		//				}
		//				else
		//				{
		//					RetVal = false;
		//					_ = VS.MessageBox.ShowAsync("Error reading file", PathToAndNameOfTheFile, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
		//				}
		//			}
		//		}
		//		catch (Exception e)
		//		{
		//			System.Diagnostics.Debug.WriteLine("String processing failed: {0}", e.ToString());
		//			RetVal = false;
		//			_ = VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
		//		}
		//		finally { }
		//	}

		//	return RetVal;
		//}

		private void OnSetNumbersButtonClicked(object sender, System.Windows.RoutedEventArgs e)
		{
			_ = OnSetNumbersButtonClickedAsync();
		}
		private async Task OnSetNumbersButtonClickedAsync(/*object sender, System.Windows.RoutedEventArgs e*/)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

			if ((VersionMajorEntryName.Text == null)
				|| (VersionMajorEntryName.Text.Length == 0)
				|| !int.TryParse(VersionMajorEntryName.Text, out int LocalVersionMajor)
				|| (LocalVersionMajor < 0)
				)
			{
				VersionMajor = int.MinValue;
				string TextHolder = VersionMajorEntryName.Text;
				VersionMajorEntryName.Text = "";
				_ = VS.MessageBox.ShowAsync("Invalid \"Version Major\" ", TextHolder, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
				VersionMajorEntryName.Focus();
				return;
			}
			else
			{
				VersionMajor = LocalVersionMajor;
			}

			if ((VersionMinorEntryName.Text == null)
				|| (VersionMinorEntryName.Text.Length == 0)
				|| !int.TryParse(VersionMinorEntryName.Text, out int LocalVersionMinor)
				|| (LocalVersionMinor < 0)
				)
			{
				VersionMinor = int.MinValue;
				string TextHolder = VersionMinorEntryName.Text;
				VersionMinorEntryName.Text = "";
				_ = VS.MessageBox.ShowAsync("Invalid \"Version Minor\" ", TextHolder, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
				VersionMinorEntryName.Focus();
				return;
			}
			else
			{
				VersionMinor = LocalVersionMinor;
			}

			if ((BuildNumberEntryName.Text == null)
				|| (BuildNumberEntryName.Text.Length == 0)
				|| !int.TryParse(BuildNumberEntryName.Text, out int LocalBuildNumber)
				|| (LocalBuildNumber < 0)
				)
			{
				BuildNumber = int.MinValue;
				string TextHolder = BuildNumberEntryName.Text;
				BuildNumberEntryName.Text = "";
				_ = VS.MessageBox.ShowAsync("Invalid \"Build Number\" ", TextHolder, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
				BuildNumberEntryName.Focus();
				return;
			}
			else
			{
				BuildNumber = LocalBuildNumber;
			}

			if ((RevisionNumberEntryName.Text == null)
				|| (RevisionNumberEntryName.Text.Length == 0)
				|| !int.TryParse(RevisionNumberEntryName.Text, out int LocalRevisionNumber)
				|| (LocalRevisionNumber < 0)
				)
			{
				RevisionNumber = int.MinValue;
				string TextHolder = RevisionNumberEntryName.Text;
				RevisionNumberEntryName.Text = "";
				_ = VS.MessageBox.ShowAsync("Invalid \"Revision Number\" ", TextHolder, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
				RevisionNumberEntryName.Focus();
				return;
			}
			else
			{
				RevisionNumber = LocalRevisionNumber;
			}


			//if ((PathToSolutionFolderEntry.Text == null)
			//	|| (PathToSolutionFolderEntry.Text.Length == 0)
			//	|| !(Directory.Exists(PathToSolutionFolderEntry.Text))
			//	)
			//{
			//	string TextHolder = PathToSolutionFolderEntry.Text;
			//	PathToSolutionFolderEntry.Text = "";
			//	_ = VS.MessageBox.ShowAsync("Invalid \"Path to Projects Folder\" ", TextHolder, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			//	PathToSolutionFolderEntry.Focus();
			//	return;
			//}

			// Save the version numbers for use next time
			XElement MAJORMINORBUILDNUMBERS = new XElement("MAJORMINORBUILDNUMBERS"
											, new XAttribute("VersionMajor", VersionMajor)
											, new XAttribute("VersionMinor", VersionMinor)
											, new XAttribute("BuildNumber", BuildNumber)
											, new XAttribute("RevisionNumber", RevisionNumber)
										);
			XDocument TheXDocument = new XDocument
				(
					new XDeclaration("1.0", "utf-8", "yes")
				);
			TheXDocument.Add(MAJORMINORBUILDNUMBERS);

			if (MajorMinorBuildRevisionNumbersXmlFileExistedAtStart)
			{
				DTE2 dte = await VS.GetServiceAsync<DTE, DTE2>();

				dte.SourceControl.CheckOutItem(PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile);
			}

			if (!WriteToXmlFile(PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile, TheXDocument))
			{
				_ = VS.MessageBox.ShowAsync("Error writing to", PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			}

			if (!MajorMinorBuildRevisionNumbersXmlFileExistedAtStart)
			{
				Community.VisualStudio.Toolkit.SolutionFolder SolutionFolderMajorMinorBuildRevisionNumbersXmlFile =
					await TheSolution.AddSolutionFolderAsync("MajorMinorBuildRevisionNumbersXmlFile").ConfigureAwait(true);

				System.Threading.Tasks.Task<IEnumerable<PhysicalFile>> refMajorMinorBuildRevisionNumbersXmlFile = SolutionFolderMajorMinorBuildRevisionNumbersXmlFile.AddExistingFilesAsync(PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile);
			}


			if (
				   SetAndroidVersionNumbers()
				&& SetiOSVersionNumbers()
				&& SetmacOSVersionNumbers()
				&& SetUWPVersionNumbers()
				)
			{
				_ = VS.MessageBox.ShowAsync("Done!", "", OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			}


			//if (!MajorMinorBuildRevisionNumbersXmlFileExistedAtStart)
			//{
			//	_ = VS.MessageBox.ShowAsync("Remember to add to Source Control ", PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile, OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			//}

		}

	}
}