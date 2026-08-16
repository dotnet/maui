using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
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

		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Border, BorderHandler>();
				});
			});
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

			SetupBuilder();

			const double maximumEditorHeight = 150;
			const double heightTolerance = 2;
			var editor = new Editor
			{
				MinimumHeightRequest = 50,
				MaximumHeightRequest = maximumEditorHeight,
				AutoSize = EditorAutoSizeOption.TextChanges,
			};
			var layout = new VerticalStackLayout
			{
				Children =
				{
					new Border
					{
						HeightRequest = 200,
						Content = new Border
						{
							HeightRequest = 150,
							Content = editor,
						},
					},
				},
			};

			await AttachAndRun<LayoutHandler>(layout, async _ =>
			{
				var editorHandler = Assert.IsType<EditorHandler>(editor.Handler);
				await editorHandler.PlatformView.WaitForLayoutOrNonZeroSize();

				Assert.True(
					editor.Height < maximumEditorHeight - heightTolerance,
					"Editor should start below MaximumHeightRequest when empty and AutoSize is TextChanges.");
			});
		}
	}
}
