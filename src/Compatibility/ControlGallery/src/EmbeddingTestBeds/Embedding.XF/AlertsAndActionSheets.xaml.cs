using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Embedding.XF
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AlertsAndActionSheets : ContentPage
	{
		public AlertsAndActionSheets()
		{
			InitializeComponent();

			ActionSheetButton.Clicked += ActionSheetButtonOnClicked;
			AlertButton.Clicked += AlertButtonOnClicked;
		}

		async void AlertButtonOnClicked(object sender, EventArgs eventArgs)
		{
			await DisplayAlert("Alert", "This alert is being displayed from a XF Page", "Cool, thanks");
		}

		async void ActionSheetButtonOnClicked(object sender, EventArgs eventArgs)
		{
			await DisplayActionSheet("ActionSheet", "Cool, thanks", "Cancel", "I'm a button!", "I, too, am a button.");
		}
	}
}