#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category("WebAuthenticator")]
	public class WebAuthenticatorLifecycleTests
	{
		[Theory]
		[InlineData(false, false, false)]
		[InlineData(true, false, true)]
		[InlineData(false, true, true)]
		public void OpenWebAuthenticatorUrlsVisitsEveryUrlAndAggregatesResults(
			bool firstHandled,
			bool secondHandled,
			bool expected)
		{
			using var firstUrl = new NSUrl("https://example.com/callback?source=first");
			using var secondUrl = new NSUrl("https://example.com/callback?source=second");
			var authenticator = new RecordingWebAuthenticator(firstHandled, secondHandled);

			var actual = EssentialsExtensions.OpenWebAuthenticatorUrls(
				new[] { firstUrl, secondUrl },
				authenticator);

			Assert.Equal(expected, actual);
			Assert.Equal(
				new[]
				{
					new Uri("https://example.com/callback?source=first"),
					new Uri("https://example.com/callback?source=second")
				},
				authenticator.ReceivedUris);
		}

		[Fact]
		public void OpenWebAuthenticatorUrlsReturnsFalseForEmptySequence()
		{
			var authenticator = new RecordingWebAuthenticator();

			var actual = EssentialsExtensions.OpenWebAuthenticatorUrls(
				Array.Empty<NSUrl>(),
				authenticator);

			Assert.False(actual);
			Assert.Empty(authenticator.ReceivedUris);
		}

		sealed class RecordingWebAuthenticator : IWebAuthenticator, IPlatformWebAuthenticatorCallback
		{
			readonly Queue<bool> _results;

			public RecordingWebAuthenticator(params bool[] results)
			{
				_results = new Queue<bool>(results);
			}

			public List<Uri> ReceivedUris { get; } = new();

			public Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions) =>
				throw new NotSupportedException();

			public Task<WebAuthenticatorResult> AuthenticateAsync(
				WebAuthenticatorOptions webAuthenticatorOptions,
				CancellationToken cancellationToken) =>
				throw new NotSupportedException();

			public bool OpenUrlCallback(Uri uri)
			{
				ReceivedUris.Add(uri);
				return _results.Dequeue();
			}
		}
	}
}
