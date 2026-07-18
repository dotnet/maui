#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Authentication
{
	/// <summary>
	/// Create and use passkeys (WebAuthn / FIDO2 public-key credentials) with the native platform
	/// authenticator (Face ID / Touch ID / Windows Hello / Android biometric).
	/// </summary>
	/// <remarks>
	/// This API brokers the standard WebAuthn JSON between a relying-party (RP) server and the OS
	/// authenticator. The server produces the options JSON and verifies the response JSON; this API only
	/// drives the native UI. It does not perform any server-side verification, attestation validation, or
	/// challenge generation.
	/// </remarks>
	public interface IPasskeys
	{
		/// <summary>
		/// Gets a value indicating whether this platform (and OS version) can create and use passkeys.
		/// </summary>
		bool IsSupported { get; }

		/// <summary>
		/// Registers a new passkey by driving the native "create credential" UI.
		/// </summary>
		/// <param name="options">The relying party's <c>PublicKeyCredentialCreationOptions</c> (server-provided).</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
		/// <returns>The WebAuthn registration response to send back to the RP server.</returns>
		/// <exception cref="FeatureNotSupportedException">Thrown when passkeys are not supported on this platform or OS version.</exception>
		/// <exception cref="TaskCanceledException">Thrown when the user cancels the flow or the operation is canceled.</exception>
		/// <exception cref="PasskeyException">Thrown when the ceremony fails (e.g. misconfigured domain association or a platform error).</exception>
		Task<PasskeyCreationResponse> CreateAsync(PasskeyCreationOptions options, CancellationToken cancellationToken = default);

		/// <summary>
		/// Authenticates with an existing passkey by driving the native "get credential" UI.
		/// </summary>
		/// <param name="options">The relying party's <c>PublicKeyCredentialRequestOptions</c> (server-provided).</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
		/// <returns>The WebAuthn assertion response to send back to the RP server.</returns>
		/// <exception cref="FeatureNotSupportedException">Thrown when passkeys are not supported on this platform or OS version.</exception>
		/// <exception cref="TaskCanceledException">Thrown when the user cancels the flow or the operation is canceled.</exception>
		/// <exception cref="PasskeyException">Thrown when the ceremony fails (e.g. no matching credential or a platform error).</exception>
		Task<PasskeyAssertionResponse> AssertAsync(PasskeyRequestOptions options, CancellationToken cancellationToken = default);
	}

	/// <summary>
	/// Represents the relying party's <c>PublicKeyCredentialCreationOptions</c> for <see cref="IPasskeys.CreateAsync"/>.
	/// </summary>
	public sealed class PasskeyCreationOptions
	{
		readonly string _json;

		/// <summary>
		/// Initializes a new instance of the <see cref="PasskeyCreationOptions"/> class.
		/// </summary>
		/// <param name="creationOptionsJson">The server's <c>PublicKeyCredentialCreationOptions</c> JSON.</param>
		public PasskeyCreationOptions(string creationOptionsJson) =>
			_json = creationOptionsJson ?? throw new ArgumentNullException(nameof(creationOptionsJson));

		/// <summary>
		/// Gets or sets a value indicating whether the ceremony should stay on this device and skip any
		/// cross-device / hybrid step (QR code, "use another device", phone-as-authenticator).
		/// </summary>
		/// <remarks>
		/// For registration this means only create a passkey if the local authenticator can do so directly;
		/// for authentication it means only offer a passkey already present on this device. Maps to the
		/// Android and Apple immediately-available options; ignored on Windows. This is an app-side behavior
		/// knob and is not part of the server JSON.
		/// </remarks>
		public bool PreferImmediatelyAvailable { get; set; }

		/// <summary>Returns the underlying <c>PublicKeyCredentialCreationOptions</c> JSON.</summary>
		public override string ToString() => _json;
	}

	/// <summary>
	/// Represents the relying party's <c>PublicKeyCredentialRequestOptions</c> for <see cref="IPasskeys.AssertAsync"/>.
	/// </summary>
	public sealed class PasskeyRequestOptions
	{
		readonly string _json;

		/// <summary>
		/// Initializes a new instance of the <see cref="PasskeyRequestOptions"/> class.
		/// </summary>
		/// <param name="requestOptionsJson">The server's <c>PublicKeyCredentialRequestOptions</c> JSON.</param>
		public PasskeyRequestOptions(string requestOptionsJson) =>
			_json = requestOptionsJson ?? throw new ArgumentNullException(nameof(requestOptionsJson));

		/// <inheritdoc cref="PasskeyCreationOptions.PreferImmediatelyAvailable"/>
		public bool PreferImmediatelyAvailable { get; set; }

		/// <summary>Returns the underlying <c>PublicKeyCredentialRequestOptions</c> JSON.</summary>
		public override string ToString() => _json;
	}

	/// <summary>
	/// Represents the result of a passkey registration. Call <see cref="ToString"/> to get the full WebAuthn
	/// registration response JSON to POST back to the relying-party server.
	/// </summary>
	public sealed class PasskeyCreationResponse
	{
		readonly string _json;
		readonly string _id;

		internal PasskeyCreationResponse(string registrationResponseJson)
		{
			_json = registrationResponseJson ?? throw new ArgumentNullException(nameof(registrationResponseJson));
			_id = PasskeyResponseParser.GetString(_json, "id")
				?? throw new PasskeyException("The registration response JSON did not contain a credential 'id'.");
		}

		/// <summary>
		/// Gets the credential id (base64url) of the created passkey, i.e. the WebAuthn
		/// <c>PublicKeyCredential.id</c>. This is the single, primary identifier of the passkey; store it to
		/// look the credential up later.
		/// </summary>
		public string Id => _id;

		/// <summary>Returns the full WebAuthn registration response JSON.</summary>
		public override string ToString() => _json;
	}

	/// <summary>
	/// Represents the result of a passkey authentication. Call <see cref="ToString"/> to get the full WebAuthn
	/// authentication response JSON to POST back to the relying-party server.
	/// </summary>
	public sealed class PasskeyAssertionResponse
	{
		readonly string _json;
		readonly string _id;
		readonly string? _userHandle;

		internal PasskeyAssertionResponse(string authenticationResponseJson)
		{
			_json = authenticationResponseJson ?? throw new ArgumentNullException(nameof(authenticationResponseJson));
			_id = PasskeyResponseParser.GetString(_json, "id")
				?? throw new PasskeyException("The authentication response JSON did not contain a credential 'id'.");
			_userHandle = PasskeyResponseParser.GetString(_json, "userHandle");
		}

		/// <summary>
		/// Gets the credential id (base64url) of the passkey used, i.e. the WebAuthn
		/// <c>PublicKeyCredential.id</c>.
		/// </summary>
		public string Id => _id;

		/// <summary>
		/// Gets the user handle (base64url) the relying party set as <c>user.id</c> at registration, i.e. the
		/// WebAuthn <c>response.userHandle</c>. Present for discoverable-credential ("username-less") sign-in;
		/// may be <see langword="null"/> when the authenticator does not return one.
		/// </summary>
		public string? UserHandle => _userHandle;

		/// <summary>Returns the full WebAuthn authentication response JSON.</summary>
		public override string ToString() => _json;
	}

	/// <summary>
	/// The exception that is thrown when a passkey ceremony fails for a reason other than user cancellation.
	/// </summary>
	/// <remarks>
	/// User cancellation is surfaced as a <see cref="TaskCanceledException"/>. This exception covers other
	/// failures such as no matching credential, a misconfigured domain association, or an underlying platform
	/// error.
	/// </remarks>
	public class PasskeyException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PasskeyException"/> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public PasskeyException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="PasskeyException"/> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		/// <param name="innerException">The exception that is the cause of the current exception.</param>
		public PasskeyException(string message, Exception? innerException)
			: base(message, innerException)
		{
		}
	}

	/// <summary>
	/// Create and use passkeys (WebAuthn / FIDO2 public-key credentials) with the native platform authenticator.
	/// </summary>
	public static class Passkeys
	{
		/// <summary>Gets a value indicating whether this platform (and OS version) can create and use passkeys.</summary>
		public static bool IsSupported => Default.IsSupported;

		/// <inheritdoc cref="IPasskeys.CreateAsync(PasskeyCreationOptions, CancellationToken)"/>
		public static Task<PasskeyCreationResponse> CreateAsync(PasskeyCreationOptions options, CancellationToken cancellationToken = default)
			=> Default.CreateAsync(options, cancellationToken);

		/// <summary>
		/// Registers a new passkey from the server's <c>PublicKeyCredentialCreationOptions</c> JSON.
		/// </summary>
		/// <param name="creationOptionsJson">The server's <c>PublicKeyCredentialCreationOptions</c> JSON.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
		/// <returns>The WebAuthn registration response to send back to the RP server.</returns>
		public static Task<PasskeyCreationResponse> CreateAsync(string creationOptionsJson, CancellationToken cancellationToken = default)
			=> Default.CreateAsync(new PasskeyCreationOptions(creationOptionsJson), cancellationToken);

		/// <inheritdoc cref="IPasskeys.AssertAsync(PasskeyRequestOptions, CancellationToken)"/>
		public static Task<PasskeyAssertionResponse> AssertAsync(PasskeyRequestOptions options, CancellationToken cancellationToken = default)
			=> Default.AssertAsync(options, cancellationToken);

		/// <summary>
		/// Authenticates with an existing passkey from the server's <c>PublicKeyCredentialRequestOptions</c> JSON.
		/// </summary>
		/// <param name="requestOptionsJson">The server's <c>PublicKeyCredentialRequestOptions</c> JSON.</param>
		/// <param name="cancellationToken">A <see cref="CancellationToken"/> to cancel the operation.</param>
		/// <returns>The WebAuthn assertion response to send back to the RP server.</returns>
		public static Task<PasskeyAssertionResponse> AssertAsync(string requestOptionsJson, CancellationToken cancellationToken = default)
			=> Default.AssertAsync(new PasskeyRequestOptions(requestOptionsJson), cancellationToken);

		static IPasskeys? defaultImplementation;

		/// <summary>
		/// Provides the default implementation for static usage of this API.
		/// </summary>
		public static IPasskeys Default =>
			defaultImplementation ??= new PasskeysImplementation();

		internal static void SetDefault(IPasskeys? implementation) =>
			defaultImplementation = implementation;
	}

	/// <summary>
	/// Minimal, dependency-free extractor for the couple of base64url string fields exposed as decoded
	/// properties (<c>id</c>, <c>userHandle</c>). WebAuthn response values for these keys are base64url
	/// strings (URL-safe alphabet, no quotes or backslashes), so a small scanner is sufficient and avoids a
	/// JSON dependency in the shared, multi-target assembly.
	/// </summary>
	static class PasskeyResponseParser
	{
		public static string? GetString(string json, string key)
		{
			if (string.IsNullOrEmpty(json))
				return null;

			var token = "\"" + key + "\"";
			var searchFrom = 0;

			while (true)
			{
				var keyIndex = json.IndexOf(token, searchFrom, StringComparison.Ordinal);
				if (keyIndex < 0)
					return null;

				var i = keyIndex + token.Length;

				// Skip whitespace before the colon.
				while (i < json.Length && char.IsWhiteSpace(json[i]))
					i++;

				// Must be followed by a colon to be a real key/value pair.
				if (i >= json.Length || json[i] != ':')
				{
					searchFrom = keyIndex + token.Length;
					continue;
				}

				i++; // consume ':'

				// Skip whitespace before the value.
				while (i < json.Length && char.IsWhiteSpace(json[i]))
					i++;

				if (i >= json.Length)
					return null;

				// Explicit JSON null.
				if (json[i] == 'n' && string.CompareOrdinal(json, i, "null", 0, 4) == 0)
					return null;

				// String value.
				if (json[i] == '"')
				{
					i++; // consume opening quote
					var start = i;
					while (i < json.Length && json[i] != '"')
						i++;

					// Unterminated string (no closing quote before end of input).
					if (i >= json.Length)
						return null;

					return json.Substring(start, i - start);
				}

				// Not a string value for this key; keep searching for another occurrence.
				searchFrom = keyIndex + token.Length;
			}
		}
	}

	/// <summary>
	/// base64url helpers (RFC 4648 §5, no padding) shared by the platform implementations that translate
	/// between the WebAuthn JSON (base64url strings) and native binary buffers.
	/// </summary>
	static class Base64Url
	{
		public static string Encode(byte[] bytes)
			=> Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

		public static byte[] Decode(string value)
		{
			var s = value.Replace('-', '+').Replace('_', '/');
			switch (s.Length % 4)
			{
				case 2: s += "=="; break;
				case 3: s += "="; break;
			}

			return Convert.FromBase64String(s);
		}
	}
}
