using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
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

#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Subscribe<PermissionItem, Exception>(this, nameof(PermissionException), async (p, ex) =>
				await DisplayAlert("Permission Error", ex.Message, "OK"));
#pragma warning restore CS0618 // Type or member is obsolete
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Unsubscribe<PermissionItem, Exception>(this, nameof(PermissionException));
#pragma warning restore CS0618 // Type or member is obsolete
		}
	}
}
