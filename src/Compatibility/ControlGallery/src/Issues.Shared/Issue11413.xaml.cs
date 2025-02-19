using System.Collections.Generic;
using Microsoft.Maui.Controls.CustomAttributes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Issue(IssueTracker.Github, 11413,
		"[Bug] Rectangle shape - Incorrect rendering/crash (depending on platform)",
		PlatformAffected.Android)]
	public partial class Issue11413 : TestContentPage
	{
		public Issue11413()
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