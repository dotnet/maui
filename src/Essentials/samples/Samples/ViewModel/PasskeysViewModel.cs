using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
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

namespace Samples.ViewModel
{
	public class PasskeysViewModel : BaseViewModel
	{
		// The relying-party (RP) server is the single Samples.WebServer project in this repo — it hosts BOTH
		// the passkeys relying party (this page) AND the OAuth pass-through. So testing is: launch that
		// one web app, then run this MAUI app.
		//
		// Dev-tunnel first: passkeys are bound to a domain (the RP ID), so the server must be reachable
		// at a real, stable, public HTTPS domain — localhost can't complete a ceremony on a device.
		// From src/Essentials/samples run `pwsh ./Configure.ps1` to provision one (it also writes
		// the domain into the server's user-secrets — see Samples.WebServer/README.md), then replace the
		// host below with the printed https://…devtunnels.ms URL. It's the SAME URL the Web
		// Authenticator page uses.
		//
		// (localhost / 10.0.2.2 only exercises the round-trip and can be added back later.)
		string serverBaseUrl = "https://your-tunnel-5177.devtunnels.ms";

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
			SignOutCommand = new Command(async () => await SignOutAsync());
			RegisterCommand = new Command(async () => await RegisterAsync());
			LoginCommand = new Command(async () => await LoginAsync());
			ExternalSignInCommand = new Command(async () => await ExternalSignInAsync());
			GetExternalProfileCommand = new Command(async () => await GetExternalProfileAsync());
			EditServerUrlCommand = new Command(async () => await EditServerUrlAsync());
		}

		public bool IsSupported => PasskeysApi.IsSupported;

		public string SupportedText => IsSupported
			? "Passkeys are supported on this device."
			: "Passkeys are NOT supported on this device/OS version.";

		public string ServerBaseUrl
		{
			get => serverBaseUrl;
			set => SetProperty(ref serverBaseUrl, value);
		}

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
			: "No passkey yet — add one for faster sign-in.";

		public string CreatePasskeyButtonText => HasPasskey ? "Add another passkey" : "Create a passkey";

		public ICommand SignUpCommand { get; }

		public ICommand SignInPasswordCommand { get; }

		public ICommand SignOutCommand { get; }

		public ICommand RegisterCommand { get; }

		public ICommand LoginCommand { get; }

		public ICommand ExternalSignInCommand { get; }

		public ICommand GetExternalProfileCommand { get; }

		public ICommand EditServerUrlCommand { get; }

		async Task SignUpAsync()
		{
			try
			{
				IsBusy = true;
				Log($"Creating account '{Username}'…");

				// ASP.NET Core Identity's MapIdentityApi: POST /account/register { email, password }.
				await PostJsonAsync("/account/register", new { email = Username, password = Password });

				// /register does NOT sign you in, so immediately bootstrap a cookie session. This mirrors a
				// real app where "create account" lands you logged in and ready to add a passkey.
				Log("Account created. Signing in…");
				await PostJsonAsync("/account/login?useCookies=true", new { email = Username, password = Password });

				await RefreshAndOfferPasskeyAsync();
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

				// The native "bootstrap": POST /account/login?useCookies=true sets the Identity auth cookie
				// on our CookieContainer. No browser, no webview — just a form post.
				await PostJsonAsync("/account/login?useCookies=true", new { email = Username, password = Password });

				await RefreshAndOfferPasskeyAsync();
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

		async Task SignOutAsync()
		{
			try
			{
				IsBusy = true;
				Log("Signing out…");

				// Clears the Identity session cookie on our CookieContainer.
				await PostJsonAsync("/account/logout", new { });
				SetSignedOutState();

				Log("Signed out.");
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

		async Task RegisterAsync()
		{
			if (!EnsureSupported())
				return;

			try
			{
				// Let the user name the passkey up front (so it isn't stored "Unnamed"). A sensible default
				// is the device name; if they clear it or cancel, the server falls back to the AAGUID-inferred
				// authenticator name (e.g. "Google Password Manager").
				var suggestedName = string.IsNullOrWhiteSpace(DeviceInfo.Name) ? "My passkey" : DeviceInfo.Name;
				var name = await DisplayPromptAsync("Name this passkey", "A label to recognise it later (e.g. your device or password manager).", suggestedName);
				var nameQuery = string.IsNullOrWhiteSpace(name) ? string.Empty : $"?name={Uri.EscapeDataString(name.Trim())}";

				IsBusy = true;
				Log("Requesting creation options…");

				// 1) Ask the RP server for PublicKeyCredentialCreationOptions (WebAuthn JSON).
				//    Enrollment is for the signed-in user (the session cookie set at sign-in identifies
				//    them), so no username is sent here.
				var creationOptionsJson = await PostAsync("/passkeys/register/begin");

				// 2) Create the passkey with the platform authenticator (biometric / PIN prompt).
				Log("Creating passkey with the platform authenticator…");
				var response = await PasskeysApi.CreateAsync(creationOptionsJson, CancellationToken.None);

				// 3) Send the attestation back to the RP server to verify + store (with the chosen name).
				Log("Verifying attestation with the server…");
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

		async Task DeletePasskeyAsync(PasskeyItem passkey)
		{
			if (passkey is null)
				return;

			try
			{
				var confirmed = await DisplayConfirmAsync(
					"Remove passkey?",
					$"“{passkey.Name}” will be removed from your account. You won't be able to sign in with it anymore.",
					"Remove",
					"Cancel");
				if (!confirmed)
					return;

				IsBusy = true;
				Log($"Removing passkey “{passkey.Name}”…");

				var client = GetClient();
				using var httpResponse = await client.DeleteAsync($"/passkeys/delete?credentialId={Uri.EscapeDataString(passkey.Id)}");
				var body = await httpResponse.Content.ReadAsStringAsync();
				if (!httpResponse.IsSuccessStatusCode)
					throw new InvalidOperationException($"Server returned {(int)httpResponse.StatusCode}: {ExtractServerMessage(body)}");

				await RefreshAccountStateAsync();
				Log("✅ Passkey removed.");
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

		async Task LoginAsync()
		{
			if (!EnsureSupported())
				return;

			try
			{
				IsBusy = true;
				Log("Requesting request options…");

				// 1) Ask the RP server for PublicKeyCredentialRequestOptions (WebAuthn JSON).
				//    Username-less / discoverable-credential sign-in: we deliberately do NOT send a
				//    username. The passkey itself carries the user handle and the OS account picker lets
				//    you choose which account to sign into — the credential is the identity. (Passing a
				//    username here would pin the ceremony to that one account and reject a passkey that
				//    belongs to a different user, which is a confusing failure in a multi-account demo.)
				var requestOptionsJson = await PostAsync("/passkeys/login/begin");

				// 2) Assert with the platform authenticator (biometric / PIN prompt).
				Log("Asserting passkey with the platform authenticator…");
				var response = await PasskeysApi.AssertAsync(requestOptionsJson, CancellationToken.None);

				// 3) Send the assertion back to the RP server to verify + sign in.
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

		// After any password sign-in, ask the server what this account has and — if there's no passkey
		// yet — offer to set one up. This is the "make it feel real" moment: a normal app nudges you to
		// enroll a passkey right after you log in with a password.
		async Task RefreshAndOfferPasskeyAsync()
		{
			await RefreshAccountStateAsync();
			if (!IsSignedIn)
				return;

			Log($"✅ Signed in as {CurrentUsername}.");

			if (!HasPasskey && PasskeysApi.IsSupported)
			{
				var wantsPasskey = await DisplayConfirmAsync(
					"Set up a passkey?",
					"Sign in faster next time using your fingerprint, face, or device PIN — no password needed. Create a passkey now?",
					"Set up",
					"Not now");

				if (wantsPasskey)
					await RegisterAsync();
			}
		}

		// Single source of truth for "am I signed in, and what passkeys do I have?" — GET /passkeys/list
		// returns { username, passkeyCount, passkeys[] } for the cookie-authenticated user, or 401 when
		// signed out. The Identity session cookie rides along on this GET via the shared CookieContainer.
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
					var item = new PasskeyItem
					{
						Id = pk.TryGetProperty("id", out var id) ? id.GetString() : null,
						Name = pk.TryGetProperty("name", out var n) ? n.GetString() : "Unnamed passkey",
						CreatedAt = pk.TryGetProperty("createdAt", out var ca) && ca.TryGetDateTimeOffset(out var dto)
							? dto.ToLocalTime().ToString("MMM d, yyyy")
							: null,
					};
					item.DeleteCommand = new Command(async () => await DeletePasskeyAsync(item));
					Passkeys.Add(item);
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

		async Task ExternalSignInAsync()
		{
			try
			{
				IsBusy = true;
				Log("Opening the external sign-in page in the browser…");

				// Flow 1 (BFF): the browser hits the server's /native-auth/external/start; the SERVER runs the
				// whole OAuth exchange with the provider, creates/links a LOCAL Identity account, and redirects
				// back to our custom scheme with a single-use code. We never see the provider's token.
				var callback = new Uri("xamarinessentials://");
				var startUrl = new Uri($"{NormalizeBaseUrl()}native-auth/external/start?provider=Dev&returnUri={Uri.EscapeDataString(callback.ToString())}");

				var result = await WebAuthenticator.AuthenticateAsync(startUrl, callback);

				if (result.Properties.TryGetValue("error", out var error) && !string.IsNullOrEmpty(error))
				{
					Log($"❌ External sign-in failed: {error}");
					return;
				}

				if (!result.Properties.TryGetValue("code", out var code) || string.IsNullOrEmpty(code))
				{
					Log("❌ The server did not return a sign-in code.");
					return;
				}

				// Exchange the one-time code over OUR HttpClient so the Identity session cookie lands in this
				// app's CookieContainer (the browser's cookies are a separate jar). After this we're signed in
				// exactly like a password/passkey sign-in — same account, so passkeys can be added too.
				Log("Exchanging the code for a session…");
				await PostJsonAsync("/native-auth/external/exchange", new { code });

				await RefreshAndOfferPasskeyAsync();
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

		async Task GetExternalProfileAsync()
		{
			try
			{
				IsBusy = true;
				Log("Asking the server for your external profile (BFF relay)…");

				// The "do something" step: WE never hold the provider token — the server does. So we ask our
				// own API, and the server uses its stored provider token to fetch the profile and relay it back.
				var client = GetClient();
				using var httpResponse = await client.GetAsync("/me/external");
				var body = await httpResponse.Content.ReadAsStringAsync();
				if (!httpResponse.IsSuccessStatusCode)
					throw new InvalidOperationException($"Server returned {(int)httpResponse.StatusCode}: {ExtractServerMessage(body)}");

				using var doc = JsonDocument.Parse(body);
				var root = doc.RootElement;

				if (root.TryGetProperty("message", out var message))
				{
					Log(message.GetString());
					return;
				}

				var provider = root.TryGetProperty("provider", out var p) ? p.GetString() : "?";
				var details = body;
				if (root.TryGetProperty("profile", out var profile))
				{
					var name = profile.TryGetProperty("name", out var n) ? n.GetString() : string.Empty;
					var email = profile.TryGetProperty("email", out var e) ? e.GetString() : string.Empty;
					details = $"Provider: {provider}{Environment.NewLine}Name: {name}{Environment.NewLine}Email: {email}";
				}

				await DisplayAlertAsync($"Fetched by the server on your behalf:{Environment.NewLine}{Environment.NewLine}{details}");
				Log($"✅ Server relayed your {provider} profile (you never saw the provider token).");
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

		async Task EditServerUrlAsync()
		{
			var url = await DisplayPromptAsync(
				"Server URL",
				"Base URL of the relying-party server (a public HTTPS dev-tunnel domain). See the server README.",
				ServerBaseUrl);

			if (string.IsNullOrWhiteSpace(url))
				return;

			ServerBaseUrl = url.Trim();
			// A new server means a fresh HttpClient with an empty cookie jar, i.e. a new session.
			SetSignedOutState();
			Log($"Server set to {ServerBaseUrl}");
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

		// Posts a JSON object (used for the ASP.NET Core Identity /account endpoints, whose success
		// responses are often empty). Shares the same cookie-preserving HttpClient so the auth cookie
		// set by /account/login flows into the subsequent passkey ceremony calls.
		Task<string> PostJsonAsync(string relativeUrl, object payload)
			=> PostAsync(relativeUrl, JsonSerializer.Serialize(payload));

		// Pulls a human-readable message out of an error body. Our endpoints return either a plain
		// string or a small { "error": "…" } / { "title": "…" } JSON object; show that rather than
		// dumping raw JSON at the user.
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
					// not JSON after all — fall through
				}
			}

			return trimmed;
		}

		HttpClient GetClient()
		{
			// A single client with a CookieContainer so the WebAuthn challenge cookie set on "/begin"
			// is sent back on the matching "/finish" request.
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
				case PasskeyException pk:
					Log($"❌ Passkey error: {pk.Message}");
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

	// One row in the signed-in passkey list. Id is the Base64Url credential id used to delete it.
	public class PasskeyItem
	{
		public string Id { get; set; }

		public string Name { get; set; }

		public string CreatedAt { get; set; }

		public string CreatedAtText => string.IsNullOrEmpty(CreatedAt) ? string.Empty : $"Added {CreatedAt}";

		public ICommand DeleteCommand { get; set; }
	}
}
