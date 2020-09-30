using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.Issues
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