using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Essentials;
using Samples.Model;

namespace Samples.View
{
	public partial class PermissionsPage : BasePage
	{
		public PermissionsPage()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			MessagingCenter.Subscribe<PermissionItem, Exception>(this, nameof(PermissionException), async (p, ex) =>
				await DisplayAlert("Permission Error", ex.Message, "OK"));
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			MessagingCenter.Unsubscribe<PermissionItem, Exception>(this, nameof(PermissionException));
		}
	}
}
