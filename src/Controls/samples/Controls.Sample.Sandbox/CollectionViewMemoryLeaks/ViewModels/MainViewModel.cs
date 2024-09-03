using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CollectionViewMemoryLeaks.Models;


namespace CollectionViewMemoryLeaks.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel()
        {
            LoadData();
        }

        public ICommand NavigateToCollectionViewSampleCommand => new Command(async () => await NavigateToCollectionViewSample());

        private async Task NavigateToCollectionViewSample()
        {
            var parameters = new Dictionary<string, object>()
            {
                { "Sample", "Hi" },
                { "Test", 1 },
            };

            await Shell.Current.GoToAsync("CollectionViewSamplePage", false, new Dictionary<string, object>
            {
                { "parameters", parameters }
            });
        }

        private ObservableCollection<RandomItem> _randomItems = new ObservableCollection<RandomItem>();
        public ObservableCollection<RandomItem> RandomItems
        {
            get => _randomItems;
            set
            {
                _randomItems = value;
                RaiseOnPropertyChanged();
            }
        }

        private void LoadData()
        {
            for (var i = 0; i < 50; i++)
            {
                RandomItems.Add(new RandomItem
                {
                    Name = $"Item {i}"
                });
            }
        }
    }
}