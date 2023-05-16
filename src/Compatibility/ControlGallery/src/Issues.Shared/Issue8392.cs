using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 8392, "[Bug] CollectionView does not marshal ObservableCollection.CollectionChanged to MainThread")]
	public class Issue8392 : TestContentPage
	{
		public Issue8392()
		{
			Title = "Issue 8392";

			var collectionView = new CollectionView
			{
				ItemTemplate = new Issue8392DataTemplate()
			};

			collectionView.SetBinding(ItemsView.ItemsSourceProperty, nameof(Issue8392ViewModel.DataList));

			var refreshView = new RefreshView
			{
				Content = collectionView
			};

			refreshView.SetBinding(RefreshView.IsRefreshingProperty, nameof(Issue8392ViewModel.IsRefreshing));
			refreshView.SetBinding(RefreshView.CommandProperty, nameof(Issue8392ViewModel.PullToRefreshCommand));

			Content = refreshView;

			BindingContext = new Issue8392ViewModel();
		}

		protected override void Init()
		{

		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			if (Content is RefreshView refreshView
				&& refreshView.Content is CollectionView)
			{
				refreshView.IsRefreshing = true;
			}
		}
	}

	[Preserve(AllMembers = true)]
	public class Issue8392DataTemplate : DataTemplate
	{
		public Issue8392DataTemplate() : base(CreateTemplate)
		{
		}

		static Layout CreateTemplate()
		{
			var numberLabel = new Label
			{
				FontSize = 14,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Center,
				HorizontalTextAlignment = TextAlignment.Start,
				VerticalTextAlignment = TextAlignment.Center,
			};
			numberLabel.SetBinding(Label.TextProperty, nameof(Issue8392Model.Number));

			var grid = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition { Height = new GridLength(20,GridUnitType.Absolute) }
				},
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
				}
			};

			grid.Add(numberLabel, 0, 0);

			return grid;
		}
	}

	public class Issue8392Model
	{
		public Issue8392Model(int number) => Number = number;

		public int Number { get; }
	}

	[Preserve(AllMembers = true)]
	public class Issue8392ViewModel : BindableObject
	{
		bool _isRefreshing;

		public ICommand PullToRefreshCommand => new Command(async () => await ExecutePullToRefreshCommand());

		public bool IsRefreshing
		{
			get { return _isRefreshing; }
			set
			{
				_isRefreshing = value;
				OnPropertyChanged();
			}
		}

		public ObservableCollection<Issue8392Model> DataList { get; } = new ObservableCollection<Issue8392Model>();

		async Task ExecutePullToRefreshCommand()
		{
			DataList.Clear();

			try
			{
				// If the items load without exception, the test has passed.
				for (int i = 0; i < 30; i++)
				{
					await Task.Delay(200).ConfigureAwait(false);
					DataList.Add(new Issue8392Model(i + 1));
				}
			}
			finally
			{
				IsRefreshing = false;
			}
		}
	}
}