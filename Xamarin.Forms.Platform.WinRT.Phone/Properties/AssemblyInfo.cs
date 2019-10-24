using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Platform.WinRT;

[assembly: AssemblyTitle("Xamarin.Forms.Platform.WinRT.Phone")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]

[assembly: Dependency (typeof (WindowsPhoneResourcesProvider))]

[assembly: ExportRenderer (typeof (SearchBar), typeof (SearchBarRenderer))]

[assembly: ExportRenderer (typeof (TabbedPage), typeof (TabbedPageRenderer))]