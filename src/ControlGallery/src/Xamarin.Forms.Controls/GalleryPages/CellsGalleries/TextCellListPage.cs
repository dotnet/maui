using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	[Preserve(AllMembers = true)]
	public class TextCellTest
	{
		public object Text { get; set; }
		public object TextColor { get; set; }
		public object Detail { get; set; }
		public object DetailColor { get; set; }
	}

	public class TextCellListPage : ContentPage
	{
		public TextCellListPage()
		{
			Title = "TextCell List Gallery - Legacy";

			if (Device.RuntimePlatform == Device.iOS && Device.Idiom == TargetIdiom.Tablet)
				Padding = new Thickness(0, 0, 0, 60);

			var label = new Label { Text = "Not Selected" };

			var dataTemplate = new DataTemplate(typeof(TextCell));
			dataTemplate.SetBinding(TextCell.TextProperty, new Binding("Text"));
			dataTemplate.SetBinding(TextCell.TextColorProperty, new Binding("TextColor"));
			dataTemplate.SetBinding(TextCell.DetailProperty, new Binding("Detail"));
			dataTemplate.SetBinding(TextCell.DetailColorProperty, new Binding("DetailColor"));

			var listView = new ListView
			{
				AutomationId = CellTypeList.CellTestContainerId,
				ItemsSource = Enumerable.Range(0, 100).Select(i => new TextCellTest
				{
					Text = "Text " + i,
					TextColor = i % 2 == 0 ? Color.Red : Color.Blue,
					Detail = "Detail " + i,
					DetailColor = i % 2 == 0 ? Color.Red : Color.Blue
				}),
				ItemTemplate = dataTemplate
			};

			listView.ItemSelected += (sender, args) => { label.Text = "I was selected"; };

			Content = new StackLayout { Children = { label, listView } };
		}
	}
}