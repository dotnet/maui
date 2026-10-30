using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Gtk.Platform;
using GtkBox = Gtk.Box;
using GtkButton = Gtk.Button;
using GtkLabel = Gtk.Label;
using GtkWindow = Gtk.Window;

namespace Microsoft.Maui.Controls.Gtk.Platform
{
	/// <summary>
	/// The alert, action sheet and prompt dialogs of <c>DisplayAlertAsync</c>, <c>DisplayActionSheetAsync</c> and <c>DisplayPromptAsync</c>:
	/// small modal <c>Gtk.Window</c>s transient for the page's window. They are the delegates of MAUI's alert "delegate convention"
	/// (registered by <c>GtkControlsAlerts</c>).
	/// </summary>
	/// <remarks>
	/// <c>Gtk.AlertDialog</c> only offers an asynchronous GIO call with no way to inspect or drive the buttons; plain windows work on
	/// every GTK 4 version, honour the flow direction and can be tested on real widgets. Every button has its text as widget name.
	/// </remarks>
	static class GtkDialogs
	{
		/// <summary>The dialog opened last that is still open; for tests.</summary>
		internal static GtkWindow? Active { get; private set; }

		public static Task<bool> ShowAlertAsync(Page page, AlertArguments arguments)
		{
			var result = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
			GtkRuntime.Post(() =>
			{
				var content = NewContent(arguments.Title, arguments.Message);
				var window = NewWindow(page, arguments.Title, arguments.FlowDirection, content);
				var buttons = NewButtonRow();

				void Finish(bool value)
				{
					result.TrySetResult(value);
					Close(window);
				}

				if (!string.IsNullOrEmpty(arguments.Cancel))
					buttons.Append(NewButton(arguments.Cancel, () => Finish(false)));

				GtkButton? accept = null;
				if (!string.IsNullOrEmpty(arguments.Accept))
				{
					accept = NewButton(arguments.Accept, () => Finish(true));
					accept.AddCssClass("suggested-action");
					buttons.Append(accept);
				}

				content.Append(buttons);
				window.CloseRequest += (_, args) =>
				{
					result.TrySetResult(false);
					Forget(window);
					args.RetVal = false;
				};

				Open(window, accept);
			});

			return result.Task;
		}

		public static Task<string> ShowActionSheetAsync(Page page, ActionSheetArguments arguments)
		{
			var result = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
			GtkRuntime.Post(() =>
			{
				var content = NewContent(arguments.Title, null);
				var window = NewWindow(page, arguments.Title, arguments.FlowDirection, content);
				var list = new GtkBox(global::Gtk.Orientation.Vertical, 6);

				void Finish(string? value)
				{
					result.TrySetResult(value!);
					Close(window);
				}

				if (!string.IsNullOrEmpty(arguments.Destruction))
				{
					var destruction = NewButton(arguments.Destruction, () => Finish(arguments.Destruction));
					destruction.AddCssClass("destructive-action");
					list.Append(destruction);
				}

				if (arguments.Buttons is not null)
				{
					foreach (var text in arguments.Buttons)
					{
						var captured = text;
						list.Append(NewButton(text, () => Finish(captured)));
					}
				}

				content.Append(list);

				if (!string.IsNullOrEmpty(arguments.Cancel))
				{
					var row = NewButtonRow();
					row.Append(NewButton(arguments.Cancel, () => Finish(arguments.Cancel)));
					content.Append(row);
				}

				window.CloseRequest += (_, args) =>
				{
					result.TrySetResult(arguments.Cancel!);
					Forget(window);
					args.RetVal = false;
				};

				Open(window, null);
			});

			return result.Task;
		}

		public static Task<string> ShowPromptAsync(Page page, PromptArguments arguments)
		{
			var result = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
			GtkRuntime.Post(() =>
			{
				var content = NewContent(arguments.Title, arguments.Message);
				var window = NewWindow(page, arguments.Title, FlowDirection.MatchParent, content);

				var entry = new global::Gtk.Entry
				{
					Text = arguments.InitialValue ?? string.Empty,
					PlaceholderText = arguments.Placeholder ?? string.Empty,
					InputPurpose = GtkInputExtensions.ToInputPurpose(arguments.Keyboard),
					Hexpand = true,
					Name = "prompt-entry",
				};
				if (arguments.MaxLength > 0)
					entry.MaxLength = arguments.MaxLength;
				content.Append(entry);

				void Finish(string? value)
				{
					result.TrySetResult(value!);
					Close(window);
				}

				var buttons = NewButtonRow();
				if (!string.IsNullOrEmpty(arguments.Cancel))
					buttons.Append(NewButton(arguments.Cancel, () => Finish(null)));

				var accept = NewButton(string.IsNullOrEmpty(arguments.Accept) ? "OK" : arguments.Accept, () => Finish(entry.Text ?? string.Empty));
				accept.AddCssClass("suggested-action");
				buttons.Append(accept);
				content.Append(buttons);

				entry.Activated += (_, _) => Finish(entry.Text ?? string.Empty);
				window.CloseRequest += (_, args) =>
				{
					result.TrySetResult(null!);
					Forget(window);
					args.RetVal = false;
				};

				Open(window, accept);
				entry.GrabFocus();
			});

			return result.Task;
		}

		static GtkBox NewContent(string? title, string? message)
		{
			var content = new GtkBox(global::Gtk.Orientation.Vertical, 12)
			{
				MarginTop = 18,
				MarginBottom = 18,
				MarginStart = 18,
				MarginEnd = 18,
			};

			if (!string.IsNullOrEmpty(title))
			{
				var heading = new GtkLabel { Text = title, Wrap = true, Xalign = 0, Halign = global::Gtk.Align.Start };
				heading.AddCssClass("title-4");
				content.Append(heading);
			}

			if (!string.IsNullOrEmpty(message))
				content.Append(new GtkLabel { Text = message, Wrap = true, Xalign = 0, Halign = global::Gtk.Align.Start });

			return content;
		}

		static GtkWindow NewWindow(Page page, string? title, FlowDirection flowDirection, global::Gtk.Widget content)
		{
			var window = new GtkWindow
			{
				Modal = true,
				Resizable = false,
				Title = title ?? string.Empty,
				DefaultWidth = 360,
				Child = content,
			};

			if (page.Window?.Handler?.PlatformView is GtkWindow parent)
				window.TransientFor = parent;

			if (flowDirection == FlowDirection.RightToLeft)
				window.Direction = global::Gtk.TextDirection.Rtl;
			else if (flowDirection == FlowDirection.LeftToRight)
				window.Direction = global::Gtk.TextDirection.Ltr;

			return window;
		}

		static GtkBox NewButtonRow() => new(global::Gtk.Orientation.Horizontal, 6) { Halign = global::Gtk.Align.End };

		static GtkButton NewButton(string text, Action clicked)
		{
			var button = new GtkButton { Label = text, Name = text };
			button.Clicked += (_, _) => clicked();
			return button;
		}

		static void Open(GtkWindow window, GtkButton? defaultButton)
		{
			Active = window;
			if (defaultButton is not null)
				window.DefaultWidget = defaultButton;
			window.Present();
		}

		static void Forget(GtkWindow window)
		{
			if (ReferenceEquals(Active, window))
				Active = null;
		}

		static void Close(GtkWindow window)
		{
			Forget(window);
			window.Close();
		}
	}
}
