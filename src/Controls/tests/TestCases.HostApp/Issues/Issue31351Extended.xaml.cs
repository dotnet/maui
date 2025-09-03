using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, 31351, "[Windows] - Extended test for Customised CollectionView ScrollTo", PlatformAffected.UWP)]
    public partial class Issue31351Extended : ContentPage
    {
        public Issue31351Extended()
        {
            InitializeComponent();
            var viewModel = new Issue31351ExtendedViewModel();
            BindingContext = viewModel;

            // Wire up the collection view reference
            var collectionView = this.FindByName<ExtendedCustomCollectionView>("extendedCollectionView");
            var scrollStatusLabel = this.FindByName<Label>("ScrollStatusLabel");
            var selectionStatusLabel = this.FindByName<Label>("SelectionStatusLabel");

            viewModel.SetCollectionView(collectionView);

            // Subscribe to scroll events
            collectionView.Scrolled += (sender, e) =>
            {
                scrollStatusLabel.Text = $"✅ Scrolled: First={e.FirstVisibleItemIndex}, Center={e.CenterItemIndex}, Last={e.LastVisibleItemIndex}";
            };

            // Subscribe to selection events
            collectionView.SelectionChanged += (sender, e) =>
            {
                if (e.CurrentSelection.Any())
                {
                    var selected = e.CurrentSelection.First() as ExtendedTestItem;
                    selectionStatusLabel.Text = $"✅ Selected: {selected?.Name} (Index {selected?.Index})";
                }
                else
                {
                    selectionStatusLabel.Text = "No selection";
                }
            };
        }
    }

    // More complex custom CollectionView with additional properties and methods
    public class ExtendedCustomCollectionView : CollectionView
    {
        public static readonly BindableProperty CustomTitleProperty =
            BindableProperty.Create(nameof(CustomTitle), typeof(string), typeof(ExtendedCustomCollectionView), string.Empty);

        public string CustomTitle
        {
            get => (string)GetValue(CustomTitleProperty);
            set => SetValue(CustomTitleProperty, value);
        }

        public static readonly BindableProperty CustomColorProperty =
            BindableProperty.Create(nameof(CustomColor), typeof(Color), typeof(ExtendedCustomCollectionView), Colors.Transparent);

        public Color CustomColor
        {
            get => (Color)GetValue(CustomColorProperty);
            set => SetValue(CustomColorProperty, value);
        }

        public ExtendedCustomCollectionView()
        {
            CustomTitle = "Extended Custom CollectionView";
            CustomColor = Colors.LightBlue;
        }

        // Custom method that might be used in real scenarios
        public void CustomScrollToWithSelection(int index)
        {
            ScrollTo(index, position: ScrollToPosition.Center, animate: true);
            if (ItemsSource is ObservableCollection<ExtendedTestItem> items && index < items.Count)
            {
                SelectedItem = items[index];
            }
        }
    }

    public class Issue31351ExtendedViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<ExtendedTestItem> Items { get; set; }
        public ICommand ScrollToTopCommand { get; set; }
        public ICommand ScrollTo10Command { get; set; }
        public ICommand ScrollTo30Command { get; set; }
        public ICommand ScrollToBottomCommand { get; set; }

        private ExtendedCustomCollectionView _collectionView;

        public Issue31351ExtendedViewModel()
        {
            // Create more complex test data
            Items = new ObservableCollection<ExtendedTestItem>();
            for (int i = 0; i < 50; i++)
            {
                Items.Add(new ExtendedTestItem
                {
                    Index = i,
                    Name = $"Extended Item {i}",
                    Description = $"This is a detailed description for item number {i}"
                });
            }

            ScrollToTopCommand = new Command(ScrollToTop);
            ScrollTo10Command = new Command(ScrollTo10);
            ScrollTo30Command = new Command(ScrollTo30);
            ScrollToBottomCommand = new Command(ScrollToBottom);
        }

        public void SetCollectionView(ExtendedCustomCollectionView collectionView)
        {
            _collectionView = collectionView;
        }

        private void ScrollToTop()
        {
            _collectionView?.ScrollTo(0, position: ScrollToPosition.Start, animate: false);
        }

        private void ScrollTo10()
        {
            // Test both ScrollTo by index and custom method
            _collectionView?.CustomScrollToWithSelection(10);
        }

        private void ScrollTo30()
        {
            // Test ScrollTo by item
            if (_collectionView != null && Items.Count > 30)
            {
                _collectionView.ScrollTo(Items[30], position: ScrollToPosition.Center, animate: true);
            }
        }

        private void ScrollToBottom()
        {
            if (_collectionView != null && Items.Count > 0)
            {
                _collectionView.ScrollTo(Items.Count - 1, position: ScrollToPosition.End, animate: false);
            }
        }

        protected virtual void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ExtendedTestItem
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
