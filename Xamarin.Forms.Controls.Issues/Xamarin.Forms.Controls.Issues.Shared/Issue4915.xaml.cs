using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4915, "Unify the image handling")]
	public class Issue4915 : TestTabbedPage
	{
		protected override void Init()
		{
			var navPage = new NavigationPage(new Issue4915ContentPage()) { Title = "nav page 1" };
#pragma warning disable CS0618 // Type or member is obsolete
			navPage.SetBinding(Page.IconProperty, "Image");
#pragma warning restore CS0618 // Type or member is obsolete
			navPage.BindingContext = new Issue4915ContentPage.ViewModel();

			Children.Add(navPage);
			Children.Add(new Issue4915ContentPage() { Title = "page 2" });
		}

#if UITEST
		[Test]
		public void LegacyImageSourceProperties()
		{
			RunningApp.WaitForElement("Nothing Crashed");
		}
#endif

	}

	[Preserve(AllMembers = true)]
	public partial class Issue4915ContentPage : ContentPage
	{
		public Issue4915ContentPage()
		{
#if APP
			InitializeComponent();
#endif
			BindingContext = new ViewModel();

		}

		[Preserve(AllMembers = true)]
		public class ViewModel
		{
			public string Image { get; set; } = "coffee.png";
		}
	}
}