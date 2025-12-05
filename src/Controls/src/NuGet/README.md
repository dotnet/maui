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

Run on Mac Catalyst (Mac only):
```bash
dotnet build -t:Run -f net10.0-maccatalyst
```

Run on Windows:
```bash
dotnet build -t:Run -f net10.0-windows10.0.19041.0
```

## 💡 Quick Start Example

To get started quickly with .NET MAUI, follow our comprehensive tutorials:

- [Build your first app](https://learn.microsoft.com/dotnet/maui/get-started/first-app) - Step-by-step guide with UI examples
- [XAML basics](https://learn.microsoft.com/dotnet/maui/xaml/fundamentals/get-started) - Learn XAML fundamentals with visual examples
- [Create a multi-page app](https://learn.microsoft.com/dotnet/maui/tutorials/notes-app/) - Build a complete note-taking app

## 🎯 Key Features

### MVVM and Data Binding

.NET MAUI fully supports the Model-View-ViewModel (MVVM) pattern with powerful data binding. Learn more:

- [Data binding fundamentals](https://learn.microsoft.com/dotnet/maui/xaml/fundamentals/data-binding-basics) - Visual examples of data binding
- [MVVM pattern](https://learn.microsoft.com/dotnet/maui/xaml/fundamentals/mvvm) - Complete MVVM guide with examples
- [.NET MAUI Community Toolkit](https://learn.microsoft.com/dotnet/communitytoolkit/maui/) - MVVM helpers and more
- [MVVM Toolkit](https://learn.microsoft.com/dotnet/communitytoolkit/mvvm/) - Source generators and commands

### XAML Enhancements

.NET MAUI includes powerful XAML features for cleaner, more efficient code:

- [XAML compilation and source generation](https://learn.microsoft.com/dotnet/maui/xaml/xamlc) - Better performance and smaller app sizes
- [XAML markup extensions](https://learn.microsoft.com/dotnet/maui/xaml/markup-extensions/consume) - Extend XAML capabilities
- [XAML hot reload](https://learn.microsoft.com/dotnet/maui/xaml/hot-reload) - See changes instantly during development

### Shell Navigation

Shell provides a structured, performant navigation experience:

- [Shell overview](https://learn.microsoft.com/dotnet/maui/fundamentals/shell/) - Introduction to Shell navigation
- [Shell navigation](https://learn.microsoft.com/dotnet/maui/fundamentals/shell/navigation) - Routing and navigation patterns
- [Shell tabs](https://learn.microsoft.com/dotnet/maui/fundamentals/shell/tabs) - Create tabbed interfaces

### Collections and Lists

Display lists and collections with powerful controls:

- [CollectionView](https://learn.microsoft.com/dotnet/maui/user-interface/controls/collectionview/) - High-performance lists with UI examples
- [ListView](https://learn.microsoft.com/dotnet/maui/user-interface/controls/listview) - Traditional list view control
- [CarouselView](https://learn.microsoft.com/dotnet/maui/user-interface/controls/carouselview/) - Scrollable carousel of items

### Responsive Layouts

Build adaptive UIs that work across different screen sizes:

- [Layouts](https://learn.microsoft.com/dotnet/maui/user-interface/layouts/) - Overview of all layout types
- [Grid layout](https://learn.microsoft.com/dotnet/maui/user-interface/layouts/grid) - Flexible grid with visual examples
- [FlexLayout](https://learn.microsoft.com/dotnet/maui/user-interface/layouts/flexlayout) - CSS flexbox-style layout
- [Adaptive layouts](https://learn.microsoft.com/dotnet/maui/user-interface/layouts/choose-layout) - Design for different screen sizes

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
