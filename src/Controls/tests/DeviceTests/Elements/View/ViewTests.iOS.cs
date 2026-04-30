using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ViewTests
	{
		[Fact]
		public async Task GestureRecognizersAttachToPlatformViewWhenNoContainerViewIsPresent()
		{
			var view = new Label()
			{
				GestureRecognizers = { new TapGestureRecognizer() }
			};
			(view as IWindowController).Window = new Window();

			var handler = await CreateHandlerAsync<LabelHandler>(view);

			var gestures = await InvokeOnMainThreadAsync(() =>
			{
				return new
				{
					PlatformView = GetGestureRecognizerTypes(handler.PlatformView),
					ContainerView = GetGestureRecognizerTypes(handler.ContainerView),
				};
			});

			Assert.Empty(gestures.ContainerView);
			Assert.NotEmpty(gestures.PlatformView);

			Assert.Contains(typeof(GesturePlatformManager.MauiUITapGestureRecognizer), gestures.PlatformView);
		}

		[Fact]
		public async Task GestureRecognizersAttachToContainerViewWhenUsingContainerView()
		{
			var view = new Label()
			{
				Shadow = new Shadow(), // this results in a container view
				GestureRecognizers = { new TapGestureRecognizer() }
			};

			(view as IWindowController).Window = new Window();

			var handler = await CreateHandlerAsync<LabelHandler>(view);

			var gestures = await InvokeOnMainThreadAsync(() =>
			{
				return new
				{
					PlatformView = GetGestureRecognizerTypes(handler.PlatformView),
					ContainerView = GetGestureRecognizerTypes(handler.ContainerView),
				};
			});

			Assert.NotEmpty(gestures.ContainerView);
			Assert.Empty(gestures.PlatformView);

			Assert.Contains(typeof(GesturePlatformManager.MauiUITapGestureRecognizer), gestures.ContainerView);	
		}

		static Type[] GetGestureRecognizerTypes(UIView view) =>
			view?.GestureRecognizers?.Select(g => g.GetType()).ToArray() ?? Array.Empty<Type>();
	}
}
