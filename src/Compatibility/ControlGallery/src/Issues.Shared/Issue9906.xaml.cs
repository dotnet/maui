using System;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{

#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif

	[Issue(IssueTracker.Github, 9906, "Change the text of a button with CharacterSpacing set to 1 while the button is displayed cause Exception in version 4.3.0.908675 and above",
		PlatformAffected.iOS)]
	public partial class Issue9906 : TestContentPage
	{
		public Issue9906()
		{
#if APP
			InitializeComponent();
			changeButton.Pressed += (s, e) =>
			{
				button.Text = "This is a very updated text";
			};
#endif
		}

		protected override void Init()
		{

		}

	}
}