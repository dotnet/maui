using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Xaml;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Issue(IssueTracker.Github, 9196, "[Bug] [iOS] CollectionView EmptyView causes the application to crash",
		PlatformAffected.iOS)]
	public partial class Issue9196 : TestContentPage
	{
		public Issue9196()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
			BindingContext = new _9196ViewModel();
		}

#if UITEST
		[Test, Category(UITestCategories.CollectionView)]
		[Compatibility.UITests.FailsOnMauiIOS]
		public void EmptyViewShouldNotCrash()
		{
			RunningApp.WaitForElement("Success");
		}
#endif
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