using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.StartScreen;

#if WINDOWS_UWP
using Windows.ApplicationModel.Activation;
#elif WINDOWS
using Microsoft.UI.Xaml;
#endif

namespace Microsoft.Maui.Essentials.Implementations
{
	public class AppActionsImplementation : IAppActions
	{
		public string Type => "XE_APP_ACTION_TYPE";
		const string appActionPrefix = "XE_APP_ACTIONS-";

		public static string IconDirectory { get; set; } = "";

		public static string IconExtension { get; set; } = "png";

		public bool IsSupported
		   => true;

		internal static async Task OnLaunched(LaunchActivatedEventArgs e)
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

			if (args?.StartsWith(appActionPrefix) ?? false)
			{
				var id = ArgumentsToId(args);

				if (!string.IsNullOrEmpty(id))
				{
					var actions = await PlatformGetAsync();
					var appAction = actions.FirstOrDefault(a => a.Id == id);

					if (appAction != null)
						AppActions.InvokeOnAppAction(null, appAction);
				}
			}
		}

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

		public Task SetAsync(params AppAction[] actions)
		{	
			return SetAsync(actions.AsEnumerable<AppAction>());
		}


		static string ArgumentsToId(string arguments)
		{
			if (arguments?.StartsWith(appActionPrefix) ?? false)
				return Encoding.Default.GetString(Convert.FromBase64String(arguments.Substring(appActionPrefix.Length)));

			return default;
		}
	}

	internal static class AppActionsExtensions
	{
		static AppAction ToAction(this JumpListItem item)
			=> new AppAction(ArgumentsToId(item.Arguments), item.DisplayName, item.Description);

		static JumpListItem ToJumpListItem(this AppAction action)
		{
			var id = appActionPrefix + Convert.ToBase64String(Encoding.Default.GetBytes(action.Id));
			var item = JumpListItem.CreateWithArguments(id, action.Title);

			if (!string.IsNullOrEmpty(action.Subtitle))
				item.Description = action.Subtitle;

			if (!string.IsNullOrEmpty(action.Icon))
			{
				var dir = IconDirectory.Trim('/', '\\').Replace('\\', '/');
				if (!string.IsNullOrEmpty(dir))
					dir += "/";

				var ext = IconExtension;
				if (!string.IsNullOrEmpty(ext) && !ext.StartsWith("."))
					ext = "." + ext;

				item.Logo = new Uri($"ms-appx:///{dir}{action.Icon}{ext}");
			}

			return item;
		}
	}
}
