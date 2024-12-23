using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 21708, "CollectionView.Scrolled event offset isn't correctly reset when items change on Android", PlatformAffected.All)]
	public partial class Issue21708 : ContentPage
	{
		ObservableCollection<int> _items;
        double _verticalOffset;

        public Issue21708()
        {
            InitializeComponent();
            _items = new ObservableCollection<int>();
            CollectionView.ItemsSource = _items;
			BindingContext = this;
        }

        public double VerticalOffset
        {
            get => _verticalOffset;
            set
            {
                _verticalOffset = value;
                OnPropertyChanged(nameof(VerticalOffset));
            }
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