using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 9588, "Frame inside SwipeView can't be swiped", PlatformAffected.iOS)]
	public partial class Issue9588 : TestContentPage
	{
		public Issue9588()
		{
#if APP
			InitializeComponent();
#endif
		}
		protected override void Init()
		{
		}
	}
}