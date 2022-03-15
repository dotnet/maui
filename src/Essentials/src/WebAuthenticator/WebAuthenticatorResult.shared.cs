using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Maui.Essentials
{
	/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticatorResult.xml" path="Type[@FullName='Microsoft.Maui.Essentials.WebAuthenticatorResult']/Docs" />
	public class WebAuthenticatorResult
	{
		/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticatorResult.xml" path="//Member[@MemberName='.ctor'][1]/Docs" />
		public WebAuthenticatorResult()
		{
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticatorResult.xml" path="//Member[@MemberName='.ctor'][3]/Docs" />
		public WebAuthenticatorResult(Uri uri)
		{
			foreach (var kvp in WebUtils.ParseQueryString(uri.ToString()))
			{
				Properties[kvp.Key] = kvp.Value;
			}
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticatorResult.xml" path="//Member[@MemberName='.ctor'][2]/Docs" />
		public WebAuthenticatorResult(IDictionary<string, string> properties)
		{
			foreach (var kvp in properties)
				Properties[kvp.Key] = kvp.Value;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticatorResult.xml" path="//Member[@MemberName='Timestamp']/Docs" />
		public DateTimeOffset Timestamp { get; set; } = new DateTimeOffset(DateTime.UtcNow);

		/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticatorResult.xml" path="//Member[@MemberName='Properties']/Docs" />
		public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

		/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticatorResult.xml" path="//Member[@MemberName='Put']/Docs" />
		public void Put(string key, string value)
			=> Properties[key] = value;

		/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticatorResult.xml" path="//Member[@MemberName='Get']/Docs" />
		public string Get(string key)
		{
			if (Properties.TryGetValue(key, out var v))
				return v;

			return default;
		}

		/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticatorResult.xml" path="//Member[@MemberName='AccessToken']/Docs" />
		public string AccessToken
			=> Get("access_token");

		/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticatorResult.xml" path="//Member[@MemberName='RefreshToken']/Docs" />
		public string RefreshToken
			=> Get("refresh_token");

		/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticatorResult.xml" path="//Member[@MemberName='IdToken']/Docs" />
		public string IdToken
			=> Get("id_token");

		/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticatorResult.xml" path="//Member[@MemberName='RefreshTokenExpiresIn']/Docs" />
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

		/// <include file="../../docs/Microsoft.Maui.Essentials/WebAuthenticatorResult.xml" path="//Member[@MemberName='ExpiresIn']/Docs" />
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
