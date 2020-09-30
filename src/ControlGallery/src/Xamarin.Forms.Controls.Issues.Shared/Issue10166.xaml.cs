using System;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
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