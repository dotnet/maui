using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.SourceGen.SourceGeneratorDriver;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests;

[Collection("XamlHotReloadTests")]
public class Maui40328Tests : SourceGenXamlInitializeComponentTestBase
{
	const string CodeBehind = """
		using Microsoft.Maui.Controls;
		namespace TestApp;
		public partial class TestPage : ContentPage
		{
			public TestPage() => InitializeComponent();
		}
		""";

	[Theory]
	[InlineData(false, false)]
	[InlineData(true, false)]
	[InlineData(false, true)]
	[InlineData(true, true)]
	public void InlineCodeCompilesWithNamedElementsAndEventHandlers(bool codeAfterContent, bool incrementalHotReload)
	{
		const string code = """
			<x:Code><![CDATA[
				using System;
				using Text = System.String;
				int clicks;
				string FormatTitle() => Text.Concat("Clicks: ", clicks);
				void OnClicked(object sender, EventArgs e)
				{
					clicks++;
					MyButton.Text = FormatTitle();
				}
			]]></x:Code>
			""";
		const string content = """
			<VerticalStackLayout>
				<Button x:Name="MyButton" Text="Click me" Clicked="OnClicked" />
				<Label Text="{FormatTitle()}" />
			</VerticalStackLayout>
			""";
		var xaml = $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.TestPage">
				{{(codeAfterContent ? content : code)}}
				{{(codeAfterContent ? code : content)}}
				<x:Code><![CDATA[
					string AnotherMember() => FormatTitle();
				]]></x:Code>
			</ContentPage>
			""";

		var (result, compilation) = Generate(xaml, incrementalHotReload);
		Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(compilation.GetDiagnostics(cancellationToken: TestContext.Current.CancellationToken).Where(d => d.Severity == DiagnosticSeverity.Error));

		var sources = result.Results.Single().GeneratedSources;
		var codeBehind = sources.Single(s => s.HintName.EndsWith(".sg.cs", StringComparison.Ordinal)).SourceText.ToString();
		var initializeComponent = sources.Single(s => s.HintName.EndsWith(".xsg.cs", StringComparison.Ordinal)
			&& !s.HintName.EndsWith(".uc.xsg.cs", StringComparison.Ordinal)).SourceText.ToString();
		Assert.Contains("using Text = System.String;", codeBehind, StringComparison.Ordinal);
		Assert.Contains("AnotherMember()", codeBehind, StringComparison.Ordinal);
		Assert.Contains("OnClicked", initializeComponent, StringComparison.Ordinal);
	}

	[Fact]
	public void InlineCodeRespectsNullableProjectContext()
	{
		const string xaml = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.TestPage">
				<x:Code><![CDATA[
					using Names = System.Collections.Generic.List<string?>;
					Names names = new();
					string Invalid() => null;
					void OnClicked(object? sender, System.EventArgs args) { }
				]]></x:Code>
				<Button Clicked="OnClicked" />
			</ContentPage>
			""";
		var (result, nullableEnabled) = Generate(xaml, incrementalHotReload: true, nullableContext: NullableContextOptions.Enable);
		var (_, nullableDisabled) = Generate(xaml, incrementalHotReload: true, nullableContext: NullableContextOptions.Disable);

		Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		var enabledDiagnostics = nullableEnabled.GetDiagnostics(cancellationToken: TestContext.Current.CancellationToken);
		Assert.DoesNotContain(enabledDiagnostics, d => d.Id == "CS8669" || d.Severity == DiagnosticSeverity.Error);
		Assert.Contains(enabledDiagnostics, d => d.Id == "CS8603");
		var disabledDiagnostics = nullableDisabled.GetDiagnostics(cancellationToken: TestContext.Current.CancellationToken);
		Assert.Contains(disabledDiagnostics, d => d.Id is "CS8632" or "CS8669");
		Assert.DoesNotContain(disabledDiagnostics, d => d.Id == "CS8603");
	}

	[Fact]
	public void InlineCodeCompilesWithNullableDisabled()
	{
		const string xaml = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.TestPage">
				<x:Code><![CDATA[void OnClicked(object sender, System.EventArgs args) { }]]></x:Code>
				<Button Clicked="OnClicked" />
			</ContentPage>
			""";
		var (result, compilation) = Generate(xaml, incrementalHotReload: true);
		var csharpCompilation = (CSharpCompilation)compilation;
		var nullableDisabled = csharpCompilation.WithOptions(csharpCompilation.Options.WithNullableContextOptions(NullableContextOptions.Disable));

		Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.DoesNotContain(nullableDisabled.GetDiagnostics(cancellationToken: TestContext.Current.CancellationToken), d => d.Id == "CS8669" || d.Severity == DiagnosticSeverity.Error);
	}

	[Theory]
	[InlineData(true, "MAUIX2015")]
	[InlineData(false, "MAUIX2016")]
	public void InvalidInlineCodePlacementReportsDiagnostic(bool nested, string diagnosticId)
	{
		var xaml = nested
			? """
				<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
				             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
				             x:Class="TestApp.TestPage">
					<VerticalStackLayout><x:Code><![CDATA[void Handler() { }]]></x:Code></VerticalStackLayout>
				</ContentPage>
				"""
			: """
				<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
				                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
					<x:Code><![CDATA[void Handler() { }]]></x:Code>
				</ResourceDictionary>
				""";

		var (result, _) = Generate(xaml, incrementalHotReload: false);
		Assert.Contains(result.Diagnostics, d => d.Id == diagnosticId);
		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIG1001");
	}

	[Theory]
	[InlineData(false, "SourceGen", "MAUIX2012")]
	[InlineData(true, "Runtime", "MAUIX2023")]
	[InlineData(true, "XamlC", "MAUIX2023")]
	[InlineData(true, "SourceGen,XamlC", "MAUIX2023")]
	[InlineData(true, "SourceGen,Runtime", "MAUIX2023")]
	public void InlineCodeRequiresPreviewAndSourceGen(bool preview, string inflator, string diagnosticId)
	{
		const string xaml = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.TestPage">
				<x:Code><![CDATA[void Handler() { }]]></x:Code>
			</ContentPage>
			""";

		var (result, _) = Generate(xaml, incrementalHotReload: false, preview, inflator);
		var diagnostic = Assert.Single(result.Diagnostics, d => d.Id == diagnosticId);
		Assert.Equal("Maui40328.xaml", diagnostic.Location.GetLineSpan().Path);
		Assert.DoesNotContain(result.Diagnostics, d => d.Id == "MAUIG1001");
	}

	[Fact]
	public void InlineCodeRejectsChildElements()
	{
		const string xaml = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.TestPage">
				<x:Code><Label /></x:Code>
			</ContentPage>
			""";
		var (result, _) = Generate(xaml, incrementalHotReload: false);
		Assert.Contains(result.Diagnostics, d => d.GetMessage().Contains("x:Code must contain only text or CDATA", StringComparison.Ordinal));
	}

	[Fact]
	public void InlineCodeCanDefineTheConstructorWithoutACodeBehindFile()
	{
		const string xaml = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.TestPage">
				<x:Code><![CDATA[
					public TestPage() => InitializeComponent();
					void OnClicked(object sender, System.EventArgs args) { }
				]]></x:Code>
				<Button Clicked="OnClicked" />
			</ContentPage>
			""";
		var compilation = CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText("", cancellationToken: TestContext.Current.CancellationToken));
		var file = new AdditionalXamlFile("Maui40328.xaml", xaml, EnablePreviewFeatures: true);
		var (_, result, updatedCompilation, _) = RunGeneratorWithChangesAndUpdateCompilation<XamlGenerator>(
			compilation, (driver, currentCompilation) => (driver, currentCompilation), file);

		Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(updatedCompilation.GetDiagnostics(cancellationToken: TestContext.Current.CancellationToken).Where(d => d.Severity == DiagnosticSeverity.Error));
	}

	[Fact]
	public void InlineCodeSupportsThe2006NamespaceAndEscapedText()
	{
		const string xaml = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
			             x:Class="TestApp.TestPage">
				<x:Code>public bool Value =&gt; 1 &lt; 2;</x:Code>
			</ContentPage>
			""";
		var (result, compilation) = Generate(xaml, incrementalHotReload: false);

		Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(compilation.GetDiagnostics(cancellationToken: TestContext.Current.CancellationToken).Where(d => d.Severity == DiagnosticSeverity.Error));
	}

	[Theory]
	[InlineData(false)]
	[InlineData(true)]
	public void ConditionalUsingsRespectCompilationSymbols(bool defineFirst)
	{
		const string xaml = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.TestPage">
				<x:Code><![CDATA[
					#if FIRST
					using Alias = System.String;
					#else
					using Alias = System.Int32;
					#endif
					#if FIRST
					public Alias Value => "first";
					#else
					public Alias Value => 42;
					#endif
				]]></x:Code>
			</ContentPage>
			""";
		var options = new CSharpParseOptions(preprocessorSymbols: defineFirst ? ["FIRST"] : []);
		var compilation = CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(CodeBehind, options, cancellationToken: TestContext.Current.CancellationToken));
		var file = new AdditionalXamlFile("Maui40328.xaml", xaml, EnablePreviewFeatures: true);
		GeneratorDriver driver = CSharpGeneratorDriver.Create([new XamlGenerator().AsSourceGenerator()], parseOptions: options)
			.AddAdditionalTexts([file.Text])
			.WithUpdatedAnalyzerConfigOptions(CreateAnalyzerConfigOptionsProvider(file));
		driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out _, cancellationToken: TestContext.Current.CancellationToken);

		Assert.Empty(driver.GetRunResult().Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(updatedCompilation.GetDiagnostics(cancellationToken: TestContext.Current.CancellationToken).Where(d => d.Severity == DiagnosticSeverity.Error));
	}

	[Fact]
	public void GlobalUsingsFollowCSharpOrderingAcrossInlineCodeBlocks()
	{
		const string xaml = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.TestPage">
				<x:Code><![CDATA[
					using System;
					DateTime Now() => DateTime.Now;
				]]></x:Code>
				<x:Code><![CDATA[
					global using Number = System.Int32;
					Number Value => 42;
				]]></x:Code>
			</ContentPage>
			""";
		var (result, compilation) = Generate(xaml, incrementalHotReload: false);

		Assert.Empty(result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(compilation.GetDiagnostics(cancellationToken: TestContext.Current.CancellationToken).Where(d => d.Severity == DiagnosticSeverity.Error));
	}

	[Fact]
	public void CompilerErrorsInInlineCodeMapToTheXamlFile()
	{
		const string xaml = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.TestPage">
				<x:Code><![CDATA[
					using Alias = Missing.Namespace;
					void Handler()
					{
						MissingMethod();
					}
				]]></x:Code>
			</ContentPage>
			""";
		var (_, compilation) = Generate(xaml, incrementalHotReload: false);
		var diagnostics = compilation.GetDiagnostics(cancellationToken: TestContext.Current.CancellationToken).Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();

		foreach (var (id, text) in new[] { ("CS0246", "using Alias"), ("CS0103", "MissingMethod") })
		{
			var diagnostic = Assert.Single(diagnostics, d => d.Id == id);
			var mapped = diagnostic.Location.GetMappedLineSpan();
			Assert.Equal("Maui40328.xaml", mapped.Path);
			Assert.Equal(Array.FindIndex(xaml.Split('\n'), line => line.Contains(text, StringComparison.Ordinal)), mapped.StartLinePosition.Line);
		}
	}

	[Fact]
	public void ErrorsAfterXmlCommentsMapToTheCorrectLine()
	{
		const string xaml = """
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.TestPage">
				<x:Code>
					<![CDATA[void First() { }]]>
					<!-- A comment
					     spanning lines -->
					<![CDATA[
						void Second() => MissingMethod();
					]]>
				</x:Code>
			</ContentPage>
			""";
		var (_, compilation) = Generate(xaml, incrementalHotReload: false);
		var diagnostic = Assert.Single(compilation.GetDiagnostics(cancellationToken: TestContext.Current.CancellationToken).Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Equal("CS0103", diagnostic.Id);
		var mapped = diagnostic.Location.GetMappedLineSpan();

		Assert.Equal("Maui40328.xaml", mapped.Path);
		Assert.Equal(Array.FindIndex(xaml.Split('\n'), line => line.Contains("MissingMethod", StringComparison.Ordinal)), mapped.StartLinePosition.Line);
	}

	[Theory]
	[InlineData(false, false)]
	[InlineData(true, false)]
	[InlineData(true, true)]
	public void UpdatingInlineCodeRebindsEvents(bool incrementalHotReload, bool leaveIncompatibleOldHandler)
	{
		static string Page(string handler, bool incompatibleOldHandler) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestApp.TestPage">
				<x:Code><![CDATA[
					{{(incompatibleOldHandler ? "void FirstHandler() { }" : "")}}
					void {{handler}}(object sender, System.EventArgs args) { }
				]]></x:Code>
				<Button Clicked="{{handler}}" />
			</ContentPage>
			""";
		var compilation = CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(CodeBehind, cancellationToken: TestContext.Current.CancellationToken));
		var first = new AdditionalXamlFile("Maui40328.xaml", Page("FirstHandler", false), EnableIncrementalHotReload: incrementalHotReload, EnablePreviewFeatures: true);
		var second = new AdditionalXamlFile("Maui40328.xaml", Page("SecondHandler", leaveIncompatibleOldHandler), EnableIncrementalHotReload: incrementalHotReload, EnablePreviewFeatures: true);
		var (initial, updated, updatedCompilation, _) = RunGeneratorWithChangesAndUpdateCompilation<XamlGenerator>(
			compilation, (driver, currentCompilation) => (driver.ReplaceAdditionalText(first.Text, second.Text), currentCompilation), first);

		Assert.Empty(initial.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(updated.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error));
		Assert.Empty(updatedCompilation.GetDiagnostics(cancellationToken: TestContext.Current.CancellationToken).Where(d => d.Severity == DiagnosticSeverity.Error));
		var generated = updated.Results.Single().GeneratedSources.Single(s => s.HintName.EndsWith(".xsg.cs", StringComparison.Ordinal)
			&& !s.HintName.EndsWith(".uc.xsg.cs", StringComparison.Ordinal)).SourceText.ToString();
		Assert.Contains("SecondHandler", generated, StringComparison.Ordinal);
	}

	[MetadataUpdateFact]
	public void RenamingInlineHandlersUpdatesLiveInstances()
	{
		static string Page(string handler, string text, bool isStatic) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
				<x:Code><![CDATA[
					{{(isStatic ? "static " : "")}}void {{handler}}(object sender, System.EventArgs args)
						=> ((Microsoft.Maui.Controls.Button)sender).Text += "{{text}}";
				]]></x:Code>
				<Button Text="" Clicked="{{handler}}" />
			</ContentPage>
			""";
		const string pageSource = """
			namespace TestE2EApp;
			public partial class MainPage : Microsoft.Maui.Controls.ContentPage
			{
				public MainPage() => InitializeComponent();
			}
			""";
		foreach (var isStatic in new[] { false, true })
		{
			using var harness = new XamlHotReloadTestHarness(nameof(RenamingInlineHandlersUpdatesLiveInstances),
				"TestE2EApp.MainPage", pageSource);
			var first = new Dictionary<string, XamlHotReloadDocument>
			{
				["MainPage.xaml"] = new(Page("FirstHandler", "old", isStatic), EnablePreviewFeatures: true)
			};
			var second = new Dictionary<string, XamlHotReloadDocument>
			{
				["MainPage.xaml"] = new(Page("SecondHandler", "new", isStatic), EnablePreviewFeatures: true)
			};
			var generation = harness.GenerateDocuments(first, second);

			harness.RunLive(generation, live =>
			{
				var page = live.GetInstance<ContentPage>();
				var button = Assert.IsType<Button>(page.Content);
				((IButtonController)button).SendClicked();
				Assert.Equal("old", button.Text);

				button.Text = "";
				live.ApplyUpdate<ContentPage>(1, "SecondHandler");
				((IButtonController)button).SendClicked();
				Assert.Equal("new", button.Text);
			});
		}
	}

	[MetadataUpdateFact]
	public void EditingOnlyInlineCodeUpdatesTheExistingHandler()
	{
		static string Page(string text) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
				<x:Code><![CDATA[
					void OnClicked(object sender, System.EventArgs args)
						=> ((Microsoft.Maui.Controls.Button)sender).Text = "{{text}}";
				]]></x:Code>
				<Button Clicked="OnClicked" />
			</ContentPage>
			""";
		const string pageSource = """
			namespace TestE2EApp;
			public partial class MainPage : Microsoft.Maui.Controls.ContentPage
			{
				public MainPage() => InitializeComponent();
			}
			""";
		using var harness = new XamlHotReloadTestHarness(nameof(EditingOnlyInlineCodeUpdatesTheExistingHandler),
			"TestE2EApp.MainPage", pageSource);
		var first = new Dictionary<string, XamlHotReloadDocument>
		{
			["MainPage.xaml"] = new(Page("old"), EnablePreviewFeatures: true)
		};
		var second = new Dictionary<string, XamlHotReloadDocument>
		{
			["MainPage.xaml"] = new(Page("new"), EnablePreviewFeatures: true)
		};
		var generation = harness.GenerateDocuments(first, second);
		Assert.DoesNotContain("XamlComponentRegistry", generation[1].UpdateComponentSource!, StringComparison.Ordinal);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var button = Assert.IsType<Button>(page.Content);
			((IButtonController)button).SendClicked();
			Assert.Equal("old", button.Text);

			live.ApplyUpdate<ContentPage>(1, "OnClicked");
			((IButtonController)button).SendClicked();
			Assert.Equal("new", button.Text);
		});
	}

	[MetadataUpdateFact]
	public void RenamingInlineHandlerWithRemainingOverloadUpdatesLiveInstances()
	{
		static string Page(bool updated) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
				<x:Code><![CDATA[
					void FirstHandler(object sender, object args)
						=> ((Microsoft.Maui.Controls.Button)sender).Text += "other";
					{{(updated ? "" : "void FirstHandler(object sender, System.EventArgs args) => ((Microsoft.Maui.Controls.Button)sender).Text += \"old\";")}}
					void SecondHandler(object sender, System.EventArgs args)
						=> ((Microsoft.Maui.Controls.Button)sender).Text += "new";
				]]></x:Code>
				<Button Text="" Clicked="{{(updated ? "SecondHandler" : "FirstHandler")}}" />
			</ContentPage>
			""";
		const string pageSource = """
			namespace TestE2EApp;
			public partial class MainPage : Microsoft.Maui.Controls.ContentPage
			{
				public MainPage() => InitializeComponent();
			}
			""";
		using var harness = new XamlHotReloadTestHarness(nameof(RenamingInlineHandlerWithRemainingOverloadUpdatesLiveInstances),
			"TestE2EApp.MainPage", pageSource);
		var first = new Dictionary<string, XamlHotReloadDocument>
		{
			["MainPage.xaml"] = new(Page(false), EnablePreviewFeatures: true)
		};
		var second = new Dictionary<string, XamlHotReloadDocument>
		{
			["MainPage.xaml"] = new(Page(true), EnablePreviewFeatures: true)
		};
		var generation = harness.GenerateDocuments(first, second);

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var button = Assert.IsType<Button>(page.Content);
			((IButtonController)button).SendClicked();
			Assert.Equal("old", button.Text);

			button.Text = "";
			live.ApplyUpdate<ContentPage>(1, "SecondHandler");
			((IButtonController)button).SendClicked();
			Assert.Equal("new", button.Text);
		});
	}

	[MetadataUpdateFact]
	public void InlineEventUpdatesAreIndependentForEachControlAndPage()
	{
		static string Page(bool firstUpdated, string? secondHandler) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
				<x:Code><![CDATA[
					void FirstHandler(object sender, System.EventArgs args)
						=> ((Microsoft.Maui.Controls.Button)sender).Text = "first";
					void SecondHandler(object sender, System.EventArgs args)
						=> ((Microsoft.Maui.Controls.Button)sender).Text = "second";
				]]></x:Code>
				<VerticalStackLayout>
					<Button Clicked="{{(firstUpdated ? "SecondHandler" : "FirstHandler")}}" />
					<Button {{(secondHandler is null ? "" : $"Clicked=\"{secondHandler}\"")}} />
				</VerticalStackLayout>
			</ContentPage>
			""";
		const string pageSource = """
			namespace TestE2EApp;
			public partial class MainPage : Microsoft.Maui.Controls.ContentPage
			{
				public MainPage() => InitializeComponent();
			}
			""";
		using var harness = new XamlHotReloadTestHarness(nameof(InlineEventUpdatesAreIndependentForEachControlAndPage),
			"TestE2EApp.MainPage", pageSource);
		var generation = harness.GenerateDocuments(
			new Dictionary<string, XamlHotReloadDocument> { ["MainPage.xaml"] = new(Page(false, "FirstHandler"), EnablePreviewFeatures: true) },
			new Dictionary<string, XamlHotReloadDocument> { ["MainPage.xaml"] = new(Page(true, "FirstHandler"), EnablePreviewFeatures: true) },
			new Dictionary<string, XamlHotReloadDocument> { ["MainPage.xaml"] = new(Page(true, null), EnablePreviewFeatures: true) },
			new Dictionary<string, XamlHotReloadDocument> { ["MainPage.xaml"] = new(Page(true, "SecondHandler"), EnablePreviewFeatures: true) });

		harness.RunLive(generation, live =>
		{
			var firstPage = live.GetInstance<ContentPage>();
			var secondPage = live.CreateInstance<ContentPage>();

			static void AssertButtons(ContentPage page, string first, string second)
			{
				var layout = Assert.IsType<VerticalStackLayout>(page.Content);
				var firstButton = Assert.IsType<Button>(layout.Children[0]);
				var secondButton = Assert.IsType<Button>(layout.Children[1]);
				firstButton.Text = "";
				secondButton.Text = "";
				((IButtonController)firstButton).SendClicked();
				((IButtonController)secondButton).SendClicked();
				Assert.Equal(first, firstButton.Text);
				Assert.Equal(second, secondButton.Text);
			}

			live.ApplyUpdate<ContentPage>(1);
			AssertButtons(firstPage, "second", "first");
			AssertButtons(secondPage, "second", "first");

			live.ApplyUpdate<ContentPage>(2);
			AssertButtons(firstPage, "second", "");
			AssertButtons(secondPage, "second", "");

			live.ApplyUpdate<ContentPage>(3);
			AssertButtons(firstPage, "second", "second");
			AssertButtons(secondPage, "second", "second");
		});
	}

	[MetadataUpdateFact]
	public void AddingInlineCodeRebindsAnExistingOverloadedHandler()
	{
		static string Page(bool updated) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
				{{(updated ? "<x:Code><![CDATA[void FirstHandler(object sender, System.EventArgs args) => ((Microsoft.Maui.Controls.Button)sender).Text += \"other\";]]></x:Code>" : "")}}
				<Button Text="" Clicked="{{(updated ? "SecondHandler" : "FirstHandler")}}" />
			</ContentPage>
			""";
		const string pageSource = """
			namespace TestE2EApp;
			public partial class MainPage : Microsoft.Maui.Controls.ContentPage
			{
				public MainPage() => InitializeComponent();
				void FirstHandler(object sender, object args)
					=> ((Microsoft.Maui.Controls.Button)sender).Text += "old";
				void SecondHandler(object sender, System.EventArgs args)
					=> ((Microsoft.Maui.Controls.Button)sender).Text += "new";
			}
			""";
		using var harness = new XamlHotReloadTestHarness(nameof(AddingInlineCodeRebindsAnExistingOverloadedHandler),
			"TestE2EApp.MainPage", pageSource);
		var generation = harness.GenerateDocuments(
			new Dictionary<string, XamlHotReloadDocument> { ["MainPage.xaml"] = new(Page(false), EnablePreviewFeatures: true) },
			new Dictionary<string, XamlHotReloadDocument> { ["MainPage.xaml"] = new(Page(true), EnablePreviewFeatures: true) });

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var button = Assert.IsType<Button>(page.Content);
			((IButtonController)button).SendClicked();
			Assert.Equal("old", button.Text);

			button.Text = "";
			live.ApplyUpdate<ContentPage>(1);
			((IButtonController)button).SendClicked();
			Assert.Equal("new", button.Text);
		});
	}

	[MetadataUpdateFact]
	public void RemovingInlineCodePreservesEventTrackingForLiveInstances()
	{
		static string Page(bool updated) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
				{{(updated ? "" : "<x:Code><![CDATA[int Value => 1;]]></x:Code>")}}
				<Button Clicked="{{(updated ? "SecondHandler" : "FirstHandler")}}" />
			</ContentPage>
			""";
		const string pageSource = """
			namespace TestE2EApp;
			public partial class MainPage : Microsoft.Maui.Controls.ContentPage
			{
				public MainPage() => InitializeComponent();
				void FirstHandler(object sender, System.EventArgs args)
					=> ((Microsoft.Maui.Controls.Button)sender).Text = "first";
				void SecondHandler(object sender, System.EventArgs args)
					=> ((Microsoft.Maui.Controls.Button)sender).Text = "second";
			}
			""";
		using var harness = new XamlHotReloadTestHarness(nameof(RemovingInlineCodePreservesEventTrackingForLiveInstances),
			"TestE2EApp.MainPage", pageSource);
		var generation = harness.GenerateDocuments(
			new Dictionary<string, XamlHotReloadDocument> { ["MainPage.xaml"] = new(Page(false), EnablePreviewFeatures: true) },
			new Dictionary<string, XamlHotReloadDocument> { ["MainPage.xaml"] = new(Page(true), EnablePreviewFeatures: true) });

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var button = Assert.IsType<Button>(page.Content);
			((IButtonController)button).SendClicked();
			Assert.Equal("first", button.Text);

			live.ApplyUpdate<ContentPage>(1);
			((IButtonController)button).SendClicked();
			Assert.Equal("second", button.Text);
		});
	}

	[MetadataUpdateFact]
	public void InlineEventUpdatesApplyToEachRealizedTemplate()
	{
		static string Page(bool updated) => $$"""
			<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
			             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			             x:Class="TestE2EApp.MainPage">
				<x:Code><![CDATA[
					void FirstHandler(object sender, System.EventArgs args)
						=> ((Microsoft.Maui.Controls.Button)sender).Text = "first";
					void SecondHandler(object sender, System.EventArgs args)
						=> ((Microsoft.Maui.Controls.Button)sender).Text = "second";
				]]></x:Code>
				<CollectionView>
					<CollectionView.ItemTemplate>
						<DataTemplate>
							<Button Clicked="{{(updated ? "SecondHandler" : "FirstHandler")}}" />
						</DataTemplate>
					</CollectionView.ItemTemplate>
				</CollectionView>
			</ContentPage>
			""";
		const string pageSource = """
			namespace TestE2EApp;
			public partial class MainPage : Microsoft.Maui.Controls.ContentPage
			{
				public MainPage() => InitializeComponent();
			}
			""";
		using var harness = new XamlHotReloadTestHarness(nameof(InlineEventUpdatesApplyToEachRealizedTemplate),
			"TestE2EApp.MainPage", pageSource);
		var generation = harness.GenerateDocuments(
			new Dictionary<string, XamlHotReloadDocument> { ["MainPage.xaml"] = new(Page(false), EnablePreviewFeatures: true) },
			new Dictionary<string, XamlHotReloadDocument> { ["MainPage.xaml"] = new(Page(true), EnablePreviewFeatures: true) });

		harness.RunLive(generation, live =>
		{
			var page = live.GetInstance<ContentPage>();
			var collection = Assert.IsType<CollectionView>(page.Content);
			var template = Assert.IsType<DataTemplate>(collection.ItemTemplate);
			var first = Assert.IsType<Button>(template.CreateContent());
			var second = Assert.IsType<Button>(template.CreateContent());

			live.ApplyUpdate<ContentPage>(1);
			((IButtonController)first).SendClicked();
			((IButtonController)second).SendClicked();
			Assert.Equal("second", first.Text);
			Assert.Equal("second", second.Text);
		});
	}

	static (GeneratorDriverRunResult result, Compilation compilation) Generate(string xaml, bool incrementalHotReload, bool preview = true, string inflator = "SourceGen", NullableContextOptions? nullableContext = null)
	{
		var compilation = (CSharpCompilation)CreateMauiCompilation().AddSyntaxTrees(CSharpSyntaxTree.ParseText(CodeBehind));
		if (nullableContext is not null)
			compilation = compilation.WithOptions(compilation.Options.WithNullableContextOptions(nullableContext.Value));
		var file = new AdditionalXamlFile("Maui40328.xaml", xaml, EnableIncrementalHotReload: incrementalHotReload, EnablePreviewFeatures: preview, Inflator: inflator);
		var (_, result, updatedCompilation, _) = RunGeneratorWithChangesAndUpdateCompilation<XamlGenerator>(
			compilation, (driver, currentCompilation) => (driver, currentCompilation), file);
		return (result, updatedCompilation);
	}
}
