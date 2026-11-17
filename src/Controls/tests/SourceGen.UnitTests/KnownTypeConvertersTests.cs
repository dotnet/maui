using System;
using System.Globalization;
using Microsoft.CodeAnalysis;
using System.Linq;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests
{
	[Collection("CultureTests")]
	public class KnownTypeConvertersTests : SourceGenXamlInitializeComponentTestBase, IDisposable
	{
		private readonly CultureInfo? _originalCulture;
		private readonly CultureInfo? _originalUICulture;

		public KnownTypeConvertersTests()
		{
			_originalCulture = System.Threading.Thread.CurrentThread.CurrentCulture;
			_originalUICulture = System.Threading.Thread.CurrentThread.CurrentUICulture;
		}

		void IDisposable.Dispose()
		{
			if (_originalCulture is not null)
			{
				System.Threading.Thread.CurrentThread.CurrentCulture = _originalCulture;
			}

			if (_originalUICulture is not null)
			{
				System.Threading.Thread.CurrentThread.CurrentUICulture = _originalUICulture;
			}
		}

		[Theory]
		[InlineData("en-US", "1.5*")]
		[InlineData("fr-FR", "2.5*")]
		[InlineData("de-DE", "3.14*")]
		[InlineData("es-ES", "0.75*")]
		[InlineData("ru-RU", "1.414*")]
		public void GridLengthTypeConverter_StarValues_ProducesConsistentOutput_AcrossCultures(string cultureName, string gridLengthValue)
		{
			// Set both current and UI cultures to test locale
			System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
			System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);

			var xaml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<ContentPage
	xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	x:Class=""Test.TestPage"">
	<Grid>
		<Grid.ColumnDefinitions>
			<ColumnDefinition Width=""{gridLengthValue}"" />
		</Grid.ColumnDefinitions>
		<Label Text=""Test"" Grid.Column=""0"" />
	</Grid>
</ContentPage>";

				var code = @"using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}";

				var (result, generated) = RunGenerator(xaml, code);
				
				// Should not have any diagnostics/errors

				Assert.False(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
					$"Generated code should not have errors. Diagnostics: {string.Join(", ", result.Diagnostics.Select(d => d.ToString()))}");

				// The generated code should contain a properly formatted GridLength with period as decimal separator
				// regardless of the current culture
				Assert.NotNull(generated);
			// Extract the numeric value from the input (e.g., "2.5*" -> "2.5")
			var numericPart = gridLengthValue.Substring(0, gridLengthValue.Length - 1);
			
			// The generated code should use period as decimal separator (culture-invariant)
			// and should contain the GridLength constructor with Star unit type
			Assert.True(generated!.Contains($"new global::Microsoft.Maui.GridLength({numericPart}, global::Microsoft.Maui.GridUnitType.Star)", StringComparison.Ordinal),
				$"Generated code should contain culture-invariant GridLength with value {numericPart}. Generated code: {generated}");
		}

			[Theory]
			[InlineData("en-US", "100.5")]
			[InlineData("fr-FR", "200.25")]
			[InlineData("de-DE", "50.75")]
			[InlineData("es-ES", "150.125")]
			[InlineData("ru-RU", "75.875")]
			public void GridLengthTypeConverter_AbsoluteValues_ProducesConsistentOutput_AcrossCultures(string cultureName, string gridLengthValue)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
			System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);

			var xaml = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<ContentPage
	xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	x:Class=""Test.TestPage"">
	<Grid>
		<Grid.RowDefinitions>
			<RowDefinition Height=""{gridLengthValue}"" />
		</Grid.RowDefinitions>
		<Label Text=""Test"" Grid.Row=""0"" />
	</Grid>
</ContentPage>";

				var code = @"using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}";

				var (result, generated) = RunGenerator(xaml, code);
				
				Assert.False(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
					$"Generated code should not have errors. Diagnostics: {string.Join(", ", result.Diagnostics.Select(d => d.ToString()))}");

				Assert.NotNull(generated);
				
				// The generated code should use period as decimal separator and Absolute unit type
				Assert.True(generated!.Contains($"new global::Microsoft.Maui.GridLength({gridLengthValue}, global::Microsoft.Maui.GridUnitType.Absolute)", StringComparison.Ordinal),
					$"Generated code should contain culture-invariant GridLength with absolute value {gridLengthValue}. Generated code: {generated}");
		}

			[Theory]
			[InlineData("en-US")]
			[InlineData("fr-FR")]
			[InlineData("de-DE")]
			[InlineData("es-ES")]
			[InlineData("ru-RU")]
			public void GridLengthTypeConverter_SpecialValues_ProducesConsistentOutput_AcrossCultures(string cultureName)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
			System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName);

			var xaml = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<ContentPage
	xmlns=""http://schemas.microsoft.com/dotnet/2021/maui""
	xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
	x:Class=""Test.TestPage"">
	<Grid>
		<Grid.ColumnDefinitions>
			<ColumnDefinition Width=""*"" />
			<ColumnDefinition Width=""Auto"" />
		</Grid.ColumnDefinitions>
		<Grid.RowDefinitions>
			<RowDefinition Height=""*"" />
			<RowDefinition Height=""Auto"" />
		</Grid.RowDefinitions>
		<Label Text=""Test"" Grid.Column=""0"" Grid.Row=""0"" />
	</Grid>
</ContentPage>";

			var code = @"using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Test;

[XamlProcessing(XamlInflator.SourceGen)]
public partial class TestPage : ContentPage
{
	public TestPage()
	{
		InitializeComponent();
	}
}";

				var (result, generated) = RunGenerator(xaml, code);
				
				Assert.False(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
					$"Generated code should not have errors. Diagnostics: {string.Join(", ", result.Diagnostics.Select(d => d.ToString()))}");

				Assert.NotNull(generated);
				
				// Check for special values that should be culture-independent
				Assert.True(generated!.Contains("global::Microsoft.Maui.GridLength.Star", StringComparison.Ordinal),
					"Generated code should contain GridLength.Star for '*' values");
				Assert.True(generated.Contains("global::Microsoft.Maui.GridLength.Auto", StringComparison.Ordinal),
					"Generated code should contain GridLength.Auto for 'Auto' values");
		}

			[Fact]
		public void GridLengthTypeConverter_StarValue_GeneratesGridLengthStar()
		{
			const string xaml = """
				<?xml version="1.0" encoding="UTF-8"?>
				<ContentPage
					xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
					xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
					x:Class="Test.TestPage">
					<Grid>
						<Grid.ColumnDefinitions>
							<ColumnDefinition Width="*"/>
						</Grid.ColumnDefinitions>
					</Grid>
				</ContentPage>
				""";

			const string code = """
				using System;
				using Microsoft.Maui.Controls;
				using Microsoft.Maui.Controls.Xaml;

				namespace Test;

				[XamlProcessing(XamlInflator.SourceGen)]
				public partial class TestPage : ContentPage
				{
					public TestPage()
					{
						InitializeComponent();
					}
				}
				""";

			var (result, generated) = RunGenerator(xaml, code);
			
			Assert.False(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
				$"Generated code should not have errors. Diagnostics: {string.Join(", ", result.Diagnostics.Select(d => d.ToString()))}");

			Assert.NotNull(generated);
			
			// Should generate GridLength.Star for "*" value
			Assert.True(generated!.Contains("global::Microsoft.Maui.GridLength.Star", StringComparison.Ordinal),
				"Generated code should contain GridLength.Star for '*' value");
		}

			[Fact]
		public void GridLengthTypeConverter_AutoValue_GeneratesGridLengthAuto()
		{
			const string xaml = """
				<?xml version="1.0" encoding="UTF-8"?>
				<ContentPage
					xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
					xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
					x:Class="Test.TestPage">
					<Grid>
						<Grid.RowDefinitions>
							<RowDefinition Height="Auto"/>
						</Grid.RowDefinitions>
					</Grid>
				</ContentPage>
				""";

			const string code = """
				using System;
				using Microsoft.Maui.Controls;
				using Microsoft.Maui.Controls.Xaml;

				namespace Test;

				[XamlProcessing(XamlInflator.SourceGen)]
				public partial class TestPage : ContentPage
				{
					public TestPage()
					{
						InitializeComponent();
					}
				}
				""";

			var (result, generated) = RunGenerator(xaml, code);
			
			Assert.False(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
				$"Generated code should not have errors. Diagnostics: {string.Join(", ", result.Diagnostics.Select(d => d.ToString()))}");

			Assert.NotNull(generated);
			
			// Should generate GridLength.Auto for "Auto" value
			Assert.True(generated!.Contains("global::Microsoft.Maui.GridLength.Auto", StringComparison.Ordinal),
				"Generated code should contain GridLength.Auto for 'Auto' value");
		}

			[Fact]
		public void EnumTypeConverter()
		{
			const string xaml = """
				<?xml version="1.0" encoding="UTF-8"?>
				<ContentPage
					xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
					xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
					x:Class="Test.TestPage">
					<FlexLayout Direction="Row" />
				</ContentPage>
				""";

			const string code = """
				using System;
				using Microsoft.Maui.Controls;
				using Microsoft.Maui.Controls.Xaml;

				namespace Test;

				[XamlProcessing(XamlInflator.SourceGen)]
				public partial class TestPage : ContentPage
				{
					public TestPage()
					{
						InitializeComponent();
					}
				}
				""";

			var (result, generated) = RunGenerator(xaml, code);
			
			Assert.False(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
				$"Generated code should not have errors. Diagnostics: {string.Join(", ", result.Diagnostics.Select(d => d.ToString()))}");

			Assert.NotNull(generated);
			
			// Should generate FlexDirection.Row for "Row" value
			Assert.True(generated!.Contains("flexLayout.SetValue(global::Microsoft.Maui.Controls.FlexLayout.DirectionProperty, global::Microsoft.Maui.Layouts.FlexDirection.Row);", StringComparison.Ordinal),
				"Generated code should contain FlexDirection.Row for 'Row' value");
		}

		[Theory]
		[InlineData("Spring(0.4)", "global::Microsoft.Maui.Easing.Spring(0.4d)")]
		[InlineData("Easing.Spring()", "global::Microsoft.Maui.Easing.Spring()")]
		[InlineData("CubicBezier(0.4, 0, 0.2, 1)", "global::Microsoft.Maui.Easing.CubicBezier(0.4d, 0d, 0.2d, 1d)")]
		[InlineData("Steps(4, start)", "global::Microsoft.Maui.Easing.Steps(4, true)")]
		[InlineData("Hold", "global::Microsoft.Maui.Easing.Hold")]
		public void EasingTypeConverter_FunctionForms_GenerateFactoryCalls(string easing, string expected)
		{
			System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");

			var (result, generated) = RunGenerator(EasingXaml(easing), EasingCode);

			Assert.False(result.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error),
				$"Generated code should not have errors. Diagnostics: {string.Join(", ", result.Diagnostics.Select(d => d.ToString()))}");
			Assert.Contains(expected, generated, StringComparison.Ordinal);
		}

		[Theory]
		[InlineData("Spring(2)")]
		[InlineData("Sprung(0.3)")]
		[InlineData("CubicBezier(1.5, 0, 0.2, 1)")]
		[InlineData("Steps(0)")]
		public void EasingTypeConverter_InvalidFunctionForms_FailTheBuild(string easing)
		{
			var (result, _) = RunGenerator(EasingXaml(easing), EasingCode);

			Assert.Contains(result.Diagnostics, d => d.Id == "MAUIG1010" && d.Severity == DiagnosticSeverity.Error);
		}

		static string EasingXaml(string easing) => $"""
			<?xml version="1.0" encoding="UTF-8"?>
			<ContentPage
				xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
				xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
				xmlns:anim="http://schemas.microsoft.com/dotnet/2021/maui/animations"
				x:Class="Test.TestPage">
				<Label>
					<VisualStateManager.VisualStateGroups>
						<VisualStateGroup Name="CommonStates">
							<VisualStateGroup.Transitions>
								<anim:VisualTransition Easing="{easing}" />
							</VisualStateGroup.Transitions>
							<VisualState Name="Normal" />
						</VisualStateGroup>
					</VisualStateManager.VisualStateGroups>
				</Label>
			</ContentPage>
			""";

		const string EasingCode = """
			using System;
			using Microsoft.Maui.Controls;
			using Microsoft.Maui.Controls.Xaml;

			namespace Test;

			[XamlProcessing(XamlInflator.SourceGen)]
			public partial class TestPage : ContentPage
			{
				public TestPage()
				{
					InitializeComponent();
				}
			}
			""";
	}
}
