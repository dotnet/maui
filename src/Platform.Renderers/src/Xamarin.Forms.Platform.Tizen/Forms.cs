using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ElmSharp;
using ElmSharp.Wearable;
using Tizen.Applications;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.Tizen;
using Xamarin.Forms.Shapes;
using DeviceOrientation = Xamarin.Forms.Internals.DeviceOrientation;
using ELayout = ElmSharp.Layout;
using TSystemInfo = Tizen.System.Information;

namespace Xamarin.Forms
{
	public enum StaticRegistrarStrategy
	{
		None,
		StaticRegistrarOnly,
		All,
	}

	public enum PlatformType
	{
		Defalut,
		Lightweight,
	}

	public class InitializationOptions
	{
		public CoreApplication Context { get; set; }
		public bool UseDeviceIndependentPixel { get; set; }
		public bool UseSkiaSharp { get; set; }
		public HandlerAttribute[] Handlers { get; set; }
		public Dictionary<Type, Func<IRegisterable>> CustomHandlers { get; set; } // for static registers
		public Assembly[] Assemblies { get; set; }
		public EffectScope[] EffectScopes { get; set; }
		public InitializationFlags Flags { get; set; }
		public StaticRegistrarStrategy StaticRegistarStrategy { get; set; }
		public PlatformType PlatformType { get; set; }
		public bool UseMessagingCenter { get; set; } = true;
		public bool UseFastLayout { get; set; } = false;

		public DisplayResolutionUnit DisplayResolutionUnit { get; set; }

		public struct EffectScope
		{
			public string Name;
			public ExportEffectAttribute[] Effects;
		}

		public InitializationOptions(CoreApplication application)
		{
			Context = application;
		}

		public InitializationOptions(CoreApplication application, bool useDeviceIndependentPixel, HandlerAttribute[] handlers)
		{
			Context = application;
			UseDeviceIndependentPixel = useDeviceIndependentPixel;
			Handlers = handlers;
		}

		public InitializationOptions(CoreApplication application, bool useDeviceIndependentPixel, params Assembly[] assemblies)
		{
			Context = application;
			UseDeviceIndependentPixel = useDeviceIndependentPixel;
			Assemblies = assemblies;
		}

		public void UseStaticRegistrar(StaticRegistrarStrategy strategy, Dictionary<Type, Func<IRegisterable>> customHandlers = null, bool disableCss = false)
		{
			StaticRegistarStrategy = strategy;
			CustomHandlers = customHandlers;
			if (disableCss) // about 10ms
				Flags = InitializationFlags.DisableCss;
		}
	}

	public static class Forms
	{
		static Lazy<string> s_profile = new Lazy<string>(() =>
		{
			//TODO : Fix me if elm_config_profile_get() unavailable
			return Elementary.GetProfile();
		});

		static Lazy<int> s_dpi = new Lazy<int>(() =>
		{
			int dpi = 0;
			if (s_profile.Value == "tv")
			{
				// Use fixed DPI value (72) if TV profile
				return 72;
			}
			TSystemInfo.TryGetValue<int>("http://tizen.org/feature/screen.dpi", out dpi);
			return dpi;
		});

		static Lazy<double> s_elmScale = new Lazy<double>(() =>
		{
			return s_deviceScale.Value / Elementary.GetScale();
		});

		static Lazy<string> s_deviceType = new Lazy<string>(() =>
		{
			if (!TSystemInfo.TryGetValue("http://tizen.org/system/device_type", out string deviceType))
			{
				// Since, above key("http://tizen.org/system/device_type") is not available on Tizen 4.0, we uses profile to decide the type of device on 4.0.
				var profile = GetProfile();
				if (profile == "mobile")
				{
					deviceType = "Mobile";
				}
				else if (profile == "tv")
				{
					deviceType = "TV";
				}
				else if (profile == "wearable")
				{
					deviceType = "Wearable";
				}
				else
				{
					deviceType = "Unknown";
				}
			}
			return deviceType;
		});

		static Lazy<double> s_deviceScale = new Lazy<double>(() =>
		{
			// This is the base scale value and varies from profile
			return ThemeManager.GetBaseScale(s_deviceType.Value);
		});

		class TizenDeviceInfo : DeviceInfo
		{
			readonly Size pixelScreenSize;

			readonly Size scaledScreenSize;

			readonly double scalingFactor;

			readonly string profile;

			public override Size PixelScreenSize
			{
				get
				{
					return this.pixelScreenSize;
				}
			}

			public override Size ScaledScreenSize
			{
				get
				{
					return this.scaledScreenSize;
				}
			}

			public Size PhysicalScreenSize { get; }

			public override double ScalingFactor
			{
				get
				{
					return this.scalingFactor;
				}
			}

			public string Profile
			{
				get
				{
					return this.profile;
				}
			}

			public TizenDeviceInfo()
			{
				int width = 0;
				int height = 0;

				TSystemInfo.TryGetValue("http://tizen.org/feature/screen.width", out width);
				TSystemInfo.TryGetValue("http://tizen.org/feature/screen.height", out height);

				var physicalScale = s_dpi.Value / 160.0;
				PhysicalScreenSize = new Size(width / physicalScale, height / physicalScale);

				scalingFactor = 1.0;  // scaling is disabled, we're using pixels as Xamarin's geometry units
				if (DisplayResolutionUnit.UseVP && DisplayResolutionUnit.ViewportWidth > 0)
				{
					scalingFactor = width / DisplayResolutionUnit.ViewportWidth;
				}
				else
				{
					if (DisplayResolutionUnit.UseDP)
					{
						scalingFactor = s_dpi.Value / 160.0;
					}

					if (DisplayResolutionUnit.UseDeviceScale)
					{
						var portraitSize = Math.Min(PhysicalScreenSize.Width, PhysicalScreenSize.Height);
						if (portraitSize > 2000)
						{
							scalingFactor *= 4;
						}
						else if (portraitSize > 1000)
						{
							scalingFactor *= 2.5;
						}
					}
				}

				pixelScreenSize = new Size(width, height);
				scaledScreenSize = new Size(width / scalingFactor, height / scalingFactor);
				profile = s_profile.Value;
			}
		}

		static StaticRegistrarStrategy s_staticRegistrarStrategy = StaticRegistrarStrategy.None;

		static PlatformType s_platformType = PlatformType.Defalut;

		static bool s_useMessagingCenter = true;

		public static event EventHandler<ViewInitializedEventArgs> ViewInitialized;

		public static CoreApplication Context
		{
			get;
			internal set;
		}

		public static EvasObject NativeParent
		{
			get; internal set;
		}

		public static ELayout BaseLayout => NativeParent as ELayout;

		public static CircleSurface CircleSurface
		{
			get; internal set;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Element RotaryFocusObject
		{
			get; internal set;
		}

		public static bool IsInitialized
		{
			get;
			private set;
		}

		public static DeviceOrientation NaturalOrientation { get; } = GetDeviceOrientation();

		public static StaticRegistrarStrategy StaticRegistrarStrategy => s_staticRegistrarStrategy;

		public static PlatformType PlatformType => s_platformType;

		public static bool UseMessagingCenter => s_useMessagingCenter;

		public static bool UseSkiaSharp { get; private set; }

		public static bool UseFastLayout { get; private set; }

		public static DisplayResolutionUnit DisplayResolutionUnit { get; private set; }

		public static int ScreenDPI => s_dpi.Value;

		public static Size PhysicalScreenSize => (Device.info as TizenDeviceInfo).PhysicalScreenSize;

		internal static TizenTitleBarVisibility TitleBarVisibility
		{
			get;
			private set;
		}

		static DeviceOrientation GetDeviceOrientation()
		{
			int width = 0;
			int height = 0;
			TSystemInfo.TryGetValue<int>("http://tizen.org/feature/screen.width", out width);
			TSystemInfo.TryGetValue<int>("http://tizen.org/feature/screen.height", out height);

			if (height >= width)
			{
				return DeviceOrientation.Portrait;
			}
			else
			{
				return DeviceOrientation.Landscape;
			}
		}

		internal static void SendViewInitialized(this VisualElement self, EvasObject nativeView)
		{
			EventHandler<ViewInitializedEventArgs> viewInitialized = Forms.ViewInitialized;
			if (viewInitialized != null)
			{
				viewInitialized.Invoke(self, new ViewInitializedEventArgs
				{
					View = self,
					NativeView = nativeView
				});
			}
		}

		static IReadOnlyList<string> s_flags;
		public static IReadOnlyList<string> Flags => s_flags ?? (s_flags = new string[0]);

		public static void SetFlags(params string[] flags)
		{
			if (IsInitialized)
			{
				throw new InvalidOperationException($"{nameof(SetFlags)} must be called before {nameof(Init)}");
			}

			s_flags = (string[])flags.Clone();
			if (s_flags.Contains("Profile"))
				Profile.Enable();
		}

		public static void SetTitleBarVisibility(TizenTitleBarVisibility visibility)
		{
			TitleBarVisibility = visibility;
		}

		public static TOut GetHandler<TOut>(Type type, params object[] args) where TOut : class, IRegisterable
		{
			if (s_staticRegistrarStrategy == StaticRegistrarStrategy.None)
			{
				// Find hander in internal registrar, that is using reflection (default).
				return Registrar.Registered.GetHandler<TOut>(type, args);
			}
			else
			{
				// 1. Find hander in static registrar first
				TOut ret = StaticRegistrar.Registered.GetHandler<TOut>(type, args);

				// 2. If there is no handler, try to find hander in internal registrar, that is using reflection.
				if (ret == null && s_staticRegistrarStrategy == StaticRegistrarStrategy.All)
				{
					ret = Registrar.Registered.GetHandler<TOut>(type, args);
				}
				return ret;
			}
		}

		public static TOut GetHandlerForObject<TOut>(object obj) where TOut : class, IRegisterable
		{
			if (s_staticRegistrarStrategy == StaticRegistrarStrategy.None)
			{
				// Find hander in internal registrar, that is using reflection (default).
				return Registrar.Registered.GetHandlerForObject<TOut>(obj);
			}
			else
			{
				// 1. Find hander in static registrar first
				TOut ret = StaticRegistrar.Registered.GetHandlerForObject<TOut>(obj);

				// 2. If there is no handler, try to find hander in internal registrar, that is using reflection.
				if (ret == null && s_staticRegistrarStrategy == StaticRegistrarStrategy.All)
				{
					ret = Registrar.Registered.GetHandlerForObject<TOut>(obj);
				}
				return ret;
			}
		}

		public static TOut GetHandlerForObject<TOut>(object obj, params object[] args) where TOut : class, IRegisterable
		{
			if (s_staticRegistrarStrategy == StaticRegistrarStrategy.None)
			{
				// Find hander in internal registrar, that is using reflection (default).
				return Registrar.Registered.GetHandlerForObject<TOut>(obj, args);
			}
			else
			{
				// 1. Find hander in static registrar first without fallback handler.
				TOut ret = StaticRegistrar.Registered.GetHandlerForObject<TOut>(obj, args);

				// 2. If there is no handler, try to find hander in internal registrar, that is using reflection.
				if (ret == null && s_staticRegistrarStrategy == StaticRegistrarStrategy.All)
				{
					ret = StaticRegistrar.Registered.GetHandlerForObject<TOut>(obj, args);
				}
				return ret;
			}
		}

		public static void Init(CoreApplication application)
		{
			Init(application, false);
		}

		public static void Init(CoreApplication application, bool useDeviceIndependentPixel)
		{
			DisplayResolutionUnit = DisplayResolutionUnit.FromInit(useDeviceIndependentPixel);
			SetupInit(application);
		}

		public static void Init(CoreApplication application, DisplayResolutionUnit unit)
		{
			DisplayResolutionUnit = unit ?? DisplayResolutionUnit.Pixel();
			SetupInit(application);
		}

		public static void Init(InitializationOptions options)
		{
			if (options == null)
			{
				throw new ArgumentException("Must be set options", nameof(options));
			}

			DisplayResolutionUnit = options.DisplayResolutionUnit ?? DisplayResolutionUnit.FromInit(options.UseDeviceIndependentPixel);
			SetupInit(options.Context, options);
		}

		static void SetupInit(CoreApplication application, InitializationOptions options = null)
		{
			Context = application;

			if (!IsInitialized)
			{
				Internals.Log.Listeners.Add(new XamarinLogListener());
				if (System.Threading.SynchronizationContext.Current == null)
				{
					TizenSynchronizationContext.Initialize();
				}

				Elementary.Initialize();
				Elementary.ThemeOverlay();
				Utility.AppendGlobalFontPath(@"/usr/share/fonts");
			}

			Device.PlatformServices = new TizenPlatformServices();
			if (Device.info != null)
			{
				((TizenDeviceInfo)Device.info).Dispose();
				Device.info = null;
			}

			Device.Info = new Forms.TizenDeviceInfo();
			Device.SetFlags(s_flags);

			string profile = ((TizenDeviceInfo)Device.Info).Profile;
			if (profile == "mobile")
			{
				Device.SetIdiom(TargetIdiom.Phone);
			}
			else if (profile == "tv")
			{
				Device.SetIdiom(TargetIdiom.TV);
			}
			else if (profile == "desktop")
			{
				Device.SetIdiom(TargetIdiom.Desktop);
			}
			else if (profile == "wearable")
			{
				Device.SetIdiom(TargetIdiom.Watch);
			}
			else
			{
				Device.SetIdiom(TargetIdiom.Unsupported);
			}

			if (!Forms.IsInitialized)
			{
				if (options != null)
				{
					s_platformType = options.PlatformType;
					s_useMessagingCenter = options.UseMessagingCenter;
					UseSkiaSharp = options.UseSkiaSharp;
					UseFastLayout = options.UseFastLayout;

					if (options.Assemblies != null && options.Assemblies.Length > 0)
					{
						TizenPlatformServices.AppDomain.CurrentDomain.AddAssemblies(options.Assemblies);
					}

					// renderers
					if (options.Handlers != null)
					{
						Registrar.RegisterRenderers(options.Handlers);
					}
					else
					{
						// Add Xamarin.Forms.Core assembly by default to apply the styles.
						TizenPlatformServices.AppDomain.CurrentDomain.AddAssembly(Assembly.GetAssembly(typeof(Xamarin.Forms.View)));

						// static registrar
						if (options.StaticRegistarStrategy != StaticRegistrarStrategy.None)
						{
							s_staticRegistrarStrategy = options.StaticRegistarStrategy;
							StaticRegistrar.RegisterHandlers(options.CustomHandlers);

							if (options.StaticRegistarStrategy == StaticRegistrarStrategy.All)
							{
								Registrar.RegisterAll(new Type[]
								{
										typeof(ExportRendererAttribute),
										typeof(ExportImageSourceHandlerAttribute),
										typeof(ExportCellAttribute),
										typeof(ExportHandlerAttribute),
										typeof(ExportFontAttribute)
								});

								if (UseSkiaSharp)
									RegisterSkiaSharpRenderers();
							}
						}
						else
						{
							// The assembly of the executing application and referenced assemblies of it are added into the list here.
							TizenPlatformServices.AppDomain.CurrentDomain.RegisterAssemblyRecursively(application.GetType().GetTypeInfo().Assembly);
							Registrar.RegisterAll(new Type[]
							{
								typeof(ExportRendererAttribute),
								typeof(ExportImageSourceHandlerAttribute),
								typeof(ExportCellAttribute),
								typeof(ExportHandlerAttribute),
								typeof(ExportFontAttribute)
							});

							if (UseSkiaSharp)
								RegisterSkiaSharpRenderers();

							if (UseFastLayout)
								Registrar.Registered.Register(typeof(Layout), typeof(FastLayoutRenderer));
						}
					}

					// effects
					var effectScopes = options.EffectScopes;
					if (effectScopes != null)
					{
						for (var i = 0; i < effectScopes.Length; i++)
						{
							var effectScope = effectScopes[0];
							Registrar.RegisterEffects(effectScope.Name, effectScope.Effects);
						}
					}

					// css
					Registrar.RegisterStylesheets(options.Flags);
				}
				else
				{
					// In .NETCore, AppDomain feature is not supported.
					// The list of assemblies returned by AppDomain.GetAssemblies() method should be registered manually.
					// The assembly of the executing application and referenced assemblies of it are added into the list here.
					TizenPlatformServices.AppDomain.CurrentDomain.RegisterAssemblyRecursively(application.GetType().GetTypeInfo().Assembly);

					Registrar.RegisterAll(new Type[]
					{
						typeof(ExportRendererAttribute),
						typeof(ExportImageSourceHandlerAttribute),
						typeof(ExportCellAttribute),
						typeof(ExportHandlerAttribute),
						typeof(ExportFontAttribute)
					});
				}
			}

			Color.SetAccent(GetAccentColor(profile));
			ExpressionSearch.Default = new TizenExpressionSearch();

			if (application is WatchApplication)
				s_platformType = PlatformType.Lightweight;

			IsInitialized = true;
		}

		static void RegisterSkiaSharpRenderers()
		{
			// Register all skiasharp-based rednerers here.
			Registrar.Registered.Register(typeof(Frame), typeof(Platform.Tizen.SkiaSharp.FrameRenderer));
			Registrar.Registered.Register(typeof(BoxView), typeof(Platform.Tizen.SkiaSharp.BoxViewRenderer));
			Registrar.Registered.Register(typeof(Image), typeof(Platform.Tizen.SkiaSharp.ImageRenderer));

			Registrar.Registered.Register(typeof(Ellipse), typeof(Platform.Tizen.SkiaSharp.EllipseRenderer));
			Registrar.Registered.Register(typeof(Line), typeof(Platform.Tizen.SkiaSharp.LineRenderer));
			Registrar.Registered.Register(typeof(Path), typeof(Platform.Tizen.SkiaSharp.PathRenderer));
			Registrar.Registered.Register(typeof(Shapes.Polygon), typeof(Platform.Tizen.SkiaSharp.PolygonRenderer));
			Registrar.Registered.Register(typeof(Polyline), typeof(Platform.Tizen.SkiaSharp.PolylineRenderer));
			Registrar.Registered.Register(typeof(Shapes.Rectangle), typeof(Platform.Tizen.SkiaSharp.RectangleRenderer));
		}

		static Color GetAccentColor(string profile)
		{
			// On Windows Phone, this is the complementary color chosen by the user.
			// Good Windows Phone applications use this as part of their styling to provide a native look and feel.
			// On iOS and Android this instance is set to a contrasting color that is visible on the default
			// background but is not the same as the default text color.

			switch (profile)
			{
				case "mobile":
					// [Tizen_3.0]Basic_Interaction_GUI_[REF].xlsx Theme 001 (Default) 1st HSB: 188 70 80
					return Color.FromRgba(61, 185, 204, 255);
				case "tv":
					return Color.FromRgba(15, 15, 15, 230);
				case "wearable":
					// Theme A (Default) 1st HSB: 207 75 16
					return Color.FromRgba(10, 27, 41, 255);
				default:
					return Color.Black;
			}
		}

		/// <summary>
		/// Converts the dp into pixel considering current DPI value.
		/// </summary>
		/// <remarks>
		/// Use this API if you want to get pixel size without scaling factor.
		/// </remarks>
		/// <param name="dp"></param>
		/// <returns></returns>
		public static int ConvertToPixel(double dp)
		{
			return (int)Math.Round(dp * s_dpi.Value / 160.0);
		}

		/// <summary>
		/// Converts the dp into pixel by using scaling factor.
		/// </summary>
		/// <remarks>
		/// Use this API if you want to get pixel size from dp by using scaling factor.
		/// If the scaling factor is 1.0 by user setting, the same value is returned. This is mean that user doesn't want to pixel independent metric.
		/// </remarks>
		/// <param name="dp"></param>
		/// <returns></returns>
		public static int ConvertToScaledPixel(double dp)
		{
			return (int)Math.Round(dp * Device.Info.ScalingFactor);
		}

		/// <summary>
		/// Converts the pixel into dp value by using scaling factor.
		/// </summary>
		/// <remarks>
		/// If the scaling factor is 1.0 by user setting, the same value is returned. This is mean that user doesn't want to pixel independent metric.
		/// </remarks>
		/// <param name="pixel"></param>
		/// <returns></returns>
		public static double ConvertToScaledDP(int pixel)
		{
			return pixel / Device.Info.ScalingFactor;
		}

		/// <summary>
		/// Converts the pixel into dp value by using scaling factor.
		/// </summary>
		/// <remarks>
		/// If the scaling factor is 1.0 by user setting, the same value is returned. This is mean that user doesn't want to pixel independent metric.
		/// </remarks>
		/// <param name="pixel"></param>
		/// <returns></returns>
		public static double ConvertToScaledDP(double pixel)
		{
			return pixel / Device.Info.ScalingFactor;
		}

		/// <summary>
		/// Converts the sp into EFL's font size metric (EFL point).
		/// </summary>
		/// <param name="sp"></param>
		/// <returns></returns>
		public static int ConvertToEflFontPoint(double sp)
		{
			return (int)Math.Round(ConvertToScaledPixel(sp) * s_elmScale.Value);
		}

		/// <summary>
		/// Convert the EFL's point into sp.
		/// </summary>
		/// <param name="eflPt"></param>
		/// <returns></returns>
		public static double ConvertToDPFont(int eflPt)
		{
			return ConvertToScaledDP(eflPt / s_elmScale.Value);
		}

		/// <summary>
		/// Get the EFL's profile
		/// </summary>
		/// <returns></returns>
		public static string GetProfile()
		{
			return s_profile.Value;
		}

		public static string GetDeviceType()
		{
			return s_deviceType.Value;
		}

		// for internal use only
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void Preload()
		{
			Elementary.Initialize();
			Elementary.ThemeOverlay();
			var window = new PreloadedWindow();
			TSystemInfo.TryGetValue("http://tizen.org/feature/screen.width", out int width);
			TSystemInfo.TryGetValue("http://tizen.org/feature/screen.height", out int height);
		}
	}

	class TizenExpressionSearch : ExpressionVisitor, IExpressionSearch
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
				var container = ((ConstantExpression)node.Expression).Value;
				var value = ((FieldInfo)node.Member).GetValue(container);

				if (_targetType.IsInstanceOfType(value))
					_results.Add(value);
			}
			return base.VisitMember(node);
		}
	}
}
