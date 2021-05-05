using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
#if APP
#if UITEST
		[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 60787, "Frames with border radius preset have this radius reset when their background color is changed.",
		PlatformAffected.Android, issueTestNumber: 1)]
	public partial class Bugzilla60787 : ContentPage
	{
		bool _colourIndicator;

		public Bugzilla60787()
		{
			InitializeComponent();

			this.btnChangeColour.Clicked += btnChangeColour_Click;
		}

		void btnChangeColour_Click(object sender, EventArgs e)
		{
			this.frmDoesChange.BackgroundColor = _colourIndicator ? Colors.LightBlue : Colors.LightGoldenrodYellow;

			_colourIndicator = !_colourIndicator;
		}
	}
#endif
}
