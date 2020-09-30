using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 9305, "Swipe View BackgroundColor Issues", PlatformAffected.Android | PlatformAffected.iOS)]
	public partial class Issue9305 : TestContentPage
	{
		public Issue9305()
		{
#if APP
			Title = "Issue 9305";
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}
	}
}