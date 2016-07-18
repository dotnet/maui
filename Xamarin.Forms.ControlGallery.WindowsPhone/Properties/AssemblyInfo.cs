using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xamarin.Forms.ControlGallery.WinRT;
using Xamarin.Forms.Controls;
using Xamarin.Forms.Platform.WinRT;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Xamarin.Forms.ControlGallery.WindowsPhone")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Xamarin.Forms.ControlGallery.WindowsPhone")]
[assembly: AssemblyCopyright("Copyright ©  2015")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: ComVisible(false)]

// Deliberately broken image source and handler so we can test handling of image loading errors
[assembly: ExportImageSourceHandler(typeof(FailImageSource), typeof(BrokenImageSourceHandler))]