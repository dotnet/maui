using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
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