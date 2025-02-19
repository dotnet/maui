using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.ListView)]
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 43214, "Setting Listview.IsRefreshing to false does not work on second \"pull\"")]
	public class Bugzilla43214 : TestContentPage
	{
		public class MyViewModel : ViewModel
		{
			bool _isBusy;

			public bool IsBusy
			{
				get { return _isBusy; }
				set
				{
					_isBusy = value;
					OnPropertyChanged();
				}
			}
		}

		protected override void Init()
		{
			var vm = new MyViewModel();

			var label = new Label { Text = "Pull list to refresh once, then pull to refresh again. If the indicator does not disappear, this test has failed." };
			var listview = new ListView
			{
				ItemsSource = Enumerable.Range(0, 20),
				IsPullToRefreshEnabled = true,
				RefreshCommand = new Command(async () =>
				{
					vm.IsBusy = true;
					await Task.Delay(1000);
					vm.IsBusy = false;
				})
			};

			listview.SetBinding(ListView.IsRefreshingProperty, nameof(MyViewModel.IsBusy));

			var stacklayout = new StackLayout { Children = { label, listview }, BindingContext = vm };

			Content = stacklayout;
		}
	}
}
