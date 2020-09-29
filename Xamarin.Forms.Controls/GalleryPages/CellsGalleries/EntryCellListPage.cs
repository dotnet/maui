using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls
{
	[Preserve(AllMembers = true)]
	public class EntryCellTest
	{
		public string Label { get; set; }
		public string Placeholder { get; set; }
		public Color LabelColor { get; set; }
		public Color PlaceholderColor { get; set; }

	}

	public class EntryCellListPage : ContentPage
	{
		public EntryCellListPage()
		{
			Title = "EntryCell List Gallery - Legacy";

			if (Device.RuntimePlatform == Device.iOS && Device.Idiom == TargetIdiom.Tablet)
				Padding = new Thickness(0, 0, 0, 60);

			var dataTemplate = new DataTemplate(typeof(EntryCell));
			dataTemplate.SetBinding(EntryCell.LabelProperty, new Binding("Label"));
			dataTemplate.SetBinding(EntryCell.LabelColorProperty, new Binding("LabelColor"));
			dataTemplate.SetBinding(EntryCell.PlaceholderProperty, new Binding("Placeholder"));

			var label = new Label
			{
				Text = "I have not been selected"
			};

			var listView = new ListView
			{
				AutomationId = CellTypeList.CellTestContainerId,
				ItemsSource = Enumerable.Range(0, 100).Select(i => new EntryCellTest
				{
					Label = "Label " + i,
					LabelColor = i % 2 == 0 ? Color.Red : Color.Blue,
					Placeholder = "Placeholder " + i,
					PlaceholderColor = i % 2 == 0 ? Color.Red : Color.Blue,
				}),
				ItemTemplate = dataTemplate
			};

			listView.ItemSelected += (sender, args) =>
			{
				label.Text = "I have been selected";
			};



			Content = new StackLayout { Children = { label, listView } };
		}
	}
}