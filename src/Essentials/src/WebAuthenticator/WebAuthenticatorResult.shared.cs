using System;
using System.Collections.Generic;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Authentication
{
	/// <summary>
	/// Represents a Web Authenticator Result object parsed from the callback Url.
	/// </summary>
	/// <remarks>
	/// All of the query string or url fragment properties are parsed into a dictionary and can be accessed by their key.
	/// </remarks>
	public class WebAuthenticatorResult
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="WebAuthenticatorResult"/> class.
		/// </summary>
		public WebAuthenticatorResult()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WebAuthenticatorResult"/> class by parsing a URI's query string parameters.
		/// </summary>
		/// <param name="uri">The callback uri that was used to end the authentication sequence.</param>
		public WebAuthenticatorResult(Uri uri) : this(uri, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="WebAuthenticatorResult"/> class by parsing a URI's query string parameters.
		/// </summary>
		/// <remarks>
		/// If the responseDecoder is non-null, then it is used to decode the fragment or query string 
		/// returned by the authorization service.  Otherwise, a default response decoder is used.
		/// </remarks>
		/// <param name="uri">The callback uri that was used to end the authentication sequence.</param>
		/// <param name="responseDecoder">The decoder that can be used to decode the callback uri.</param>
		public WebAuthenticatorResult(Uri uri, IWebAuthenticatorResponseDecoder responseDecoder)
		{
			CallbackUri = uri;
			var properties = responseDecoder?.DecodeResponse(uri) ?? WebUtils.ParseQueryString(uri);
			foreach (var kvp in properties)
			{
				Properties[kvp.Key] = kvp.Value;
			}
		}

		/// <summary>
		/// Create a new instance from an existing dictionary.
		/// </summary>
		/// <param name="properties">The dictionary of properties to incorporate.</param>
		public WebAuthenticatorResult(IDictionary<string, string> properties)
		{
			foreach (var kvp in properties)
				Properties[kvp.Key] = kvp.Value;
		}

		/// <summary>
		/// The uri that was used to call back with the access token.
		/// </summary>
		/// <value>
		/// The value of the callback URI, including the fragment or query string bearing 
		/// the access token and associated information.
		/// </value>
		public Uri CallbackUri { get; }

		/// <summary>
		/// The timestamp when the class was instantiated, which usually corresponds with the parsed result of a request.
		/// </summary>
		public DateTimeOffset Timestamp { get; set; } = new DateTimeOffset(DateTime.UtcNow);

		/// <summary>
		/// The dictionary of key/value pairs parsed form the callback URI's query string.
		/// </summary>
		public Dictionary<string, string> Properties { get; set; } = new(StringComparer.Ordinal);

		/// <summary>Puts a key/value pair into the dictionary.</summary>
		public void Put(string key, string value)
			=> Properties[key] = value;

		/// <summary>Gets a value for a given key from the dictionary.</summary>
		/// <param name="key">Key from the callback URI's query string.</param>
		public string Get(string key)
		{
			if (Properties.TryGetValue(key, out var v))
				return v;

			return default;
		}

		/// <summary>The value for the `access_token` key.</summary>
		/// <value>Access Token parsed from the callback URI access_token parameter.</value>
		public string AccessToken
			=> Get("access_token");

		/// <summary>The value for the `refresh_token` key.</summary>
		/// <value>Refresh Token parsed from the callback URI refresh_token parameter.</value>
		public string RefreshToken
			=> Get("refresh_token");

		/// <summary>The value for the `id_token` key.</summary>
		/// <value>The value for the `id_token` key.</value>
		/// <remarks>Apple doesn't return an access token on iOS native sign in, but it does return id_token as a JWT.</remarks>
		public string IdToken
			=> Get("id_token");

		/// <summary>
		/// The refresh token expiry date as calculated by the timestamp of when the result was created plus 
		/// the value in seconds for the refresh_token_expires_in key.
		/// </summary>
		/// <value>Timestamp of the creation of the object instance plus the expires_in seconds parsed from the callback URI.</value>
		public DateTimeOffset? RefreshTokenExpiresIn
		{
			get
			{
				if (Properties.TryGetValue("refresh_token_expires_in", out var v))
				{
					if (int.TryParse(v, out var i))
						return Timestamp.AddSeconds(i);
				}

				return null;
			}
		}

		/// <summary>
		/// The expiry date as calculated by the timestamp of when the result was created plus 
		/// the value in seconds for the `expires_in` key.
		/// </summary>
		/// <value>Timestamp of the creation of the object instance plus the expires_in seconds parsed from the callback URI.</value>
		public DateTimeOffset? ExpiresIn
		{
			get
			{
				if (Properties.TryGetValue("expires_in", out var v))
				{
					if (int.TryParse(v, out var i))
						return Timestamp.AddSeconds(i);
				}

				return null;
			}
		}
	}
}
