using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 10166, "[iOS] IsClippedToBounds Property is ignored by Frames", PlatformAffected.iOS)]
	public partial class Issue10166 : TestContentPage
	{
		public Issue10166()
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