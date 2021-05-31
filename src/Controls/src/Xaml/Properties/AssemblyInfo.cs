using System.Runtime.CompilerServices;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

[assembly: InternalsVisibleTo("Microsoft.Maui.Controls.Xaml.UnitTests")]
[assembly: InternalsVisibleTo("Microsoft.Maui.Controls.Build.Tasks")]
[assembly: InternalsVisibleTo("Microsoft.Maui.Controls.Xaml.Design")]
[assembly: InternalsVisibleTo("Microsoft.Maui.Controls.Loader")]// Microsoft.Maui.Controls.Loader.dll Microsoft.Maui.Controls.Xaml.XamlLoader.Load(object, string), kzu@microsoft.com
[assembly: InternalsVisibleTo("Microsoft.Maui.Controls.HotReload.Forms")]
[assembly: InternalsVisibleTo("Microsoft.Maui.Controls.HotReload.UnitTests")]
[assembly: InternalsVisibleTo("Microsoft.Maui.Controls.SourceGen")]
[assembly: Preserve]

[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/2021/maui", "Microsoft.Maui.Controls.Xaml")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/2021/maui/design", "Microsoft.Maui.Controls.Xaml")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "Microsoft.Maui.Controls.Xaml")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "System", AssemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "System", AssemblyName = "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2009/xaml", "Microsoft.Maui.Controls.Xaml")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2009/xaml", "System", AssemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2009/xaml", "System", AssemblyName = "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]

#pragma warning disable CS0612 // Type or member is obsolete
[assembly: TypeForwardedTo(typeof(Microsoft.Maui.Controls.Xaml.Internals.INameScopeProvider))]
#pragma warning restore CS0612 // Type or member is obsolete

[assembly: TypeForwardedTo(typeof(Microsoft.Maui.Controls.Xaml.Diagnostics.DebuggerHelper))]
[assembly: TypeForwardedTo(typeof(Microsoft.Maui.Controls.Xaml.Diagnostics.VisualDiagnostics))]
[assembly: TypeForwardedTo(typeof(Microsoft.Maui.Controls.Xaml.Diagnostics.VisualTreeChangeEventArgs))]
[assembly: TypeForwardedTo(typeof(Microsoft.Maui.Controls.Xaml.Diagnostics.XamlSourceInfo))]
