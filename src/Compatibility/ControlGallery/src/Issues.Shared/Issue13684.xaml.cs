using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.CustomAttributes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Issue(IssueTracker.Github, 13684,
		"[Bug] Label: LineBreakMode incompatible with MaxLines",
		PlatformAffected.Android)]
	public partial class Issue13684 : TestContentPage
	{
		public Issue13684()
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