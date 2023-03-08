using System;
using System.Linq;
using GLib;
using Gtk;
using Microsoft.Maui.Controls.Internals;


namespace Microsoft.Maui.Controls.Platform
{
	// Ported from https://github.com/xamarin/Xamarin.Forms/blob/5.0.0/Xamarin.Forms.Platform.GTK/Helpers/DialogHelper.cs
	internal static class DialogHelper
	{
		public static void ShowAlert(Gtk.Window window, AlertArguments arguments)
		{
			var messageDialog = new MessageDialog(
				window,
				DialogFlags.DestroyWithParent | DialogFlags.UseHeaderBar,
				MessageType.Other,
				GetAlertButtons(arguments),
				arguments.Message
			);

			SetDialogTitle(arguments.Title, messageDialog);
			SetButtonText(arguments.Accept, ResponseType.Ok, messageDialog);
			SetButtonText(arguments.Cancel, ResponseType.Cancel, messageDialog);

			ResponseType result = (ResponseType)messageDialog.Run();

			if (result == ResponseType.Ok)
			{
				arguments.SetResult(true);
			}
			else
			{
				arguments.SetResult(false);
			}

			messageDialog.Destroy();
		}

		public static void ShowActionSheet(Gtk.Window window, ActionSheetArguments arguments)
		{
			// as title bar is not shown, use dialog body for title text

			var messageDialog = new MessageDialog(
				window,
				DialogFlags.DestroyWithParent | DialogFlags.UseHeaderBar,
				MessageType.Other,
				ButtonsType.Cancel,
				arguments.Title
			);


			//SetDialogTitle(arguments.Title, messageDialog);
			SetButtonText(arguments.Cancel, ResponseType.Cancel, messageDialog);
			SetDestructionButton(arguments.Destruction, messageDialog);
			AddExtraButtons(arguments, messageDialog);

			var result = (ResponseType)messageDialog.Run();

			if (result == ResponseType.Cancel)
			{
				arguments.SetResult(arguments.Cancel);
			}
			else if (result == ResponseType.Reject)
			{
				arguments.SetResult(arguments.Destruction);
			}

			messageDialog.Destroy();
		}

		private static void SetDialogTitle(string title, MessageDialog messageDialog)
		{
			messageDialog.Title = title ?? string.Empty;
		}

		private static void SetButtonText(string text, ResponseType type, MessageDialog messageDialog)
		{
			var button = messageDialog.GetWidgetForResponse((int)type) as Gtk.Button;

			if (button is null)
				return;

			if (string.IsNullOrEmpty(text))
			{
				button.Hide();
			}
			else
			{
				button.Label = text;
			}
		}

		private static void SetDestructionButton(string destruction, MessageDialog messageDialog)
		{
			if (!string.IsNullOrEmpty(destruction))
			{
				var destructionButton =
					messageDialog.AddButton(destruction, ResponseType.Reject) as Gtk.Button;

				if (destructionButton is null)
					return;

				var destructionColor = Microsoft.Maui.Graphics.Colors.Red;
				destructionButton.Child.SetForegroundColor(destructionColor);
			}
		}

		private static void AddExtraButtons(ActionSheetArguments arguments, MessageDialog messageDialog)
		{
			foreach (var buttonText in arguments.Buttons)
			{
				var button = new Gtk.Button();
				button.Label = buttonText;
				button.Clicked += (obj, eventArgs) =>
				{
					arguments.SetResult(button.Label);
					messageDialog.Destroy();
				};
				button.Show();
				messageDialog.AddActionWidget(button, ResponseType.None);
			}
		}

		private static ButtonsType GetAlertButtons(AlertArguments arguments)
		{
			bool hasAccept = !string.IsNullOrEmpty(arguments.Accept);
			bool hasCancel = !string.IsNullOrEmpty(arguments.Cancel);

			ButtonsType type = ButtonsType.None;

			if (hasAccept && hasCancel)
			{
				type = ButtonsType.OkCancel;
			}
			else if (hasAccept && !hasCancel)
			{
				type = ButtonsType.Ok;
			}
			else if (!hasAccept && hasCancel)
			{
				type = ButtonsType.Cancel;
			}

			return type;
		}
	}
}
