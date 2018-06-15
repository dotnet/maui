using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1875, "NSRangeException adding items through ItemAppearing", PlatformAffected.iOS)]
#if UITEST
	[NUnit.Framework.Category(UITestCategories.ListView)]
#endif
	public class Issue1875
		: TestContentPage
	{
		MainViewModel _viewModel;
		int _start = 0;
		const int NumberOfRecords = 15;


		protected override void Init()
		{
			Button loadData = new Button { Text = "Load", HorizontalOptions = LayoutOptions.FillAndExpand };
			ListView mainList = new ListView
			{
				VerticalOptions = LayoutOptions.FillAndExpand,
				HorizontalOptions = LayoutOptions.FillAndExpand
			};

			mainList.SetBinding(ListView.ItemsSourceProperty, "Items");

			_viewModel = new MainViewModel();
			BindingContext = _viewModel;
			loadData.Clicked += async (sender, e) =>
			{
				await LoadData();
			};

			mainList.ItemAppearing += OnItemAppearing;

			Content = new StackLayout
			{
				Children = {
					loadData,
					mainList
				}
			};
		}

		async void OnItemAppearing(object sender, ItemVisibilityEventArgs e)
		{
			if (e.Item == null)
				return;
			var item = (int)e.Item;
			if (!_viewModel.IsLoading && item == _viewModel.Items.Last())
				await LoadData();
		}

		async Task LoadData()
		{
			await _viewModel.LoadData(_start, NumberOfRecords);
			_start = _start + NumberOfRecords;
		}

		public class MainViewModel : INotifyPropertyChanged
		{
			public event PropertyChangedEventHandler PropertyChanged;

			public MainViewModel()
			{
			}

			ObservableCollection<int> _items;
			public ObservableCollection<int> Items
			{
				get
				{
					if (_items == null)
						_items = new ObservableCollection<int>();

					return _items;
				}
				set
				{
					_items = value;
					PropertyChanged(this, new PropertyChangedEventArgs("Items"));
				}
			}

			bool _isLoading;
			public bool IsLoading
			{
				get
				{
					return _isLoading;
				}
				set
				{
					if (_isLoading != value)
					{
						_isLoading = value;
						PropertyChanged(this, new PropertyChangedEventArgs("IsLoading"));
					}
				}
			}

#pragma warning disable 1998 // considered for removal
			public async Task LoadData(int start, int numberOfRecords)
#pragma warning restore 1998
			{
				IsLoading = true;
				for (int counter = 0; counter < numberOfRecords; counter++)
					Items.Add(start + counter);

				IsLoading = false;
			}
		}

#if UITEST
		[Test]
		public void NSRangeException()
		{
			RunningApp.WaitForElement(q => q.Marked("Load"));
			RunningApp.Tap(q => q.Marked("Load"));
			RunningApp.WaitForElement(q => q.Marked("5"));
		}
#endif
	}
}