using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Editor)]
	public partial class EditorTests : HandlerTestBase
	{
#if !IOS
		// iOS is broken until this point
		// https://github.com/dotnet/maui/issues/3425
		[Theory]
		[InlineData(EditorAutoSizeOption.Disabled)]
		[InlineData(EditorAutoSizeOption.TextChanges)]
#endif
		public async Task AutoSizeInitializesCorrectly(EditorAutoSizeOption option)
		{
			var editor = new Editor
			{
				AutoSize = option,
				Text = "Test"
			};

			IView layout = new VerticalStackLayout()
			{
				Children =
				{
					editor
				}
			};

			await InvokeOnMainThreadAsync(() =>
			{
				_ = CreateHandler<LayoutHandler>(layout);

				layout.Arrange(new Graphics.Rectangle(Graphics.Point.Zero, layout.Measure(1000, 1000)));
				var initialHeight = editor.Height;

				editor.Text += Environment.NewLine + " Some new text" + Environment.NewLine;
				layout.Arrange(new Graphics.Rectangle(Graphics.Point.Zero, layout.Measure(1000, 1000)));

				if (option == EditorAutoSizeOption.Disabled)
					Assert.Equal(initialHeight, editor.Height);
				else
					Assert.True(initialHeight < editor.Height);
			});
		}
	}
}
