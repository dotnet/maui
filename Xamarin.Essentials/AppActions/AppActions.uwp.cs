using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.UI.StartScreen;

namespace Xamarin.Essentials
{
    public static partial class AppActions
    {
        const string appActionPrefix = "XE_APP_ACTIONS-";

        internal static bool PlatformIsSupported
           => true;

        internal static async Task OnLaunched(LaunchActivatedEventArgs e)
        {
            if (e?.Arguments?.StartsWith(appActionPrefix) ?? false)
            {
                var id = ArgumentsToId(e.Arguments);

                if (!string.IsNullOrEmpty(id))
                {
                    var actions = await PlatformGetAsync();
                    var appAction = actions.FirstOrDefault(a => a.Id == id);

                    if (appAction != null)
                        AppActions.InvokeOnAppAction(null, appAction);
                }
            }
        }

        static async Task<IEnumerable<AppAction>> PlatformGetAsync()
        {
            // Load existing items
            var jumpList = await JumpList.LoadCurrentAsync();

            var actions = new List<AppAction>();
            foreach (var item in jumpList.Items)
                actions.Add(item.ToAction());

            return actions;
        }

        static async Task PlatformSetAsync(IEnumerable<AppAction> actions)
        {
            // Load existing items
            var jumpList = await JumpList.LoadCurrentAsync();

            // Set as custom, not system or frequent
            jumpList.SystemGroupKind = JumpListSystemGroupKind.None;

            // Clear the existing items
            jumpList.Items.Clear();

            // Add each action
            foreach (var a in actions)
                jumpList.Items.Add(a.ToJumpListItem());

            // Save the changes
            await jumpList.SaveAsync();
        }

        static AppAction ToAction(this JumpListItem item)
            => new AppAction(item.DisplayName, ArgumentsToId(item.Arguments), item.Description);

        static string ArgumentsToId(string arguments)
        {
            if (arguments?.StartsWith(appActionPrefix) ?? false)
                return System.Text.Encoding.Default.GetString(Convert.FromBase64String(arguments.Substring(appActionPrefix.Length)));

            return default;
        }

        static JumpListItem ToJumpListItem(this AppAction action)
        {
            var a = appActionPrefix + Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(action.Id.ToString()));

            var item = JumpListItem.CreateWithArguments(a, action.Title);

            if (!string.IsNullOrEmpty(action.Subtitle))
                item.Description = action.Subtitle;

            // if (!string.IsNullOrEmpty(action.Icon))
            //    item.Logo = new Uri("ms-appx://Assets/" + action.Icon + ".png");

            return item;
        }
    }
}
