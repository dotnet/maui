using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 11715, "[Bug] Brushes API - Android's direction is inverted to iOS's",
		PlatformAffected.Android)]
	public partial class Issue11715 : TestContentPage
	{
		public Issue11715()
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