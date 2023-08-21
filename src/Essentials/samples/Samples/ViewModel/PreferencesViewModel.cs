// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Storage;

namespace Samples.ViewModel
{
	public class PreferencesViewModel : BaseViewModel
	{
		const string preferenceKey = "PreferenceKey";

		string preferenceValue;

		public PreferencesViewModel()
		{
			preferenceValue = Preferences.Get(preferenceKey, string.Empty);
		}

		public string PreferenceValue
		{
			get => preferenceValue;
			set
			{
				preferenceValue = value;
				Preferences.Set(preferenceKey, value);

				OnPropertyChanged();
			}
		}
	}
}
