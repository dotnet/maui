using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using System.Windows.Input;
using System.Diagnostics;

using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
{

#if UITEST
	[NUnit.Framework.Category(Core.UITests.UITestCategories.Github5000)]
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
