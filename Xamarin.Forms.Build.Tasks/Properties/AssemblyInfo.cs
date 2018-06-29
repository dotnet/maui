using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("Xamarin.Forms.Build.Tasks")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyFileVersion(
	  ThisAssembly.Git.BaseVersion.Major + "."
	+ ThisAssembly.Git.BaseVersion.Minor + "."
	+ ThisAssembly.Git.BaseVersion.Patch + "."
	+ ThisAssembly.Git.Commits)]

[assembly: AssemblyInformationalVersion(
	ThisAssembly.Git.BaseVersion.Major + "."
	+ ThisAssembly.Git.BaseVersion.Minor + "."
	+ ThisAssembly.Git.BaseVersion.Patch + "."
	+ ThisAssembly.Git.Commits + "-"
	+ ThisAssembly.Git.Branch + "+"
	+ ThisAssembly.Git.Commit)]

#if DEBUG
[assembly:InternalsVisibleTo("Xamarin.Forms.Xaml.UnitTests")]
#endif