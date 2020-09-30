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
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Github5000)]
	[NUnit.Framework.Category(Core.UITests.UITestCategories.UwpIgnore)]
#endif
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


			var urlNavPage = new NavigationPage(new Issue4915ContentPage()) { Title = "nav page 2" };
			urlNavPage.SetBinding(Page.IconImageSourceProperty, "ImageUrl");
			urlNavPage.BindingContext = new Issue4915ContentPage.ViewModel();

			var titleIconPage = new Issue4915ContentPage();
			var justSetOnNavPage = new NavigationPage(titleIconPage) { Title = "nav page 3" };
#pragma warning disable CS0618 // Type or member is obsolete
			NavigationPage.SetTitleIcon(titleIconPage, "coffee.png");
#pragma warning restore CS0618 // Type or member is obsolete

			Children.Add(navPage);
			Children.Add(urlNavPage);
			Children.Add(justSetOnNavPage);
			Children.Add(new Issue4915ContentPage() { Title = "page 2" });


		}

#if UITEST
		[Test]
		public void LegacyImageSourceProperties()
		{
			RunningApp.WaitForElement("Nothing Crashed");
			RunningApp.QueryUntilPresent(
				() =>
				{
					var result = RunningApp.WaitForElement("Image1");

					if (result[0].Rect.Height > 50)
						return result;

					return null;
				}, 10, 2000);

			// ensure url images have loaded
			System.Threading.Thread.Sleep(2000);
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

		private void ButtonClicked(object sender, EventArgs e)
		{
			ViewModel vm = null;
			if ((BindingContext as ViewModel).Image != "oasis.png")
			{
				vm = new ViewModel()
				{
					Image = "oasis.png",
					ImageUrl = "https://raw.githubusercontent.com/xamarin/Xamarin.Forms/78385f9fc1fc56dc88bd98e73bf9c8f2f2d0a90a/Xamarin.Forms.ControlGallery.iOS/Resources/jet.png"
				};

			}
			else
			{
				vm = new ViewModel();
			}

			BindingContext = vm;
			Parent.BindingContext = vm;
			Parent.Parent.BindingContext = vm;
		}

		[Preserve(AllMembers = true)]
		public class ViewModel
		{
			public string Image { get; set; } = "coffee.png";
			public string ImageUrl { get; set; } = "https://raw.githubusercontent.com/xamarin/Xamarin.Forms/f27f5a3650f37894d4a1ac925d6fab4dc7350087/Xamarin.Forms.ControlGallery.iOS/oasis.jpg";
		}
	}
}