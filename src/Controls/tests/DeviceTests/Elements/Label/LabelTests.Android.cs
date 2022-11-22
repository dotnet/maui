using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Text;
using Android.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LabelTests
	{
		[Fact]
		public async Task FormattedStringSpanTextHasCorrectColorWhenChanges()
		{
			var expected = Colors.Red;

			var formattedLabel = new Label
			{
				WidthRequest = 200,
				HeightRequest = 50,
				FormattedText = new FormattedString
				{
					Spans =
					{
						new Span { Text = "short" },
						new Span { Text = " long second string"}
					}
				},
			};

			formattedLabel.TextColor = expected;

			await ValidateHasColor(formattedLabel, expected);
		}

		TextView GetPlatformLabel(LabelHandler labelHandler) =>
			labelHandler.PlatformView;

		TextUtils.TruncateAt GetPlatformLineBreakMode(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).Ellipsize;

		int GetPlatformMaxLines(LabelHandler labelHandler) =>
			GetPlatformLabel(labelHandler).MaxLines;

		Task ValidateHasColor(Label label, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(async () =>
			{
				var labelHandler = CreateHandler<LabelHandler>(label);
				action?.Invoke();
				await labelHandler.PlatformView.AssertContainsColor(color);
			});
		}
	}
}