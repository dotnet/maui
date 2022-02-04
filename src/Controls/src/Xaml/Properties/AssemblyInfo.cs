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
[assembly: InternalsVisibleTo("Microsoft.Maui.Controls.Compatibility")]
[assembly: InternalsVisibleTo("CommunityToolkit.Maui")]
[assembly: InternalsVisibleTo("CommunityToolkit.Maui.Core")]
[assembly: InternalsVisibleTo("CommunityToolkit.Maui.UnitTests")]
[assembly: InternalsVisibleTo("CommunityToolkit.Maui.Markup")]
[assembly: InternalsVisibleTo("CommunityToolkit.Maui.Markup.UnitTests")]
[assembly: Preserve]

[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/2021/maui", "Microsoft.Maui.Controls.Xaml")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/dotnet/2021/maui/design", "Microsoft.Maui.Controls.Xaml")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "Microsoft.Maui.Controls.Xaml")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "System", AssemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2006/xaml", "System", AssemblyName = "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2009/xaml", "Microsoft.Maui.Controls.Xaml")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2009/xaml", "System", AssemblyName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/winfx/2009/xaml", "System", AssemblyName = "System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]