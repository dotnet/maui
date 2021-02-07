using Xamarin.Essentials;

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
