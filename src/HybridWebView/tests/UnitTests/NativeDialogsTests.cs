using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.HybridWebApps;
using Microsoft.Maui.Controls.HybridWebApps.UI;
using Microsoft.Maui.Controls.Internals;
using Xunit;

namespace Maui.Controls.Sample.HybridWebApp.Tests;

public sealed class NativeDialogsTests
{
	[Theory]
	[InlineData(null)]
	[InlineData("")]
	[InlineData("你好 👋 — longer than maxLength")]
	public async Task PromptCallsOwningPageWithUnmodifiedOptionsAndReturnsNativeScalar(string? result)
	{
		using var host = new DialogHost { Result = result };
		var response = await host.Call("dialogs.prompt",
			"""{"title":"t","message":"m","accept":"","cancel":" ","maxLength":2,"initialValue":"longer than maxLength","keyboard":{"name":"email"}}""");
		var (page, args) = Assert.Single(host.Prompts);
		Assert.Same(host.Context.Page, page);
		Assert.Equal("", args.Accept);
		Assert.Equal(" ", args.Cancel);
		Assert.Equal(2, args.MaxLength);
		Assert.Equal("longer than maxLength", args.InitialValue);
		Assert.Same(Keyboard.Email, args.Keyboard);
		Assert.Equal(result is null ? JsonValueKind.Null : JsonValueKind.String, response.ValueKind);
		Assert.Equal(result, response.GetString());
		Assert.True(host.Registry.IsInteractive("dialogs.prompt"));
	}

	[Theory]
	[InlineData("")]
	[InlineData(""","keyboard":null""")]
	[InlineData(""","keyboard":{"name":"default"}""")]
	public async Task PromptRetainsNativeDefaultsAndAcceptsNegativeNativeIntegers(string keyboard)
	{
		using var host = new DialogHost();
		await host.Call("dialogs.prompt", $$"""{"title":null,"message":null,"maxLength":-2{{keyboard}}}""");
		var args = Assert.Single(host.Prompts).Args;
		Assert.Null(args.Title);
		Assert.Null(args.Message);
		Assert.Equal("OK", args.Accept);
		Assert.Equal("Cancel", args.Cancel);
		Assert.Null(args.Placeholder);
		Assert.Equal("", args.InitialValue);
		Assert.Equal(-2, args.MaxLength);
		Assert.Same(Keyboard.Default, args.Keyboard);
	}

	[Fact]
	public async Task AllDialogsLeaveLabelValidationAndButtonCountsToPage()
	{
		using var host = new DialogHost();
		await host.Call("dialogs.alert", """{"title":"t","message":"m","cancel":" ","flowDirection":"matchParent"}""");
		await host.Call("dialogs.confirm", """{"title":"t","message":"m","accept":"","cancel":" ","flowDirection":"leftToRight"}""");
		Assert.Equal(" ", host.Alerts[0].Args.Cancel);
		Assert.Equal("", host.Alerts[1].Args.Accept);
		Assert.Equal(FlowDirection.LeftToRight, host.Alerts[1].Args.FlowDirection);
		Assert.All(host.Alerts, item => Assert.Same(host.Context.Page, item.Page));
		var buttons = Enumerable.Repeat("", 65).ToArray();
		await host.Call("dialogs.actionSheet", JsonSerializer.Serialize(new
		{
			title = "t",
			cancel = "",
			destruction = " ",
			flowDirection = "rightToLeft",
			buttons
		}));
		var (page, sheet) = Assert.Single(host.Sheets);
		Assert.Same(host.Context.Page, page);
		Assert.Equal(buttons, sheet.Buttons);
		Assert.Equal("", sheet.Cancel);
		Assert.Equal(" ", sheet.Destruction);
		Assert.Equal(FlowDirection.RightToLeft, sheet.FlowDirection);
		await Assert.ThrowsAsync<ArgumentNullException>(() => host.Call("dialogs.alert",
			"""{"title":"t","message":"m","cancel":"","flowDirection":"matchParent"}"""));
		Assert.Equal(2, host.Alerts.Count);
	}

	[Theory]
	[InlineData("""[]""", KeyboardFlags.None)]
	[InlineData("""["none"]""", KeyboardFlags.None)]
	[InlineData("""["all"]""", KeyboardFlags.All)]
	[InlineData("""["spellcheck","suggestions","capitalizeNone"]""", KeyboardFlags.Spellcheck | KeyboardFlags.Suggestions | KeyboardFlags.CapitalizeNone)]
	public async Task PromptUsesActualKeyboardCreateFlags(string flags, KeyboardFlags expected)
	{
		using var host = new DialogHost();
		await host.Call("dialogs.prompt", $$$"""{"title":"t","message":"m","keyboard":{"flags":{{{flags}}}}}""");
		var keyboard = Assert.IsType<CustomKeyboard>(Assert.Single(host.Prompts).Args.Keyboard);
		Assert.Equal(expected, keyboard.Flags);
		Assert.NotSame(Keyboard.Plain, keyboard);
	}

	[Theory]
	[InlineData("default")]
	[InlineData("plain")]
	[InlineData("chat")]
	[InlineData("text")]
	[InlineData("email")]
	[InlineData("numeric")]
	[InlineData("telephone")]
	[InlineData("url")]
	[InlineData("date")]
	[InlineData("time")]
	[InlineData("password")]
	public async Task KeyboardSingletonsRoundTripThroughDeclaredStrictContextAndNativePage(string name)
	{
		var metadata = HybridJsonContracts.Get<Keyboard>(NativeKeyboardJsonContext.Default);
		var json = $$"""{"name":"{{name}}"}""";
		var keyboard = JsonSerializer.Deserialize(json, metadata)!;
		Assert.Equal(json, JsonSerializer.Serialize(keyboard, metadata));
		Assert.Same(keyboard, JsonSerializer.Deserialize(json, metadata));
		using var host = new DialogHost();
		await host.Call("dialogs.prompt", $$"""{"title":"t","message":"m","keyboard":{{json}}}""");
		Assert.Same(keyboard, Assert.Single(host.Prompts).Args.Keyboard);
	}

	[Fact]
	public void KeyboardConverterIsReusableAndRejectsUndefinedOutputFlags()
	{
		var metadata = HybridJsonContracts.Get<Keyboard>(NativeKeyboardJsonContext.Default);
		foreach (var flags in new[] { KeyboardFlags.None, KeyboardFlags.All, KeyboardFlags.Spellcheck | KeyboardFlags.Suggestions })
		{
			var keyboard = Keyboard.Create(flags);
			var json = JsonSerializer.Serialize(keyboard, metadata);
			Assert.Equal(flags, Assert.IsType<CustomKeyboard>(JsonSerializer.Deserialize(json, metadata)).Flags);
			Assert.Contains("\"flags\":", json, StringComparison.Ordinal);
		}
		Assert.Equal("""{"name":"plain"}""", JsonSerializer.Serialize(Keyboard.Plain, metadata));
		Assert.Throws<JsonException>(() => JsonSerializer.Serialize(Keyboard.Create((KeyboardFlags)64), metadata));
		Assert.Null(JsonSerializer.Deserialize("null", metadata));
	}

	[Fact]
	public async Task CancellationKeepsTheNativeInteractionSlotUntilDismissalAndContextRemainsRequired()
	{
		using var host = new DialogHost();
		using var cancellation = new CancellationTokenSource();
		host.Pending = new(TaskCreationOptions.RunContinuationsAsynchronously);
		var first = host.Call("dialogs.prompt", """{"title":"first","message":""}""", cancellation.Token);
		Assert.Single(host.Prompts);
		cancellation.Cancel();
		var second = host.Call("dialogs.prompt", """{"title":"second","message":""}""");
		Assert.Single(host.Prompts);
		host.Pending.SetResult("late");
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() => first);
		Assert.Equal("late", (await second).GetString());
		Assert.Equal(2, host.Prompts.Count);
		await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
			host.Call("dialogs.prompt", """{"title":"cancelled","message":""}""", cancellation.Token));
		Assert.Equal(2, host.Prompts.Count);
		host.Context.Dispose();
		Assert.Equal("context_unavailable", (await Assert.ThrowsAsync<CommandException>(() =>
			host.Call("dialogs.prompt", """{"title":"detached","message":""}"""))).Code);
	}

	sealed class DialogHost : IDisposable
	{
		readonly ServiceProvider _native;
		internal HybridViewContext Context { get; } = new();
		internal CommandRegistry Registry { get; }
		internal List<(Page Page, PromptArguments Args)> Prompts { get; } = [];
		internal List<(Page Page, AlertArguments Args)> Alerts { get; } = [];
		internal List<(Page Page, ActionSheetArguments Args)> Sheets { get; } = [];
		internal string? Result { get; init; }
		internal TaskCompletionSource<string>? Pending { get; set; }

		internal DialogHost()
		{
			var services = new ServiceCollection();
			services.AddKeyedSingleton<Func<Page, PromptArguments, Task<string>>>("Microsoft.Maui.Controls.DisplayPrompt",
				(_, _) => (page, args) => { Prompts.Add((page, args)); return Pending?.Task ?? Task.FromResult(Result!); });
			services.AddKeyedSingleton<Func<Page, AlertArguments, Task<bool>>>("Microsoft.Maui.Controls.DisplayAlert",
				(_, _) => (page, args) => { Alerts.Add((page, args)); return Task.FromResult(true); });
			services.AddKeyedSingleton<Func<Page, ActionSheetArguments, Task<string>>>("Microsoft.Maui.Controls.DisplayActionSheet",
				(_, _) => (page, args) => { Sheets.Add((page, args)); return Task.FromResult(args.Cancel); });
			_native = services.BuildServiceProvider();
			var maui = new MauiContext(_native);
			var handler = new DialogWindowHandler();
			handler.SetMauiContext(maui);
			var window = new Window { Handler = handler };
			var pageHandler = new DialogPageHandler();
			pageHandler.SetMauiContext(maui);
			var page = new ContentPage { Handler = pageHandler };
			window.Page = page;
			Context.Bind(page, window, "index.html");
			Registry = TestComposition.CreateRegistry(Context);
		}

		internal Task<JsonElement> Call(string command, string arguments, CancellationToken cancellation = default)
		{
			using var json = JsonDocument.Parse(arguments);
			return Registry.DispatchAsync(command, json.RootElement.Clone(),
				new Operation("session", Guid.NewGuid().ToString("D"), cancellation, (_, _) => { }));
		}
		public void Dispose() { Context.Dispose(); _native.Dispose(); }
	}

	sealed class DialogWindowHandler() : Microsoft.Maui.Handlers.ElementHandler<IWindow, object>(new PropertyMapper<IWindow>())
	{
		protected override object CreatePlatformElement() => new();
	}

	sealed class DialogPageHandler() : Microsoft.Maui.Handlers.ViewHandler<IView, object>(new PropertyMapper<IView>())
	{
		protected override object CreatePlatformView() => new();
	}
}
