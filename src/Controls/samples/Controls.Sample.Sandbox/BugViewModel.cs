using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Maui.Controls.Sample
{
    public partial class BugViewModel : ObservableObject
    {
        [ObservableProperty]
        private string? title;

        public BugViewModel()
        {

            this.Section1Items = new MvvmHelpers.ObservableRangeCollection<ItemViewModel>();
            this.Section2Items = new MvvmHelpers.ObservableRangeCollection<ItemViewModel>();
            this.CurrentItems = new MvvmHelpers.ObservableRangeCollection<ItemViewModel>();

            this.Section1Items.Add(new ItemViewModel() { Title = "Section 1" });
            this.Section2Items.Add(new ItemViewModel() { Title = "Section 2" });

            this.CurrentItems.ReplaceRange(this.Section1Items);
        }

        public MvvmHelpers.ObservableRangeCollection<ItemViewModel> Section1Items { get; set; }
        public MvvmHelpers.ObservableRangeCollection<ItemViewModel> Section2Items { get; set; }
        public MvvmHelpers.ObservableRangeCollection<ItemViewModel> CurrentItems { get; set; }

        [RelayCommand]
        public void LoadSection1()
        {
            // When we come back to this, the Section1Items already are missing the properties
            this.CurrentItems.ReplaceRange(this.Section1Items);
        }

        [RelayCommand]
        public void LoadSection2()
        {
            this.CurrentItems.ReplaceRange(this.Section2Items);
        }
    }
}
