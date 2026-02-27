using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

/// <summary>
/// Tests for x:Type in multi-file hot reload scenarios.
/// </summary>
public class XTypeMultiFileHotReloadTests : SourceGenXamlInitializeComponentTestBase
{
    protected record CSharpFile(string Path, string Content);
    protected record XamlFile(string Path, string Content, string? CodeBehind = null);

    /// <summary>
    /// Scenario 1: Change one XAML file while x:Type references types from unchanged C# files
    /// </summary>
    [Fact]
    public void XTypeWithMultipleViewModels_ModifyOneXamlFile()
    {
        var viewModel1 = new CSharpFile("ViewModels/PersonViewModel.cs", """
            namespace Test.ViewModels;
            public class PersonViewModel { public string Name { get; set; } }
            """);

        var viewModel2 = new CSharpFile("ViewModels/ProductViewModel.cs", """
            namespace Test.ViewModels;
            public class ProductViewModel { public string Title { get; set; } }
            """);

        var viewModel3 = new CSharpFile("ViewModels/OrderViewModel.cs", """
            namespace Test.ViewModels;
            public class OrderViewModel { public int OrderId { get; set; } }
            """);

        var page1 = new XamlFile("Pages/Page1.xaml", 
            Content: """
                <?xml version="1.0" encoding="utf-8" ?>
                <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                             xmlns:vm="clr-namespace:Test.ViewModels"
                             x:Class="Test.Pages.Page1">
                    <ContentPage.Resources>
                        <x:Array x:Key="ViewModels" Type="{x:Type vm:PersonViewModel}" />
                    </ContentPage.Resources>
                    <Label Text="Hello" TextColor="Blue" />
                </ContentPage>
                """,
            CodeBehind: """
                using Microsoft.Maui.Controls;
                using Microsoft.Maui.Controls.Xaml;
                namespace Test.Pages;
                public partial class Page1 : ContentPage { public Page1() { InitializeComponent(); } }
                """);

        var page2 = new XamlFile("Pages/Page2.xaml",
            Content: """
                <?xml version="1.0" encoding="utf-8" ?>
                <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                             xmlns:vm="clr-namespace:Test.ViewModels"
                             x:Class="Test.Pages.Page2">
                    <ContentPage.Resources>
                        <x:Array x:Key="ViewModels" Type="{x:Type vm:ProductViewModel}" />
                    </ContentPage.Resources>
                </ContentPage>
                """,
            CodeBehind: """
                using Microsoft.Maui.Controls;
                using Microsoft.Maui.Controls.Xaml;
                namespace Test.Pages;
                public partial class Page2 : ContentPage { public Page2() { InitializeComponent(); } }
                """);

        // Modified: Change TextColor in Page1 only
        var page1Modified = page1 with {
            Content = """
                <?xml version="1.0" encoding="utf-8" ?>
                <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                             xmlns:vm="clr-namespace:Test.ViewModels"
                             x:Class="Test.Pages.Page1">
                    <ContentPage.Resources>
                        <x:Array x:Key="ViewModels" Type="{x:Type vm:PersonViewModel}" />
                    </ContentPage.Resources>
                    <Label Text="Hello" TextColor="Red" />
                </ContentPage>
                """,
            CodeBehind = null // Code behind didn't change
        };

        TestMultiFileScenario(
            initialFiles: new object[] { viewModel1, viewModel2, viewModel3, page1, page2 },
            modifiedFiles: new object[] { page1Modified }
        );
    }

    /// <summary>
    /// Scenario 2: Add a new ViewModel class and reference it in existing XAML
    /// </summary>
    [Fact]
    public void XTypeWithNewViewModel_AddNewClassAndUpdateXaml()
    {
        var viewModel1 = new CSharpFile("ViewModels/UserViewModel.cs", """
            namespace Test.ViewModels;
            public class UserViewModel { public string Email { get; set; } }
            """);

        var page = new XamlFile("MainPage.xaml",
            Content: """
                <?xml version="1.0" encoding="utf-8" ?>
                <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                             xmlns:vm="clr-namespace:Test.ViewModels"
                             x:Class="Test.MainPage">
                    <ContentPage.Resources>
                        <x:Array x:Key="ViewModels" Type="{x:Type vm:UserViewModel}" />
                    </ContentPage.Resources>
                </ContentPage>
                """,
            CodeBehind: """
                using Microsoft.Maui.Controls;
                using Microsoft.Maui.Controls.Xaml;
                namespace Test;
                public partial class MainPage : ContentPage { public MainPage() { InitializeComponent(); } }
                """);

        // Add new ViewModel
        var viewModel2 = new CSharpFile("ViewModels/AccountViewModel.cs", """
            namespace Test.ViewModels;
            public class AccountViewModel { public decimal Balance { get; set; } }
            """);

        // Update XAML to reference both
        var pageModified = page with {
            Content = """
                <?xml version="1.0" encoding="utf-8" ?>
                <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                             xmlns:vm="clr-namespace:Test.ViewModels"
                             x:Class="Test.MainPage">
                    <ContentPage.Resources>
                        <x:Array x:Key="ViewModels1" Type="{x:Type vm:UserViewModel}" />
                        <x:Array x:Key="ViewModels2" Type="{x:Type vm:AccountViewModel}" />
                    </ContentPage.Resources>
                </ContentPage>
                """,
            CodeBehind = null
        };

        TestMultiFileScenario(
            initialFiles: new object[] { viewModel1, page },
            modifiedFiles: new object[] { viewModel2, pageModified }
        );
    }

    /// <summary>
    /// Scenario 3: Multiple pages sharing same ViewModels, modify one page's style
    /// </summary>
    [Fact]
    public void XTypeInSharedResources_ModifyPageStyle()
    {
        var sharedViewModel = new CSharpFile("Shared/SharedViewModel.cs", """
            namespace Test.Shared;
            public class SharedViewModel { }
            """);

        var app = new XamlFile("App.xaml",
            Content: """
                <?xml version="1.0" encoding="utf-8" ?>
                <Application xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                             xmlns:shared="clr-namespace:Test.Shared"
                             x:Class="Test.App">
                    <Application.Resources>
                        <x:Array x:Key="GlobalViewModels" Type="{x:Type shared:SharedViewModel}" />
                    </Application.Resources>
                </Application>
                """,
            CodeBehind: """
                using Microsoft.Maui.Controls;
                using Microsoft.Maui.Controls.Xaml;
                namespace Test;
                public partial class App : Application { public App() { InitializeComponent(); } }
                """);

        var page1 = new XamlFile("HomePage.xaml",
            Content: """
                <?xml version="1.0" encoding="utf-8" ?>
                <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                             x:Class="Test.HomePage"
                             BackgroundColor="White">
                </ContentPage>
                """,
            CodeBehind: """
                using Microsoft.Maui.Controls;
                using Microsoft.Maui.Controls.Xaml;
                namespace Test;
                public partial class HomePage : ContentPage { public HomePage() { InitializeComponent(); } }
                """);

        // Modify HomePage background
        var page1Modified = page1 with {
            Content = """
                <?xml version="1.0" encoding="utf-8" ?>
                <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                             x:Class="Test.HomePage"
                             BackgroundColor="LightGray">
                </ContentPage>
                """,
            CodeBehind = null
        };

        TestMultiFileScenario(
            initialFiles: new object[] { sharedViewModel, app, page1 },
            modifiedFiles: new object[] { page1Modified }
        );
    }

    /// <summary>
    /// Scenario 8: x:Type with interfaces, add implementation class
    /// </summary>
    [Fact]
    public void XTypeWithInterfaces_AddImplementation()
    {
        var interfaceFile = new CSharpFile("Interfaces/IService.cs", """
            namespace Test.Interfaces;
            public interface IService { void Execute(); }
            """);

        var impl1 = new CSharpFile("Services/ServiceA.cs", """
            namespace Test.Services;
            public class ServiceA : Test.Interfaces.IService { public void Execute() { } }
            """);

        var page = new XamlFile("ServicePage.xaml",
            Content: """
                <?xml version="1.0" encoding="utf-8" ?>
                <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                             xmlns:services="clr-namespace:Test.Services"
                             x:Class="Test.ServicePage">
                    <ContentPage.Resources>
                        <x:Array x:Key="Services" Type="{x:Type services:ServiceA}" />
                    </ContentPage.Resources>
                </ContentPage>
                """,
            CodeBehind: """
                using Microsoft.Maui.Controls;
                using Microsoft.Maui.Controls.Xaml;
                namespace Test;
                public partial class ServicePage : ContentPage { public ServicePage() { InitializeComponent(); } }
                """);

        // Add new implementation
        var impl2 = new CSharpFile("Services/ServiceB.cs", """
            namespace Test.Services;
            public class ServiceB : Test.Interfaces.IService { public void Execute() { } }
            """);

        // Update XAML
        var pageModified = page with {
            Content = """
                <?xml version="1.0" encoding="utf-8" ?>
                <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                             xmlns:services="clr-namespace:Test.Services"
                             x:Class="Test.ServicePage">
                    <ContentPage.Resources>
                        <x:Array x:Key="ServicesA" Type="{x:Type services:ServiceA}" />
                        <x:Array x:Key="ServicesB" Type="{x:Type services:ServiceB}" />
                    </ContentPage.Resources>
                </ContentPage>
                """,
            CodeBehind = null
        };

        TestMultiFileScenario(
            initialFiles: new object[] { interfaceFile, impl1, page },
            modifiedFiles: new object[] { impl2, pageModified }
        );
    }

    /// <summary>
    /// Scenario 14: x:Type with converters, add new converter
    /// </summary>
    [Fact]
    public void XTypeWithConverters_AddNewConverter()
    {
        var converter1 = new CSharpFile("Converters/BoolToColorConverter.cs", """
            using System;
            using System.Globalization;
            using Microsoft.Maui.Controls;
            namespace Test.Converters;
            public class BoolToColorConverter : IValueConverter {
                public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => null;
                public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
            }
            """);

        var page = new XamlFile("ConverterPage.xaml",
            Content: """
                <?xml version="1.0" encoding="utf-8" ?>
                <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                             xmlns:conv="clr-namespace:Test.Converters"
                             x:Class="Test.ConverterPage">
                    <ContentPage.Resources>
                        <conv:BoolToColorConverter x:Key="BoolToColor" />
                        <x:Array x:Key="Converters" Type="{x:Type conv:BoolToColorConverter}" />
                    </ContentPage.Resources>
                </ContentPage>
                """,
            CodeBehind: """
                using Microsoft.Maui.Controls;
                using Microsoft.Maui.Controls.Xaml;
                namespace Test;
                public partial class ConverterPage : ContentPage { public ConverterPage() { InitializeComponent(); } }
                """);

        // Add new converter
        var converter2 = new CSharpFile("Converters/IntToStringConverter.cs", """
            using System;
            using System.Globalization;
            using Microsoft.Maui.Controls;
            namespace Test.Converters;
            public class IntToStringConverter : IValueConverter {
                public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => null;
                public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
            }
            """);

        // Update XAML
        var pageModified = page with {
            Content = """
                <?xml version="1.0" encoding="utf-8" ?>
                <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                             xmlns:conv="clr-namespace:Test.Converters"
                             x:Class="Test.ConverterPage">
                    <ContentPage.Resources>
                        <conv:BoolToColorConverter x:Key="BoolToColor" />
                        <conv:IntToStringConverter x:Key="IntToString" />
                        <x:Array x:Key="Converters1" Type="{x:Type conv:BoolToColorConverter}" />
                        <x:Array x:Key="Converters2" Type="{x:Type conv:IntToStringConverter}" />
                    </ContentPage.Resources>
                </ContentPage>
                """,
            CodeBehind = null
        };

        TestMultiFileScenario(
            initialFiles: new object[] { converter1, page },
            modifiedFiles: new object[] { converter2, pageModified }
        );
    }

    /// <summary>
    /// Scenario 30: x:Type with multiple C# files and cross-references
    /// </summary>
    [Fact]
    public void XTypeWithCrossReferences_ModifyMultipleFiles()
    {
        var baseClass = new CSharpFile("Base/BaseViewModel.cs", """
            namespace Test.Base;
            public class BaseViewModel { public int Id { get; set; } }
            """);

        var derived1 = new CSharpFile("ViewModels/PersonViewModel.cs", """
            namespace Test.ViewModels;
            public class PersonViewModel : Test.Base.BaseViewModel { public string Name { get; set; } }
            """);

        var derived2 = new CSharpFile("ViewModels/CompanyViewModel.cs", """
            namespace Test.ViewModels;
            public class CompanyViewModel : Test.Base.BaseViewModel { public string CompanyName { get; set; } }
            """);

        var helper1 = new CSharpFile("Helpers/PersonHelper.cs", """
            namespace Test.Helpers;
            public class PersonHelper { public void Process(Test.ViewModels.PersonViewModel vm) { } }
            """);

        var helper2 = new CSharpFile("Helpers/CompanyHelper.cs", """
            namespace Test.Helpers;
            public class CompanyHelper { public void Process(Test.ViewModels.CompanyViewModel vm) { } }
            """);

        var page1 = new XamlFile("Pages/PersonPage.xaml",
            Content: """
                <?xml version="1.0" encoding="utf-8" ?>
                <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                             xmlns:vm="clr-namespace:Test.ViewModels"
                             x:Class="Test.Pages.PersonPage">
                    <ContentPage.Resources>
                        <x:Array x:Key="People" Type="{x:Type vm:PersonViewModel}" />
                    </ContentPage.Resources>
                    <Label Text="Person" Margin="5" />
                </ContentPage>
                """,
            CodeBehind: """
                using Microsoft.Maui.Controls;
                using Microsoft.Maui.Controls.Xaml;
                namespace Test.Pages;
                public partial class PersonPage : ContentPage { public PersonPage() { InitializeComponent(); } }
                """);

        var page2 = new XamlFile("Pages/CompanyPage.xaml",
            Content: """
                <?xml version="1.0" encoding="utf-8" ?>
                <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                             xmlns:vm="clr-namespace:Test.ViewModels"
                             x:Class="Test.Pages.CompanyPage">
                    <ContentPage.Resources>
                        <x:Array x:Key="Companies" Type="{x:Type vm:CompanyViewModel}" />
                    </ContentPage.Resources>
                    <Label Text="Company" Margin="5" />
                </ContentPage>
                """,
            CodeBehind: """
                using Microsoft.Maui.Controls;
                using Microsoft.Maui.Controls.Xaml;
                namespace Test.Pages;
                public partial class CompanyPage : ContentPage { public CompanyPage() { InitializeComponent(); } }
                """);

        // Modify base class and one page
        var baseClassModified = new CSharpFile("Base/BaseViewModel.cs", """
            namespace Test.Base;
            public class BaseViewModel { 
                public int Id { get; set; }
                public System.DateTime CreatedDate { get; set; }
            }
            """);

        var page1Modified = page1 with {
            Content = """
                <?xml version="1.0" encoding="utf-8" ?>
                <ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                             xmlns:vm="clr-namespace:Test.ViewModels"
                             x:Class="Test.Pages.PersonPage">
                    <ContentPage.Resources>
                        <x:Array x:Key="People" Type="{x:Type vm:PersonViewModel}" />
                    </ContentPage.Resources>
                    <Label Text="Person" Margin="10" />
                </ContentPage>
                """,
            CodeBehind = null
        };

        // Modify one helper
        var helper1Modified = new CSharpFile("Helpers/PersonHelper.cs", """
            namespace Test.Helpers;
            public class PersonHelper { 
                public void Process(Test.ViewModels.PersonViewModel vm) { }
                public void Validate(Test.ViewModels.PersonViewModel vm) { }
            }
            """);

        TestMultiFileScenario(
            initialFiles: new object[] { baseClass, derived1, derived2, helper1, helper2, page1, page2 },
            modifiedFiles: new object[] { baseClassModified, page1Modified, helper1Modified }
        );
    }

    /// <summary>
    /// Test multi-file scenario with incremental compilation
    /// </summary>
    private void TestMultiFileScenario(
        IEnumerable<object> initialFiles,
        IEnumerable<object> modifiedFiles)
    {
        var initialFileList = FlattenFiles(initialFiles).ToList();
        var modifiedFileList = FlattenFiles(modifiedFiles).ToList();

        // Build initial compilation with all files
        var compilation = CreateMauiCompilation();
        var xamlFiles = new System.Collections.Generic.List<AdditionalFile>();

        // Add all C# files to compilation
        foreach (var (path, content) in initialFileList.Where(f => f.path.EndsWith(".cs", StringComparison.Ordinal)))
        {
            compilation = compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(content));
        }

        // Add all XAML files as additional files
        foreach (var (path, content) in initialFileList.Where(f => f.path.EndsWith(".xaml", StringComparison.Ordinal)))
        {
            var xamlFile = new AdditionalXamlFile(
                Path: System.IO.Path.Combine(Environment.CurrentDirectory, path),
                Content: content,
                ManifestResourceName: $"Test.{path.Replace("/", ".", StringComparison.Ordinal).Replace("\\", ".", StringComparison.Ordinal)}");
            xamlFiles.Add((AdditionalFile)xamlFile);
        }

        // Run initial generation
        var result = RunGeneratorWithChanges<XamlGenerator>(compilation, ApplyChanges, xamlFiles.ToArray());

        var result1 = result.result1.Results.Single();
        var result2 = result.result2.Results.Single();

        // Verify no x:Type errors in either compilation
        Assert.False(result1.Diagnostics.Any(d => d.GetMessage().Contains("invalid x:Type", StringComparison.OrdinalIgnoreCase)),
            $"First compilation should not have 'invalid x:Type' errors. Diagnostics: {string.Join(", ", result1.Diagnostics.Select(d => d.GetMessage()))}");

        Assert.False(result2.Diagnostics.Any(d => d.GetMessage().Contains("invalid x:Type", StringComparison.OrdinalIgnoreCase)),
            $"Second compilation (after modifications) should not have 'invalid x:Type' errors. Diagnostics: {string.Join(", ", result2.Diagnostics.Select(d => d.GetMessage()))}");

        // Verify no general errors
        Assert.False(result1.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
            $"First compilation should not have errors. Diagnostics: {string.Join(", ", result1.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => $"{d.Id}: {d.GetMessage()}"))}");
        
        Assert.False(result2.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
            $"Second compilation should not have errors. Diagnostics: {string.Join(", ", result2.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => $"{d.Id}: {d.GetMessage()}"))}");

        (GeneratorDriver, Compilation) ApplyChanges(GeneratorDriver driver, Compilation comp)
        {
            // Apply modifications
            foreach (var (path, content) in modifiedFileList)
            {
                if (path.EndsWith(".cs", StringComparison.Ordinal))
                {
                    // For C# files, add to compilation (simulates file change)
                    comp = comp.AddSyntaxTrees(CSharpSyntaxTree.ParseText(content));
                }
                else if (path.EndsWith(".xaml", StringComparison.Ordinal))
                {
                    // For XAML files, replace in additional files
                    var fullPath = System.IO.Path.Combine(Environment.CurrentDirectory, path);
                    var oldFile = xamlFiles.FirstOrDefault(f => f.Text.Path == fullPath);
                    if (oldFile != null)
                    {
                        var newXamlFile = new AdditionalXamlFile(
                            Path: fullPath,
                            Content: content,
                            ManifestResourceName: $"Test.{path.Replace("/", ".", StringComparison.Ordinal).Replace("\\", ".", StringComparison.Ordinal)}");
                        driver = driver.ReplaceAdditionalText(oldFile.Text, newXamlFile.Text);
                    }
                    else
                    {
                        // New file
                        var newXamlFile = new AdditionalXamlFile(
                            Path: fullPath,
                            Content: content,
                            ManifestResourceName: $"Test.{path.Replace("/", ".", StringComparison.Ordinal).Replace("\\", ".", StringComparison.Ordinal)}");
                        xamlFiles.Add((AdditionalFile)newXamlFile);
                    }
                }
            }

            return (driver, comp);
        }
    }

    private IEnumerable<(string path, string content)> FlattenFiles(IEnumerable<object> files)
    {
        foreach (var file in files)
        {
            if (file is CSharpFile cs)
            {
                yield return (cs.Path, cs.Content);
            }
            else if (file is XamlFile xaml)
            {
                yield return (xaml.Path, xaml.Content);
                if (xaml.CodeBehind != null)
                {
                    yield return (xaml.Path + ".cs", xaml.CodeBehind);
                }
            }
        }
    }
}