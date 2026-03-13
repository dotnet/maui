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
using Microsoft.Maui.Graphics;
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

		private async void TestLocationWhenInUse_Clicked(object sender, EventArgs e)
		{
			await TestLocationPermissionConsistency<Permissions.LocationWhenInUse>(
				LocationWhenInUseCheckLabel, 
				LocationWhenInUseRequestLabel);
		}

		private async void TestLocationAlways_Clicked(object sender, EventArgs e)
		{
			await TestLocationPermissionConsistency<Permissions.LocationAlways>(
				LocationAlwaysCheckLabel, 
				LocationAlwaysRequestLabel);
		}

		private async Task TestLocationPermissionConsistency<T>(Label checkLabel, Label requestLabel) where T : Permissions.BasePermission, new()
		{
			try
			{
				var permission = new T();
				
				// Test CheckStatusAsync
				var checkStatus = await permission.CheckStatusAsync();
				checkLabel.Text = $"Check: {checkStatus}";
				
				// Test RequestAsync 
				var requestStatus = await permission.RequestAsync();
				requestLabel.Text = $"Request: {requestStatus}";
				
				// Update consistency result
				UpdateConsistencyResult();
			}
			catch (Exception ex)
			{
				await DisplayAlert("Error", $"Error testing {typeof(T).Name}: {ex.Message}", "OK");
			}
		}

		private void UpdateConsistencyResult()
		{
			// Check if both location permission tests have been run
			var whenInUseCheck = LocationWhenInUseCheckLabel.Text;
			var whenInUseRequest = LocationWhenInUseRequestLabel.Text;
			var alwaysCheck = LocationAlwaysCheckLabel.Text;
			var alwaysRequest = LocationAlwaysRequestLabel.Text;

			if (whenInUseCheck.Contains("Unknown", StringComparison.Ordinal) || whenInUseRequest.Contains("Unknown", StringComparison.Ordinal) ||
			    alwaysCheck.Contains("Unknown", StringComparison.Ordinal) || alwaysRequest.Contains("Unknown", StringComparison.Ordinal))
			{
				ConsistencyResultLabel.Text = "Consistency: Not fully tested";
				ConsistencyResultLabel.TextColor = Colors.Gray;
				return;
			}

			// Extract status values and compare
			var whenInUseCheckStatus = ExtractStatus(whenInUseCheck);
			var whenInUseRequestStatus = ExtractStatus(whenInUseRequest);
			var alwaysCheckStatus = ExtractStatus(alwaysCheck);
			var alwaysRequestStatus = ExtractStatus(alwaysRequest);

			bool isConsistent = (whenInUseCheckStatus == whenInUseRequestStatus) && 
			                   (alwaysCheckStatus == alwaysRequestStatus);

			if (isConsistent)
			{
				ConsistencyResultLabel.Text = "Consistency: ✅ PASS - CheckStatusAsync matches RequestAsync";
				ConsistencyResultLabel.TextColor = Colors.Green;
			}
			else
			{
				ConsistencyResultLabel.Text = "Consistency: ❌ FAIL - CheckStatusAsync differs from RequestAsync";
				ConsistencyResultLabel.TextColor = Colors.Red;
			}
		}

		private string ExtractStatus(string text)
		{
			var parts = text.Split(':');
			return parts.Length > 1 ? parts[1].Trim() : "";
		}
	}
}
