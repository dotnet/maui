using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Label)]
	public partial class FormattedStringTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task NativeFormattedStringContainsSpan()
		{
			var formattedString = new FormattedString();
			formattedString.Spans.Add(new Span { Text = "HELLO" });
			formattedString.Spans.Add(new Span { Text = "WORLD" });

			var label = new Label();
			var handler = await CreateHandlerAsync<LabelHandler>(label);

			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			var plainText = formattedString.ToString();
			var actual = await InvokeOnMainThreadAsync(() =>
			{
				var result = string.Empty;
#if __IOS__
				var attributed = formattedString.ToNSAttributedString(fontManager);

				attributed.EnumerateAttributes(new Foundation.NSRange(0, attributed.Length),
					 Foundation.NSAttributedStringEnumeration.None, new Foundation.NSAttributedRangeCallback((Foundation.NSDictionary dict, Foundation.NSRange range, ref bool flag) =>
					 {
						 result += attributed.Substring(range.Location, range.Length)?.Value;
					 }));
#elif __ANDROID__
				var spannable = formattedString.ToSpannableString(fontManager);

				result = spannable.ToString();
#elif WINDOWS
				var tuples = formattedString.ToRunAndColorsTuples(fontManager: fontManager);

				foreach (var t in tuples)
					result += t.Item1.Text;
#endif
				return result;
			});

			Assert.Equal("HELLOWORLD", actual);
		}
	}
}