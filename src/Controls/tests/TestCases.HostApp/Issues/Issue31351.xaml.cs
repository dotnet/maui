using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [Issue(IssueTracker.Github, 31351, "[Windows] - Customised CollectionView (inherited from) does not [ScrollTo] and display selection correctly", PlatformAffected.UWP)]
    public partial class Issue31351 : ContentPage
    {
        public Issue31351()
        {
            InitializeComponent();
            var viewModel = new Issue31351ViewModel();
            BindingContext = viewModel;

            // Wire up the collection view reference
            var collectionView = this.FindByName<CustomCollectionView>("customCollectionView");
            viewModel.SetCollectionView(collectionView);

            // Subscribe to scroll events to update status
            var statusLabel = this.FindByName<Label>("statusLabel");
            collectionView.Scrolled += (sender, e) =>
            {
                statusLabel.Text = $"Scrolled to: First={e.FirstVisibleItemIndex}, Center={e.CenterItemIndex}, Last={e.LastVisibleItemIndex}";
            };
        }
    }

    // Custom CollectionView class that inherits from CollectionView
    // This reproduces the exact issue scenario
    public class CustomCollectionView : CollectionView
    {
        public CustomCollectionView()
        {
            // This custom CollectionView should work the same as regular CollectionView
            // The issue was that ScrollTo didn't work on Windows for inherited classes
        }
    }

    public class Issue31351ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<TestItem> Items { get; set; }
        public ICommand ScrollToIndex25Command { get; set; }
        public ICommand ScrollToItem40Command { get; set; }
        public ICommand ScrollToEndCommand { get; set; }

        private CustomCollectionView _collectionView;

        public Issue31351ViewModel()
        {
            // Create test data
            Items = new ObservableCollection<TestItem>();
            for (int i = 0; i < 100; i++)
            {
                Items.Add(new TestItem { Index = i, Name = $"Test Item {i}" });
            }

            ScrollToIndex25Command = new Command(ScrollToIndex25);
            ScrollToItem40Command = new Command(ScrollToItem40);
            ScrollToEndCommand = new Command(ScrollToEnd);
        }

        public void SetCollectionView(CustomCollectionView collectionView)
        {
            _collectionView = collectionView;
        }

        private void ScrollToIndex25()
        {
            _collectionView?.ScrollTo(25, position: ScrollToPosition.Start, animate: false);
        }

        private void ScrollToItem40()
        {
            if (_collectionView != null && Items.Count > 40)
            {
                _collectionView.ScrollTo(Items[40], position: ScrollToPosition.Start, animate: false);
            }
        }

        private void ScrollToEnd()
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

    public class TestItem
    {
        public int Index { get; set; }
        public string Name { get; set; }
    }
}
