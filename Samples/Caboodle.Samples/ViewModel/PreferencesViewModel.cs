using MvvmHelpers;
using Microsoft.Caboodle;

namespace Caboodle.Samples.ViewModel
{
    public class PreferencesViewModel : BaseViewModel
    {
        private const string PreferenceKey = nameof(PreferenceKey);

        private readonly Preferences preferences;

        private string preferenceValue;

        public PreferencesViewModel()
        {
            preferences = new Preferences();

            preferenceValue = preferences.Get(PreferenceKey, string.Empty);
        }

        public string PreferenceValue
        {
            get => preferenceValue;
            set
            {
                preferenceValue = value;
                preferences.Set(PreferenceKey, value);

                OnPropertyChanged();
            }
        }
    }
}
