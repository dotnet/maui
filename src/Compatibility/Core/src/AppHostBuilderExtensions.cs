#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Maui.Platform;

#if ANDROID
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.Compatibility.Platform.Android.AppCompat;
using FrameRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers.FrameRenderer;
using LabelRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers.LabelRenderer;
using ImageRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers.ImageRenderer;
using ButtonRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.FastRenderers.ButtonRenderer;
using DefaultRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Android.Platform.DefaultRenderer;
#elif WINDOWS
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using BoxRenderer = Microsoft.Maui.Controls.Compatibility.Platform.UWP.BoxViewBorderRenderer;
using CellRenderer = Microsoft.Maui.Controls.Compatibility.Platform.UWP.TextCellRenderer;
using ResourcesProvider = Microsoft.Maui.Controls.Compatibility.Platform.UWP.WindowsResourcesProvider;
using StreamImagesourceHandler = Microsoft.Maui.Controls.Compatibility.Platform.UWP.StreamImageSourceHandler;
using ImageLoaderSourceHandler = Microsoft.Maui.Controls.Compatibility.Platform.UWP.UriImageSourceHandler;
using DefaultRenderer = Microsoft.Maui.Controls.Compatibility.Platform.UWP.DefaultRenderer;
#elif __IOS__
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using WebViewRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.WkWebViewRenderer;
using RadioButtonRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform.DefaultRenderer;
using DefaultRenderer = Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform.DefaultRenderer;
#elif TIZEN
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;
using Microsoft.Maui.Graphics.Skia;
using StreamImagesourceHandler = Microsoft.Maui.Controls.Compatibility.Platform.Tizen.StreamImageSourceHandler;
using ImageLoaderSourceHandler = Microsoft.Maui.Controls.Compatibility.Platform.Tizen.UriImageSourceHandler;
using DefaultRenderer = Microsoft.Maui.Controls.Compatibility.Platform.Tizen.DefaultRenderer;
#endif

namespace Microsoft.Maui.Controls.Compatibility.Hosting
{
	public static class MauiAppBuilderExtensions
	{
		internal const string UseMapperInstead = "This renderer is obsolete please try to use the corresponding handler/mapper";

		internal static void CheckForCompatibility([CallerMemberName] string memberName = "")
		{
			Controls.Hosting.CompatibilityCheck.CheckForCompatibility(memberName);
		}

		internal static void ResetCompatibilityCheck()
		{
			Controls.Hosting.CompatibilityCheck.ResetCompatibilityCheck();
		}

		[Obsolete("The compatibility functionality is deprecated. This will be removed in the future, please make sure to migrate to handlers. " +
		"You might already not be using any of the compatibility functionality anymore, in that case please remove this call, and the reference " +
		"to the Compatibility NuGet package, as it will introduce unnecessary overhead that negatively impacts performance. " +
		"Learn how to transition off of the Compatibility packages on Microsoft Learn: https://aka.ms/maui/renderer-to-handler")]
		public static MauiAppBuilder UseMauiCompatibility(this MauiAppBuilder builder)
		{
			Controls.Hosting.CompatibilityCheck.UseCompatibility();

#if PLATFORM
			// initialize compatibility DependencyService
			DependencyService.SetToInitialized();
			DependencyService.Register<NativeBindingService>();
			DependencyService.Register<NativeValueConverterService>();
#endif

			builder.ConfigureCompatibilityLifecycleEvents();
			builder
				.ConfigureMauiHandlers(handlers =>
				{
#if PLATFORM

#if !WINDOWS
#if !(MACCATALYST || MACOS)
#endif
#else
#pragma warning disable CS0618 // Type or member is obsolete
					handlers.TryAddCompatibilityRenderer(typeof(Layout), typeof(LayoutRenderer));
#pragma warning restore CS0618 // Type or member is obsolete
#endif
					handlers.TryAddCompatibilityRenderer(typeof(NativeViewWrapper), typeof(NativeViewWrapperRenderer));

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
					handlers.TryAddCompatibilityRenderer(typeof(Microsoft.Maui.Controls.Compatibility.Layout<View>), typeof(DefaultRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Microsoft.Maui.Controls.Compatibility.RelativeLayout), typeof(DefaultRenderer));
					handlers.TryAddCompatibilityRenderer(typeof(Microsoft.Maui.Controls.Compatibility.AbsoluteLayout), typeof(DefaultRenderer));
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0612 // Type or member is obsolete

					// Shimmed renderers go directly to the registrar to load Image Handlers
					Internals.Registrar.Registered.Register(typeof(FileImageSource), typeof(FileImageSourceHandler));
					Internals.Registrar.Registered.Register(typeof(StreamImageSource), typeof(StreamImagesourceHandler));
					Internals.Registrar.Registered.Register(typeof(UriImageSource), typeof(ImageLoaderSourceHandler));
					Internals.Registrar.Registered.Register(typeof(FontImageSource), typeof(FontImageSourceHandler));
					Internals.Registrar.Registered.Register(typeof(Microsoft.Maui.EmbeddedFont), typeof(Microsoft.Maui.EmbeddedFontLoader));
#endif

#if IOS || MACCATALYST
					Internals.Registrar.RegisterEffect("Xamarin", "ShadowEffect", typeof(ShadowEffect));
#endif
				});

#if WINDOWS
			builder.AddMauiCompat();
#endif

			return builder;
		}

		static MauiAppBuilder AddMauiCompat(this MauiAppBuilder builder)
		{
			builder.Services.TryAddEnumerable(ServiceDescriptor.Transient<IMauiInitializeService, MauiCompatInitializer>());
			return builder;
		}

		class MauiCompatInitializer : IMauiInitializeService
		{
			public void Initialize(IServiceProvider services)
			{
#if WINDOWS
				var dispatcher =
					services.GetService<IDispatcher>() ??
					IPlatformApplication.Current?.Services.GetRequiredService<IDispatcher>() ??
					throw new InvalidOperationException("Unable to find Application Services");

					dispatcher.DispatchIfRequired(() =>
					{
						var dictionaries = UI.Xaml.Application.Current?.Resources?.MergedDictionaries;
						if (UI.Xaml.Application.Current?.Resources != null && dictionaries != null)
						{
							// Microsoft.Maui.Controls.Compatibility
							UI.Xaml.Application.Current.Resources.AddLibraryResources("MicrosoftMauiControlsCompatibilityIncluded", "ms-appx:///Microsoft.Maui.Controls.Compatibility/Windows/Resources.xbf");
						}
					});
#endif
			}
		}
	}
}
