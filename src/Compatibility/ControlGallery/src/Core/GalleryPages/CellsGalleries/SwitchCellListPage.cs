using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery
{
	[Preserve(AllMembers = true)]
	public class SwitchCellItem
	{
		public string Label { get; set; }
		public bool SwitchOn { get; set; }
		public Color OnColor { get; set; }
	}

	public class SwitchCellListPage : ContentPage
	{
		public SwitchCellListPage()
		{
			Title = "SwitchCell List Gallery - Legacy";

			if (Device.RuntimePlatform == Device.iOS && Device.Idiom == TargetIdiom.Tablet)
				Padding = new Thickness(0, 0, 0, 60);

			var dataTemplate = new DataTemplate(typeof(SwitchCell))
			{
				Bindings = {
					{SwitchCell.TextProperty, new Binding ("Label")},
					{SwitchCell.OnProperty, new Binding ("SwitchOn")},
					{SwitchCell.OnColorProperty, new Binding ("OnColor")},
				}
			};

			var label = new Label { Text = "I have not been selected" };

			var listView = new ListView
			{
				AutomationId = CellTypeList.CellTestContainerId,
				ItemsSource = Enumerable.Range(0, 100).Select(i => new SwitchCellItem
				{
					Label = "Label " + i,
					SwitchOn = i % 2 == 0 ? false : true,
					OnColor = i % 2 == 0 ? Colors.Firebrick : Colors.GreenYellow
				}),
				ItemTemplate = dataTemplate
			};

			listView.ItemSelected += (sender, args) => label.Text = "I was selected.";

			Content = new StackLayout { Children = { label, listView } };
		}

	}
}