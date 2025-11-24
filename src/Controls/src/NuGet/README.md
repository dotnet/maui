# .NET Multi-platform App UI (.NET MAUI)

![Build Status](https://img.shields.io/azure-devops/build/xamarin/public/57/main?label=Build&style=flat-square) 
![NuGet](https://img.shields.io/nuget/v/Microsoft.Maui.Controls?style=flat-square&label=NuGet)
![License](https://img.shields.io/github/license/dotnet/maui?style=flat-square&label=License)

[.NET Multi-platform App UI (.NET MAUI)](https://dotnet.microsoft.com/apps/maui) is a cross-platform framework for creating native mobile and desktop apps with C# and XAML. Using .NET MAUI, you can develop apps that run on Android, iOS, iPadOS, macOS, and Windows from a single shared codebase.

## ✨ What is .NET MAUI?

The **Microsoft.Maui.Controls** package provides the UI controls and XAML infrastructure for building beautiful, native cross-platform applications. It includes:

- **40+ UI controls** - Buttons, labels, entries, pickers, lists, grids, and more
- **XAML support** - Design your UI with declarative markup
- **Layout system** - Flexible layouts including Grid, StackLayout, FlexLayout, and AbsoluteLayout  
- **Navigation** - Shell navigation, NavigationPage, TabbedPage, FlyoutPage
- **Data binding** - Two-way data binding with MVVM support
- **Styling and theming** - Application-wide styles, dynamic resources, and light/dark theme support
- **Platform integration** - Access platform-specific features seamlessly
- **Hot Reload** - See UI changes instantly during development

## 🚀 Supported Platforms

.NET MAUI applications run on the following platforms:

| Platform | Minimum Version |
|----------|----------------|
| Android | API 24 (Android 7.0) |
| iOS | iOS 13.0+ |
| iPadOS | iPadOS 13.0+ |
| macOS | macOS 11.0+ (via Mac Catalyst) |
| Windows | Windows 11, Windows 10 (Version 1809+) |

## 📦 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (or .NET 9 for previous versions)
- Platform-specific tools:
  - **Android**: Android SDK (installed via Visual Studio or Android Studio)
  - **iOS/macOS**: Xcode (Mac required)
  - **Windows**: Windows App SDK

### Installation

Install the .NET MAUI workload:

```bash
dotnet workload install maui
```

### Create a New Project

Create a new .NET MAUI app using the CLI:

```bash
dotnet new maui -n MyMauiApp
cd MyMauiApp
```

Or create with sample content including Community Toolkit and Syncfusion Toolkit:

```bash
dotnet new maui -n MyMauiApp -sc
```

### Run Your App

Run on Android:
```bash
dotnet build -t:Run -f net10.0-android
```

Run on iOS (Mac only):
```bash
dotnet build -t:Run -f net10.0-ios
```

Run on Windows:
```bash
dotnet build -t:Run -f net10.0-windows
```

## 💡 Quick Start Example

Here's a simple .NET MAUI page with various controls:

### XAML (MainPage.xaml)

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MyMauiApp.MainPage"
             Background="{DynamicResource PageBackgroundColor}">

    <ScrollView>
        <VerticalStackLayout Spacing="25" Padding="30">
            
            <Image Source="dotnet_bot.png" 
                   HeightRequest="185"
                   Aspect="AspectFit" />
            
            <Label Text="Hello, .NET MAUI!"
                   Style="{StaticResource Headline}"
                   HorizontalOptions="Center" />
            
            <Label Text="Build native apps for mobile and desktop"
                   HorizontalOptions="Center" />
            
            <Entry Placeholder="Enter your name"
                   x:Name="NameEntry" />
            
            <Button Text="Click Me"
                    Clicked="OnCounterClicked"
                    HorizontalOptions="Fill" />
            
            <Label x:Name="CounterLabel"
                   Text="Button not clicked yet"
                   HorizontalOptions="Center" />
                   
        </VerticalStackLayout>
    </ScrollView>

</ContentPage>
```

### Code-Behind (MainPage.xaml.cs)

```csharp
namespace MyMauiApp;

public partial class MainPage : ContentPage
{
    private int count = 0;

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnCounterClicked(object sender, EventArgs e)
    {
        count++;
        
        if (count == 1)
            CounterLabel.Text = $"Hello {NameEntry.Text}! Button clicked {count} time";
        else
            CounterLabel.Text = $"Hello {NameEntry.Text}! Button clicked {count} times";

        SemanticScreenReader.Announce(CounterLabel.Text);
    }
}
```

## 🎯 Key Features

### MVVM and Data Binding

.NET MAUI fully supports the Model-View-ViewModel (MVVM) pattern with powerful data binding:

```xml
<Label Text="{Binding UserName}" />
<Entry Text="{Binding Email, Mode=TwoWay}" />
```

Use the [.NET MAUI Community Toolkit](https://github.com/CommunityToolkit/Maui) for MVVM helpers and the [MVVM Toolkit](https://github.com/CommunityToolkit/dotnet) for source generators and commands.

### XAML Enhancements

.NET MAUI includes powerful XAML features for cleaner, more efficient code:

- **XAML Source Generation** - Compile-time XAML parsing for better performance and smaller app sizes
- **XAML Simplifications** - Reduced boilerplate with improved syntax and type inference

Learn more about [XAML compilation and source generation](https://learn.microsoft.com/dotnet/maui/xaml/xamlc?view=net-maui-10.0).

### Shell Navigation

Shell provides a structured, performant navigation experience:

```xml
<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui">
    <TabBar>
        <ShellContent Title="Home" Icon="home.png" ContentTemplate="{DataTemplate local:HomePage}" />
        <ShellContent Title="Profile" Icon="profile.png" ContentTemplate="{DataTemplate local:ProfilePage}" />
    </TabBar>
</Shell>
```

### Collections and Lists

Display lists and collections with CollectionView:

```xml
<CollectionView ItemsSource="{Binding Items}">
    <CollectionView.ItemTemplate>
        <DataTemplate>
            <Grid Padding="10">
                <Label Text="{Binding Name}" FontSize="18" />
            </Grid>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

### Responsive Layouts

Build adaptive UIs that work across different screen sizes:

```xml
<Grid RowDefinitions="Auto,*" ColumnDefinitions="*,*">
    <Label Grid.Row="0" Grid.ColumnSpan="2" Text="Header" />
    <BoxView Grid.Row="1" Grid.Column="0" Color="Red" />
    <BoxView Grid.Row="1" Grid.Column="1" Color="Blue" />
</Grid>
```

## 📚 Documentation and Resources

### Official Documentation
- [.NET MAUI Documentation](https://learn.microsoft.com/dotnet/maui/) - Complete guide to building apps
- [API Reference](https://learn.microsoft.com/dotnet/api/?view=net-maui-10.0) - Detailed API documentation
- [Controls Documentation](https://learn.microsoft.com/dotnet/maui/user-interface/controls/) - All available controls
- [What's New in .NET 10](https://learn.microsoft.com/dotnet/maui/whats-new/dotnet-10) - Latest features and improvements

### Learning Resources
- [.NET MAUI Samples](https://github.com/dotnet/maui-samples) - Official sample applications
- [.NET MAUI Workshop](https://github.com/dotnet-presentations/dotnet-maui-workshop) - Hands-on learning workshop
- [Microsoft Learn](https://learn.microsoft.com/training/paths/build-apps-with-dotnet-maui/) - Free training modules
- [.NET MAUI Blog](https://devblogs.microsoft.com/dotnet/category/net-maui/) - Latest news and updates

### Community Resources
- [.NET MAUI Community Toolkit](https://learn.microsoft.com/dotnet/communitytoolkit/maui/) - Additional controls, behaviors, and converters
- [Awesome .NET MAUI](https://github.com/jsuarezruiz/awesome-dotnet-maui) - Curated list of resources
- [Stack Overflow](https://stackoverflow.com/questions/tagged/.net-maui) - Community Q&A

## 💬 Feedback and Support

We welcome your feedback and contributions!

### Getting Help
- **Documentation**: Check the [official documentation](https://learn.microsoft.com/dotnet/maui/)
- **Q&A**: Ask questions on [Stack Overflow](https://stackoverflow.com/questions/tagged/.net-maui) with the `.net-maui` tag
- **Discussions**: Join [GitHub Discussions](https://github.com/dotnet/maui/discussions) for community conversations

### Reporting Issues
- **Bug Reports**: File bugs at [GitHub Issues](https://github.com/dotnet/maui/issues)
- **Feature Requests**: Suggest new features in [GitHub Issues](https://github.com/dotnet/maui/issues)
- **Security Issues**: Report security vulnerabilities via [Microsoft Security Response Center](https://msrc.microsoft.com/)

### Community
- **Discord**: Join the [.NET Discord server](https://aka.ms/dotnet-discord) or the [MAUIverse Discord](https://mauiverse.net/discord) (community-driven, not Microsoft official)
- **X (formerly Twitter)**: Follow [@dotnet](https://twitter.com/dotnet)
- **YouTube**: Watch tutorials on [.NET YouTube channel](https://www.youtube.com/dotnet)

## 🤝 Contributing

We encourage contributions from the community! .NET MAUI is an open-source project.

- **Contributing Guide**: Read our [Contributing Guidelines](https://github.com/dotnet/maui/blob/main/.github/CONTRIBUTING.md)
- **Development Guide**: See the [Development Guide](https://github.com/dotnet/maui/blob/main/.github/DEVELOPMENT.md) for building locally
- **Code of Conduct**: Review our [Code of Conduct](https://github.com/dotnet/maui/blob/main/.github/CODE_OF_CONDUCT.md)
- **Good First Issues**: Find [good first issues](https://github.com/dotnet/maui/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22) to start contributing

## 🔧 Related Packages

.NET MAUI consists of several packages that work together:

| Package | Description |
|---------|-------------|
| [Microsoft.Maui.Controls](https://www.nuget.org/packages/Microsoft.Maui.Controls) | Core UI controls and XAML (this package) |
| [Microsoft.Maui.Core](https://www.nuget.org/packages/Microsoft.Maui.Core) | Platform abstractions and handlers |
| [Microsoft.Maui.Essentials](https://www.nuget.org/packages/Microsoft.Maui.Essentials) | Cross-platform APIs (merged into Core) |
| [Microsoft.Maui.Graphics](https://www.nuget.org/packages/Microsoft.Maui.Graphics) | Cross-platform graphics library |
| [CommunityToolkit.Maui](https://www.nuget.org/packages/CommunityToolkit.Maui) | Community-built controls and helpers |

## 📄 License

.NET MAUI is licensed under the [MIT License](https://github.com/dotnet/maui/blob/main/LICENSE.TXT).

## 🎉 Acknowledgements

.NET MAUI is the evolution of Xamarin.Forms, building on years of mobile and cross-platform development experience. We thank the community for their continued support and contributions.

---

**[Get Started with .NET MAUI](https://dotnet.microsoft.com/apps/maui)** | 
**[Documentation](https://learn.microsoft.com/dotnet/maui/)** | 
**[Samples](https://github.com/dotnet/maui-samples)** | 
**[GitHub](https://github.com/dotnet/maui)**
