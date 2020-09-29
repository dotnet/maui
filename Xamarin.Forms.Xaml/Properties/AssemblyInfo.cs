using System.Runtime.CompilerServices;

using Xamarin.Forms;
using Xamarin.Forms.Internals;

[assembly: InternalsVisibleTo("Xamarin.Forms.Xaml.UnitTests")]
[assembly: InternalsVisibleTo("Xamarin.Forms.Build.Tasks")]
[assembly: InternalsVisibleTo("Xamarin.Forms.Xaml.Design")]
[assembly: InternalsVisibleTo("Xamarin.Forms.Loader")]// Xamarin.Forms.Loader.dll Xamarin.Forms.Xaml.XamlLoader.Load(object, string), kzu@microsoft.com
[assembly: InternalsVisibleTo("Xamarin.HotReload.Forms")]
[assembly: InternalsVisibleTo("Xamarin.HotReload.UnitTests")]
[assembly: Preserve]

[assembly: XmlnsDefinition("http://xamarin.com/schemas/2014/forms", "Xamarin.Forms.Xaml")]
[assembly: XmlnsDefinition("http://xamarin.com/schemas/2014/forms/design", "Xamarin.Forms.Xaml")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "Xamarin.Forms.Xaml")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "System", AssemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "System", AssemblyName = "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2009/xaml", "Xamarin.Forms.Xaml")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2009/xaml", "System", AssemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2009/xaml", "System", AssemblyName = "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]

#pragma warning disable CS0612 // Type or member is obsolete
[assembly: TypeForwardedTo(typeof(Xamarin.Forms.Xaml.Internals.INameScopeProvider))]
#pragma warning restore CS0612 // Type or member is obsolete

[assembly: TypeForwardedTo(typeof(Xamarin.Forms.Xaml.Diagnostics.DebuggerHelper))]
[assembly: TypeForwardedTo(typeof(Xamarin.Forms.Xaml.Diagnostics.VisualDiagnostics))]
[assembly: TypeForwardedTo(typeof(Xamarin.Forms.Xaml.Diagnostics.VisualTreeChangeEventArgs))]
[assembly: TypeForwardedTo(typeof(Xamarin.Forms.Xaml.Diagnostics.XamlSourceInfo))]