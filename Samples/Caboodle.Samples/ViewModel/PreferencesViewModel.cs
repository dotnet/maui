using Microsoft.Caboodle;
using MvvmHelpers;

namespace Caboodle.Samples.ViewModel
{
    public class PreferencesViewModel : BaseViewModel
    {
        private const string preferenceKey = "PreferenceKey";

        private readonly Preferences preferences;

        private string preferenceValue;

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
