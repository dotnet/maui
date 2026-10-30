using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;

namespace Microsoft.Maui.Essentials.Gtk;

/// <summary>
/// <see cref="ISecureStorage"/> for Linux backed by the freedesktop Secret Service through libsecret
/// (GNOME Keyring, KWallet's secret service, KeePassXC...). Secrets are scoped by the application id.
/// </summary>
/// <remarks>
/// Throws <see cref="FeatureNotSupportedException"/> on first use when libsecret is not installed.
/// Calls block on the Secret Service (it may prompt to unlock the keyring), so they run on the thread pool.
/// </remarks>
sealed class GtkSecureStorage : ISecureStorage
{
	readonly string service;
	readonly Lazy<ISecretBackend> backend;

	internal GtkSecureStorage(string applicationId, ISecretBackend? backend = null)
	{
		service = applicationId;
		this.backend = new Lazy<ISecretBackend>(() =>
			backend
			?? LibSecret.TryCreate("org.maui.SecureStorage")
			?? throw new FeatureNotSupportedException("SecureStorage on Linux requires libsecret (libsecret-1.so.0) and a running Secret Service."));
	}

	public Task<string?> GetAsync(string key)
	{
		ValidateKey(key);
		return Task.Run(() => backend.Value.Lookup(service, key));
	}

	public Task SetAsync(string key, string value)
	{
		ValidateKey(key);
		if (value is null)
			throw new ArgumentNullException(nameof(value));

		return Task.Run(() => backend.Value.Store(service, key, $"{service}: {key}", value));
	}

	public bool Remove(string key)
	{
		ValidateKey(key);
		return backend.Value.Clear(service, key);
	}

	public void RemoveAll() =>
		backend.Value.ClearAll(service);

	static void ValidateKey(string key)
	{
		if (key is null)
			throw new ArgumentNullException(nameof(key));
	}
}
