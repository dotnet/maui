using System.Runtime.CompilerServices;

using System.Maui;
using System.Maui.Internals;

[assembly: InternalsVisibleTo("System.Maui.Xaml.UnitTests")]
[assembly: InternalsVisibleTo("System.Maui.Build.Tasks")]
[assembly: InternalsVisibleTo("System.Maui.Xaml.Design")]
[assembly: InternalsVisibleTo("System.Maui.Loader")]// System.Maui.Loader.dll System.Maui.Xaml.XamlLoader.Load(object, string), kzu@microsoft.com
[assembly: InternalsVisibleTo("Xamarin.HotReload.Forms")]
[assembly: InternalsVisibleTo("Xamarin.HotReload.UnitTests")]
[assembly: Preserve]

[assembly: XmlnsDefinition("http://xamarin.com/schemas/2014/forms", "System.Maui.Xaml")]
[assembly: XmlnsDefinition("http://xamarin.com/schemas/2014/forms/design", "System.Maui.Xaml")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "System.Maui.Xaml")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "System", AssemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "System", AssemblyName = "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2009/xaml", "System.Maui.Xaml")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2009/xaml", "System", AssemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2009/xaml", "System", AssemblyName = "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]

#pragma warning disable CS0612 // Type or member is obsolete
[assembly: TypeForwardedTo(typeof(System.Maui.Xaml.Internals.INameScopeProvider))]
#pragma warning restore CS0612 // Type or member is obsolete

[assembly: TypeForwardedTo(typeof(System.Maui.Xaml.Diagnostics.DebuggerHelper))]
[assembly: TypeForwardedTo(typeof(System.Maui.Xaml.Diagnostics.VisualDiagnostics))]
[assembly: TypeForwardedTo(typeof(System.Maui.Xaml.Diagnostics.VisualTreeChangeEventArgs))]
[assembly: TypeForwardedTo(typeof(System.Maui.Xaml.Diagnostics.XamlSourceInfo))]
