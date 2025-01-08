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

				Section1Items.Add(new ItemViewModel() { Title = "Section 1" });
				Section2Items.Add(new ItemViewModel() { Title = "Section 2" });

				LoadSection1Command = new Command(() => CurrentItems = new List<ItemViewModel>(Section1Items));
				LoadSection2Command = new Command(() => CurrentItems = new List<ItemViewModel>(Section2Items));

				CurrentItems = new List<ItemViewModel>(Section1Items);
			}

			private List<ItemViewModel> _section1Items;
			public List<ItemViewModel> Section1Items
			{
				get => _section1Items;
				set
				{
					_section1Items = value;
					OnPropertyChanged();
				}
			}

			private List<ItemViewModel> _section2Items;
			public List<ItemViewModel> Section2Items
			{
				get => _section2Items;
				set
				{
					_section2Items = value;
					OnPropertyChanged();
				}
			}
			private List<ItemViewModel> _currentItems;
			public List<ItemViewModel> CurrentItems
			{
				get => _currentItems;
				set
				{
					_currentItems = value;
					OnPropertyChanged();
				}
			}
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

			public string Title { get; set; }
			static int cnt = 0;

			public ItemViewModel()
			{
				Choices =
				[
					new ChoiceItem() { ChoiceId = 1, ChoiceText = $"Choice {cnt++}" },
					new ChoiceItem() { ChoiceId = 2, ChoiceText = $"Choice {cnt++}" },
					new ChoiceItem() { ChoiceId = 3, ChoiceText = $"Choice {cnt++}" },
				];

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