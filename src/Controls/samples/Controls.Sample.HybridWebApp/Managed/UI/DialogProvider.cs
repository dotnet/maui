using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.HybridApp.UI;

/// <summary>Marshals TypeScript popup options to actual dialogs on the calling page.</summary>
public sealed class DialogProvider(HybridViewContext context)
{
	[HybridAction("dialogs.prompt", MainThread = true, Interactive = true)]
	internal Task<DialogPromptResult> Prompt(string title, string message, CancellationToken cancellation,
		string accept = "OK", string cancel = "Cancel", string? placeholder = null, int maxLength = -1,
		KeyboardKind keyboard = KeyboardKind.Default, KeyboardFlags? keyboardFlags = null, string initialValue = "")
	{
		ValidateLabel(accept); ValidateLabel(cancel);
		if (maxLength < -1 || (maxLength >= 0 && initialValue.Length > maxLength))
			throw new CommandException("invalid_arguments", "Prompt maxLength must be -1 or nonnegative, and initial text must fit it.");
		var nativeKeyboard = KeyboardFor(keyboard, keyboardFlags);
		return context.InteractAsync(async () =>
		{
			var text = await context.Page.DisplayPromptAsync(title, message, accept, cancel, placeholder, maxLength, nativeKeyboard, initialValue);
			return new DialogPromptResult(text is null, text);
		}, cancellation);
	}

	static void ValidateLabel(string label)
	{
		if (string.IsNullOrWhiteSpace(label))
			throw new CommandException("invalid_arguments", "Native button labels must not be blank.");
	}
	static Keyboard KeyboardFor(KeyboardKind value, KeyboardFlags? flags)
	{
		if (flags is not null)
		{
			if (value != KeyboardKind.Custom)
				throw new CommandException("invalid_arguments", "Use custom keyboard with keyboard flags.");
			return Keyboard.Create(flags.Value);
		}
		return value switch
		{
			KeyboardKind.Default => Keyboard.Default, KeyboardKind.Plain => Keyboard.Plain, KeyboardKind.Chat => Keyboard.Chat, KeyboardKind.Text => Keyboard.Text,
			KeyboardKind.Email => Keyboard.Email, KeyboardKind.Numeric => Keyboard.Numeric, KeyboardKind.Telephone => Keyboard.Telephone, KeyboardKind.Url => Keyboard.Url,
			KeyboardKind.Date => Keyboard.Date, KeyboardKind.Time => Keyboard.Time, KeyboardKind.Password => Keyboard.Password,
			KeyboardKind.Custom => Keyboard.Create(KeyboardFlags.None),
			_ => throw new CommandException("invalid_arguments", "Unknown native keyboard type."),
		};
	}
}
enum KeyboardKind { Default, Plain, Chat, Text, Email, Numeric, Telephone, Url, Date, Time, Password, Custom }
sealed record DialogPromptResult(bool Cancelled, string? Text);
