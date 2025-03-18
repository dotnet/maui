using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 26534, "Exception occurred when using GroupShortNameBinding in Grouped ListView", PlatformAffected.UWP)]
	public partial class Issue26534 : ContentPage
	{

		public Issue26534()
		{
			Grid gridView = new Grid();
			var listView = new ListView();
			ObservableCollection<GroupedVeggieModel> grouped = CreateData();

			listView.ItemsSource = grouped;
			listView.AutomationId = "listview";
			listView.IsGroupingEnabled = true;
			listView.GroupDisplayBinding = new Binding("LongName");
			listView.GroupShortNameBinding = new Binding("ShortName");
			listView.ItemTemplate = new DataTemplate(typeof(TextCell));
			listView.ItemTemplate.SetBinding(TextCell.TextProperty, "Name");
			gridView.Children.Add(listView);

			Content = gridView;
		}


		static ObservableCollection<GroupedVeggieModel> CreateData()
		{
			var grouped = new ObservableCollection<GroupedVeggieModel>();

			var veggieGroup = new GroupedVeggieModel() { LongName = "Vegetables", ShortName = "V" };
			veggieGroup.Add(new VeggieModel() { Name = "Carrot" });
			veggieGroup.Add(new VeggieModel() { Name = "Spinach" });
			veggieGroup.Add(new VeggieModel() { Name = "Potato" });

			var fruitGroup = new GroupedVeggieModel() { LongName = "Fruit", ShortName = "F" };
			fruitGroup.Add(new VeggieModel() { Name = "Banana" });
			fruitGroup.Add(new VeggieModel() { Name = "Strawberry" });
			fruitGroup.Add(new VeggieModel() { Name = "Cherry" });

			grouped.Add(veggieGroup);
			grouped.Add(fruitGroup);

			return grouped;
		}

		public class VeggieModel
		{
			public string Name { get; set; }
			public string Comment { get; set; }
		}

		public class GroupedVeggieModel : ObservableCollection<VeggieModel>
		{
			public string LongName { get; set; }
			public string ShortName { get; set; }
		}
	}
}
