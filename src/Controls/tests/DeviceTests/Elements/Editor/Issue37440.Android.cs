#nullable enable annotations

using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Editor)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class Issue37440 : ControlsHandlerTestBase
	{
		const string IssueNumber = "37440";

		static string? GetReplicationIssue()
		{
#if ANDROID
			return global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner
				.MauiTestInstrumentation.Current?.Arguments?.GetString("MAUI_REPRODUCTION_ISSUE");
#elif IOS || MACCATALYST
			return global::Foundation.NSProcessInfo.ProcessInfo.Environment["MAUI_REPRODUCTION_ISSUE"]?.ToString();
#else
			return Environment.GetEnvironmentVariable("MAUI_REPRODUCTION_ISSUE");
#endif
		}

		[Fact]
		public async Task EmptyAutoSizeEditorStartsBelowMaximumHeight()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<Editor, EditorHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
				});
			});

			var editor = new Editor
			{
				AutoSize = EditorAutoSizeOption.TextChanges,
				MinimumHeightRequest = 50,
				MaximumHeightRequest = 150,
			};

			var innerBorder = new Border
			{
				HeightRequest = 150,
				StrokeThickness = 0,
				VerticalOptions = LayoutOptions.Start,
				Content = editor,
			};

			var outerBorder = new Border
			{
				HeightRequest = 200,
				StrokeThickness = 0,
				Content = innerBorder,
			};

			var layout = new VerticalStackLayout
			{
				WidthRequest = 300,
				Children =
				{
					outerBorder,
				},
			};

			await AttachAndRun<LayoutHandler>(layout, _ =>
			{
				Assert.True(
					editor.Height < editor.MaximumHeightRequest,
					"Editor should start below MaximumHeightRequest when empty and AutoSize is TextChanges.");
			});
		}
	}
}
