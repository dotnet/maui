using System.Collections.ObjectModel;
using System.Windows.Input;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using Xamarin.UITest;
using Xamarin.Forms.Core.UITests;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.Gestures)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1331, "[Android] ViewCell shows ContextActions on tap instead of long press", 
		PlatformAffected.Android)]
	public partial class GitHub1331 : TestContentPage
	{
		const string Action = "Action 1";
		const string ActionItemTapped = "Action Item Tapped";
		const string CellItem = "item 1";

		public GitHub1331 ()
		{
#if APP
			InitializeComponent ();

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
							new Command(() => DisplayAlert("Action tapped", "item 2", "Cancel"))
					},
					new GH1331ItemViewModel
					{
						Text = "item 3",
						ActionText = "Action 3",
						ActionTappedCommand =
							new Command(() => DisplayAlert("Action tapped", "item 3", "Cancel"))
					}
				})
			};

			BindingContext = mainViewModel;

			Title = "GH 1331";
#endif
		}

		protected override void Init()
		{
		}

		[Preserve(AllMembers = true)]
		class GH1331ViewModel
		{
			public ObservableCollection<GH1331ItemViewModel> Items { get; set; }
		}

		[Preserve(AllMembers = true)]
		class GH1331ItemViewModel
		{
			public string Text { get; set; }
			public string ActionText { get; set; }
			public ICommand ActionTappedCommand { get; set; }
		}

#if UITEST && __ANDROID__ // This test only makes sense on platforms using Long Press to activate context menus
		[Test]
		public void SingleTapOnCellDoesNotActivateContext()
		{
			RunningApp.WaitForElement(Action);
			
			RunningApp.Tap(Action);
			RunningApp.WaitForElement(ActionItemTapped);

			// Tapping the part of the cell without a tap gesture should *not* display the context action
			RunningApp.Tap(CellItem);
			RunningApp.WaitForNoElement("Context Action");

			// But a Long Press *should* still display the context action
			RunningApp.TouchAndHold(CellItem);
			RunningApp.WaitForElement("Context Action");
		}
#endif

	}
}