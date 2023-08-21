// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Controls;

namespace Samples.ViewModel
{
	public class PhoneDialerViewModel : BaseViewModel
	{
		string phoneNumber;

		public PhoneDialerViewModel()
		{
			OpenPhoneDialerCommand = new Command(OnOpenPhoneDialer);
		}

		public ICommand OpenPhoneDialerCommand { get; }

		public string PhoneNumber
		{
			get => phoneNumber;
			set => SetProperty(ref phoneNumber, value);
		}

		async void OnOpenPhoneDialer()
		{
			try
			{
				PhoneDialer.Open(PhoneNumber);
			}
			catch (Exception ex)
			{
				await DisplayAlertAsync($"Dialer is not supported: {ex.Message}");
			}
		}
	}
}
