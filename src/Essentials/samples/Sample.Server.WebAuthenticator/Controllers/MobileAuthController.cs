using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Sample.Server.WebAuthenticator
{
	[Route("mobileauth")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		const string callbackScheme = "xamarinessentials";

		[HttpGet("{scheme}")]
		public async Task Get([FromRoute] string scheme)
		{
			var auth = await Request.HttpContext.AuthenticateAsync(scheme);

			if (!auth.Succeeded
				|| auth?.Principal == null
				|| !auth.Principal.Identities.Any(id => id.IsAuthenticated)
				|| string.IsNullOrEmpty(auth.Properties.GetTokenValue("access_token")))
			{
				// Not authenticated, challenge
				await Request.HttpContext.ChallengeAsync(scheme);
			}
			else
			{
				var claims = auth.Principal.Identities.FirstOrDefault()?.Claims;
				var email = string.Empty;
				email = claims?.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Email)?.Value;

				// Get parameters to send back to the callback
				var qs = new Dictionary<string, string>
				{
					{ "access_token", auth.Properties.GetTokenValue("access_token") },
					{ "refresh_token", auth.Properties.GetTokenValue("refresh_token") ?? string.Empty },
					{ "expires_in", (auth.Properties.ExpiresUtc?.ToUnixTimeSeconds() ?? -1).ToString() },
					{ "email", email },
					{ "code", auth.Properties.GetTokenValue("code") ?? Guid.NewGuid().ToString() },
					{ "state", auth.Properties.GetTokenValue("state") ?? string.Empty },
				};

				// Build the result url
				var url = callbackScheme + "://#" + string.Join(
					"&",
					qs.Where(kvp => !string.IsNullOrEmpty(kvp.Value) && kvp.Value != "-1")
					.Select(kvp => $"{WebUtility.UrlEncode(kvp.Key)}={WebUtility.UrlEncode(kvp.Value)}"));

				// Redirect to final url
				Request.HttpContext.Response.Redirect(url);
			}
		}
	}
}