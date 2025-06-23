using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25649, "CollectionView OnCollectionViewScrolled Calls and parameters are inconsistent or incorrect", PlatformAffected.All)]
	public partial class Issue25649 : ContentPage
	{
		public Issue25649()
		{
			InitializeComponent();
		}

		void OnCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
		{
			lastVisibleItemIndex.Text = e.LastVisibleItemIndex.ToString();
		}
	}

	public class _25649MainViewModel : BindableObject
	{
		public ObservableCollection<_25649Person> _25649People { get; set; }
		public _25649MainViewModel()
		{

			_25649People = new ObservableCollection<_25649Person>()
			{
				new _25649Person() { Name = "Person 0" },
				new _25649Person() { Name = "Person 1" },
				new _25649Person() { Name = "Person 2" },
				new _25649Person() { Name = "Person 3" },
				new _25649Person() { Name = "Person 4" },
				new _25649Person() { Name = "Person 5" },
				new _25649Person() { Name = "Person 6" },
				new _25649Person() { Name = "Person 7" },
				new _25649Person() { Name = "Person 8" },
				new _25649Person() { Name = "Person 9" },
				new _25649Person() { Name = "Person 10" },
				new _25649Person() { Name = "Person 11" },
				new _25649Person() { Name = "Person 12" },
				new _25649Person() { Name = "Person 13" },
				new _25649Person() { Name = "Person 14" },
				new _25649Person() { Name = "Person 15" },
				new _25649Person() { Name = "Person 16" },
				new _25649Person() { Name = "Person 17" },
				new _25649Person() { Name = "Person 18" },
				new _25649Person() { Name = "Person 19" },
				new _25649Person() { Name = "Person 20" },
				new _25649Person() { Name = "Person 21" },
				new _25649Person() { Name = "Person 22" },
				new _25649Person() { Name = "Person 23" },
				new _25649Person() { Name = "Person 24" },
				new _25649Person() { Name = "Person 25" },
				new _25649Person() { Name = "Person 26" },
				new _25649Person() { Name = "Person 27" },
				new _25649Person() { Name = "Person 28" },
				new _25649Person() { Name = "Person 29" },
				new _25649Person() { Name = "Person 30" },
			};
		}
	}

	public class _25649Person
	{
		public string Name { get; set; }
	}
}



