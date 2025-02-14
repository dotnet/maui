using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25514, "Grouped CollectionView with header template and templateselector crashes", PlatformAffected.iOS)]
	public partial class Issue25514 : ContentPage
	{
		public Issue25514()
		{
			InitializeComponent();
			BindingContext = new Issue25514ViewModel();
		}
	}

	public class Issue25514ViewModel : ViewModel
	{
		public Command LoadCollectionCommand => new(() =>
		{
			StringsGroups = [];
			for (int i = 0; i < 2; i++)
			{
				StringsGroups.Add(new StringsGroup("Hello", ["Item 1", "Item 2"]));
			}
		});

		private ObservableCollection<StringsGroup> _stringsGroups = [];
		public ObservableCollection<StringsGroup> StringsGroups
		{
			get => _stringsGroups;
			set
			{
				_stringsGroups = value;
				OnPropertyChanged();
			}
		}

		public class StringsGroup(string header, List<string> strings) : List<string>(strings)
		{
			public string Header { get; private set; } = header;
		}
	}
}
