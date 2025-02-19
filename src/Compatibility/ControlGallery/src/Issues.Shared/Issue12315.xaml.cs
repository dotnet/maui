using Microsoft.Maui.Controls.CustomAttributes;

#if UITEST
using Microsoft.Maui.Controls.Compatibility.UITests;
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Issue(IssueTracker.Github, 12315, "[Bug] Button disappears when setting CornerRadius",
		PlatformAffected.iOS)]
	public partial class Issue12315 : TestContentPage
	{
		public Issue12315()
		{
#if APP
			InitializeComponent();
#endif
		}

		protected override void Init()
		{

		}

#if APP
		void OnMarginSliderValueChanged(object sender, ValueChangedEventArgs e) => IssueButton.CornerRadius = (int)e.NewValue;
#endif
	}
}