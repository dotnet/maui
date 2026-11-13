using System;
using System.Globalization;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui34713 : ContentPage
{
	public Maui34713()
	{
		InitializeComponent();
	}

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests()
		{
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			Application.SetCurrentApplication(null);
			DispatcherProvider.SetCurrent(null);
		}

		const string SharedCs = @"
using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
public class Maui34713ViewModel { public bool IsActive { get; set; } public string Name { get; set; } = """"; }
public class Maui34713BoolToTextConverter : IValueConverter {
		public object Convert(object v, Type t, object p, CultureInfo c) => v is true ? ""Active"" : ""Inactive"";
		public object ConvertBack(object v, Type t, object p, CultureInfo c) => throw new NotImplementedException();
	}
}";

[Fact]
internal void SourceGenResolvesConverterAtCompileTime_ImplicitResources()
{
	// When converter IS in page resources (implicit), source gen should
	// resolve it at compile time - no runtime ProvideValue needed.
	var xaml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests""
x:Class=""Microsoft.Maui.Controls.Xaml.UnitTests.Maui34713Test1"">
<ContentPage.Resources>
<local:Maui34713BoolToTextConverter x:Key=""BoolToTextConverter"" />
</ContentPage.Resources>
<VerticalStackLayout x:DataType=""local:Maui34713ViewModel"">
<Label x:Name=""label0"" Text=""{Binding IsActive, Converter={StaticResource BoolToTextConverter}}"" />
</VerticalStackLayout>
	</ContentPage>";

	var cs = @"
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Maui34713Test1 : ContentPage { public Maui34713Test1() { InitializeComponent(); } }
	}" + SharedCs;

	var result = CreateMauiCompilation()
	.WithAdditionalSource(cs, hintName: "Maui34713Test1.xaml.cs")
	.RunMauiSourceGenerator(new AdditionalXamlFile("Issues/Maui34713Test1.xaml", xaml, TargetFramework: "net10.0"));

	var generated = result.GeneratedInitializeComponent();

	Assert.Contains("TypedBinding", generated, StringComparison.Ordinal);
	// Converter should be resolved at compile time - no ProvideValue call
	Assert.DoesNotContain(".ProvideValue(", generated, StringComparison.Ordinal);
}

[Fact]
internal void SourceGenResolvesConverterAtCompileTime_ExplicitResourceDictionary()
{
	// When converter IS in page resources (explicit RD), source gen should
	// also resolve it at compile time.
	var xaml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ContentPage xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
xmlns:local=""clr-namespace:Microsoft.Maui.Controls.Xaml.UnitTests""
x:Class=""Microsoft.Maui.Controls.Xaml.UnitTests.Maui34713Test2"">
<ContentPage.Resources>
<ResourceDictionary>
<local:Maui34713BoolToTextConverter x:Key=""BoolToTextConverter"" />
</ResourceDictionary>
</ContentPage.Resources>
<VerticalStackLayout x:DataType=""local:Maui34713ViewModel"">
<Label x:Name=""label0"" Text=""{Binding IsActive, Converter={StaticResource BoolToTextConverter}}"" />
</VerticalStackLayout>
	</ContentPage>";

	var cs = @"
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
[XamlProcessing(XamlInflator.Runtime, true)]
public partial class Maui34713Test2 : ContentPage { public Maui34713Test2() { InitializeComponent(); } }
	}" + SharedCs;

	var result = CreateMauiCompilation()
	.WithAdditionalSource(cs, hintName: "Maui34713Test2.xaml.cs")
	.RunMauiSourceGenerator(new AdditionalXamlFile("Issues/Maui34713Test2.xaml", xaml, TargetFramework: "net10.0"));

	var generated = result.GeneratedInitializeComponent();

	Assert.Contains("TypedBinding", generated, StringComparison.Ordinal);
	// Converter should be resolved at compile time - no ProvideValue call
	Assert.DoesNotContain(".ProvideValue(", generated, StringComparison.Ordinal);
}

[Fact]
internal void SourceGenCompilesBindingWithConverterToTypedBinding()
{
	// When the converter is NOT in page resources, the binding should
	// still be compiled into a TypedBinding.
	var result = CreateMauiCompilation()
	.WithAdditionalSource(
	@"using System;
	using System.Globalization;
	using Microsoft.Maui.Controls;
	using Microsoft.Maui.Controls.Xaml;

	namespace Microsoft.Maui.Controls.Xaml.UnitTests;

	[XamlProcessing(XamlInflator.Runtime, true)]
	public partial class Maui34713 : ContentPage
	{
		public Maui34713() => InitializeComponent();
	}

	public class Maui34713ViewModel
	{
		public bool IsActive { get; set; }
		public string Name { get; set; } = string.Empty;
	}

	public class Maui34713BoolToTextConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> value is true ? ""Active"" : ""Inactive"";

		public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
		=> throw new NotImplementedException();
	}
	")
	.RunMauiSourceGenerator(typeof(Maui34713));

	var generated = result.GeneratedInitializeComponent();

	Assert.Contains("TypedBinding", generated, StringComparison.Ordinal);
	Assert.Contains("Converter = extension.Converter", generated, StringComparison.Ordinal);
	Assert.DoesNotContain("new global::Microsoft.Maui.Controls.Binding(", generated, StringComparison.Ordinal);
}

[Theory]
[XamlInflatorData]
internal void BindingWithConverterFromAppResourcesWorksCorrectly(XamlInflator inflator)
{
	var mockApp = new MockApplication();
	mockApp.Resources.Add("BoolToTextConverter", new Maui34713BoolToTextConverter());
	Application.SetCurrentApplication(mockApp);

	var page = new Maui34713(inflator);
	page.BindingContext = new Maui34713ViewModel { IsActive = true, Name = "Test" };

	Assert.Equal("Active", page.label0.Text);
	Assert.Equal("Test", page.label1.Text);
}
}
}

#nullable enable

public class Maui34713ViewModel
{
	public bool IsActive { get; set; }
	public string Name { get; set; } = string.Empty;
}

public class Maui34713BoolToTextConverter : IValueConverter
{
	public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
	=> value is true ? "Active" : "Inactive";

	public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
	=> throw new NotImplementedException();
}
