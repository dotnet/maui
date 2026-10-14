using System.Text.Json.Serialization;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;

namespace Maui.HybridApp.UI;

/// <summary>Renders JS-authored owning-window menus; native click callbacks emit IDs, never execute C# feature recipes.</summary>
[HybridTopic("menus.clicked")]
public sealed class MenuProvider : IDisposable
{
	readonly HybridViewContext _context;
	readonly IDeviceInfo _device;
	readonly NativeNotifications _notifications;
	Installation? _current;
	bool _disposed;
	sealed class Installation(Page page) : IDisposable
	{
		internal Page Page { get; } = page;
		internal string Source { get; } = Guid.NewGuid().ToString("D");
		internal List<MenuBarItem> Bars { get; } = [];
		internal Dictionary<string, (MenuNode Node, MenuFlyoutItem? Item, MenuBarItem? Bar)> Items { get; } = new(StringComparer.Ordinal);
		internal List<(MenuFlyoutItem Item, EventHandler Click, EventHandler Changed)> Handlers { get; } = [];
#if MACCATALYST
		internal Dictionary<string, global::UIKit.UIMenuElement> CheckSnapshots { get; } = new(StringComparer.Ordinal);
#endif
		public void Dispose()
		{
			foreach (var (item, click, changed) in Handlers) { item.Clicked -= click; item.HandlerChanged -= changed; }
			Handlers.Clear();
#if MACCATALYST
			CheckSnapshots.Clear();
#endif
		}
	}
	/// <summary>Creates a scoped renderer with no app menu definitions or global page lookup.</summary>
	/// <param name="context">Owning page/window binding.</param>
	/// <param name="device">Native platform metadata.</param>
	/// <param name="notifications">Current-document native item-ID event source.</param>
	public MenuProvider(HybridViewContext context, IDeviceInfo device, NativeNotifications notifications)
	{
		_context = context; _device = device; _notifications = notifications;
		context.DocumentEnded += DocumentEnded;
	}
	bool Supported => _device.Platform == DevicePlatform.MacCatalyst || _device.Platform == DevicePlatform.WinUI;
	bool Checks => _device.Platform == DevicePlatform.MacCatalyst;
	internal MenuInfo Read() => new(Supported, Checks, _current?.Source);
	internal void ValidateOwner(string id)
	{
		if (id != _context.Id) throw new CommandException("invalid_window", "This menu does not belong to the calling window.");
	}
	KeyboardAcceleratorModifiers Modifiers(MenuShortcut shortcut)
	{
		if (shortcut.Key.Length is < 1 or > 64 || shortcut.Key.Any(char.IsControl) ||
			shortcut.Modifiers.Length > 5 || shortcut.Modifiers.Distinct().Count() != shortcut.Modifiers.Length)
			throw new CommandException("invalid_arguments", "Use a native key string and at most five unique accelerator modifiers.");
		var result = KeyboardAcceleratorModifiers.None;
		foreach (var modifier in shortcut.Modifiers)
		{
			var flag = modifier switch
			{
				MenuModifier.Primary => Checks ? KeyboardAcceleratorModifiers.Cmd : KeyboardAcceleratorModifiers.Ctrl,
				MenuModifier.Shift => KeyboardAcceleratorModifiers.Shift, MenuModifier.Ctrl => KeyboardAcceleratorModifiers.Ctrl,
				MenuModifier.Alt => KeyboardAcceleratorModifiers.Alt,
				MenuModifier.Cmd when Checks => KeyboardAcceleratorModifiers.Cmd,
				MenuModifier.Windows when _device.Platform == DevicePlatform.WinUI => KeyboardAcceleratorModifiers.Windows,
				MenuModifier.Cmd or MenuModifier.Windows => throw new NotSupportedException("This accelerator modifier is not supported by the native platform."),
				_ => throw new CommandException("invalid_arguments", "Unknown accelerator modifier."),
			};
			if ((result & flag) != 0) throw new CommandException("invalid_arguments", "Accelerator modifiers resolve to duplicate native flags.");
			result |= flag;
		}
		return result;
	}
	void Validate(MenuNode[] nodes)
	{
		if (nodes.Length is < 1 or > 8 || nodes.Any(node => node.Kind != MenuKind.Submenu))
			throw new CommandException("invalid_arguments", "Install one to eight top-level submenus.");
		var ids = new HashSet<string>(StringComparer.Ordinal);
		void Node(MenuNode node, int depth)
		{
			if (depth > 4 || ids.Count >= 128 || node.Id.Length is < 1 or > 64 ||
				node.Id.Any(c => !char.IsAsciiLetterOrDigit(c) && c != '-') || !ids.Add(node.Id) ||
				node.Text.Length > 160 ||
				(node.Kind == MenuKind.Submenu ? node.Items is null : node.Items is not null))
				throw new CommandException("invalid_arguments", "Use unique bounded IDs/labels, valid kinds and at most four nesting levels/128 nodes.");
			if ((node.Checked || node.Kind == MenuKind.Check) && !Checks)
				throw new NotSupportedException("Native checked menu items currently require Mac Catalyst.");
			if (node.Shortcut is not null)
			{
				if (node.Kind is not (MenuKind.Item or MenuKind.Check))
					throw new CommandException("unsupported", "Native accelerators require an actionable menu item.");
				Modifiers(node.Shortcut);
			}
			if (node.Items is not null) foreach (var child in node.Items) Node(child, depth + 1);
		}
		foreach (var node in nodes) Node(node, 0);
	}
	internal MenuInfo Install(MenuNode[] nodes)
	{
		if (!Supported) throw new NotSupportedException("Native menu bars require Mac Catalyst or Windows.");
		ObjectDisposedException.ThrowIf(_disposed, this);
		Validate(nodes);
		var page = _context.Page;
		var prepared = new Installation(page);
		try
		{
			foreach (var node in nodes)
			{
				var bar = new MenuBarItem { Text = node.Text, IsEnabled = node.Enabled };
				prepared.Items.Add(node.Id, (node, null, bar));
				foreach (var child in node.Items!) bar.Add(Create(child, prepared));
				prepared.Bars.Add(bar);
			}
		}
		catch { prepared.Dispose(); throw; }
		var previous = _current;
		try
		{
			foreach (var bar in prepared.Bars) page.MenuBarItems.Add(bar);
			foreach (var id in prepared.Items.Keys) SyncCheck(prepared, id);
			if (previous is not null) foreach (var bar in previous.Bars) previous.Page.MenuBarItems.Remove(bar);
			_current = prepared;
			previous?.Dispose();
			return Read();
		}
		catch
		{
			try
			{
				foreach (var bar in prepared.Bars) if (page.MenuBarItems.Contains(bar)) page.MenuBarItems.Remove(bar);
				if (previous is not null) foreach (var bar in previous.Bars) if (!previous.Page.MenuBarItems.Contains(bar)) previous.Page.MenuBarItems.Add(bar);
				_current = previous;
			}
			catch
			{
				_current = null;
				previous?.Dispose();
				throw new CommandException("menu_restore_failed", "Native menu replacement and restoration failed. Reinstall the owning window menu.");
			}
			finally { prepared.Dispose(); }
			throw;
		}
	}
	IMenuElement Create(MenuNode node, Installation installation)
	{
		if (node.Kind == MenuKind.Separator)
		{
			installation.Items.Add(node.Id, (node, null, null));
			return new MenuFlyoutSeparator();
		}
		if (node.Kind == MenuKind.Submenu)
		{
			var sub = new MenuFlyoutSubItem { Text = node.Text, IsEnabled = node.Enabled };
			installation.Items.Add(node.Id, (node, sub, null));
			foreach (var child in node.Items!) sub.Add(Create(child, installation));
			return sub;
		}
		var item = new MenuFlyoutItem { Text = node.Text, IsEnabled = node.Enabled };
		installation.Items.Add(node.Id, (node, item, null));
		if (node.Shortcut is not null)
			item.KeyboardAccelerators.Add(new KeyboardAccelerator { Key = node.Shortcut.Key, Modifiers = Modifiers(node.Shortcut) });
		EventHandler click = (_, _) =>
		{
			if (_disposed || !ReferenceEquals(_current, installation) ||
				!installation.Items.TryGetValue(node.Id, out var current) || !current.Node.Enabled) return;
			if (current.Node.Kind == MenuKind.Check)
			{
				installation.Items[node.Id] = (current.Node with { Checked = !current.Node.Checked }, current.Item, current.Bar);
				Refresh(installation, node.Id);
			}
			_notifications.Publish("menus.clicked", new MenuClick(installation.Source, node.Id, installation.Items[node.Id].Node.Checked), MenuJsonContext.Default.MenuClick);
		};
		EventHandler changed = (_, _) => SyncCheck(installation, node.Id);
		item.Clicked += click; item.HandlerChanged += changed;
		installation.Handlers.Add((item, click, changed));
		return item;
	}
	static void SyncCheck(Installation installation, string id)
	{
#if MACCATALYST
		if (!installation.Items.TryGetValue(id, out var current) || current.Node.Kind != MenuKind.Check) return;
		// Apple handlers are disconnected after BuildMenu; retain the native snapshot, not the handler.
		if (current.Item?.Handler?.PlatformView is global::UIKit.UIMenuElement snapshot &&
			snapshot is global::UIKit.UICommand or global::UIKit.UIAction)
			installation.CheckSnapshots[id] = snapshot;
		if (!installation.CheckSnapshots.TryGetValue(id, out var native)) return;
		var state = current.Node.Checked ? global::UIKit.UIMenuElementState.On : global::UIKit.UIMenuElementState.Off;
		if (native is global::UIKit.UICommand command) command.State = state;
		else if (native is global::UIKit.UIAction action) action.State = state;
#endif
	}
	void Refresh(Installation installation, string id)
	{
		SyncCheck(installation, id);
		// Apple menu labels/checks are build-time snapshots, with no live Text mapper.
		// The owning window mapper requests a rebuild and HandlerChanged reapplies check state.
		if (Checks) _context.Window.Handler?.UpdateValue(nameof(IMenuBarElement.MenuBar));
	}
	[HybridAction("menus.update", MainThread = true)]
	internal MenuInfo Update(string windowId, string sourceId, string id, string? text = null, bool? enabled = null, bool? @checked = null)
	{
		ValidateOwner(windowId);
		var installation = _current;
		if (installation?.Source != sourceId || !installation.Items.TryGetValue(id, out var current) || current.Node.Kind == MenuKind.Separator)
			throw new CommandException("invalid_menu", "This menu item does not belong to the calling live installation.");
		if (text?.Length > 160) throw new CommandException("invalid_arguments", "Native labels are limited to 160 characters.");
		if (@checked is not null && (!Checks || current.Node.Kind != MenuKind.Check))
			throw new NotSupportedException("Native checked updates require a Mac Catalyst check item.");
		var node = current.Node with { Text = text ?? current.Node.Text, Enabled = enabled ?? current.Node.Enabled, Checked = @checked ?? current.Node.Checked };
		installation.Items[id] = (node, current.Item, current.Bar);
		if (current.Item is not null) { current.Item.Text = node.Text; current.Item.IsEnabled = node.Enabled; }
		if (current.Bar is not null) { current.Bar.Text = node.Text; current.Bar.IsEnabled = node.Enabled; }
		Refresh(installation, id);
		return Read();
	}
	[HybridAction("menus.item", MainThread = true)]
	internal MenuItemState Item(string windowId, string sourceId, string id)
	{
		ValidateOwner(windowId);
		var installation = _current;
		if (installation?.Source != sourceId || !installation.Items.TryGetValue(id, out var current))
			throw new CommandException("invalid_menu", "This item does not belong to the calling live menu installation.");
		bool? check = null;
		if (current.Node.Kind == MenuKind.Check)
		{
#if MACCATALYST
			if (!installation.CheckSnapshots.TryGetValue(id, out var native))
				throw new CommandException("context_unavailable", "The native check item has not been built.");
			if (native is global::UIKit.UICommand command) check = command.State == global::UIKit.UIMenuElementState.On;
			else if (native is global::UIKit.UIAction action) check = action.State == global::UIKit.UIMenuElementState.On;
#else
			throw new NotSupportedException("Native check item state requires Mac Catalyst.");
#endif
		}
		return new(current.Item?.Text ?? current.Bar?.Text ?? current.Node.Text,
			current.Item?.IsEnabled ?? current.Bar?.IsEnabled ?? current.Node.Enabled, check, current.Node.Kind);
	}
	void DocumentEnded()
	{
		var installation = _current;
		if (installation is null) return;
		Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() => { if (ReferenceEquals(_current, installation)) Clear(); });
	}
	internal void Clear()
	{
		var installation = _current;
		if (installation is null) return;
		foreach (var bar in installation.Bars) installation.Page.MenuBarItems.Remove(bar);
		installation.Dispose(); _current = null;
	}
	/// <summary>Detaches this installation's native controls/listeners.</summary>
	public void Dispose()
	{
		_context.DocumentEnded -= DocumentEnded;
		Clear(); _disposed = true;
	}
	[HybridAction("menus.info", MainThread = true)]
	internal MenuInfo Info(string windowId)
	{
		ValidateOwner(windowId);
		return Read();
	}
	[HybridAction("menus.install", MainThread = true)]
	internal MenuInfo Install(string windowId, MenuNode[] items)
	{
		ValidateOwner(windowId);
		return Install(items);
	}
	[HybridAction("menus.clear", MainThread = true)]
	internal MenuInfo Clear(string windowId, string sourceId)
	{
		ValidateOwner(windowId);
		if (Read().SourceId != sourceId) throw new CommandException("invalid_menu", "This menu installation has been replaced.");
		Clear();
		return Read();
	}
}
enum MenuKind { Submenu, Item, Check, Separator }
enum MenuModifier { Primary, Shift, Ctrl, Alt, Cmd, Windows }
sealed record MenuShortcut(string Key, MenuModifier[] Modifiers);
sealed record MenuNode(string Id, string Text, MenuKind Kind, bool Enabled, bool Checked, MenuNode[]? Items, MenuShortcut? Shortcut = null);
sealed record MenuItemState(string Text, bool Enabled, bool? Checked, MenuKind Kind);
sealed record MenuInfo(bool Supported, bool SupportsChecked, string? SourceId);
sealed record MenuClick(string SourceId, string Id, bool Checked);
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
	UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
	RespectNullableAnnotations = true, RespectRequiredConstructorParameters = true)]
[JsonSerializable(typeof(MenuClick))]
partial class MenuJsonContext : JsonSerializerContext;
