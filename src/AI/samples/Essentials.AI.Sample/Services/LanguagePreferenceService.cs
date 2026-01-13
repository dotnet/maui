namespace Maui.Controls.Sample.Services;

/// <summary>
/// Simple service to track the user's preferred language for AI responses.
/// </summary>
public class LanguagePreferenceService
{
	public Dictionary<string, string> SupportedLanguages { get; } = new()
	{
		{ "Chinese", "zh" },
		{ "English", "en" },
		{ "French", "fr" },
		{ "German", "de" },
		{ "Indonesian", "id" },
		{ "Italian", "it" },
		{ "Japanese", "ja" },
		{ "Korean", "ko" },
		{ "Portuguese", "pt" },
		{ "Spanish", "es" },
	};

	public string SelectedLanguage { get; set; } = "English";
}
