#if MACCATALYST
#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.MenuFlyout)]
	public class Issue17210 : ControlsHandlerTestBase
	{
		const string IssueNumber = "17210";

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
		public async Task ChangingIconImageSourceRequestsReplacementImage()
		{
			if (!string.Equals(
				GetReplicationIssue(),
				IssueNumber,
				StringComparison.Ordinal))
			{
				return;
			}

			var replacementWasRequested = await InvokeOnMainThreadAsync(() =>
			{
				var initial = new TrackingFileImageSource();
				var replacement = new TrackingFileImageSource();
				var item = new MenuFlyoutItem
				{
					Text = "Test item",
					IconImageSource = initial
				};

				try
				{
					_ = CreateHandler<MenuFlyoutItemHandler>(item);
					item.IconImageSource = replacement;

					return replacement.WasRequested;
				}
				finally
				{
					item.Handler?.DisconnectHandler();
					MenuFlyoutItemHandler.Reset();
				}
			});

			Assert.True(
				replacementWasRequested,
				"The replacement MenuFlyoutItem icon should be requested after IconImageSource changes.");
		}

		sealed class TrackingFileImageSource : ImageSource, IFileImageSource
		{
			public override bool IsEmpty => false;

			public bool WasRequested { get; private set; }

			public string File
			{
				get
				{
					WasRequested = true;
					return "icon.png";
				}
			}
		}
	}
}
#endif
