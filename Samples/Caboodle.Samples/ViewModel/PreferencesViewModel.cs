using Microsoft.Caboodle;

namespace Caboodle.Samples.ViewModel
{
    public class PreferencesViewModel : BaseViewModel
    {
        const string preferenceKey = "PreferenceKey";

        readonly Preferences preferences;

        string preferenceValue;

        public PreferencesViewModel()
        {
            preferences = new Preferences();

            preferenceValue = preferences.Get(preferenceKey, string.Empty);
        }

        public string PreferenceValue
        {
            get => preferenceValue;
            set
            {
                preferenceValue = value;
                preferences.Set(preferenceKey, value);

                OnPropertyChanged();
            }
        }
    }
}
