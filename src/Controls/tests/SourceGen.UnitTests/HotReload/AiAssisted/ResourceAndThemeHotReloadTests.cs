#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.SourceGen.UnitTests.HotReload.AiAssisted;

// Wave2 scope note: this file owns RT-01..RT-11 for the resources/theme portfolio.
// RT-01..RT-07 cover resource dictionaries, keyed styles, BasedOn styles and triggered styles.
// RT-08..RT-11 (Phase-2) cover AppThemeBinding branch edits, application-scope DynamicResource
// fanout, and multi-document (Source= merged dictionary / malformed->repair) scenarios; they rely
// on the Phase-0/1 harness capabilities cap-app-host, cap-theme-flip, cap-multi-instance and
// cap-multi-doc. Where the harness cannot faithfully reach a live invariant (Source= dictionaries
// have no compiled resource payload in this in-memory generator/ALC harness), coverage is provided
// at the strongest faithful level (generator/compile-atomic) with a paired live RED-PROBE that
// documents the precise boundary — never an always-skipped empty body.
[Collection("XamlHotReloadTests")]
public partial class ResourceAndThemeHotReloadTests
{
	const string PageClass = "TestAiAssisted.MainPage";

	const string PageStub = """
		namespace TestAiAssisted;

		public partial class MainPage : global::Microsoft.Maui.Controls.ContentPage
		{
			private partial void InitializeComponent();

			public MainPage()
			{
				InitializeComponent();
			}
		}
		""";

	static XamlHotReloadTestHarness CreateHarness(string scenarioName) =>
		new(scenarioName, PageClass, PageStub);

	static XamlHotReloadTestHarness CreateHostedHarness(
		XamlHotReloadApplicationOptions options, string scenarioName) =>
		new(scenarioName, PageClass, PageStub, options);
}
