using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
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
			if (DeviceInfo.Platform == DevicePlatform.iOS)
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
		[Compatibility.UITests.FailsOnMauiIOS]
		public void Issue5470Test() 
		{
			Thread.Sleep(500); // give it time to crash
			RunningApp.WaitForElement (q => q.Marked ("IssuePageLabel"));
		}
#endif
	}
}