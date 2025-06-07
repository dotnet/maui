using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 26015, "MAUI Grouped ListView Remove Item Causes Error when more than one Group item exists", PlatformAffected.Android)]
	public partial class Issue26015 : ContentPage
	{
		public Issue26015()
		{
			InitializeComponent();
			BindingContext = new Issue26015ViewModel();
		}
	}

	class Issue26015ViewModel : ViewModelBase
	{
		public class GroupData : ObservableCollection<DetailData>
		{
			public string Name { get; private set; }
			public GroupData(string name, ObservableCollection<DetailData> d) : base(d)
			{ Name = name; }
		}

		public class DetailData
		{
			public int Nr { get; set; }
			public string Detail { get; set; }
		}

		private ObservableCollection<GroupData> _data;
		public ObservableCollection<GroupData> Data
		{
			get => _data;
			set
			{
				_data = value;
				OnPropertyChanged();
			}
		}

		public Command RemoveItemCommand => new Command<DetailData>(RemoveItem);

		public Issue26015ViewModel()
		{
			Data =
			[
				new GroupData("Group1",
					[
						new(){ Nr = 1, Detail = "Test1" },
						new(){ Nr = 2, Detail = "Test2" }
					]),
					new GroupData("Group2",
					[
						new(){ Nr = 3, Detail = "Test3" },
						new(){ Nr = 4, Detail = "Test4" }
					]),
				];
		}

		private void RemoveItem(DetailData data)
		{
			foreach (var item in Data)
			{
				foreach (var child in item)
				{
					if (child.Nr == data.Nr)
					{
						item.Remove(child);
						break;
					}
				}
			}
		}
	}
}