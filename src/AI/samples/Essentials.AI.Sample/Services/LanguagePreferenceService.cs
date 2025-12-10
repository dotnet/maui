namespace Maui.Controls.Sample.Services;

/// <summary>
/// Simple service to track the user's preferred language for AI responses.
/// </summary>
public class LanguagePreferenceService
{
	public static readonly Dictionary<string, string> SupportedLanguages = new()
	{
		{ "English", "en" },
		{ "Spanish", "es" },
		{ "French", "fr" },
		{ "German", "de" },
		{ "Indonesian", "id" },
		{ "Italian", "it" },
		{ "Portuguese", "pt" },
		{ "Japanese", "ja" },
		{ "Chinese", "zh" }
	};

	public string SelectedLanguage { get; set; } = "English";
}
