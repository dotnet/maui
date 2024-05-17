using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

#if UITEST && __ANDROID__
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using NUnit.Framework;
using Microsoft.Maui.Controls.Compatibility.UITests;
using System.Linq;
#endif

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 4744, "forms in UWP show empty button for DisplayActionSheet", PlatformAffected.UWP)]
	public partial class Issue4744 : TestContentPage
	{
#if APP
		public Issue4744()
		{
			InitializeComponent();
		}
#endif
		protected override void Init()
		{
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			DisplayActionSheet("Title", "Cancel", "Destruction", "button", string.Empty);
		}

		private void Button_Clicked_1(object sender, EventArgs e)
		{
			DisplayActionSheet("Title", "Cancel", "Destruction", "button", null);
		}

		private void Button_Clicked_2(object sender, EventArgs e)
		{
			DisplayActionSheet("Title", "Cancel", "Destruction", "button", "Hello");
		}
	}
}
