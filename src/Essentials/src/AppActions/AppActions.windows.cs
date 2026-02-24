using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Windows.UI.StartScreen;

namespace Microsoft.Maui.ApplicationModel
{
	class AppActionsImplementation : IAppActions, IPlatformAppActions
	{
		public bool IsSupported => true;

		public async Task<IEnumerable<AppAction>> GetAsync()
		{
			// Load existing items
			var jumpList = await JumpList.LoadCurrentAsync();

			var actions = new List<AppAction>();
			foreach (var item in jumpList.Items)
				actions.Add(item.ToAction());

			return actions;
		}

		public async Task SetAsync(IEnumerable<AppAction> actions)
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

		public event EventHandler<AppActionEventArgs> AppActionActivated;

		public async Task OnLaunched(LaunchActivatedEventArgs e)
		{
			var args = e?.Arguments;
#if !WINDOWS_UWP
			if (string.IsNullOrEmpty(args))
			{
				var cliArgs = Environment.GetCommandLineArgs();
				if (cliArgs?.Length > 1)
					args = cliArgs[1];
			}
#endif

			if (args?.StartsWith(AppActionsExtensions.AppActionPrefix) ?? false)
			{
				var id = AppActionsExtensions.ArgumentsToId(args);

				if (!string.IsNullOrEmpty(id))
				{
					var actions = await GetAsync();
					var appAction = actions.FirstOrDefault(a => a.Id == id);

					if (appAction != null)
						AppActionActivated?.Invoke(null, new AppActionEventArgs(appAction));
				}
			}
		}
	}

	static partial class AppActionsExtensions
	{
		internal const string AppActionPrefix = "XE_APP_ACTIONS-";

		internal const string iconDirectory = "";
		internal const string iconExtension = ".png";

		internal static string ArgumentsToId(this string arguments)
		{
			if (arguments?.StartsWith(AppActionPrefix) ?? false)
				return Encoding.Default.GetString(Convert.FromBase64String(arguments.Substring(AppActionPrefix.Length)));

			return default;
		}

		internal static AppAction ToAction(this JumpListItem item)
			=> new AppAction(ArgumentsToId(item.Arguments), item.DisplayName, item.Description);

		internal static JumpListItem ToJumpListItem(this AppAction action)
		{
			var id = AppActionPrefix + Convert.ToBase64String(Encoding.Default.GetBytes(action.Id));
			var item = JumpListItem.CreateWithArguments(id, action.Title);

			if (!string.IsNullOrEmpty(action.Subtitle))
				item.Description = action.Subtitle;

			if (!string.IsNullOrEmpty(action.Icon))
			{
				var dir = iconDirectory?.Trim('/', '\\').Replace('\\', '/');
				if (!string.IsNullOrEmpty(dir))
					dir += "/";

				var ext = iconExtension;
				if (!string.IsNullOrEmpty(ext) && !ext.StartsWith("."))
					ext = "." + ext;

				item.Logo = new Uri($"ms-appx:///{dir}{action.Icon}{ext}");
			}

			return item;
		}
	}
}
