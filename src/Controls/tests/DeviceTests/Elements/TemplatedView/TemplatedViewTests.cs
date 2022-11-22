using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.TemplatedView)]
	public partial class TemplatedViewTests : ControlsHandlerTestBase
	{
		public TemplatedViewTests()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<IContentView, ContentViewHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});
		}

		[Fact]
		public async Task ControlTemplateInitializesCorrectly()
		{
			var content = new Label { Text = "Content 1" };
			var templatedView = new TemplatedView
			{
				ControlTemplate = new ControlTemplate(() => content)
			};

			var handler = await CreateHandlerAsync<ContentViewHandler>(templatedView);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.Equal(1, GetChildCount(handler));
				Assert.Equal(content.Handler.PlatformView, GetChild(handler));
			});
		}

		[Fact]
		public async Task ControlTemplateCanBeReplacedCorrectly()
		{
			var content1 = new Label { Text = "Content 1" };
			var controlTemplate1 = new ControlTemplate(() => content1);

			var content2 = new Label { Text = "Content 2" };
			var controlTemplate2 = new ControlTemplate(() => content2);

			var templatedView = new TemplatedView { ControlTemplate = controlTemplate1 };

			var handler = await CreateHandlerAsync<ContentViewHandler>(templatedView);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.Equal(1, GetChildCount(handler));
				Assert.Equal(content1.Handler.PlatformView, GetChild(handler));

				templatedView.ControlTemplate = controlTemplate2;

				Assert.Equal(1, GetChildCount(handler));
				Assert.Equal(content2.Handler.PlatformView, GetChild(handler));
			});
		}
	}
}
