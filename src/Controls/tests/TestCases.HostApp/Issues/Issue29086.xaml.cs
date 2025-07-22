using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29086, "SwipeView Closes when Content Changes even with SwipeBehaviorOnInvoked is set to RemainOpen", PlatformAffected.iOS)]
public partial class Issue29086 : ContentPage
{
	public ObservableCollection<NumberItem> Numbers { get; set; }
	public ICommand IncrementCommand { get; }
	public ICommand DecrementCommand { get; }

	public Issue29086()
	{
		InitializeComponent();

		Numbers = new ObservableCollection<NumberItem>
		{
			new NumberItem { Value = 1 },
		};

		IncrementCommand = new Command<NumberItem>((item) => item.Value++);
		DecrementCommand = new Command<NumberItem>((item) => item.Value--);

		BindingContext = this;
	}

	public class NumberItem : ViewModelBase
	{
		int _value;
		public int Value
		{
			get => _value;
			set { _value = value; OnPropertyChanged(); }
		}
	}
}