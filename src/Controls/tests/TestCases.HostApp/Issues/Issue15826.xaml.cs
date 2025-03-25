using System.ComponentModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 15826, "ListView visibility doesn't work well", PlatformAffected.Android)]
	public partial class Issue15826 : ContentPage
	{
		private Issue15826Settings settings;

		public Issue15826()
		{
			settings = new Issue15826Settings();

			InitializeComponent();
			BindingContext = settings;
			SetListsStatus();
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			if (settings.List1Visible)
			{
				settings.List1Visible = false;
				settings.List2Visible = true;
			}
			else
			{
				settings.List1Visible = true;
				settings.List2Visible = false;
			}

			SetListsStatus();
		}

		private void SetListsStatus()
		{
			var listName = List1.IsVisible && !List2.IsVisible ? "List 1" : !List1.IsVisible && List2.IsVisible ? "List 2" : "";

			settings.ListsStatus = string.IsNullOrEmpty(listName) ? "No lists are visible" : $"{listName} is now visible";
		}
	}

	internal class Issue15826Settings : INotifyPropertyChanged
	{
		private List<ListData> list1Data = new List<ListData> {
			new ListData{ Text = "List 1 - Item 1" },
			new ListData{ Text = "List 1 - Item 2" },
			new ListData{ Text = "List 1 - Item 3" }
		};
		private List<ListData> list2Data = new List<ListData> {
			new ListData{ Text = "List 2 - Item 1" },
			new ListData{ Text = "List 2 - Item 2" },
			new ListData{ Text = "List 2 - Item 3" }
		};
		private bool list1Visible = true;
		private bool list2Visible = false;
		private string listsStatus;

		public event PropertyChangedEventHandler PropertyChanged;

		public List<ListData> List1Data
		{
			get => list1Data;
			set
			{
				list1Data = value;
				OnPropertyChanged(nameof(List1Data));
			}
		}

		public List<ListData> List2Data
		{
			get => list2Data;
			set
			{
				list2Data = value;
				OnPropertyChanged(nameof(List2Data));
			}
		}

		public bool List1Visible
		{
			get => list1Visible;
			set
			{
				list1Visible = value;
				OnPropertyChanged(nameof(List1Visible));
			}
		}

		public bool List2Visible
		{
			get => list2Visible;
			set
			{
				list2Visible = value;
				OnPropertyChanged(nameof(List2Visible));
			}
		}

		public string ListsStatus
		{
			get => listsStatus;
			set
			{
				listsStatus = value;
				OnPropertyChanged(nameof(ListsStatus));
			}
		}

		private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	internal class ListData
	{
		public string Text { get; set; }
	}
}