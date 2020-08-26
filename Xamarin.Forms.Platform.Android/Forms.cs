using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Util;
using Android.Views;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Android;
using Resource = Android.Resource;
using Trace = System.Diagnostics.Trace;
using System.ComponentModel;
using AColor = Android.Graphics.Color;
#if __ANDROID_29__
using AndroidX.Core.Content;
#else
using Android.Support.V4.Content;
#endif

namespace Xamarin.Forms
{
	public struct InitializationOptions
	{
		public struct EffectScope
		{
			public string Name;
			public ExportEffectAttribute[] Effects;
		}

		public InitializationOptions(Context activity, Bundle bundle, Assembly resourceAssembly)
		{
			this = default(InitializationOptions);
			Activity = activity;
			Bundle = bundle;
			ResourceAssembly = resourceAssembly;
		}
		public Context Activity;
		public Bundle Bundle;
		public Assembly ResourceAssembly;
		public HandlerAttribute[] Handlers;
		public EffectScope[] EffectScopes;
		public InitializationFlags Flags;
	}

	public  static partial class Forms
	{
		const int TabletCrossover = 600;

		[Obsolete("Context is obsolete as of version 2.5. Please use a local context instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Context Context { get; internal set; }

		// One per process; does not change, suitable for loading resources (e.g., ResourceProvider)
		internal static Context ApplicationContext { get; private set; }

		public static bool IsInitialized { get; private set; }
		static bool FlagsSet { get; set; }

		static bool _ColorButtonNormalSet;
		static Color _ColorButtonNormal = Color.Default;
		public static Color ColorButtonNormalOverride { get; set; }

		

		public static float GetFontSizeNormal(Context context)
		{
			float size = 50;
			if (!IsLollipopOrNewer)
				return size;

			// Android 5.0+
			//this doesn't seem to work
			using (var value = new TypedValue())
			{
				if (context.Theme.ResolveAttribute(Resource.Attribute.TextSize, value, true))
				{
					size = value.Data;
				}
			}

			return size;
		}

		public static Color GetColorButtonNormal(Context context)
		{
			if (!_ColorButtonNormalSet)
			{
				_ColorButtonNormal = GetButtonColor(context);
				_ColorButtonNormalSet = true;
			}

			return _ColorButtonNormal;
		}

		// Provide backwards compat for Forms.Init and AndroidActivity
		// Why is bundle a param if never used?
		public static void Init(Context activity, Bundle bundle)
		{
			Assembly resourceAssembly;

			Profile.FrameBegin("Assembly.GetCallingAssembly");
			resourceAssembly = Assembly.GetCallingAssembly();
			Profile.FrameEnd("Assembly.GetCallingAssembly");

			Profile.FrameBegin();
			SetupInit(activity, resourceAssembly, null);
			Profile.FrameEnd();
		}

		public static void Init(Context activity, Bundle bundle, Assembly resourceAssembly)
		{
			Profile.FrameBegin();
			SetupInit(activity, resourceAssembly, null);
			Profile.FrameEnd();
		}

		public static void Init(InitializationOptions options)
		{
			Profile.FrameBegin();
			SetupInit(
				options.Activity,
				options.ResourceAssembly,
				options
			);
			Profile.FrameEnd();
		}

		/// <summary>
		/// Sets title bar visibility programmatically. Must be called after Xamarin.Forms.Forms.Init() method
		/// </summary>
		/// <param name="visibility">Title bar visibility enum</param>
		[Obsolete("SetTitleBarVisibility(AndroidTitleBarVisibility) is obsolete as of version 2.5. "
			+ "Please use SetTitleBarVisibility(Activity, AndroidTitleBarVisibility) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetTitleBarVisibility(AndroidTitleBarVisibility visibility)
		{
			if (Context.GetActivity() == null)
				throw new NullReferenceException("Must be called after Xamarin.Forms.Forms.Init() method");

			if (visibility == AndroidTitleBarVisibility.Never)
			{
				if (!Context.GetActivity().Window.Attributes.Flags.HasFlag(WindowManagerFlags.Fullscreen))
					Context.GetActivity().Window.AddFlags(WindowManagerFlags.Fullscreen);
			}
			else
			{
				if (Context.GetActivity().Window.Attributes.Flags.HasFlag(WindowManagerFlags.Fullscreen))
					Context.GetActivity().Window.ClearFlags(WindowManagerFlags.Fullscreen);
			}
		}

		public static void SetTitleBarVisibility(Activity activity, AndroidTitleBarVisibility visibility)
		{
			if (visibility == AndroidTitleBarVisibility.Never)
			{
				if (!activity.Window.Attributes.Flags.HasFlag(WindowManagerFlags.Fullscreen))
					activity.Window.AddFlags(WindowManagerFlags.Fullscreen);
			}
			else
			{
				if (activity.Window.Attributes.Flags.HasFlag(WindowManagerFlags.Fullscreen))
					activity.Window.ClearFlags(WindowManagerFlags.Fullscreen);
			}
		}

		public static event EventHandler<ViewInitializedEventArgs> ViewInitialized;

		internal static void SendViewInitialized(this VisualElement self, global::Android.Views.View nativeView)
		{
			EventHandler<ViewInitializedEventArgs> viewInitialized = ViewInitialized;
			if (viewInitialized != null)
				viewInitialized(self, new ViewInitializedEventArgs { View = self, NativeView = nativeView });
		}

		static void SetupInit(
			Context activity,
			Assembly resourceAssembly,
			InitializationOptions? maybeOptions = null
		)
		{
			Profile.FrameBegin();

			if (!IsInitialized)
			{
				// Only need to get this once; it won't change
				ApplicationContext = activity.ApplicationContext;
			}

#pragma warning disable 618 // Still have to set this up so obsolete code can function
			Context = activity;
#pragma warning restore 618

			if (!IsInitialized)
			{
				// Only need to do this once
				Profile.FramePartition("ResourceManager.Init");
				ResourceManager.Init(resourceAssembly);
			}

			Profile.FramePartition("Color.SetAccent()");
			// We want this to be updated when we have a new activity (e.g. on a configuration change)
			// This could change if the UI mode changes (e.g., if night mode is enabled)
			Color.SetAccent(GetAccentColor(activity));
			_ColorButtonNormalSet = false;

			if (!IsInitialized)
			{
				// Only need to do this once
				Profile.FramePartition("Log.Listeners");
				Internals.Log.Listeners.Add(new DelegateLogListener((c, m) => Trace.WriteLine(m, c)));
			}

			// We want this to be updated when we have a new activity (e.g. on a configuration change)
			// because AndroidPlatformServices needs a current activity to launch URIs from
			Profile.FramePartition("Device.PlatformServices");
			Device.PlatformServices = new AndroidPlatformServices(activity);

			// use field and not property to avoid exception in getter
			if (Device.info != null)
			{
				((AndroidDeviceInfo)Device.info).Dispose();
				Device.info = null;
			}

			// We want this to be updated when we have a new activity (e.g. on a configuration change)
			// because Device.Info watches for orientation changes and we need a current activity for that
			Profile.FramePartition("create AndroidDeviceInfo");
			Device.Info = new AndroidDeviceInfo(activity);

			Profile.FramePartition("setFlags");
			Device.SetFlags(s_flags);

			Profile.FramePartition("AndroidTicker");
			Ticker.SetDefault(null);

			Profile.FramePartition("RegisterAll");

			if (!IsInitialized)
			{
				if (maybeOptions.HasValue)
				{
					var options = maybeOptions.Value;
					var handlers = options.Handlers;
					var flags = options.Flags;
					var effectScopes = options.EffectScopes;

					//TODO: ExportCell?
					//TODO: ExportFont

					// renderers
					Registrar.RegisterRenderers(handlers);

					// effects
					if (effectScopes != null)
					{
						for (var i = 0; i < effectScopes.Length; i++)
						{
							var effectScope = effectScopes[0];
							Registrar.RegisterEffects(effectScope.Name, effectScope.Effects);
						}
					}

					// css
					Registrar.RegisterStylesheets(flags);
				}
				else
				{
					// Only need to do this once
					Registrar.RegisterAll(new[] {
						typeof(ExportRendererAttribute),
						typeof(ExportCellAttribute),
						typeof(ExportImageSourceHandlerAttribute),
						typeof(ExportFontAttribute)
					});
				}
			}

			Profile.FramePartition("Epilog");

			var currentIdiom = TargetIdiom.Unsupported;

			// first try UIModeManager
			using (var uiModeManager = UiModeManager.FromContext(ApplicationContext))
			{
				try
				{
					var uiMode = uiModeManager?.CurrentModeType ?? UiMode.TypeUndefined;
					currentIdiom = DetectIdiom(uiMode);
				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine($"Unable to detect using UiModeManager: {ex.Message}");
				}
			}

			if (TargetIdiom.Unsupported == currentIdiom)
			{
				// This could change as a result of a config change, so we need to check it every time
				int minWidthDp = activity.Resources.Configuration.SmallestScreenWidthDp;
				Device.SetIdiom(minWidthDp >= TabletCrossover ? TargetIdiom.Tablet : TargetIdiom.Phone);
			}

			if (SdkInt >= BuildVersionCodes.JellyBeanMr1)
				Device.SetFlowDirection(activity.Resources.Configuration.LayoutDirection.ToFlowDirection());

			if (ExpressionSearch.Default == null)
				ExpressionSearch.Default = new AndroidExpressionSearch();

			IsInitialized = true;
			Profile.FrameEnd();
		}

		static TargetIdiom DetectIdiom(UiMode uiMode)
		{
			var returnValue = TargetIdiom.Unsupported;
			if (uiMode.HasFlag(UiMode.TypeNormal))
				returnValue = TargetIdiom.Unsupported;
			else if (uiMode.HasFlag(UiMode.TypeTelevision))
				returnValue = TargetIdiom.TV;
			else if (uiMode.HasFlag(UiMode.TypeDesk))
				returnValue = TargetIdiom.Desktop;
			else if (SdkInt >= BuildVersionCodes.KitkatWatch && uiMode.HasFlag(UiMode.TypeWatch))
				returnValue = TargetIdiom.Watch;

			Device.SetIdiom(returnValue);
			return returnValue;
		}

		static IReadOnlyList<string> s_flags;
		public static IReadOnlyList<string> Flags => s_flags ?? (s_flags = new string[0]);

		public static void SetFlags(params string[] flags)
		{
			if (FlagsSet)
			{
				// Don't try to set the flags again if they've already been set
				// (e.g., during a configuration change where OnCreate runs again)
				return;
			}

			if (IsInitialized)
			{
				throw new InvalidOperationException($"{nameof(SetFlags)} must be called before {nameof(Init)}");
			}

			s_flags = (string[])flags.Clone();
			if (s_flags.Contains ("Profile"))
				Profile.Enable();
			FlagsSet = true;
		}

		static Color GetAccentColor(Context context)
		{
			Color rc;
			using (var value = new TypedValue())
			{
				if (context.Theme.ResolveAttribute(global::Android.Resource.Attribute.ColorAccent, value, true) && Forms.IsLollipopOrNewer) // Android 5.0+
				{
					rc = Color.FromUint((uint)value.Data);
				}
				else if (context.Theme.ResolveAttribute(context.Resources.GetIdentifier("colorAccent", "attr", context.PackageName), value, true))  // < Android 5.0
				{
					rc = Color.FromUint((uint)value.Data);
				}
				else                    // fallback to old code if nothing works (don't know if that ever happens)
				{
					// Detect if legacy device and use appropriate accent color
					// Hardcoded because could not get color from the theme drawable
					var sdkVersion = (int)SdkInt;
					if (sdkVersion <= 10)
					{
						// legacy theme button pressed color
						rc = Color.FromHex("#fffeaa0c");
					}
					else
					{
						// Holo dark light blue
						rc = Color.FromHex("#ff33b5e5");
					}
				}
			}
			return rc;
		}

		static Color GetButtonColor(Context context)
		{
			Color rc = ColorButtonNormalOverride;

			if (ColorButtonNormalOverride == Color.Default)
			{
				using (var value = new TypedValue())
				{
					if (context.Theme.ResolveAttribute(global::Android.Resource.Attribute.ColorButtonNormal, value, true) && Forms.IsLollipopOrNewer) // Android 5.0+
					{
						rc = Color.FromUint((uint)value.Data);
					}
					else if (context.Theme.ResolveAttribute(context.Resources.GetIdentifier("colorButtonNormal", "attr", context.PackageName), value, true))  // < Android 5.0
					{
						rc = Color.FromUint((uint)value.Data);
					}
				}
			}
			return rc;
		}

		class AndroidDeviceInfo : DeviceInfo
		{
			bool _disposed;
			readonly Context _formsActivity;
			Size _scaledScreenSize;
			Size _pixelScreenSize;
			double _scalingFactor;

			Orientation _previousOrientation = Orientation.Undefined;
			Platform.Android.DualScreen.IDualScreenService DualScreenService => DependencyService.Get<Platform.Android.DualScreen.IDualScreenService>();

			public AndroidDeviceInfo(Context formsActivity)
			{
				CheckOrientationChanged(formsActivity);

				// This will not be an implementation of IDeviceInfoProvider when running inside the context
				// of layoutlib, which is what the Android Designer does.
				// It also won't be IDeviceInfoProvider when using Page Embedding
				if (formsActivity is IDeviceInfoProvider)
				{
					_formsActivity = formsActivity;
					((IDeviceInfoProvider)_formsActivity).ConfigurationChanged += ConfigurationChanged;
				}
			}

			public override Size PixelScreenSize
			{
				get { return _pixelScreenSize; }
			}

			public override Size ScaledScreenSize => _scaledScreenSize;

			public override double ScalingFactor
			{
				get { return _scalingFactor; }
			}


			public override double DisplayRound(double value) =>
				Math.Round(ScalingFactor * value) / ScalingFactor;

			protected override void Dispose(bool disposing)
			{
				if (_disposed)
				{
					return;
				}

				_disposed = true;

				if (disposing)
				{
					var provider = _formsActivity as IDeviceInfoProvider;
					if (provider != null)
						provider.ConfigurationChanged -= ConfigurationChanged;
				}

				base.Dispose(disposing);
			}

			void UpdateScreenMetrics(Context formsActivity)
			{
				using (DisplayMetrics display = formsActivity.Resources.DisplayMetrics)
				{
					_scalingFactor = display.Density;
					_pixelScreenSize = new Size(display.WidthPixels, display.HeightPixels);
					_scaledScreenSize = new Size(_pixelScreenSize.Width / _scalingFactor, _pixelScreenSize.Height / _scalingFactor);
				}
			}

			void CheckOrientationChanged(Context formsActivity)
			{
				Orientation orientation;

				if (DualScreenService?.IsSpanned == true)
				{
					orientation = (DualScreenService.IsLandscape) ? Orientation.Landscape : Orientation.Portrait;
				}
				else
				{
					orientation = formsActivity.Resources.Configuration.Orientation;
				}

				if (!_previousOrientation.Equals(orientation))
					CurrentOrientation = orientation.ToDeviceOrientation();

				_previousOrientation = orientation;

				UpdateScreenMetrics(formsActivity);
			}

			void ConfigurationChanged(object sender, EventArgs e)
			{
				CheckOrientationChanged(_formsActivity);
			}
		}

		class AndroidExpressionSearch : ExpressionVisitor, IExpressionSearch
		{
			List<object> _results;
			Type _targetType;

			public List<T> FindObjects<T>(Expression expression) where T : class
			{
				_results = new List<object>();
				_targetType = typeof(T);
				Visit(expression);
				return _results.Select(o => o as T).ToList();
			}

			protected override Expression VisitMember(MemberExpression node)
			{
				if (node.Expression is ConstantExpression && node.Member is FieldInfo)
				{
					object container = ((ConstantExpression)node.Expression).Value;
					object value = ((FieldInfo)node.Member).GetValue(container);

					if (_targetType.IsInstanceOfType(value))
						_results.Add(value);
				}
				return base.VisitMember(node);
			}
		}
	}
}
