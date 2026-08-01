using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Devices;
using PasskeysApi = Microsoft.Maui.Authentication.Passkeys;

namespace Samples.ViewModel;

public class PasskeysViewModel : BaseViewModel
{
	// The relying-party server (Samples.Server.Passkeys). Passkeys are bound to a domain, so it must be a
	// public HTTPS host reachable from the device. Run `pwsh ./Configure-Passkeys.ps1` in src/Essentials/samples
	// to provision a dev tunnel; it bakes the URL in via AssemblyMetadata (see Essentials.Sample.csproj).
	readonly string serverBaseUrl = GetConfiguredServerUrl();

	static string GetConfiguredServerUrl()
	{
		foreach (var attribute in typeof(PasskeysViewModel).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>())
		{
			if (attribute.Key == "PasskeysServerUrl" && !string.IsNullOrWhiteSpace(attribute.Value))
				return attribute.Value;
		}

		return "https://your-tunnel-5177.devtunnels.ms";
	}

	string username = string.Empty;
	string password = string.Empty;
	string status = string.Empty;
	bool isSignedIn;
	string currentUsername;
	int passkeyCount;

	HttpClient httpClient;

	public ObservableCollection<PasskeyItem> Passkeys { get; } = new();

	public PasskeysViewModel()
	{
		SignUpCommand = new Command(async () => await SignUpAsync());
		SignInPasswordCommand = new Command(async () => await SignInPasswordAsync());
		SignOutCommand = new Command(SignOut);
		RegisterCommand = new Command(async () => await RegisterAsync());
		LoginCommand = new Command(async () => await LoginAsync());
	}

	public bool IsSupported => PasskeysApi.IsSupported;

	public string SupportedText => IsSupported
		? "Passkeys are supported on this device."
		: "Passkeys are NOT supported on this device/OS version.";

	public string ServerBaseUrl => serverBaseUrl;

	public string Username
	{
		get => username;
		set => SetProperty(ref username, value);
	}

	public string Password
	{
		get => password;
		set => SetProperty(ref password, value);
	}

	public string Status
	{
		get => status;
		set => SetProperty(ref status, value);
	}

	public bool IsSignedIn
	{
		get => isSignedIn;
		set => SetProperty(ref isSignedIn, value, onChanged: () =>
		{
			OnPropertyChanged(nameof(IsLoggedOut));
			OnPropertyChanged(nameof(AccountStatusText));
		});
	}

	public bool IsLoggedOut => !IsSignedIn;

	public string CurrentUsername
	{
		get => currentUsername;
		set => SetProperty(ref currentUsername, value, onChanged: () => OnPropertyChanged(nameof(AccountStatusText)));
	}

	public int PasskeyCount
	{
		get => passkeyCount;
		set => SetProperty(ref passkeyCount, value, onChanged: () =>
		{
			OnPropertyChanged(nameof(HasPasskey));
			OnPropertyChanged(nameof(PasskeyStatusText));
			OnPropertyChanged(nameof(CreatePasskeyButtonText));
		});
	}

	public bool HasPasskey => PasskeyCount > 0;

	public string AccountStatusText => $"Signed in as {CurrentUsername}";

	public string PasskeyStatusText => HasPasskey
		? (PasskeyCount == 1 ? "✓ 1 passkey on this account." : $"✓ {PasskeyCount} passkeys on this account.")
		: "No passkey yet - add one for faster sign-in.";

	public string CreatePasskeyButtonText => HasPasskey ? "Add another passkey" : "Create a passkey";

	public ICommand SignUpCommand { get; }

	public ICommand SignInPasswordCommand { get; }

	public ICommand SignOutCommand { get; }

	public ICommand RegisterCommand { get; }

	public ICommand LoginCommand { get; }

	async Task SignUpAsync()
	{
		try
		{
			IsBusy = true;
			Log($"Creating account '{Username}'…");

			// POST /account/register { email, password }.
			await PostJsonAsync("/account/register", new { email = Username, password = Password });

			// /register doesn't sign you in, so log in immediately.
			Log("Account created. Signing in…");
			await PostJsonAsync("/account/login?useCookies=true", new { email = Username, password = Password });

			await RefreshAfterSignInAsync();
		}
		catch (Exception ex)
		{
			HandleError(ex);
		}
		finally
		{
			IsBusy = false;
		}
	}

	async Task SignInPasswordAsync()
	{
		try
		{
			IsBusy = true;
			Log($"Signing in as '{Username}'…");

			// POST /account/login?useCookies=true sets the auth cookie on our CookieContainer.
			await PostJsonAsync("/account/login?useCookies=true", new { email = Username, password = Password });

			await RefreshAfterSignInAsync();
		}
		catch (Exception ex)
		{
			HandleError(ex);
		}
		finally
		{
			IsBusy = false;
		}
	}

	void SignOut()
	{
		// The auth cookie is a self-contained ticket held in our CookieContainer, and the dev server keeps
		// no server-side session - so "signing out" is just discarding the cookie jar. Drop the HttpClient;
		// the next request builds a fresh one with an empty CookieContainer.
		httpClient?.Dispose();
		httpClient = null;
		SetSignedOutState();
		Log("Signed out.");
	}

	async Task RegisterAsync()
	{
		if (!EnsureSupported())
			return;

		try
		{
			IsBusy = true;
			Log("Requesting creation options…");

			// Get the WebAuthn creation options for the signed-in user.
			var creationOptionsJson = await PostAsync("/passkeys/register/begin");

			// Create the passkey with the platform authenticator (biometric / PIN).
			Log("Creating passkey with the platform authenticator…");
			var response = await PasskeysApi.CreateAsync(creationOptionsJson, CancellationToken.None);

			// Send the attestation back to be verified and stored, labelled with this device so passkeys
			// from different test devices are easy to tell apart in the list.
			Log("Verifying attestation with the server…");
			var nameQuery = $"?name={Uri.EscapeDataString(BuildDeviceName())}";
			await PostAsync($"/passkeys/register/finish{nameQuery}", response.ToString());

			await RefreshAccountStateAsync();
			Log("✅ Passkey created. You can now sign in with it.");
		}
		catch (Exception ex)
		{
			HandleError(ex);
		}
		finally
		{
			IsBusy = false;
		}
	}

	// A descriptive, auto-generated label from the Essentials DeviceInfo API so passkeys created on
	// different devices are easy to tell apart — no user prompt. e.g. "iPhone 16 Pro (iOS 18.0)".
	static string BuildDeviceName()
	{
		var name = string.IsNullOrWhiteSpace(DeviceInfo.Name)
			? $"{DeviceInfo.Manufacturer} {DeviceInfo.Model}".Trim()
			: DeviceInfo.Name;
		if (string.IsNullOrWhiteSpace(name))
			name = "Unknown device";
		return $"{name} ({DeviceInfo.Platform} {DeviceInfo.VersionString})";
	}

	async Task LoginAsync()
	{
		if (!EnsureSupported())
			return;

		try
		{
			IsBusy = true;
			Log("Requesting request options…");

			// Get the WebAuthn request options. Username-less: the passkey carries the identity and the
			// OS account picker chooses the account, so no username is sent.
			var requestOptionsJson = await PostAsync("/passkeys/login/begin");

			// Assert with the platform authenticator (biometric / PIN).
			Log("Asserting passkey with the platform authenticator…");
			var response = await PasskeysApi.AssertAsync(requestOptionsJson, CancellationToken.None);

			// Send the assertion back to be verified and signed in.
			Log("Verifying assertion with the server…");
			await PostAsync("/passkeys/login/finish", response.ToString());

			await RefreshAccountStateAsync();
			Log($"✅ Signed in with a passkey as {CurrentUsername}.");
		}
		catch (Exception ex)
		{
			HandleError(ex);
		}
		finally
		{
			IsBusy = false;
		}
	}

	// After sign-in, offer to set up a passkey if the account doesn't have one yet.
	async Task RefreshAfterSignInAsync()
	{
		await RefreshAccountStateAsync();
		if (IsSignedIn)
			Log($"✅ Signed in as {CurrentUsername}.");
	}

	// Refreshes signed-in state and the passkey list from GET /passkeys/list (401 when signed out).
	async Task RefreshAccountStateAsync()
	{
		var client = GetClient();
		using var httpResponse = await client.GetAsync("/passkeys/list");

		if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
		{
			SetSignedOutState();
			return;
		}

		var body = await httpResponse.Content.ReadAsStringAsync();
		if (!httpResponse.IsSuccessStatusCode)
			throw new InvalidOperationException($"Server returned {(int)httpResponse.StatusCode}: {ExtractServerMessage(body)}");

		using var doc = JsonDocument.Parse(body);
		var root = doc.RootElement;
		CurrentUsername = root.TryGetProperty("username", out var u) ? u.GetString() : Username;
		PasskeyCount = root.TryGetProperty("passkeyCount", out var c) ? c.GetInt32() : 0;

		Passkeys.Clear();
		if (root.TryGetProperty("passkeys", out var list) && list.ValueKind == JsonValueKind.Array)
		{
			foreach (var pk in list.EnumerateArray())
			{
				Passkeys.Add(new PasskeyItem
				{
					Id = pk.TryGetProperty("id", out var id) ? id.GetString() : null,
					Name = pk.TryGetProperty("name", out var n) ? n.GetString() : null,
					CreatedAt = pk.TryGetProperty("createdAt", out var ca) && ca.TryGetDateTimeOffset(out var dto)
						? dto.ToLocalTime().ToString("MMM d, yyyy")
						: null,
				});
			}
		}

		IsSignedIn = true;
	}

	void SetSignedOutState()
	{
		IsSignedIn = false;
		CurrentUsername = null;
		PasskeyCount = 0;
		Passkeys.Clear();
	}

	bool EnsureSupported()
	{
		OnPropertyChanged(nameof(IsSupported));
		OnPropertyChanged(nameof(SupportedText));
		if (!PasskeysApi.IsSupported)
		{
			Log("Passkeys are not supported on this device/OS version.");
			return false;
		}

		return true;
	}

	async Task<string> PostAsync(string relativeUrl, string jsonBody = null)
	{
		var client = GetClient();
		using var content = new StringContent(jsonBody ?? string.Empty, Encoding.UTF8, "application/json");
		using var httpResponse = await client.PostAsync(relativeUrl, content);

		var body = await httpResponse.Content.ReadAsStringAsync();
		if (!httpResponse.IsSuccessStatusCode)
			throw new InvalidOperationException($"Server returned {(int)httpResponse.StatusCode}: {ExtractServerMessage(body)}");

		return body;
	}

	// Posts a JSON object over the shared cookie-preserving HttpClient.
	Task<string> PostJsonAsync(string relativeUrl, object payload)
		=> PostAsync(relativeUrl, JsonSerializer.Serialize(payload));

	// Pulls a readable message out of an error body ({ "error": … } / { "title": … } or plain text).
	static string ExtractServerMessage(string body)
	{
		if (string.IsNullOrWhiteSpace(body))
			return "(no details)";

		var trimmed = body.Trim();
		if (trimmed[0] == '{')
		{
			try
			{
				using var doc = JsonDocument.Parse(trimmed);
				if (doc.RootElement.TryGetProperty("error", out var error))
					return error.GetString() ?? trimmed;
				if (doc.RootElement.TryGetProperty("title", out var title))
					return title.GetString() ?? trimmed;
			}
			catch (JsonException)
			{
				// not JSON after all - fall through
			}
		}

		return trimmed;
	}

	HttpClient GetClient()
	{
		// A single client with a CookieContainer so the /begin cookie is sent back on /finish.
		if (httpClient is null || httpClient.BaseAddress?.ToString() != NormalizeBaseUrl())
		{
			httpClient?.Dispose();
			var handler = new HttpClientHandler
			{
				CookieContainer = new CookieContainer(),
				UseCookies = true,
			};
			httpClient = new HttpClient(handler)
			{
				BaseAddress = new Uri(NormalizeBaseUrl()),
				Timeout = TimeSpan.FromMinutes(3),
			};
		}

		return httpClient;
	}

	string NormalizeBaseUrl()
	{
		var url = (ServerBaseUrl ?? string.Empty).Trim();
		if (url.Length == 0 || url[url.Length - 1] != '/')
			url += "/";
		return url;
	}

	void HandleError(Exception ex)
	{
		switch (ex)
		{
			case OperationCanceledException:
				Log("⚠️ Canceled by the user.");
				break;
			case HttpRequestException http:
				Log($"❌ Network error: {http.Message}. Is the server URL correct and reachable?");
				break;
			default:
				Log($"❌ {ex.GetType().Name}: {ex.Message}");
				break;
		}
	}

	void Log(string message)
	{
		// Only the latest message is shown (status strip at the bottom of the page).
		MainThread.BeginInvokeOnMainThread(() =>
			Status = $"{DateTime.Now:HH:mm:ss}  {message}");
	}
}

// One row in the signed-in passkey list. Name is the auto-generated device label; Id is the Base64Url
// credential id (raw bytes encoded), shown as a secondary, technical identifier.
public class PasskeyItem
{
	public string Id { get; set; }

	public string Name { get; set; }

	public string CreatedAt { get; set; }

	// The device label, falling back to the short credential id when the server has no name for it.
	public string DisplayName => string.IsNullOrWhiteSpace(Name) ? ShortId : Name;

	// A short, readable form of the (long) Base64Url credential id.
	public string ShortId => string.IsNullOrEmpty(Id)
		? "(unknown id)"
		: Id.Length <= 16 ? Id : Id.Substring(0, 16) + "…";

	public string CreatedAtText => string.IsNullOrEmpty(CreatedAt) ? string.Empty : $"Added {CreatedAt}";
}
