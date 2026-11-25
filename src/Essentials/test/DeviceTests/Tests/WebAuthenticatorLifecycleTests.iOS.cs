#nullable enable
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.TestUtils.DeviceTests.Runners;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[CollectionDefinition("WebAuthenticatorLifecycle", DisableParallelization = true)]
	public class WebAuthenticatorLifecycleCollection
	{
	}

	[Collection("WebAuthenticatorLifecycle")]
	[Category("WebAuthenticator")]
	public class WebAuthenticatorLifecycleTests
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public Task DefaultHostRoutesApplicationAndSceneUserActivity(bool handled) =>
			Utils.OnMainThread(() =>
			{
				var services = TestServices.Services;
				var lifecycle = services.GetRequiredService<ILifecycleEventService>();
				var applicationHandler = Assert.Single(
					lifecycle.GetEventDelegates<iOSLifecycle.ContinueUserActivity>(
						nameof(iOSLifecycle.ContinueUserActivity)));
				var sceneHandler = Assert.Single(
					lifecycle.GetEventDelegates<iOSLifecycle.SceneContinueUserActivity>(
						nameof(iOSLifecycle.SceneContinueUserActivity)));
				var scene = GetConnectedWindowScene();

				var original = WebAuthenticator.Default;
				var authenticator = new RecordingWebAuthenticator(handled);
				using var activity = new NSUserActivity(NSUserActivityType.BrowsingWeb.ToString())
				{
					WebPageUrl = new NSUrl("https://example.com/callback?code=123")
				};

				WebAuthenticator.SetDefault(authenticator);
				try
				{
					var applicationResult = applicationHandler(
						UIApplication.SharedApplication,
						activity,
						_ => { });
					var sceneResult = sceneHandler(scene, activity);

					Assert.Equal(handled, applicationResult);
					Assert.Equal(handled, sceneResult);
					Assert.Equal(
						new[]
						{
							new Uri("https://example.com/callback?code=123"),
							new Uri("https://example.com/callback?code=123")
						},
						authenticator.ReceivedUris);
				}
				finally
				{
					WebAuthenticator.SetDefault(original);
				}
			});

		[Fact]
		public Task DefaultHostSceneUserActivityWithoutWebPageUrlIsUnhandled() =>
			Utils.OnMainThread(() =>
			{
				var lifecycle = TestServices.Services.GetRequiredService<ILifecycleEventService>();
				var sceneHandler = Assert.Single(
					lifecycle.GetEventDelegates<iOSLifecycle.SceneContinueUserActivity>(
						nameof(iOSLifecycle.SceneContinueUserActivity)));
				var scene = GetConnectedWindowScene();

				var original = WebAuthenticator.Default;
				var authenticator = new RecordingWebAuthenticator(true);
				using var activity = new NSUserActivity(NSUserActivityType.BrowsingWeb.ToString());

				WebAuthenticator.SetDefault(authenticator);
				try
				{
					Assert.False(sceneHandler(scene, activity));
					Assert.Empty(authenticator.ReceivedUris);
				}
				finally
				{
					WebAuthenticator.SetDefault(original);
				}
			});

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public Task DefaultHostRoutesApplicationUrlAndRegistersSceneBridge(bool handled) =>
			Utils.OnMainThread(() =>
			{
				var lifecycle = TestServices.Services.GetRequiredService<ILifecycleEventService>();
				var applicationHandler = Assert.Single(
					lifecycle.GetEventDelegates<iOSLifecycle.OpenUrl>(
						nameof(iOSLifecycle.OpenUrl)));
				var sceneHandler = Assert.Single(
					lifecycle.GetEventDelegates<iOSLifecycle.SceneOpenUrl>(
						nameof(iOSLifecycle.SceneOpenUrl)));
				var scene = GetConnectedWindowScene();
				var original = WebAuthenticator.Default;
				var authenticator = new RecordingWebAuthenticator(handled);
				using var url = new NSUrl("https://example.com/callback?code=123");
				using var options = new NSDictionary();
				using var contexts = new NSSet<UIOpenUrlContext>();

				WebAuthenticator.SetDefault(authenticator);
				try
				{
					Assert.Equal(
						handled,
						applicationHandler(UIApplication.SharedApplication, url, options));
					Assert.Equal(
						new[] { new Uri("https://example.com/callback?code=123") },
						authenticator.ReceivedUris);

					Assert.False(sceneHandler(scene, contexts));
					Assert.Single(authenticator.ReceivedUris);
				}
				finally
				{
					WebAuthenticator.SetDefault(original);
				}
			});

		static UIWindowScene GetConnectedWindowScene()
		{
			using var connectedScenes = UIApplication.SharedApplication.ConnectedScenes;
			var scene = connectedScenes.OfType<UIWindowScene>().FirstOrDefault();

			Assert.NotNull(scene);
			Assert.NotEqual(NativeHandle.Zero, scene.Handle);

			return scene;
		}

		sealed class RecordingWebAuthenticator : IWebAuthenticator, IPlatformWebAuthenticatorCallback
		{
			readonly bool _handled;

			public RecordingWebAuthenticator(bool handled)
			{
				_handled = handled;
			}

			public System.Collections.Generic.List<Uri> ReceivedUris { get; } = new();

			public Task<WebAuthenticatorResult> AuthenticateAsync(WebAuthenticatorOptions webAuthenticatorOptions) =>
				throw new NotSupportedException();

			public Task<WebAuthenticatorResult> AuthenticateAsync(
				WebAuthenticatorOptions webAuthenticatorOptions,
				CancellationToken cancellationToken) =>
				throw new NotSupportedException();

			public bool OpenUrlCallback(Uri uri)
			{
				ReceivedUris.Add(uri);
				return _handled;
			}
		}
	}
}
