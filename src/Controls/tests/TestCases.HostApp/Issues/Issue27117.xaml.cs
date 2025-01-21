using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 27117, "CollectionView ScrollTo not working under android", PlatformAffected.All)]
	public partial class Issue27117 : ContentPage
	{
		public Issue27117()
		{
			InitializeComponent();
		}

		private void CollectionView_Loaded(object sender, EventArgs e)
		{
			collectionView.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
		}
	}

	public class _27117MainViewModel : BindableObject
	{
		public ObservableCollection<_27117Person> _27117People { get; set; }
		public _27117MainViewModel()
		{
			_27117People = new ObservableCollection<_27117Person>();
			for (int i = 0; i < 100; i++)
			{
				_27117People.Add(new _27117Person($"Person {i}"));
			}
		}
	}

	public class _27117Person
	{
		public string Name { get; set; }

		public _27117Person(string name)
		{
			Name = name;
		}
	}
}