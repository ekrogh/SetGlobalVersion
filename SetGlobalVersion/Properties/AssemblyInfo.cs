using SetGlobalVersion;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle(Vsix.Name)]
[assembly: AssemblyDescription(Vsix.Description)]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany(Vsix.Author)]
[assembly: AssemblyProduct(Vsix.Name)]
[assembly: AssemblyCopyright(Vsix.Author)]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

[assembly: AssemblyVersion("5.3.1.1")]
[assembly: AssemblyFileVersion(Vsix.Version)]

namespace System.Runtime.CompilerServices
{
	public class IsExternalInit { }
}
