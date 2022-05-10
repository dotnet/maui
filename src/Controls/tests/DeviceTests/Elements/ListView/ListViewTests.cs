using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ListView)]
	public partial class ListViewTests : HandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<ViewCell, ViewCellRenderer>();
					handlers.AddHandler<ListView, ListViewRenderer>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});
		}

		[Fact(DisplayName = "ReAssigning ListView in VSL Crashes")]
		public async Task ReAssigninListViewInVSLCrashes()
		{
			SetupBuilder();
			var listView = new ListView()
			{
				ItemTemplate = new DataTemplate(() =>
				{
					return new ViewCell()
					{
						View = new VerticalStackLayout()
							{
								new Label()
								{
									Text = "Cat"
								}
							}
					};
				})
			};

			var layout = new VerticalStackLayout()
			{
				listView
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async (handler) =>
			{
				listView.ItemsSource = Enumerable.Range(1, 1);
				await Task.Delay(100);
				listView.ItemsSource = Enumerable.Range(1, 2);
				await Task.Delay(100);
			});
		}
	}
}