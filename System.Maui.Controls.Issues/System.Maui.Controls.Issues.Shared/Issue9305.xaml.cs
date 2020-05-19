using System.Maui.CustomAttributes;
using System.Collections.Generic;

#if UITEST
using System.Maui.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace System.Maui.Controls.Issues
{
	[Issue(IssueTracker.Github, 9305, "Swipe View BackgroundColor Issues", PlatformAffected.Android | PlatformAffected.iOS)]
	public partial class Issue9305 : TestContentPage
	{
		public Issue9305()
		{
#if APP
			Device.SetFlags(new List<string> { ExperimentalFlags.SwipeViewExperimental });
			Title = "Issue 9305";
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}
	}
}