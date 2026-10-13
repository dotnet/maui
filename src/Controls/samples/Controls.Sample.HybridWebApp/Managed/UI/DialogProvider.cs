using System.Text.Json.Serialization;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.HybridApp.UI;

/// <summary>Marshals TypeScript popup options to actual dialogs on the calling page.</summary>
public sealed class DialogProvider(HybridViewContext context, NativeDispatcher dispatcher) : IHybridCommandProvider
{
	/// <inheritdoc />
	public void Register(CommandRegistry registry)
	{
		var json = DialogJsonContext.Default;
		registry.Register("dialogs.alert", json.AlertOptions, json.DialogAccepted, (args, op) =>
		{
			ValidateLabel(args.Cancel);
			var direction = Direction(args.FlowDirection);
			return Run(async () => { await context.Page.DisplayAlertAsync(args.Title, args.Message, args.Cancel, direction); return new DialogAccepted(true); }, op);
		}, interactive: true);
		registry.Register("dialogs.confirm", json.ConfirmOptions, json.DialogAccepted, (args, op) =>
		{
			ValidateLabel(args.Accept); ValidateLabel(args.Cancel);
			var direction = Direction(args.FlowDirection);
			return Run(async () => new DialogAccepted(await context.Page.DisplayAlertAsync(args.Title, args.Message, args.Accept, args.Cancel, direction)), op);
		}, interactive: true);
		registry.Register("dialogs.prompt", json.PromptOptions, json.DialogPromptResult, (args, op) =>
		{
			ValidateLabel(args.Accept); ValidateLabel(args.Cancel);
			if (args.MaxLength < -1 || (args.MaxLength >= 0 && args.InitialValue.Length > args.MaxLength))
				throw new CommandException("invalid_arguments", "Prompt maxLength must be -1 or nonnegative, and initial text must fit it.");
			var keyboard = KeyboardFor(args.Keyboard, args.KeyboardFlags);
			return Run(async () =>
			{
				var text = await context.Page.DisplayPromptAsync(args.Title, args.Message, args.Accept, args.Cancel,
					args.Placeholder, args.MaxLength, keyboard, args.InitialValue);
				return new DialogPromptResult(text is null, text);
			}, op);
		}, interactive: true);
		registry.Register("dialogs.actionSheet", json.ActionSheetOptions, json.ActionSheetResult, (args, op) =>
		{
			if (args.Cancel is not null) ValidateLabel(args.Cancel);
			if (args.Destruction is not null) ValidateLabel(args.Destruction);
			foreach (var button in args.Buttons ?? []) ValidateLabel(button);
			var direction = Direction(args.FlowDirection);
			return Run(async () => new ActionSheetResult(await context.Page.DisplayActionSheetAsync(
				args.Title, args.Cancel, args.Destruction, direction, args.Buttons ?? [])), op);
		}, interactive: true);
	}
	Task<T> Run<T>(Func<Task<T>> action, Operation op) =>
		dispatcher.RunAsync(() => context.InteractAsync(action, op.Cancellation), op.Cancellation);
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
sealed record AlertOptions(string Title, string Message, string Cancel = "OK", string FlowDirection = "matchParent");
sealed record ConfirmOptions(string Title, string Message, string Accept = "OK", string Cancel = "Cancel", string FlowDirection = "matchParent");
sealed record PromptOptions(string Title, string Message, string Accept = "OK", string Cancel = "Cancel",
	string? Placeholder = null, int MaxLength = -1, string Keyboard = "default", string[]? KeyboardFlags = null, string InitialValue = "");
sealed record DialogAccepted(bool Accepted);
sealed record DialogPromptResult(bool Cancelled, string? Text);
sealed record ActionSheetOptions(string Title, string? Cancel = null, string? Destruction = null, string[]? Buttons = null, string FlowDirection = "matchParent");
sealed record ActionSheetResult(string? Selection);
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(AlertOptions))]
[JsonSerializable(typeof(ConfirmOptions))]
[JsonSerializable(typeof(PromptOptions))]
[JsonSerializable(typeof(DialogAccepted))]
[JsonSerializable(typeof(DialogPromptResult))]
[JsonSerializable(typeof(ActionSheetOptions))]
[JsonSerializable(typeof(ActionSheetResult))]
partial class DialogJsonContext : JsonSerializerContext;
