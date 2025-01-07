using static Maui.Controls.Sample.Issues.Issue18702;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25729, "Picker Selected item gets cleared in BindableLayout", PlatformAffected.All)]
	public partial class Issue25729 : ContentPage
	{
		public Issue25729()
		{
			InitializeComponent();
			BindingContext = new Issue25729ViewModel();
		}

		public class Issue25729ViewModel : ViewModel
		{

			public Command LoadSection1Command { get; set; }
			public Command LoadSection2Command { get; set; }

			public Issue25729ViewModel()
			{
				Section1Items = [];
				Section2Items = [];
				CurrentItems = [];

				Section1Items.Add(new ItemViewModel(0, 1, 2) { Title = "Section 1" });
				Section2Items.Add(new ItemViewModel(10, 11, 12, 13) { Title = "Section 2" });

				LoadSection1Command = new Command(() => CurrentItems.ReplaceRange(Section1Items));
				LoadSection2Command = new Command(() => CurrentItems.ReplaceRange(Section2Items));

				CurrentItems.ReplaceRange(Section1Items);
			}

			public ObservableRangeCollection<ItemViewModel> Section1Items { get; set; }
			public ObservableRangeCollection<ItemViewModel> Section2Items { get; set; }
			public ObservableRangeCollection<ItemViewModel> CurrentItems { get; set; }
		}

		public class ItemViewModel : ViewModel
		{
			private ChoiceItem _selectedChoice;
			public ChoiceItem SelectedChoice
			{
				get => _selectedChoice;
				set
				{
					_selectedChoice = value;
					SelectedChoiceText = value?.ChoiceText ?? string.Empty;
					OnPropertyChanged();
				}
			}

			private string _selectedChoiceText;
			public string SelectedChoiceText
			{
				get => _selectedChoiceText;
				set
				{
					_selectedChoiceText = value;
					OnPropertyChanged();
				}
			}

			private int _selectedChoiceIndex;
			public int SelectedChoiceIndex
			{
				get => _selectedChoiceIndex;
				set
				{
					_selectedChoiceIndex = value;
					OnPropertyChanged();
				}
			}

			public string Title { get; set; }

			public ItemViewModel(params int[] choices)
			{
				Choices = choices.Select(c => new ChoiceItem { ChoiceId = c, ChoiceText = $"Choice {c}" }).ToList();
				SelectedChoice = Choices[0];
			}

			public List<ChoiceItem> Choices { get; set; }
		}

		public class ChoiceItem
		{
			public int ChoiceId { get; set; }
			public string ChoiceText { get; set; }
		}
	}
}