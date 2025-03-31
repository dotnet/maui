using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ListView)]
	public partial class ListViewTests
	{
#pragma warning disable CS0618 // Type or member is obsolete
		void ValidatePlatformCells(ListView listView)
#pragma warning restore CS0618 // Type or member is obsolete
		{

		}

		[Fact]
		public async Task InsertItemWorks()
		{
			SetupBuilder();

			var data = new ObservableCollection<string>();
#pragma warning disable CS0618 // Type or member is obsolete
			var listView = new ListView()
			{
				ItemsSource = data
			};
#pragma warning restore CS0618 // Type or member is obsolete

			var layout = new VerticalStackLayout()
			{
				listView
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async (handler) =>
			{
				await Task.Delay(100);

				data.Add("Item 1");
				data.Add("Item 3");
				data.Insert(1, "Item 2");

				await Task.Delay(200);

				var actualListView = listView.ToPlatform() as ListViewRenderer;
				var textChildren = actualListView.GetChildren<UI.Xaml.Controls.TextBlock>();

				Assert.Contains(textChildren, (x) => x.Text == "Item 3");
			});
		}
	}
}
