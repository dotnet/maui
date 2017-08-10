using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Xamarin.Forms;
using Xamarin.Forms.ControlGallery.iOS;
using Xamarin.Forms.Controls;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle ("Xamarin.Forms.ControlGallery.iOS")]
[assembly: AssemblyDescription ("")]
[assembly: AssemblyConfiguration ("")]
[assembly: AssemblyCompany ("")]
[assembly: AssemblyProduct ("Xamarin.Forms.ControlGallery.iOS")]
[assembly: AssemblyCopyright ("Copyright ©  2013")]
[assembly: AssemblyTrademark ("")]
[assembly: AssemblyCulture ("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible (false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid ("5098d081-687d-442c-9f92-77fa599779f9")]

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
[assembly: AssemblyVersion ("1.0.0.0")]
[assembly: AssemblyFileVersion ("1.0.0.0")]
[assembly: Xamarin.Forms.ResolutionGroupName (Xamarin.Forms.Controls.Issues.Effects.ResolutionGroupName)]

// Deliberately broken image source and handler so we can test handling of image loading errors
[assembly: ExportImageSourceHandler(typeof(FailImageSource), typeof(BrokenImageSourceHandler))]