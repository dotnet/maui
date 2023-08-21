// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Networking;

namespace Samples.ViewModel
{
	public class ConnectivityViewModel : BaseViewModel
	{
		public ConnectivityViewModel()
		{
		}

		public string NetworkAccess =>
			Connectivity.NetworkAccess.ToString();

		public string ConnectionProfiles
		{
			get
			{
				var profiles = string.Empty;
				foreach (var p in Connectivity.ConnectionProfiles)
					profiles += "\n" + p.ToString();
				return profiles;
			}
		}

		public override void OnAppearing()
		{
			base.OnAppearing();

			Connectivity.ConnectivityChanged += OnConnectivityChanged;
		}

		public override void OnDisappearing()
		{
			Connectivity.ConnectivityChanged -= OnConnectivityChanged;

			base.OnDisappearing();
		}

		void OnConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
		{
			OnPropertyChanged(nameof(ConnectionProfiles));
			OnPropertyChanged(nameof(NetworkAccess));
		}
	}
}
