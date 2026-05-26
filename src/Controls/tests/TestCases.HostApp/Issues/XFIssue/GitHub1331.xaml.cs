using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 1331, "[Android] ViewCell shows ContextActions on tap instead of long press",
	PlatformAffected.Android)]
public partial class GitHub1331 : TestContentPage
{
	const string Action = "Action 1";
	const string ActionItemTapped = "Action Item Tapped";
	const string CellItem = "item 1";

	public GitHub1331()
	{

		InitializeComponent();

		var mainViewModel = new GH1331ViewModel
		{
			Items = new ObservableCollection<GH1331ItemViewModel>(new[]
			{
				new GH1331ItemViewModel
				{
					Text = CellItem,
					ActionText = Action,
					ActionTappedCommand =
						new Command(() => Result.Text = ActionItemTapped)
				},
				new GH1331ItemViewModel
				{
					Text = "item 2",
					ActionText = "Action 2",
					ActionTappedCommand =
						new Command(() => DisplayAlertAsync("Action tapped", "item 2", "Cancel"))
				},
				new GH1331ItemViewModel
				{
					Text = "item 3",
					ActionText = "Action 3",
					ActionTappedCommand =
						new Command(() => DisplayAlertAsync("Action tapped", "item 3", "Cancel"))
				}
			})
		};

		BindingContext = mainViewModel;

		Title = "GH 1331";

	}

	protected override void Init()
	{
	}


	class GH1331ViewModel
	{
		public ObservableCollection<GH1331ItemViewModel> Items { get; set; }
	}


	class GH1331ItemViewModel
	{
		public string Text { get; set; }
		public string ActionText { get; set; }
		public ICommand ActionTappedCommand { get; set; }
	}
}