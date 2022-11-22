using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ContentView)]
	public partial class ContentViewTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<IContentView, ContentViewHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
				});
			});
		}

		[Fact("ContentView updating it's ControlTemplate works")]
		public async Task ControlTemplateUpdates()
		{
			SetupBuilder();
			var child = new Label { Text = "Content 1" };
			var contentView = new Microsoft.Maui.Controls.ContentView();
			var header = new Label { Text = "Header" };
			var footer = new Label { Text = "Footer" };
			var presenter = new ContentPresenter();
			var grid = new Grid();

			var contentViewHandler = await CreateHandlerAsync<ContentViewHandler>(contentView);

			await InvokeOnMainThreadAsync(() =>
			{
				contentView.Content = child;
				Assert.True(GetChildCount(contentViewHandler) == 1);
				Assert.True(GetContentChildCount(contentViewHandler) == 0);
				grid.Children.Add(header);
				grid.Children.Add(presenter);
				grid.Children.Add(footer);
				var dataTemplate = new ControlTemplate(() => grid);
				contentView.ControlTemplate = dataTemplate;
				Assert.True(GetChildCount(contentViewHandler) == 1);
				Assert.True(GetContentChildCount(contentViewHandler) == 3);
			});
		}
	}
}
