using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
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

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, (_) =>
			{
				layout.Arrange(new Graphics.Rect(Graphics.Point.Zero, layout.Measure(1000, 1000)));
				var initialHeight = editor.Height;

				editor.Text += Environment.NewLine + " Some new text" + Environment.NewLine;
				layout.Arrange(new Graphics.Rect(Graphics.Point.Zero, layout.Measure(1000, 1000)));

				if (option == EditorAutoSizeOption.Disabled)
					Assert.Equal(initialHeight, editor.Height);
				else
					Assert.True(initialHeight < editor.Height);

				return Task.CompletedTask;
			});
		}
#endif

		[Theory]
		[ClassData(typeof(TextTransformCases))]
		public async Task InitialTextTransformApplied(string text, TextTransform transform, string expected)
		{
			var control = new Editor() { Text = text, TextTransform = transform };
			var platformText = await GetPlatformText(await CreateHandlerAsync<EditorHandler>(control));
			Assert.Equal(expected, platformText);
		}

		[Theory]
		[ClassData(typeof(TextTransformCases))]
		public async Task TextTransformUpdated(string text, TextTransform transform, string expected)
		{
			var control = new Editor() { Text = text };
			var handler = await CreateHandlerAsync<EditorHandler>(control);
			await InvokeOnMainThreadAsync(() => control.TextTransform = transform);
			var platformText = await GetPlatformText(handler);
			Assert.Equal(expected, platformText);
		}
	}
}
