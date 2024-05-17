using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[Category(UITestCategories.RadioButton)]
#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.None, 0, "RadioButton: Template From Style", PlatformAffected.All)]
	public class RadioButtonTemplateFromStyle : TestNavigationPage
	{
		protected override void Init()
		{
#if APP
			PushAsync(new GalleryPages.RadioButtonGalleries.TemplateFromStyle());
#endif
		}

#if UITEST
		[Test]
		public void ContentRenderers()
		{
			RunningApp.WaitForElement("A");
			RunningApp.WaitForElement("B");
			RunningApp.WaitForElement("C");
		}
#endif
	}
}
