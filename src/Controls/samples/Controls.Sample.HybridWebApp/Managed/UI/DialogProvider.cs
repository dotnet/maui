using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.HybridApp.UI;

/// <summary>Marshals TypeScript popup options to actual dialogs on the calling page.</summary>
public sealed class DialogProvider(HybridViewContext context)
{
	[HybridAction("dialogs.alert", MainThread = true, Interactive = true)]
	internal Task<DialogAccepted> Alert(string title, string message, CancellationToken cancellation,
		string cancel = "OK", string flowDirection = "matchParent")
	{
		ValidateLabel(cancel);
		var direction = Direction(flowDirection);
		return context.InteractAsync(async () =>
		{
			await context.Page.DisplayAlertAsync(title, message, cancel, direction);
			return new DialogAccepted(true);
		}, cancellation);
	}

	[HybridAction("dialogs.confirm", MainThread = true, Interactive = true)]
	internal Task<DialogAccepted> Confirm(string title, string message, CancellationToken cancellation,
		string accept = "OK", string cancel = "Cancel", string flowDirection = "matchParent")
	{
		ValidateLabel(accept); ValidateLabel(cancel);
		var direction = Direction(flowDirection);
		return context.InteractAsync(async () =>
			new DialogAccepted(await context.Page.DisplayAlertAsync(title, message, accept, cancel, direction)), cancellation);
	}

	[HybridAction("dialogs.prompt", MainThread = true, Interactive = true)]
	internal Task<DialogPromptResult> Prompt(string title, string message, CancellationToken cancellation,
		string accept = "OK", string cancel = "Cancel", string? placeholder = null, int maxLength = -1,
		string keyboard = "default", string[]? keyboardFlags = null, string initialValue = "")
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

	[HybridAction("dialogs.actionSheet", MainThread = true, Interactive = true)]
	internal Task<ActionSheetResult> ActionSheet(string title, CancellationToken cancellation,
		string? cancel = null, string? destruction = null, string[]? buttons = null, string flowDirection = "matchParent")
	{
		if (cancel is not null) ValidateLabel(cancel);
		if (destruction is not null) ValidateLabel(destruction);
		foreach (var button in buttons ?? []) ValidateLabel(button);
		var direction = Direction(flowDirection);
		return context.InteractAsync(async () => new ActionSheetResult(await context.Page.DisplayActionSheetAsync(
			title, cancel, destruction, direction, buttons ?? [])), cancellation);
	}
	static void ValidateLabel(string label)
	{
		if (string.IsNullOrWhiteSpace(label))
			throw new CommandException("invalid_arguments", "Native button labels must not be blank.");
	}
	static FlowDirection Direction(string value) => value switch
	{
		"matchParent" => FlowDirection.MatchParent, "leftToRight" => FlowDirection.LeftToRight,
		"rightToLeft" => FlowDirection.RightToLeft,
		_ => throw new CommandException("invalid_arguments", "Unknown flow direction."),
	};
	static Keyboard KeyboardFor(string value, string[]? flags)
	{
		if (flags is not null)
		{
			if (value != "custom" || flags.Distinct(StringComparer.Ordinal).Count() != flags.Length)
				throw new CommandException("invalid_arguments", "Use custom keyboard with unique keyboard flags.");
			var combined = KeyboardFlags.None;
			foreach (var flag in flags)
				combined |= flag switch
				{
					"capitalizeSentence" => KeyboardFlags.CapitalizeSentence, "spellcheck" => KeyboardFlags.Spellcheck,
					"suggestions" => KeyboardFlags.Suggestions, "capitalizeWord" => KeyboardFlags.CapitalizeWord,
					"capitalizeCharacter" => KeyboardFlags.CapitalizeCharacter, "capitalizeNone" => KeyboardFlags.CapitalizeNone,
					_ => throw new CommandException("invalid_arguments", "Unknown custom keyboard flag."),
				};
			return Keyboard.Create(combined);
		}
		return value switch
		{
			"default" => Keyboard.Default, "plain" => Keyboard.Plain, "chat" => Keyboard.Chat, "text" => Keyboard.Text,
			"email" => Keyboard.Email, "numeric" => Keyboard.Numeric, "telephone" => Keyboard.Telephone, "url" => Keyboard.Url,
			"date" => Keyboard.Date, "time" => Keyboard.Time, "password" => Keyboard.Password,
			_ => throw new CommandException("invalid_arguments", "Unknown native keyboard type."),
		};
	}
}
sealed record DialogAccepted(bool Accepted);
sealed record DialogPromptResult(bool Cancelled, string? Text);
sealed record ActionSheetResult(string? Selection);
