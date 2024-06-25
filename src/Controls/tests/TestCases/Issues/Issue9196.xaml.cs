using System.Collections.Generic;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 9196, "[Bug] [iOS] CollectionView EmptyView causes the application to crash",
		PlatformAffected.iOS)]
	public partial class Issue9196 : TestContentPage
	{
		public Issue9196()
		{
			InitializeComponent();
		}

		protected override void Init()
		{
			BindingContext = new _9196ViewModel();
		}
	}

	public class _9196ViewModel
	{
		public _9196ViewModel()
		{
			ReceiptsList = new List<string>();
		}

		public List<string> ReceiptsList { get; set; }
	}
}