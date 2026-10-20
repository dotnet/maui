#nullable enable

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Microsoft.Maui.Dispatching;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload.AiAssisted;

/// <summary>
/// Wave-2 Phase-1 nested/custom-control hot reload coverage (NC-01..NC-03).
/// Fixture: a same-compilation "ProbeCard" (<see cref="global::Microsoft.Maui.Controls.ContentView"/>)
/// used twice per page, each instance carrying its own nested
/// <c>&lt;ProbeCard.Resources&gt;</c> keyed converter and a child <c>Label</c> bound via
/// <c>x:Reference</c> back to its own card. This is a generic, anonymized stand-in for the common
/// "reusable card control with per-instance local resources" shape — no proprietary names or text
/// are used or copied from any specific application.
/// </summary>
[Collection("XamlHotReloadTests")]
public partial class NestedControlHotReloadTests : IDisposable
{
	public NestedControlHotReloadTests() => DispatcherProvider.SetCurrent(new StubDispatcherProvider());

	public void Dispose() => DispatcherProvider.SetCurrent(null);

	const string PageClass = "TestAiAssisted.MainPage";

	const string PageStub = """
		namespace TestAiAssisted;

		public partial class MainPage : global::Microsoft.Maui.Controls.ContentPage
		{
			private partial void InitializeComponent();

			public void InitializeComponentRuntime() { }

			public MainPage()
			{
				InitializeComponent();
			}

			// x:Name'd elements below need a matching field for the generator's InitializeComponent
			// to assign into (the harness only wires XamlGenerator, not the separate code-behind
			// field-declaration generator that normally supplies these in a real build).
			private global::TestAiAssisted.ProbeCard CardA = default!;
			private global::TestAiAssisted.ProbeCard CardB = default!;
			private global::Microsoft.Maui.Controls.Label CardALabel = default!;
			private global::Microsoft.Maui.Controls.Label CardBLabel = default!;
		}
		""";

	const string ProbeCardStub = """
		namespace TestAiAssisted;

		// Generic same-compilation custom control: a reusable "card" with its own bindable Value.
		public class ProbeCard : global::Microsoft.Maui.Controls.ContentView
		{
			public static readonly global::Microsoft.Maui.Controls.BindableProperty ValueProperty =
				global::Microsoft.Maui.Controls.BindableProperty.Create(nameof(Value), typeof(string), typeof(ProbeCard), default(string));

			public string? Value
			{
				get => (string?)GetValue(ValueProperty);
				set => SetValue(ValueProperty, value);
			}
		}
		""";

	const string GeneratedProbeCardStub = """
		namespace TestAiAssisted;

		public partial class ProbeCard : global::Microsoft.Maui.Controls.ContentView
		{
			public static readonly global::Microsoft.Maui.Controls.BindableProperty ValueProperty =
				global::Microsoft.Maui.Controls.BindableProperty.Create(nameof(Value), typeof(string), typeof(ProbeCard), default(string));

			public string? Value
			{
				get => (string?)GetValue(ValueProperty);
				set => SetValue(ValueProperty, value);
			}

			private partial void InitializeComponent();

			private global::TestAiAssisted.ProbeCard Root = default!;
			private global::Microsoft.Maui.Controls.Label CardLabel = default!;

			public ProbeCard()
			{
				InitializeComponent();
			}
		}
		""";

	const string ProbeConverterStubs = """
		namespace TestAiAssisted;

		public sealed class ProbeConverterOriginal : global::Microsoft.Maui.Controls.IValueConverter
		{
			public static int InvocationCount;

			public object? Convert(object? value, global::System.Type targetType, object? parameter, global::System.Globalization.CultureInfo culture)
			{
				InvocationCount++;
				return $"Original:{value}";
			}

			public object? ConvertBack(object? value, global::System.Type targetType, object? parameter, global::System.Globalization.CultureInfo culture) => value;
		}

		public sealed class ProbeConverterUpdated : global::Microsoft.Maui.Controls.IValueConverter
		{
			public static int InvocationCount;

			public object? Convert(object? value, global::System.Type targetType, object? parameter, global::System.Globalization.CultureInfo culture)
			{
				InvocationCount++;
				return $"Updated:{value}";
			}

			public object? ConvertBack(object? value, global::System.Type targetType, object? parameter, global::System.Globalization.CultureInfo culture) => value;
		}
		""";

	const string XamlTemplate = """
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             xmlns:local="clr-namespace:TestAiAssisted"
		             x:Class="TestAiAssisted.MainPage">
		  <VerticalStackLayout>
		    <local:ProbeCard x:Name="CardA" Value="__CARD_A_VALUE__">
		      <local:ProbeCard.Resources>
		        <ResourceDictionary>
		          <local:__CARD_A_CONVERTER__ x:Key="Fmt" />
		        </ResourceDictionary>
		      </local:ProbeCard.Resources>
		      <Label x:Name="CardALabel" Text="{Binding Source={x:Reference CardA}, Path=Value, Converter={StaticResource Fmt}}" />
		    </local:ProbeCard>
		    <local:ProbeCard x:Name="CardB" Value="__CARD_B_VALUE__">
		      <local:ProbeCard.Resources>
		        <ResourceDictionary>
		          <local:__CARD_B_CONVERTER__ x:Key="Fmt" />
		        </ResourceDictionary>
		      </local:ProbeCard.Resources>
		      <Label x:Name="CardBLabel" Text="{Binding Source={x:Reference CardB}, Path=Value, Converter={StaticResource Fmt}}" />
		    </local:ProbeCard>
		  </VerticalStackLayout>
		</ContentPage>
		""";

	static string Xaml(string cardAValue, string cardAConverter, string cardBValue, string cardBConverter) =>
		XamlTemplate
			.Replace("__CARD_A_VALUE__", cardAValue, StringComparison.Ordinal)
			.Replace("__CARD_A_CONVERTER__", cardAConverter, StringComparison.Ordinal)
			.Replace("__CARD_B_VALUE__", cardBValue, StringComparison.Ordinal)
			.Replace("__CARD_B_CONVERTER__", cardBConverter, StringComparison.Ordinal);

	static int GetStaticInvocationCount(object anyInstanceFromGeneratedAssembly, string fullyQualifiedTypeName)
	{
		var assembly = anyInstanceFromGeneratedAssembly.GetType().Assembly;
		var type = assembly.GetType(fullyQualifiedTypeName) ?? throw new InvalidOperationException($"Type '{fullyQualifiedTypeName}' not found in generated assembly.");
		var field = type.GetField("InvocationCount", BindingFlags.Public | BindingFlags.Static) ?? throw new InvalidOperationException("InvocationCount field not found.");
		return (int)field.GetValue(null)!;
	}

	// PageStub's x:Name'd fields are declared `private` (matching the real code-behind field
	// modifier default) — the harness does not run the code-behind field-declaration generator
	// that would normally expose a public accessor, so tests reach them via reflection.
	static T GetNamedField<T>(object page, string fieldName) where T : class
	{
		var field = page.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
			?? throw new InvalidOperationException($"Field '{fieldName}' not found on '{page.GetType()}'.");
		return (T)field.GetValue(page)!;
	}

	static void ApplyGeneratedUpdate(object root)
	{
		var updateMethod = root.GetType().GetMethod(
			"UpdateComponent",
			BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
			?? throw new InvalidOperationException($"UpdateComponent not found on '{root.GetType()}'.");
		updateMethod.Invoke(root, null);
	}

	static IReadOnlyDictionary<string, XamlHotReloadDocument> Documents(string page, string probeCard) =>
		new Dictionary<string, XamlHotReloadDocument>(StringComparer.Ordinal)
		{
			["MainPage.xaml"] = new XamlHotReloadDocument(page),
			["ProbeCard.xaml"] = new XamlHotReloadDocument(probeCard),
		};

	const string Nc04PageTemplate = """
		<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             xmlns:local="clr-namespace:TestAiAssisted"
		             x:Class="TestAiAssisted.MainPage">
		  <VerticalStackLayout>
		    <local:ProbeCard x:Name="CardA" Value="__CARD_A_VALUE__" />
		    <local:ProbeCard x:Name="CardB" Value="__CARD_B_VALUE__" />
		  </VerticalStackLayout>
		</ContentPage>
		""";

	const string Nc04ProbeCardTemplate = """
		<ContentView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
		             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
		             xmlns:local="clr-namespace:TestAiAssisted"
		             x:Class="TestAiAssisted.ProbeCard"
		             x:Name="Root">
		  <ContentView.Resources>
		    <ResourceDictionary>
		      <local:__CONVERTER__ x:Key="Fmt" />
		    </ResourceDictionary>
		  </ContentView.Resources>
		  <Label x:Name="CardLabel"
		         Text="{Binding Source={x:Reference Root}, Path=Value, Converter={StaticResource Fmt}}" />
		</ContentView>
		""";

	static string Nc04Page(string cardAValue, string cardBValue) =>
		Nc04PageTemplate
			.Replace("__CARD_A_VALUE__", cardAValue, StringComparison.Ordinal)
			.Replace("__CARD_B_VALUE__", cardBValue, StringComparison.Ordinal);

	static string Nc04ProbeCard(string converter) =>
		Nc04ProbeCardTemplate.Replace("__CONVERTER__", converter, StringComparison.Ordinal);

	static string GetProbeCardUpdateV2(XamlHotReloadGeneration generation)
	{
		var updateComponentSource = Assert.Single(generation[1].GeneratedRoots,
			static root => root.TypeName == "TestAiAssisted.ProbeCard").UpdateComponentSource;
		Assert.NotNull(updateComponentSource);
		return updateComponentSource!;
	}

	static void AssertProbeCardResourceDecline(string updateComponentSource)
	{
		Assert.Contains("XamlComponentRegistry.GetResourceKeys(this)", updateComponentSource, StringComparison.Ordinal);
		Assert.Contains("XamlComponentRegistry.RegisterResourceKeys(this, global::System.Array.Empty<string>())", updateComponentSource, StringComparison.Ordinal);
		Assert.DoesNotContain("ProbeConverterUpdated", updateComponentSource, StringComparison.Ordinal);
	}

	sealed class StubDispatcher : IDispatcher
	{
		public bool IsDispatchRequired => false;

		public bool Dispatch(Action action)
		{
			action();
			return true;
		}

		public bool DispatchDelayed(TimeSpan delay, Action action)
		{
			action();
			return true;
		}

		public IDispatcherTimer CreateTimer() => throw new NotSupportedException();
	}

	sealed class StubDispatcherProvider : IDispatcherProvider
	{
		public IDispatcher GetForCurrentThread() => new StubDispatcher();
	}
}
