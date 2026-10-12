using System.Runtime.InteropServices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Storage;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace Maui.Controls.Sample.HybridWebApp.Playground;

sealed class MauiPlaygroundPlatform : IPlaygroundPlatform
{
	public Task<IdentityResult> GetIdentityAsync(CancellationToken cancellation) => OnMainThread(() =>
		Task.FromResult(new IdentityResult(AppInfo.Current.Name, AppInfo.Current.VersionString, AppInfo.Current.BuildString,
			DeviceInfo.Current.Platform.ToString(), DeviceInfo.Current.VersionString, DeviceInfo.Current.Model,
			RuntimeInformation.ProcessArchitecture.ToString(), Environment.ProcessId, MauiProgram.StartupId,
			MauiProgram.StartedAt.ToString("O"))), cancellation);

	public Task<TextResult> GetClipboardAsync(CancellationToken cancellation) => OnMainThread(async () =>
	{
		var text = await Clipboard.Default.GetTextAsync();
		if (text is not null)
			EssentialsProvider.ValidateText(text);
		return new TextResult(text);
	}, cancellation);

	public Task<Ack> SetClipboardAsync(string text, CancellationToken cancellation) => OnMainThread(async () =>
	{
		await Clipboard.Default.SetTextAsync(text);
		return new Ack(true);
	}, cancellation);

	public Task<PreferenceResult> GetPreferenceAsync(string key, CancellationToken cancellation) => OnMainThread(() =>
	{
		var name = EssentialsProvider.PreferenceNamespace + key;
		var value = Preferences.Default.ContainsKey(name) ? Preferences.Default.Get(name, "") : null;
		if (value is not null)
			EssentialsProvider.ValidateText(value);
		return Task.FromResult(new PreferenceResult(key, value));
	}, cancellation);

	public Task<PreferenceResult> SetPreferenceAsync(string key, string? value, CancellationToken cancellation) => OnMainThread(() =>
	{
		var name = EssentialsProvider.PreferenceNamespace + key;
		if (value is null)
			Preferences.Default.Remove(name);
		else
			Preferences.Default.Set(name, value);
		return Task.FromResult(new PreferenceResult(key, value));
	}, cancellation);

	static Task<T> OnMainThread<T>(Func<Task<T>> action, CancellationToken cancellation) =>
		MainThread.InvokeOnMainThreadAsync(async () =>
		{
			cancellation.ThrowIfCancellationRequested();
			try
			{
				var result = await action();
				cancellation.ThrowIfCancellationRequested();
				return result;
			}
			catch (FeatureNotEnabledException)
			{
				throw new CommandException("configuration_required", "The native feature is disabled in system settings. Enable it before retrying.");
			}
		});
}
