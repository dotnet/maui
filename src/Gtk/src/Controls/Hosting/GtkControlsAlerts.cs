using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Gtk.Platform;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.Controls.Gtk.Hosting
{
	/// <summary>
	/// Registers the dialogs of <c>DisplayAlertAsync</c>, <c>DisplayActionSheetAsync</c> and <c>DisplayPromptAsync</c> through MAUI's
	/// alert delegate convention: keyed <c>Func&lt;Page, TArguments, Task&lt;TResult&gt;&gt;</c> services that the alert manager of every
	/// window resolves (<c>DelegateAlertSubscription</c>); the delegate's result completes the argument's task.
	/// </summary>
	static class GtkControlsAlerts
	{
		public static void Register(MauiAppBuilder builder)
		{
			var services = builder.Services;

			services.AddKeyedSingleton<Func<Page, AlertArguments, Task<bool>>>(AlertManager.DisplayAlertServiceKey, (_, _) => GtkDialogs.ShowAlertAsync);
			services.AddKeyedSingleton<Func<Page, ActionSheetArguments, Task<string>>>(AlertManager.DisplayActionSheetServiceKey, (_, _) => GtkDialogs.ShowActionSheetAsync);
			services.AddKeyedSingleton<Func<Page, PromptArguments, Task<string>>>(AlertManager.DisplayPromptServiceKey, (_, _) => GtkDialogs.ShowPromptAsync);
		}
	}
}
