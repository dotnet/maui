using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 11413,
		"[Bug] Rectangle shape - Incorrect rendering/crash (depending on platform)",
		PlatformAffected.Android)]
	public partial class Issue11413 : TestContentPage
	{
		public Issue11413()
		{
#if APP
			Device.SetFlags(new List<string> { ExperimentalFlags.ShapesExperimental });
			InitializeComponent();
#endif
		}

		protected override void Init()
		{
		}
	}
}