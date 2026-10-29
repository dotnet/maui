using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public partial class ContentButtonTests : CoreHandlerTestBase<ContentButtonHandler, ContentButtonStub>
	{
		[Theory]
		[InlineData(-1)]
		[InlineData(0)]
		[InlineData(3)]
		public async Task ExplicitSizeIncludesInsetsOnce(double stroke)
		{
			var button = new ContentButtonStub
			{
				Content = new ButtonStub { Text = "Presentation" },
				Width = 120,
				Height = 80,
				Padding = new Thickness(1, 2, 3, 4),
				StrokeThickness = stroke
			};
			await CreateHandlerAsync(button);
			var size = await InvokeOnMainThreadAsync(() => button.Measure(double.PositiveInfinity, double.PositiveInfinity));
			Assert.Equal(120, size.Width, 1);
			Assert.Equal(80, size.Height, 1);
		}

		[Fact]
		public async Task MaximumSizeConstrainsAttachedContent()
		{
			var button = new ContentButtonStub
			{
				Content = new ButtonStub { Text = "Presentation", Width = 200, Height = 100 },
				MaximumWidth = 120,
				MaximumHeight = 80,
				Padding = new Thickness(5),
			};
			await AttachAndRun(button, handler =>
			{
				var size = handler.GetDesiredSize(double.PositiveInfinity, double.PositiveInfinity);
				Assert.InRange(size.Width, 0, 120);
				Assert.InRange(size.Height, 0, 80);
				return Task.CompletedTask;
			});
		}

		[Fact]
		public async Task ContentCanBeReplacedAndRemoved()
		{
			var first = new ButtonStub { Text = "First" };
			var second = new ButtonStub { Text = "Second" };
			var button = new ContentButtonStub { Content = first, Width = 100, Height = 60 };
			await AttachAndRun(button, handler =>
			{
				Assert.NotNull(first.Handler);
				button.Content = second;
				handler.UpdateValue(nameof(IContentView.Content));
				Assert.NotNull(second.Handler);
				button.Content = null;
				handler.UpdateValue(nameof(IContentView.Content));
				handler.GetDesiredSize(100, 60);
				return Task.CompletedTask;
			});
		}
	}
}
