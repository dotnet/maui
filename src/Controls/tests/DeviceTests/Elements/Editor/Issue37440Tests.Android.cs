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
		[Fact]
		public async Task EmptyAutoSizingEditorStartsBelowMaximumHeight()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<Editor, EditorHandler>();
				});
			});

			var editor = new Editor
			{
				AutoSize = EditorAutoSizeOption.TextChanges,
				MinimumHeightRequest = 50,
				MaximumHeightRequest = 150
			};

			var innerBorder = new Border
			{
				HeightRequest = 150,
				StrokeThickness = 0,
				Content = editor
			};

			var outerBorder = new Border
			{
				HeightRequest = 200,
				StrokeThickness = 0,
				Content = innerBorder
			};

			await AttachAndRun(outerBorder, async _ =>
			{
				await OnFrameSetToNotEmpty(editor);

				Assert.True(
					editor.Height < editor.MaximumHeightRequest - 5,
					$"Editor should start below its maximum height. Expected less than {editor.MaximumHeightRequest - 5}, but was {editor.Height}.");
			});
		}
	}
}
