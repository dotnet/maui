using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CollectionViewMemoryLeaks.Models;


namespace CollectionViewMemoryLeaks.ViewModels
{
    public class CollectionViewSampleViewModel : BaseViewModel
    {
        public CollectionViewSampleViewModel()
        {
            LoadData();
        }

        public ICommand GoBackCommand => new Command(async () => await GoBack());

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

        private async Task GoBack()
        {
            await Shell.Current.Navigation.PopAsync();
        }
    }
}