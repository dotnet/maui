using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{

#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2447, "Force label text direction", PlatformAffected.Android)]
	public partial class Issue2447 : ContentPage
	{
#if APP
		public Issue2447()
		{
			InitializeComponent();
		}
#endif
	}
}
