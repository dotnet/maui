using CommunityToolkit.Mvvm.ComponentModel;

namespace Maui.Controls.Sample
{
    public partial class ItemViewModel : ObservableObject
    {
        // [ObservableProperty]
        // [NotifyPropertyChangedFor(nameof(SelectedChoiceText))]
        // private ChoiceItem selectedChoice;

        ChoiceItem? selectedChoice;
        public ChoiceItem? SelectedChoice
        {
            get => selectedChoice;
            set => SetProperty(ref selectedChoice, value);
        }

        [ObservableProperty]
        private string? title;

        public string TJString { get; set; } = "This is a TJ String";

        public string? SelectedChoiceText => this.SelectedChoice != null ? this.SelectedChoice.ChoiceText : string.Empty;


        public ItemViewModel()
        {
            this.Choices = new List<ChoiceItem>();

            this.Choices.Add(new ChoiceItem() { ChoiceId = 1, ChoiceText = "Choice 1" });
            this.Choices.Add(new ChoiceItem() { ChoiceId = 2, ChoiceText = "Choice 2" });
            this.Choices.Add(new ChoiceItem() { ChoiceId = 3, ChoiceText = "Choice 3" });

            this.SelectedChoice = this.Choices[0];
        }

        public List<ChoiceItem> Choices { get; set; }
    }

    public class ChoiceItem
    {
        public int ChoiceId { get; set; }
        public string? ChoiceText { get; set; }
    }
}
