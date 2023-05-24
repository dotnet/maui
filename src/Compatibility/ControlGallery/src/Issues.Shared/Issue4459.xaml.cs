using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4459, "[UWP] BoxView CornerRadius doesn't work", PlatformAffected.UWP)]
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	public partial class Issue4459 : ContentPage
	{
		public Issue4459()
		{
#if APP
			InitializeComponent();
#endif
		}

		void InputView_OnTextChanged(object sender, TextChangedEventArgs e)
		{
#if APP
			BoxView.CornerRadius = new CornerRadius(double.Parse(TopLeft.Text), double.Parse(TopRight.Text),
			double.Parse(BottomLeft.Text), double.Parse(BottomRight.Text));
#endif
		}
	}
}