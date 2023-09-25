using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if APP
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5801, "TextDecorations Strikethrough not called for FormattedText in iOS", PlatformAffected.iOS)]

	public partial class Issue5801 : ContentPage
	{
		public Issue5801()
		{
			InitializeComponent();
		}
	}
#endif
}