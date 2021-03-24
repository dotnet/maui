using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Util;
using Android.Views;
using AndroidX.Core.Content;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
using Microsoft.Maui.Controls.DualScreen.Android;
using Microsoft.Maui.Controls.Internals;
using AColor = Android.Graphics.Color;
using AndroidResource = Android.Resource;
using Trace = System.Diagnostics.Trace;

namespace Microsoft.Maui.Controls.Compatibility
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

	public static class Forms
	{
		const int TabletCrossover = 600;

		static BuildVersionCodes? s_sdkInt;
		static bool? s_is29OrNewer;
		static bool? s_isJellyBeanMr1OrNewer;
		static bool? s_isLollipopOrNewer;
		static bool? s_isMarshmallowOrNewer;
		static bool? s_isNougatOrNewer;
		static bool? s_isOreoOrNewer;
		static bool? s_isPieOrNewer;
		static FontManager s_fontManager;

		// One per process; does not change, suitable for loading resources (e.g., ResourceProvider)
		internal static Context ApplicationContext { get; private set; } = global::Android.App.Application.Context;
		internal static IMauiContext MauiContext { get; private set; }

		public static bool IsInitialized { get; private set; }
		static bool FlagsSet { get; set; }

		static bool _ColorButtonNormalSet;
		static Color _ColorButtonNormal = Color.Default;
		public static Color ColorButtonNormalOverride { get; set; }

		internal static BuildVersionCodes SdkInt
		{
			get
			{
				if (!s_sdkInt.HasValue)
					s_sdkInt = Build.VERSION.SdkInt;
				return (BuildVersionCodes)s_sdkInt;
			}
		}

		internal static bool Is29OrNewer
		{
			get
			{
				if (!s_is29OrNewer.HasValue)
					s_is29OrNewer = (int)SdkInt >= 29;
				return s_is29OrNewer.Value;
			}
		}

		internal static bool IsJellyBeanMr1OrNewer
		{
			get
			{
				if (!s_isJellyBeanMr1OrNewer.HasValue)
					s_isJellyBeanMr1OrNewer = SdkInt >= BuildVersionCodes.JellyBeanMr1;
				return s_isJellyBeanMr1OrNewer.Value;
			}
		}

		internal static bool IsLollipopOrNewer
		{
			get
			{
				if (!s_isLollipopOrNewer.HasValue)
					s_isLollipopOrNewer = SdkInt >= BuildVersionCodes.Lollipop;
				return s_isLollipopOrNewer.Value;
			}
		}

		internal static bool IsMarshmallowOrNewer
		{
			get
			{
				if (!s_isMarshmallowOrNewer.HasValue)
					s_isMarshmallowOrNewer = SdkInt >= BuildVersionCodes.M;
				return s_isMarshmallowOrNewer.Value;
			}
		}

		internal static bool IsNougatOrNewer
		{
			get
			{
				if (!s_isNougatOrNewer.HasValue)
					s_isNougatOrNewer = SdkInt >= BuildVersionCodes.N;
				return s_isNougatOrNewer.Value;
			}
		}

		internal static bool IsOreoOrNewer
		{
			get
			{
				if (!s_isOreoOrNewer.HasValue)
					s_isOreoOrNewer = SdkInt >= BuildVersionCodes.O;
				return s_isOreoOrNewer.Value;
			}
		}

		internal static bool IsPieOrNewer
		{
			get
			{
				if (!s_isPieOrNewer.HasValue)
					s_isPieOrNewer = SdkInt >= BuildVersionCodes.P;
				return s_isPieOrNewer.Value;
			}
		}

		internal static IFontManager FontManager =>
			s_fontManager ??= new FontManager(Registrar.FontRegistrar);

		public static float GetFontSizeNormal(Context context)
		{
			float size = 50;
			if (!IsLollipopOrNewer)
				return size;

			// Android 5.0+
			//this doesn't seem to work
			using (var value = new TypedValue())
			{
				if (context.Theme.ResolveAttribute(AndroidResource.Attribute.TextSize, value, true))
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

		public static void Init(IActivationState activationState) =>
			Init(activationState.Context, activationState.SavedInstance);

		// Provide backwards compat for Forms.Init and AndroidActivity
		// Why is bundle a param if never used?
		public static void Init(Context activity, Bundle bundle) =>
			Init(new MauiContext(activity), bundle);

		public static void Init(IMauiContext context, Bundle bundle)
		{
			Assembly resourceAssembly;

			Profile.FrameBegin("Assembly.GetCallingAssembly");
			resourceAssembly = Assembly.GetCallingAssembly();
			Profile.FrameEnd("Assembly.GetCallingAssembly");

			Profile.FrameBegin();
			SetupInit(context, resourceAssembly, null);
			Profile.FrameEnd();
		}

		public static void Init(Context activity, Bundle bundle, Assembly resourceAssembly) =>
			Init(new MauiContext(activity), bundle, resourceAssembly);

		public static void Init(IMauiContext context, Bundle bundle, Assembly resourceAssembly)
		{
			Profile.FrameBegin();
			SetupInit(context, resourceAssembly, null);
			Profile.FrameEnd();
		}

		public static void Init(InitializationOptions options)
		{
			Profile.FrameBegin();
			SetupInit(
				new MauiContext(options.Activity),
				options.ResourceAssembly,
				options
			);
			Profile.FrameEnd();
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

		static bool IsInitializedRenderers;


		internal static void RegisterCompatRenderers(
			Assembly[] assemblies,
			Assembly defaultRendererAssembly,
			Action<Type> viewRegistered)
		{
			if (IsInitializedRenderers)
				return;

			IsInitializedRenderers = true;

			// Only need to do this once
			Registrar.RegisterAll(
				assemblies,
				defaultRendererAssembly,
				new[] {
						typeof(ExportRendererAttribute),
						typeof(ExportCellAttribute),
						typeof(ExportImageSourceHandlerAttribute),
						typeof(ExportFontAttribute)
					}, default(InitializationFlags),
				viewRegistered);
		}

		internal static void RegisterCompatRenderers(InitializationOptions? maybeOptions)
		{
			if (!IsInitializedRenderers)
			{
				IsInitializedRenderers = true;
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
		}

		static void SetupInit(
			IMauiContext context,
			Assembly resourceAssembly,
			InitializationOptions? maybeOptions = null
		)
		{
			var activity = context.Context;
			Profile.FrameBegin();
			Registrar.RegisterRendererToHandlerShim(RendererToHandlerShim.CreateShim);

			// Allow this multiple times to support the app and then the activity
			ApplicationContext = activity.ApplicationContext;
			MauiContext = context;

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

			var androidServices = new AndroidPlatformServices(activity);

			Device.PlatformServices = androidServices;
			Device.PlatformInvalidator = androidServices;

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

			if (maybeOptions?.Flags.HasFlag(InitializationFlags.SkipRenderers) != true)
				RegisterCompatRenderers(maybeOptions);

			Profile.FramePartition("Epilog");

			var currentIdiom = TargetIdiom.Unsupported;

			// First try UIModeManager
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

			// Then try Configuration
			if (TargetIdiom.Unsupported == currentIdiom)
			{
				var configuration = activity.Resources.Configuration;

				if (configuration != null)
				{
					var minWidth = configuration.SmallestScreenWidthDp;
					var isWide = minWidth >= TabletCrossover;
					currentIdiom = isWide ? TargetIdiom.Tablet : TargetIdiom.Phone;
				}
				else
				{
					// Start clutching at straws
					var metrics = activity.Resources?.DisplayMetrics;

					if (metrics != null)
					{
						var minSize = Math.Min(metrics.WidthPixels, metrics.HeightPixels);
						var isWide = minSize * metrics.Density >= TabletCrossover;
						currentIdiom = isWide ? TargetIdiom.Tablet : TargetIdiom.Phone;
					}
				}
			}

			Device.SetIdiom(currentIdiom);

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
			if (uiMode == UiMode.TypeNormal)
				returnValue = TargetIdiom.Unsupported;
			else if (uiMode == UiMode.TypeTelevision)
				returnValue = TargetIdiom.TV;
			else if (uiMode == UiMode.TypeDesk)
				returnValue = TargetIdiom.Desktop;
			else if (SdkInt >= BuildVersionCodes.KitkatWatch && uiMode == UiMode.TypeWatch)
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
			if (s_flags.Contains("Profile"))
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
			IDualScreenService DualScreenService => DependencyService.Get<IDualScreenService>();

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

		class AndroidPlatformServices : IPlatformServices, IPlatformInvalidate
		{
			double _buttonDefaultSize;
			double _editTextDefaultSize;
			double _labelDefaultSize;
			double _largeSize;
			double _mediumSize;

			double _microSize;
			double _smallSize;

			static Handler s_handler;

			readonly Context _context;

			public AndroidPlatformServices(Context context)
			{
				_context = context;
			}

			public void BeginInvokeOnMainThread(Action action)
			{
				if (_context.IsDesignerContext())
				{
					action();
					return;
				}

				if (s_handler == null || s_handler.Looper != Looper.MainLooper)
				{
					s_handler = new Handler(Looper.MainLooper);
				}

				s_handler.Post(action);
			}

			public Ticker CreateTicker()
			{
				return new AndroidTicker();
			}

			public Assembly[] GetAssemblies()
			{
				return AppDomain.CurrentDomain.GetAssemblies();
			}

			public string GetHash(string input) => Crc64.GetHash(input);

			string IPlatformServices.GetMD5Hash(string input) => GetHash(input);

			public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
			{
				if (_smallSize == 0)
				{
					_smallSize = ConvertTextAppearanceToSize(AndroidResource.Attribute.TextAppearanceSmall, AndroidResource.Style.TextAppearanceDeviceDefaultSmall, 12);
					_mediumSize = ConvertTextAppearanceToSize(AndroidResource.Attribute.TextAppearanceMedium, AndroidResource.Style.TextAppearanceDeviceDefaultMedium, 14);
					_largeSize = ConvertTextAppearanceToSize(AndroidResource.Attribute.TextAppearanceLarge, AndroidResource.Style.TextAppearanceDeviceDefaultLarge, 18);
					_buttonDefaultSize = ConvertTextAppearanceToSize(AndroidResource.Attribute.TextAppearanceButton, AndroidResource.Style.TextAppearanceDeviceDefaultWidgetButton, 14);
					_editTextDefaultSize = ConvertTextAppearanceToSize(AndroidResource.Style.TextAppearanceWidgetEditText, AndroidResource.Style.TextAppearanceDeviceDefaultWidgetEditText, 18);
					_labelDefaultSize = _smallSize;
					// as decreed by the android docs, ALL HAIL THE ANDROID DOCS, ALL GLORY TO THE DOCS, PRAISE HYPNOTOAD
					_microSize = Math.Max(1, _smallSize - (_mediumSize - _smallSize));
				}

				if (useOldSizes)
				{
					switch (size)
					{
						case NamedSize.Default:
							if (typeof(Button).IsAssignableFrom(targetElementType))
								return _buttonDefaultSize;
							if (typeof(Label).IsAssignableFrom(targetElementType))
								return _labelDefaultSize;
							if (typeof(Editor).IsAssignableFrom(targetElementType) || typeof(Entry).IsAssignableFrom(targetElementType) || typeof(SearchBar).IsAssignableFrom(targetElementType))
								return _editTextDefaultSize;
							return 14;
						case NamedSize.Micro:
							return 10;
						case NamedSize.Small:
							return 12;
						case NamedSize.Medium:
							return 14;
						case NamedSize.Large:
							return 18;
						case NamedSize.Body:
							return 16;
						case NamedSize.Caption:
							return 12;
						case NamedSize.Header:
							return 96;
						case NamedSize.Subtitle:
							return 16;
						case NamedSize.Title:
							return 24;
						default:
							throw new ArgumentOutOfRangeException("size");
					}
				}
				switch (size)
				{
					case NamedSize.Default:
						if (typeof(Button).IsAssignableFrom(targetElementType))
							return _buttonDefaultSize;
						if (typeof(Label).IsAssignableFrom(targetElementType))
							return _labelDefaultSize;
						if (typeof(Editor).IsAssignableFrom(targetElementType) || typeof(Entry).IsAssignableFrom(targetElementType))
							return _editTextDefaultSize;
						return _mediumSize;
					case NamedSize.Micro:
						return _microSize;
					case NamedSize.Small:
						return _smallSize;
					case NamedSize.Medium:
						return _mediumSize;
					case NamedSize.Large:
						return _largeSize;
					case NamedSize.Body:
						return 16;
					case NamedSize.Caption:
						return 12;
					case NamedSize.Header:
						return 96;
					case NamedSize.Subtitle:
						return 16;
					case NamedSize.Title:
						return 24;
					default:
						throw new ArgumentOutOfRangeException("size");
				}
			}

			public Color GetNamedColor(string name)
			{
				int color;
				switch (name)
				{
					case NamedPlatformColor.BackgroundDark:
						color = ContextCompat.GetColor(_context, AndroidResource.Color.BackgroundDark);
						break;
					case NamedPlatformColor.BackgroundLight:
						color = ContextCompat.GetColor(_context, AndroidResource.Color.BackgroundLight);
						break;
					case NamedPlatformColor.Black:
						color = ContextCompat.GetColor(_context, AndroidResource.Color.Black);
						break;
					case NamedPlatformColor.DarkerGray:
						color = ContextCompat.GetColor(_context, AndroidResource.Color.DarkerGray);
						break;
					case NamedPlatformColor.HoloBlueBright:
						color = ContextCompat.GetColor(_context, AndroidResource.Color.HoloBlueBright);
						break;
					case NamedPlatformColor.HoloBlueDark:
						color = ContextCompat.GetColor(_context, AndroidResource.Color.HoloBlueDark);
						break;
					case NamedPlatformColor.HoloBlueLight:
						color = ContextCompat.GetColor(_context, AndroidResource.Color.HoloBlueLight);
						break;
					case NamedPlatformColor.HoloGreenDark:
						color = ContextCompat.GetColor(_context, AndroidResource.Color.HoloGreenDark);
						break;
					case NamedPlatformColor.HoloGreenLight:
						color = ContextCompat.GetColor(_context, AndroidResource.Color.HoloGreenLight);
						break;
					case NamedPlatformColor.HoloOrangeDark:
						color = ContextCompat.GetColor(_context, AndroidResource.Color.HoloOrangeDark);
						break;
					case NamedPlatformColor.HoloOrangeLight:
						color = ContextCompat.GetColor(_context, AndroidResource.Color.HoloOrangeLight);
						break;
					case NamedPlatformColor.HoloPurple:
						color = ContextCompat.GetColor(_context, AndroidResource.Color.HoloPurple);
						break;
					case NamedPlatformColor.HoloRedDark:
						color = ContextCompat.GetColor(_context, AndroidResource.Color.HoloRedDark);
						break;
					case NamedPlatformColor.HoloRedLight:
						color = ContextCompat.GetColor(_context, AndroidResource.Color.HoloRedLight);
						break;
					case NamedPlatformColor.TabIndicatorText:
						color = ContextCompat.GetColor(_context, AndroidResource.Color.TabIndicatorText);
						break;
					case NamedPlatformColor.Transparent:
						color = ContextCompat.GetColor(_context, AndroidResource.Color.Transparent);
						break;
					case NamedPlatformColor.White:
						color = ContextCompat.GetColor(_context, AndroidResource.Color.White);
						break;
					case NamedPlatformColor.WidgetEditTextDark:
						color = ContextCompat.GetColor(_context, AndroidResource.Color.WidgetEditTextDark);
						break;
					default:
						return Color.Default;
				}

				if (color != 0)
					return new AColor(color).ToColor();

				return Color.Default;
			}

			public async Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken)
			{
				using (var client = new HttpClient())
				{
					// Do not remove this await otherwise the client will dispose before
					// the stream even starts
					var result = await StreamWrapper.GetStreamAsync(uri, cancellationToken, client).ConfigureAwait(false);

					return result;
				}
			}

			public IIsolatedStorageFile GetUserStoreForApplication()
			{
				return new _IsolatedStorageFile(IsolatedStorageFile.GetUserStoreForApplication());
			}

			public bool IsInvokeRequired
			{
				get
				{
					return Looper.MainLooper != Looper.MyLooper();
				}
			}

			public string RuntimePlatform => Device.Android;

			public void OpenUriAction(Uri uri)
			{
				global::Android.Net.Uri aUri = global::Android.Net.Uri.Parse(uri.ToString());
				var intent = new Intent(Intent.ActionView, aUri);
				intent.SetFlags(ActivityFlags.ClearTop);
				intent.SetFlags(ActivityFlags.NewTask);

				// This seems to work fine even if the context has been destroyed (while another activity is in the
				// foreground). If we run into a situation where that's not the case, we'll have to do some work to
				// make sure this uses the active activity when launching the Intent
				_context.StartActivity(intent);
			}

			public void StartTimer(TimeSpan interval, Func<bool> callback)
			{
				var handler = new Handler(Looper.MainLooper);
				handler.PostDelayed(() =>
				{
					if (callback())
						StartTimer(interval, callback);

					handler.Dispose();
					handler = null;
				}, (long)interval.TotalMilliseconds);
			}

			double ConvertTextAppearanceToSize(int themeDefault, int deviceDefault, double defaultValue)
			{
				double myValue;

				if (TryGetTextAppearance(themeDefault, out myValue) && myValue > 0)
					return myValue;
				if (TryGetTextAppearance(deviceDefault, out myValue) && myValue > 0)
					return myValue;
				return defaultValue;
			}

			static int Hex(int v)
			{
				if (v < 10)
					return '0' + v;
				return 'a' + v - 10;
			}

			bool TryGetTextAppearance(int appearance, out double val)
			{
				val = 0;
				try
				{
					using (var value = new TypedValue())
					{
						if (_context.Theme.ResolveAttribute(appearance, value, true))
						{
							var textSizeAttr = new[] { AndroidResource.Attribute.TextSize };
							const int indexOfAttrTextSize = 0;
							using (TypedArray array = _context.ObtainStyledAttributes(value.Data, textSizeAttr))
							{
								val = _context.FromPixels(array.GetDimensionPixelSize(indexOfAttrTextSize, -1));
								return true;
							}
						}
					}
				}
				catch (Exception ex)
				{
					Internals.Log.Warning("Microsoft.Maui.Controls.Compatibility.Platform.Android.AndroidPlatformServices", "Error retrieving text appearance: {0}", ex);
				}
				return false;
			}

			public void QuitApplication()
			{
				Internals.Log.Warning(nameof(AndroidPlatformServices), "Platform doesn't implement QuitApp");
			}

			public SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
			{
				return Platform.Android.AppCompat.Platform.GetNativeSize(view, widthConstraint, heightConstraint);
			}

			public void Invalidate(VisualElement visualElement)
			{
				var renderer = visualElement.GetRenderer();
				if (renderer == null || renderer.View.IsDisposed())
				{
					return;
				}

				renderer.View.Invalidate();
				renderer.View.RequestLayout();
			}

			public OSAppTheme RequestedTheme
			{
				get
				{
					var nightMode = _context.Resources.Configuration.UiMode & UiMode.NightMask;
					switch (nightMode)
					{
						case UiMode.NightYes:
							return OSAppTheme.Dark;
						case UiMode.NightNo:
							return OSAppTheme.Light;
						default:
							return OSAppTheme.Unspecified;
					};
				}
			}

			public class _IsolatedStorageFile : IIsolatedStorageFile
			{
				readonly IsolatedStorageFile _isolatedStorageFile;

				public _IsolatedStorageFile(IsolatedStorageFile isolatedStorageFile)
				{
					_isolatedStorageFile = isolatedStorageFile;
				}

				public Task CreateDirectoryAsync(string path)
				{
					_isolatedStorageFile.CreateDirectory(path);
					return Task.FromResult(true);
				}

				public Task<bool> GetDirectoryExistsAsync(string path)
				{
					return Task.FromResult(_isolatedStorageFile.DirectoryExists(path));
				}

				public Task<bool> GetFileExistsAsync(string path)
				{
					return Task.FromResult(_isolatedStorageFile.FileExists(path));
				}

				public Task<DateTimeOffset> GetLastWriteTimeAsync(string path)
				{
					return Task.FromResult(_isolatedStorageFile.GetLastWriteTime(path));
				}

				public Task<Stream> OpenFileAsync(string path, FileMode mode, FileAccess access)
				{
					Stream stream = _isolatedStorageFile.OpenFile(path, mode, access);
					return Task.FromResult(stream);
				}

				public Task<Stream> OpenFileAsync(string path, FileMode mode, FileAccess access, FileShare share)
				{
					Stream stream = _isolatedStorageFile.OpenFile(path, mode, access, share);
					return Task.FromResult(stream);
				}
			}
		}
	}
}
