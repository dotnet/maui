namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 9196, "CollectionView EmptyView causes the application to crash", PlatformAffected.iOS)]
	public partial class EmptyViewNoCrash : ContentPage
	{

		public EmptyViewNoCrash()
		{
			InitializeComponent();

			BindingContext = new EmptyViewNoCrashViewModel();
		}
	}

	public class EmptyViewNoCrashViewModel
	{
		public EmptyViewNoCrashViewModel()
		{
			ReceiptsList = new List<string>();
		}

		public List<string> ReceiptsList { get; set; }
	}
}