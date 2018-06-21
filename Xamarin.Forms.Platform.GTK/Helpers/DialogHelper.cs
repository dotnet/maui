using Gtk;
using System.Linq;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.GTK.Extensions;

namespace Xamarin.Forms.Platform.GTK.Helpers
{
    internal static class DialogHelper
    {
        public static void ShowAlert(PlatformRenderer platformRender, AlertArguments arguments)
        {
            MessageDialog messageDialog = new MessageDialog(
                    platformRender.Toplevel as Window,
                    DialogFlags.DestroyWithParent,
                    MessageType.Other,
                    GetAlertButtons(arguments),
                    arguments.Message);

            SetDialogTitle(arguments.Title, messageDialog);
            SetButtonText(arguments.Accept, ButtonsType.Ok, messageDialog);
            SetButtonText(arguments.Cancel, ButtonsType.Cancel, messageDialog);

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

        public static void ShowActionSheet(PlatformRenderer platformRender, ActionSheetArguments arguments)
        {
            MessageDialog messageDialog = new MessageDialog(
               platformRender.Toplevel as Window,
               DialogFlags.DestroyWithParent,
               MessageType.Other,
               ButtonsType.Cancel,
               null);

            SetDialogTitle(arguments.Title, messageDialog);
            SetButtonText(arguments.Cancel, ButtonsType.Cancel, messageDialog);
            SetDestructionButton(arguments.Destruction, messageDialog);
            AddExtraButtons(arguments, messageDialog);

            int result = messageDialog.Run();

            if ((ResponseType)result == ResponseType.Cancel)
            {
                arguments.SetResult(arguments.Cancel);
            }
            else if ((ResponseType)result == ResponseType.Reject)
            {
                arguments.SetResult(arguments.Destruction);
            }

            messageDialog.Destroy();
        }

        private static void SetDialogTitle(string title, MessageDialog messageDialog)
        {
            messageDialog.Title = title ?? string.Empty;
        }

        private static void SetButtonText(string text, ButtonsType type, MessageDialog messageDialog)
        {
            string gtkLabel = string.Empty;

            switch (type)
            {
                case ButtonsType.Ok:
                    gtkLabel = "gtk-ok";
                    break;
                case ButtonsType.Cancel:
                    gtkLabel = "gtk-cancel";
                    break;
            }

            var buttonsBox = messageDialog.GetDescendants()
                .OfType<HButtonBox>()
                .FirstOrDefault();

            if (buttonsBox == null) return;

            var targetButton = buttonsBox.GetDescendants()
                .OfType<Gtk.Button>()
                .FirstOrDefault(x => x.Label == gtkLabel);

            if (targetButton == null) return;

            if (string.IsNullOrEmpty(text))
            {
                targetButton.Hide();
            }
            else
            {
                targetButton.Label = text;
            }
        }

        private static void SetDestructionButton(string destruction, MessageDialog messageDialog)
        {
            if (!string.IsNullOrEmpty(destruction))
            {
                var destructionButton =
                    messageDialog.AddButton(destruction, ResponseType.Reject) as Gtk.Button;

                var destructionColor = Color.Red.ToGtkColor();
                destructionButton.Child.ModifyFg(StateType.Normal, destructionColor);
                destructionButton.Child.ModifyFg(StateType.Prelight, destructionColor);
                destructionButton.Child.ModifyFg(StateType.Active, destructionColor);
            }
        }

        private static void AddExtraButtons(ActionSheetArguments arguments, MessageDialog messageDialog)
        {
            var vbox = messageDialog.VBox;

            // As we are not showing any message in this dialog, we just 
            // hide default container and avoid it from using space
            vbox.Children[0].Hide();

            if (arguments.Buttons.Any())
            {
                for (int i = 0; i < arguments.Buttons.Count(); i++)
                {
                    var button = new Gtk.Button();
                    button.Label = arguments.Buttons.ElementAt(i);
                    button.Clicked += (o, e) =>
                    {
                        arguments.SetResult(button.Label);
                        messageDialog.Destroy();
                    };
                    button.Show();

                    vbox.PackStart(button, false, false, 0);
                }
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