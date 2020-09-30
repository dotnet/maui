using System;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if UITEST
	[Category(UITestCategories.AppLinks)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5470, "ApplinkEntry Thumbnail required after upgrading to 3.5/3.6", PlatformAffected.iOS)]
	public class Issue5470 : TestContentPage
	{
		protected override void Init()
		{
			// android needs firebase for this to work, so skip it unless iOS
			if (Device.RuntimePlatform == Device.iOS)
				Application.Current.AppLinks.RegisterLink(GetEntry());

			// Initialize ui here instead of ctor
			Content = new Label
			{
				AutomationId = "IssuePageLabel",
				Text = "If this is iOS, I just tried to register an applink without a Thumbnail. If this did not crash, this test has succeeded."
			};
		}

		AppLinkEntry GetEntry()
		{
			if (string.IsNullOrEmpty(Title))
				Title = "ApplinkEntry Thumbnail required after upgrading to 3.5/3.6";

			var type = GetType().ToString();
			var entry = new AppLinkEntry
			{
				Title = Title,
				Description = $"ApplinkEntry Thumbnail required after upgrading to 3.5/3.6",
				AppLinkUri = new Uri($"http://blah/gallery/{type}", UriKind.RelativeOrAbsolute),
				IsLinkActive = true,
				Thumbnail = null
			};

			entry.KeyValues.Add("contentType", "GalleryPage");
			entry.KeyValues.Add("appName", "blah");
			entry.KeyValues.Add("companyName", "Xamarin");

			return entry;
		}

#if UITEST && __IOS__
		[Test]
		public void Issue5470Test() 
		{
			Thread.Sleep(500); // give it time to crash
			RunningApp.WaitForElement (q => q.Marked ("IssuePageLabel"));
		}
#endif
	}
}