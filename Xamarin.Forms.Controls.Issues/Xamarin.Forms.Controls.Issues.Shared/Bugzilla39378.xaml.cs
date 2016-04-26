using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif


namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 39378, "Image binding with caching not operating as expected", PlatformAffected.All)]
	public partial class Bugzilla39378 : TestContentPage
	{
#if APP
		public Bugzilla39378()
		{
			InitializeComponent();
		}
#endif

		protected override void Init()
		{
			BindingContext = new ImageController();
		}

		class ImageController : ViewModelBase
		{
			
			public ImageController()
			{
				HomeImage = "http://xamarin.com/content/images/pages/forms/example-app.png";
				LocalBackgroundImage = "Default-568h@2x.png";
				BackgroundColor = "#00FF00";
			}

			public string BackgroundColor
			{
				get
				{ 
					return _backgroundColor;
				}

				set
				{
					_backgroundColor = value;
					OnPropertyChanged();
				}
			}

			public string HomeImage
			{
				get
				{ 
					return _homeImage;
				}

				set
				{
					_homeImage = value;
					OnPropertyChanged();
				}
			}

			public string LocalBackgroundImage
			{
				get
				{ 
					return _localBackgroundImage;
				}

				set
				{
					_localBackgroundImage = value;
					OnPropertyChanged();
				}
			}


			string _backgroundColor;
			string _homeImage;
			string _localBackgroundImage;
		}

#if UITEST
		[Test]
		public void ImageIsPresent()
		{
			RunningApp.WaitForElement(q => q.Marked("image1"));
			Assert.Inconclusive("Please verify image is present");
		}
#endif
	}
}
