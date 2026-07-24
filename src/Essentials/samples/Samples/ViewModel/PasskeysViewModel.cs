using System;
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

		public PasskeysViewModel()
		{
			SignUpCommand = new Command(async () => await SignUpAsync());
			SignInPasswordCommand = new Command(async () => await SignInPasswordAsync());
			SignOutCommand = new Command(async () => await SignOutAsync());
			RegisterCommand = new Command(async () => await RegisterAsync());
			LoginCommand = new Command(async () => await LoginAsync());
			EditServerUrlCommand = new Command(async () => await EditServerUrlAsync());
		}

		public bool IsSupported => Passkeys.IsSupported;

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
				IsBusy = true;
				Log("Requesting creation options…");

				// 1) Ask the RP server for PublicKeyCredentialCreationOptions (WebAuthn JSON).
				//    Enrollment is for the signed-in user (the session cookie set at sign-in identifies
				//    them), so no username is sent here.
				var creationOptionsJson = await PostAsync("/passkeys/register/begin");

				// 2) Create the passkey with the platform authenticator (biometric / PIN prompt).
				Log("Creating passkey with the platform authenticator…");
				var response = await Passkeys.CreateAsync(creationOptionsJson, CancellationToken.None);

				// 3) Send the attestation back to the RP server to verify + store.
				Log("Verifying attestation with the server…");
				await PostAsync("/passkeys/register/finish", response.ToString());

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
				var response = await Passkeys.AssertAsync(requestOptionsJson, CancellationToken.None);

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

			if (!HasPasskey && Passkeys.IsSupported)
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

		// Single source of truth for "am I signed in, and do I have a passkey?" — GET /passkeys/list
		// returns { username, passkeyCount } for the cookie-authenticated user, or 401 when signed out.
		// The Identity session cookie rides along on this GET via the shared CookieContainer.
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
			IsSignedIn = true;
		}

		void SetSignedOutState()
		{
			IsSignedIn = false;
			CurrentUsername = null;
			PasskeyCount = 0;
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
			if (!Passkeys.IsSupported)
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
			// Only the latest message is shown (one-line status strip at the bottom of the page).
			MainThread.BeginInvokeOnMainThread(() =>
				Status = $"{DateTime.Now:HH:mm:ss}  {message}");
		}
	}
}
