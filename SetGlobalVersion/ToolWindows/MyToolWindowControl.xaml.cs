using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml;
using System.Xml.Linq;
using static SetGlobalVersion.Helpers.CheckSolutionItems;

namespace SetGlobalVersion
{
	public partial class MyToolWindowControl : UserControl
	{
		private bool WasInvisible { get; set; } = false;
		Style DataGridTextColumnElementStyle;
		public MyToolWindowControl()
		{
			InitializeComponent();

			DataGridTextColumnElementStyle = new(typeof(TextBlock));
			DataGridTextColumnElementStyle.Setters.Add(new Setter(TextBlock.TextWrappingProperty, TextWrapping.Wrap));

			solCol0.Binding = new Binding("ThisSolutionName");
			solCol1.Binding = new Binding("ThisSolutionPath");
			solCol0.Header = "Solution";
			solCol1.Header = "Path";
			solCol1.ElementStyle = DataGridTextColumnElementStyle;

			projCol0.Binding = new Binding("ThisSolutionProject");
			projCol1.Binding = new Binding("ThisSolutionProjectPath");
			projCol0.Header = "Project";
			projCol1.Header = "File containing version";
			projCol1.ElementStyle = DataGridTextColumnElementStyle;

			_ = GetShowPathsToVersionContainingFilesAsync();

			IsVisibleChanged += SetVersionNumberControl_IsVisibleChanged;

			VS.Events.SolutionEvents.OnAfterCloseSolution += SolutionEvents_OnAfterCloseSolution;
			VS.Events.SolutionEvents.OnAfterBackgroundSolutionLoadComplete +=
				SolutionEvents_OnAfterBackgroundSolutionLoadComplete;


		}

		private void SolutionEvents_OnAfterBackgroundSolutionLoadComplete()
		{
			_ = GetShowPathsToVersionContainingFilesAsync();

			WasInvisible = false;
			Visibility = Visibility.Visible;
		}

		private void SolutionEvents_OnAfterCloseSolution()
		{
			SetNumbersButton.IsEnabled = false;

			WasInvisible = true;
			Visibility = Visibility.Hidden;

			CleanUp();

		}


		private void SetVersionNumberControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (!(bool)e.NewValue && (bool)e.OldValue)
			{
				WasInvisible = true;
			}
			else
			{
				if (WasInvisible && ((bool)e.NewValue))
				{
					Visibility = Visibility.Visible;

					_ = GetShowPathsToVersionContainingFilesAsync();
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
			public string ThisSolutionName { set; get; }
			public string ThisSolutionPath { set; get; }
		}

		// Project
		public struct MyProjectsData
		{
			public string ThisSolutionProject { set; get; }
			public string ThisSolutionProjectPath { set; get; }
		}

		DataGridTextColumn solCol0 = new();
		DataGridTextColumn solCol1 = new();
		DataGridTextColumn projCol0 = new();
		DataGridTextColumn projCol1 = new();

		private async Task GetShowPathsToVersionContainingFilesAsync()
		{
			try
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

				CleanUp();


				if (await GetVersionContainingFilesInSolutionAsync())
				{

					// Show solution and its path
					mySolutionDataGrid.Columns.Add(solCol0);
					mySolutionDataGrid.Columns.Add(solCol1);

					mySolutionDataGrid.Items.Add(new MySolutionData { ThisSolutionName = TheSolution.Name, ThisSolutionPath = TheSolution.FullPath });


					// Show paths to files containing version
					myProjectsDataGrid.Columns.Add(projCol0);
					myProjectsDataGrid.Columns.Add(projCol1);

					// Show the search result
					foreach (KeyValuePair<string, List<VersionFilePathAndType>> VFPT in ProjsWithVersionFiles)
					{
						foreach (VersionFilePathAndType FPAN in VFPT.Value)
						{
							myProjectsDataGrid.Items.Add(new MyProjectsData { ThisSolutionProject = VFPT.Key, ThisSolutionProjectPath = FPAN.FilePathAndName });
						}
					}

					// MajorMinorBuildRevisionNumbersXmlFile
					if (MajorMinorBuildRevisionNumbersXmlFileExistedAtStart && !MajorMinorBuildRevisionNumbersXmlFileJustCreated)
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

					// Now version can be set
					SetNumbersButton.IsEnabled = true;
				}
				else
				{
					if (TheSolution != null)
					{
						// Show "error"  message

						mySolutionDataGrid.Columns.Clear();
						mySolutionDataGrid.Columns.Add(solCol0);
						mySolutionDataGrid.Columns.Add(solCol1);

						mySolutionDataGrid.Items.Add(new MySolutionData { ThisSolutionName = "Not Supported.", ThisSolutionPath = "" });
					}
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("String processing failed: {0}", e.ToString());
				_ = VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			}
			finally { }
		}

		private void CleanUp()
		{
			// Clear 
			VersionMajor = int.MinValue;
			VersionMajorEntryName.Text = "";
			VersionMinor = int.MinValue;
			VersionMinorEntryName.Text = "";
			BuildNumber = int.MinValue;
			BuildNumberEntryName.Text = "";
			RevisionNumber = int.MinValue;
			RevisionNumberEntryName.Text = "";

			SetNumbersButton.IsEnabled = false;

			mySolutionDataGrid.Items.Clear();
			myProjectsDataGrid.Items.Clear();
			mySolutionDataGrid.Columns.Clear();
			myProjectsDataGrid.Columns.Clear();
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
				XmlReaderSettings settings = new()
				{
					Async = true
				};
				settings.DtdProcessing = DtdProcessing.Parse;

				using XmlReader reader = XmlReader.Create(XmlFilePathAndName, settings);
				XDocument TheLoadedXDocument = XDocument.Load(reader);
				return TheLoadedXDocument;
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
				XmlWriterSettings settings = new()
				{
					Async = true
					,
					Indent = true
					,
					WriteEndDocumentOnClose = false
				};
				using (XmlWriter writer = XmlWriter.Create(XmlFilePathAndFileName, settings))
				{
					TheXDocument.WriteTo(writer);
					writer.Flush();
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
			CleanUp();

			InitializeComponent();

			_ = GetShowPathsToVersionContainingFilesAsync();
		}


		private bool SetVersionNumbersInManifestXmlFiles(VersionFilePathAndType verFile)
		{
			bool RetVal = true;

			try
			{
				if (System.IO.File.Exists(verFile.FilePathAndName))
				{
					// Read the version numbers for use next time
					XDocument TheXDocument =
						ReadFromXmlFile(verFile.FilePathAndName);

					if (TheXDocument != null)
					{
						bool versionCodeFound = false;
						bool versionNameFound = false;

						XElement element = TheXDocument.Descendants("manifest").Single();

						IEnumerable<XAttribute> XAttributesList = TheXDocument.Descendants("manifest").Single().Attributes();

						using (IEnumerator<XAttribute> sequenceEnum = XAttributesList.GetEnumerator())
						{
							while (sequenceEnum.MoveNext())
							{
								// Do something with sequenceEnum.Current.
								switch (sequenceEnum.Current.Name.LocalName)
								{
									case "versionCode":
										{
											sequenceEnum.Current.Value = BuildNumber.ToString();
											versionCodeFound = true;
											break;
										}
									case "versionName":
										{
											sequenceEnum.Current.Value = VersionMajor.ToString() + '.' + VersionMinor.ToString();
											versionNameFound = true;
											break;
										}
								}
							}
						}
						if (versionCodeFound && versionNameFound)
						{
							if (!WriteToXmlFile(verFile.FilePathAndName, TheXDocument))
							{
								RetVal = false;
								_ = VS.MessageBox.ShowAsync("Error writing to", verFile.FilePathAndName, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
							}
						}
						else
						{
							RetVal = false;
							_ = VS.MessageBox.ShowAsync("Could not find \"versionCode\" and/or \"versionName\" in file ", verFile.FilePathAndName, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
						}
					}
					else
					{
						RetVal = false;
						_ = VS.MessageBox.ShowAsync("Invalid \".xml\" file", verFile.FilePathAndName, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
					}
				}
				else
				{
					RetVal = false;
					_ = VS.MessageBox.ShowAsync("File not found: ", verFile.FilePathAndName, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("String processing failed: {0}", e.ToString());
				RetVal = false;
				_ = VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			}
			finally { }

			return RetVal;
		}


		private bool SetVersionNumbersInInfoplistFiles(VersionFilePathAndType verFile)
		{
			bool RetVal = true;

			try
			{

				// Read the version numbers for use next time
				XDocument TheXDocument =
					ReadFromXmlFile(verFile.FilePathAndName);

				if (TheXDocument != null)
				{
					IEnumerable<XElement> keyValues = TheXDocument.Descendants("dict");

					// Check if CFBundleVersion exists
					IEnumerable<XElement> CFBundleVersion_keyValue = keyValues
								 .SelectMany(d => d.Elements("key")
										.Where(e => e.Value == "CFBundleVersion"));
					// Check if CFBundleShortVersionString exists
					IEnumerable<XElement> CFBundleShortVersionString_keyValue = keyValues
							 .SelectMany(d => d.Elements("key")
									.Where(e => e.Value == "CFBundleShortVersionString"));
					// Add missing elements
					XElement dictEntry = keyValues.FirstOrDefault();
					if ((!CFBundleVersion_keyValue.Any() || !CFBundleShortVersionString_keyValue.Any()) && (dictEntry != null))
					{

						if (!CFBundleVersion_keyValue.Any())
						{
							// Add CFBundleVersion
							dictEntry.Add(new XElement("key", "CFBundleVersion"));
							dictEntry.Add(new XElement("string", "1.1"));
						}

						if (!CFBundleShortVersionString_keyValue.Any())
						{
							// Add CFBundleShortVersionString
							dictEntry.Add(new XElement("key", "CFBundleShortVersionString"));
							dictEntry.Add(new XElement("string", "1.1"));
						}

						_ = WriteToXmlFile(verFile.FilePathAndName, TheXDocument);
					}

					string thePlist = System.IO.File.ReadAllText(verFile.FilePathAndName);
					if (thePlist != "Error")
					{
						// CFBundleVersion
						int indexOfVersionKey = thePlist.IndexOf("CFBundleVersion");
						int indexOfVersion = thePlist.IndexOf("string>", indexOfVersionKey) + 7;
						int indexOfVersionEnd = thePlist.IndexOf("</", indexOfVersion);
						if ((indexOfVersionKey >= 0) && (indexOfVersion >= 0) && (indexOfVersionEnd >= 0))
						{
							int VersionLength = indexOfVersionEnd - indexOfVersion;
							string newVersion = BuildNumber.ToString() + '.' + RevisionNumber.ToString();
							thePlist = thePlist.Remove(indexOfVersion, VersionLength).Insert(indexOfVersion, newVersion);

							// CFBundleShortVersionString
							if ((indexOfVersionKey = thePlist.IndexOf("CFBundleShortVersionString")) < 0)
							{
							}
							indexOfVersionKey = thePlist.IndexOf("CFBundleShortVersionString");
							indexOfVersion = thePlist.IndexOf("string>", indexOfVersionKey) + 7;
							indexOfVersionEnd = thePlist.IndexOf("</", indexOfVersion);
							if ((indexOfVersionKey >= 0) && (indexOfVersion >= 0) && (indexOfVersionEnd >= 0))
							{
								VersionLength = indexOfVersionEnd - indexOfVersion;
								newVersion = VersionMajor.ToString() + '.' + VersionMinor.ToString();
								thePlist = thePlist.Remove(indexOfVersion, VersionLength).Insert(indexOfVersion, newVersion);

								System.IO.File.WriteAllText(verFile.FilePathAndName, thePlist);
							}
							else
							{
								RetVal = false;
								_ = VS.MessageBox.ShowAsync("Error finding \"CFBundleShortVersionString\" in file", verFile.FilePathAndName, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
							}
						}
						else
						{
							RetVal = false;
							_ = VS.MessageBox.ShowAsync("Error finding \"CFBundleVersion\" in file", verFile.FilePathAndName, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
						}
					}
				}
				else
				{
					RetVal = false;
					_ = VS.MessageBox.ShowAsync("Error reading file", verFile.FilePathAndName, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("String processing failed: {0}", e.ToString());
				RetVal = false;
				_ = VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			}
			finally { }

			return RetVal;
		}


		private bool SetVersionNumbersInAppxmanifestFiles(VersionFilePathAndType verFile)
		{
			bool RetVal = true;

			try
			{
				if (System.IO.File.Exists(verFile.FilePathAndName))
				{
					string theAppxmanifest = System.IO.File.ReadAllText(verFile.FilePathAndName);
					if (theAppxmanifest != "Error")
					{
						// Version
						int indexOfPublisherAttribute = theAppxmanifest.IndexOf("Publisher");
						int indexOfVersionAttribute = theAppxmanifest.IndexOf("Version", indexOfPublisherAttribute);
						int indexOfVersion = theAppxmanifest.IndexOf('"', indexOfVersionAttribute) + 1;
						int indexOfVersionEnd = theAppxmanifest.IndexOf('"', indexOfVersion);
						if ((indexOfPublisherAttribute >= 0) && (indexOfVersionAttribute >= 0) && (indexOfVersion >= 0) && (indexOfVersionEnd >= 0))
						{
							int VersionLength = indexOfVersionEnd - indexOfVersion;
							string newVersion = VersionMajor.ToString() + '.' + VersionMinor.ToString() + '.' + BuildNumber.ToString() + ".0"; // Revision must be 0 (used be Microsoft store)
							theAppxmanifest = theAppxmanifest.Remove(indexOfVersion, VersionLength).Insert(indexOfVersion, newVersion);

							// Publisher
							int indexOfPublisher = theAppxmanifest.IndexOf('"', indexOfPublisherAttribute) + 1;
							int indexOfPublisherEnd = theAppxmanifest.IndexOf('"', indexOfPublisher);
							if ((indexOfPublisher >= 0) && (indexOfPublisherEnd >= 0))
							{
								int PublisherLength = indexOfPublisherEnd - indexOfPublisher;
								theAppxmanifest = theAppxmanifest.Remove(indexOfPublisher, PublisherLength).Insert(indexOfPublisher, "CN=Eigil Krogh Sorensen");

								System.IO.File.WriteAllText(verFile.FilePathAndName, theAppxmanifest);
							}
							else
							{
								RetVal = false;
								_ = VS.MessageBox.ShowAsync("Error finding \"Index Of Publisher\" in file", verFile.FilePathAndName, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
							}
						}
						else
						{
							RetVal = false;
							_ = VS.MessageBox.ShowAsync("Error finding \"Index Of Version\" in file", verFile.FilePathAndName, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
						}
					}
					else
					{
						RetVal = false;
						_ = VS.MessageBox.ShowAsync("Error reading file", verFile.FilePathAndName, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
					}
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("String processing failed: {0}", e.ToString());
				RetVal = false;
				_ = VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			}
			finally { }

			return RetVal;
		}

		private bool SetVersionNumbersInAssemblyinfo_cs_Files(VersionFilePathAndType verFile)
		{
			bool RetVal = true;

			try
			{
				var FileLines = File.ReadAllLines(verFile.FilePathAndName);

				for (int i = 0; i < FileLines.Length; i++)
				{
					if (FileLines[i].Contains("AssemblyVersion") && !FileLines[i].TrimStart(' ').StartsWith("//"))
					{
						int fst = FileLines[i].IndexOf("(");
						int lst = FileLines[i].IndexOf(")", fst);

						string OldVNbr = FileLines[i].Substring(fst + 1, lst - fst - 1);

						string rplsmnt =
							"\""
							+ VersionMajor.ToString()
							+ '.'
							+ VersionMinor.ToString()
							+ '.'
							+ BuildNumber.ToString()
							+ '.'
							+ RevisionNumber.ToString()
							+ "\"";

						FileLines[i] = FileLines[i].Replace(OldVNbr, rplsmnt);

						File.WriteAllLines(verFile.FilePathAndName, FileLines);

						return true;
					}
				}
			}
			catch (Exception e)
			{
				System.Diagnostics.Debug.WriteLine("String processing failed: {0}", e.ToString());
				RetVal = false;
				_ = VS.MessageBox.ShowAsync("Error: ", e.ToString(), OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			}
			finally { }

			return RetVal;
		}

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

			// Save the version numbers for use next time
			XElement MAJORMINORBUILDNUMBERS = new("MAJORMINORBUILDNUMBERS"
											, new XAttribute("VersionMajor", VersionMajor)
											, new XAttribute("VersionMinor", VersionMinor)
											, new XAttribute("BuildNumber", BuildNumber)
											, new XAttribute("RevisionNumber", RevisionNumber)
										);
			XDocument TheXDocument = new(
					new XDeclaration("1.0", "utf-8", "yes")
				);
			TheXDocument.Add(MAJORMINORBUILDNUMBERS);

			if (!WriteToXmlFile(PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile, TheXDocument))
			{
				_ = VS.MessageBox.ShowAsync("Error writing to", PathToAndNameOfMajorMinorBuildRevisionNumbersXmlFile, OLEMSGICON.OLEMSGICON_CRITICAL, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			}


			// Handle the search result
			bool HandleResOK = true;
			foreach (KeyValuePair<string, List<VersionFilePathAndType>> VFPT in ProjsWithVersionFiles)
			{
				foreach (VersionFilePathAndType FPAN in VFPT.Value)
				{
					myProjectsDataGrid.Items.Add(new MyProjectsData { ThisSolutionProject = VFPT.Key, ThisSolutionProjectPath = FPAN.FilePathAndName });

					switch (FPAN.FileType)
					{
						case FilesContainingVersionTypes.infoplist:
							{
								HandleResOK &= SetVersionNumbersInInfoplistFiles(FPAN);
								break;
							}
						case FilesContainingVersionTypes.appxmanifest:
							{
								HandleResOK &= SetVersionNumbersInAppxmanifestFiles(FPAN);
								break;
							}
						case FilesContainingVersionTypes.manifestxml:
							{
								HandleResOK &= SetVersionNumbersInManifestXmlFiles(FPAN);
								break;
							}
						case FilesContainingVersionTypes.assemblyinfo_cs:
							{
								HandleResOK &= SetVersionNumbersInAssemblyinfo_cs_Files(FPAN);
								break;
							}
						case FilesContainingVersionTypes.notsupported:
							{
								break;
							}

					}
				}
			}
			if (HandleResOK)
			{
				_ = VS.MessageBox.ShowAsync("Done!", "", OLEMSGICON.OLEMSGICON_INFO, OLEMSGBUTTON.OLEMSGBUTTON_OK);
			}

		}

	}
}