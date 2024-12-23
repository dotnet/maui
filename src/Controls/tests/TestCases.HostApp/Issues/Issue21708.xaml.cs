using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 21708, "CollectionView.Scrolled event offset isn't correctly reset when items change on Android", PlatformAffected.All)]
	public partial class Issue21708 : ContentPage
	{
		public static readonly BindableProperty ItemsProperty = BindableProperty.Create(nameof(Items), typeof(IEnumerable<int>), typeof(MainPage), default(IEnumerable<int>));
		public static readonly BindableProperty VerticalOffsetProperty = BindableProperty.Create(nameof(VerticalOffset), typeof(double), typeof(MainPage), default(double));
		readonly ObservableCollection<int> _items;

		public IEnumerable<int> Items
		{
			get => (IEnumerable<int>)GetValue(ItemsProperty);
			set => SetValue(ItemsProperty, value);
		}

		public double VerticalOffset
		{
			get => (double)GetValue(VerticalOffsetProperty);
			set => SetValue(VerticalOffsetProperty, value);
		}

		public Issue21708()
		{
			InitializeComponent();

			_items = new ObservableCollection<int>();
			Items = new ReadOnlyObservableCollection<int>(_items);

			BindingContext = this;
		}

		void CollectionView_OnScrolled(object sender, ItemsViewScrolledEventArgs e)
		{
			VerticalOffset = e.VerticalOffset;
		}

		void EmptyButton_OnClicked(object sender, EventArgs e)
		{
			_items.Clear();
		}

		void FillButton_OnClicked(object sender, EventArgs e)
		{
			foreach (var i in Enumerable.Range(0, 50))
				_items.Add(i);
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			CollectionView.ScrollTo(50);
		}

	}
}