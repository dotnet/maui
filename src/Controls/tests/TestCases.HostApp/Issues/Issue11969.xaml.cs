using System.Diagnostics;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 11969,
		"[Bug] Disabling Swipe view not handling tap gesture events on the content in iOS of Xamarin Forms",
		PlatformAffected.iOS)]
	public partial class Issue11969 : TestContentPage
	{
		const string SwipeViewId = "SwipeViewId";
		const string SwipeButtonId = "SwipeButtonId";

		const string Failed = "SwipeView Button not tapped";
		const string Success = "SUCCESS";

		public Issue11969()
		{
			InitializeComponent();
		}

		protected override void Init()
		{

		}

		void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Debug.WriteLine("CollectionView SelectionChanged");
		}

		void OnSwipeViewSwipeStarted(object sender, SwipeStartedEventArgs e)
		{
			Debug.WriteLine("SwipeView SwipeStarted");
		}

		void OnSwipeViewSwipeEnded(object sender, SwipeEndedEventArgs e)
		{
			Debug.WriteLine("SwipeView SwipeEnded");
		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			Debug.WriteLine("Button Clicked");
			TestLabel.Text = Success;
			DisplayAlertAsync("Issue 11969", "Button Clicked", "Ok");
		}
	}
}