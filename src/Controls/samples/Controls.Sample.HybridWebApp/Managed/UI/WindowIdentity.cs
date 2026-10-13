namespace Maui.HybridApp.UI;

static class WindowIdentity
{
	internal static string Validate(string id)
	{
		if (id.Length is < 1 or > 64 || id.Any(character => !char.IsAsciiLetterOrDigit(character) && character is not '-' and not '_' and not '.'))
			throw new CommandException("invalid_window_id", "Window IDs use 1–64 ASCII letters, digits, dots, underscores or hyphens, not titles or URLs.");
		return id;
	}
}
