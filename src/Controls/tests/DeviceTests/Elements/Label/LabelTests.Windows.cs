using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LabelTests
	{
		TextBlock GetPlatformLabel(LabelHandler labelHandler) =>
			labelHandler.PlatformView;

		// Yes, this looks wrong (because ultimately, it is)
		// We're returning TextTrimming instead of the obviously more correct TextWrapping because
		// LineBreakMode is a fundamentally incorrect conflation of wrapping and trimming. 
		// But for now we have to preserve the old Forms behavior and make the tests pass, so
		// these tests will consider Windows's "LineBreakMode" to be it's text trimming mode
		TextTrimming GetPlatformLineBreakMode(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).TextTrimming;

		int GetPlatformMaxLines(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).MaxLines;

		Task<float> GetPlatformOpacity(LabelHandler labelHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformLabel(labelHandler);
				return (float)nativeView.Opacity;
			});
		}

		Task<bool> GetPlatformIsVisible(LabelHandler labelHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformLabel(labelHandler);
				return nativeView.Visibility == Microsoft.UI.Xaml.Visibility.Visible;
			});
		}

		[Fact]
		public async Task FormattedTextDoesNotHangOrCrashWhenLabelIsShownAfterBeingSetWhileHidden()
		{
			SetupBuilder();

			var label = new Label
			{
				IsVisible = false,
				WidthRequest = 30,
				HorizontalOptions = LayoutOptions.Start,
				LineBreakMode = LineBreakMode.WordWrap,
			};

			var layout = new VerticalStackLayout { label };

			await AttachAndRun(layout, async (handler) =>
			{
				label.FormattedText = new FormattedString
				{
					Spans =
					{
						new Span { Text = "Aa Bb Cc" },
						new Span { Text = "\n" },
					}
				};

				await Task.Delay(50);

				// Showing the label triggers a real arrange pass, which calls
				// RecalculateSpanPositions(). Before the fix, this would hang the UI thread and
				// eventually crash with an OutOfMemoryException because the label wraps to 3
				// lines ("Aa", "Bb", "Cc") at this width. Simply completing (instead of hanging or
				// throwing) proves the fix works.
				label.IsVisible = true;

				await Task.Delay(250);
			});
		}
	}
}
