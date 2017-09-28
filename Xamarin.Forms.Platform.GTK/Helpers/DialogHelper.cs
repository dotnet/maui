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
                    ButtonsType.Ok,
                    arguments.Message);

            SetDialogTitle(arguments.Title, messageDialog);

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
            SetCancelButton(arguments.Cancel, messageDialog);
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

        private static void SetCancelButton(string cancel, MessageDialog messageDialog)
        {
            var buttonsBox = messageDialog.GetDescendants()
                .OfType<HButtonBox>()
                .FirstOrDefault();

            if (buttonsBox == null) return;

            var cancelButton = buttonsBox.GetDescendants()
                .OfType<Gtk.Button>()
                .FirstOrDefault();

            if (cancelButton == null) return;

            if (string.IsNullOrEmpty(cancel))
            {
                cancelButton.Hide();
            }
            else
            {
                cancelButton.Label = cancel;
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
    }
}