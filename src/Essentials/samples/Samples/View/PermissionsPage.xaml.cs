using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
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

			WeakReferenceMessenger.Default.Register<Exception, string>(
				this,
				nameof(PermissionException),
				async (p, ex) => await DisplayAlertAsync("Permission Error", ex.Message, "OK"));
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			WeakReferenceMessenger.Default.Unregister<Exception, string>(this, nameof(PermissionException));
		}
	}
}
