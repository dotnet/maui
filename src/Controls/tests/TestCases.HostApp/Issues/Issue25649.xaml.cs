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

		private void OnCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
		{
			labelFirstLastVisible.Text = $"Scrolled: {e.FirstVisibleItemIndex} to {e.LastVisibleItemIndex}";
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
				new _25649Person() { Name = "Person 31" },
				new _25649Person() { Name = "Person 32" },
				new _25649Person() { Name = "Person 33" },
				new _25649Person() { Name = "Person 34" },
				new _25649Person() { Name = "Person 35" },
				new _25649Person() { Name = "Person 36" },
				new _25649Person() { Name = "Person 37" },
				new _25649Person() { Name = "Person 38" },
				new _25649Person() { Name = "Person 39" },
				new _25649Person() { Name = "Person 40" },
				new _25649Person() { Name = "Person 41" },
				new _25649Person() { Name = "Person 42" },
				new _25649Person() { Name = "Person 43" },
				new _25649Person() { Name = "Person 44" },
				new _25649Person() { Name = "Person 45" },
				new _25649Person() { Name = "Person 46" },
				new _25649Person() { Name = "Person 47" },
				new _25649Person() { Name = "Person 48" },
				new _25649Person() { Name = "Person 49" },
				new _25649Person() { Name = "Person 50" },

			};
		}
	}

	public class _25649Person
	{
		public string Name { get; set; }
	}
}



