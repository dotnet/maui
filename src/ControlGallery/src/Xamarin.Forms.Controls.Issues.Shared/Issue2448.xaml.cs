using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 2448, "Setting FlowDirection of Alerts and ActionSheets", PlatformAffected.iOS | PlatformAffected.Android)]
#if APP
	[XamlCompilation(XamlCompilationOptions.Compile)]
#endif
	public partial class Issue2448 : ContentPage
	{
		public Issue2448()
		{
#if APP
			InitializeComponent();
#endif
		}

#if APP
		void AlertNoFlow_Clicked(object sender, EventArgs args)
		{
			var alert = DisplayAlert("Alert", "You have been alerted", "OK");
		}

		void AlertMatchParent_Clicked(object sender, EventArgs args)
		{
			var alert = DisplayAlert("Alert", "You have been alerted", "OK", FlowDirection.MatchParent);
		}

		void AlertRTL_Clicked(object sender, EventArgs args)
		{
			var alert = DisplayAlert("Alert", "You have been alerted", "OK", FlowDirection.RightToLeft);
		}

		void AlertLTR_Clicked(object sender, EventArgs args)
		{
			var alert = DisplayAlert("Alert", "You have been alerted", "OK", FlowDirection.LeftToRight);
		}

		void ActionsheetNoFlow_Clicked(object sender, EventArgs args)
		{
			var alert = DisplayActionSheet("ActionSheet: SavePhoto?", "Cancel", "Delete", "Photo Roll", "Email");
		}

		void ActionsheetMatchParent_Clicked(object sender, EventArgs args)
		{
			var alert = DisplayActionSheet("ActionSheet: SavePhoto?", "Cancel", "Delete", FlowDirection.MatchParent, "Photo Roll", "Email");
		}

		void ActionsheetRTL_Clicked(object sender, EventArgs args)
		{
			var alert = DisplayActionSheet("ActionSheet: SavePhoto?", "Cancel", "Delete", FlowDirection.RightToLeft, "Photo Roll", "Email");
		}

		void ActionsheetLTR_Clicked(object sender, EventArgs args)
		{
			var alert = DisplayActionSheet("ActionSheet: SavePhoto?", "Cancel", "Delete", FlowDirection.LeftToRight, "Photo Roll", "Email");
		}
#endif
	}
}