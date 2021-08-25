using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell.Interop;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SetVersionNumberGloballyXam
{
	public class CheckSolutionType
	{
		public static List<EnvDTE.Project> XamarinFormsProjectsList = new List<EnvDTE.Project>();
		public static async Task<bool> ThisIsXamarinAsync()
		{
			if (XamarinFormsProjectsList.Count != 0)
			{
				return true;
			}
			else
			{
				await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

				try
				{

					DTE2 dte = await VS.GetRequiredServiceAsync<DTE, DTE2>();
					Projects projs = dte.Solution.Projects;

					if (projs.Count != 0)
					{
						NuGet.VisualStudio.IVsPackageInstallerServices installerServices =
							await VS.GetMefServiceAsync<NuGet.VisualStudio.IVsPackageInstallerServices>();

						foreach (EnvDTE.Project proj in projs)
						{
							try
							{
								IEnumerable<NuGet.VisualStudio.IVsPackageMetadata> installedPackages =
									installerServices.GetInstalledPackages(proj);

								foreach (NuGet.VisualStudio.IVsPackageMetadata installedPack in installedPackages)
								{
									if (installedPack.Id.ToLower().Contains("xamarin.forms"))
									{
										XamarinFormsProjectsList.Add(proj);
									}
								}
							}
							catch (Exception)
							{
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

				if (XamarinFormsProjectsList.Count != 0)
				{
					return true;
				}
				else
				{
					return false;
				}

			}
		}
	}
}
