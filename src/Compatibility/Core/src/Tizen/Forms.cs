using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ElmSharp;
using ElmSharp.Wearable;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Devices;
using Tizen.Applications;
using Color = Microsoft.Maui.Graphics.Color;
using ELayout = ElmSharp.Layout;
using Size = Microsoft.Maui.Graphics.Size;
using TSystemInfo = Tizen.System.Information;

namespace Microsoft.Maui.Controls.Compatibility
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

	[Obsolete]
	public class InitializationOptions
	{
		public CoreApplication Context { get; set; }
		public bool UseDeviceIndependentPixel { get; set; }
		public bool UseSkiaSharp { get; set; } = true;
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
		public InitializationOptions()
		{
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

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
	public static class Forms
#pragma warning disable CS0612 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
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

		static Lazy<double> s_scalingFactor = new Lazy<double>(() =>
		{
			int width = 0;
			int height = 0;

			TSystemInfo.TryGetValue("http://tizen.org/feature/screen.width", out width);
			TSystemInfo.TryGetValue("http://tizen.org/feature/screen.height", out height);

			var scalingFactor = 1.0;  // scaling is disabled, we're using pixels as Xamarin's geometry units
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
			return scalingFactor;
		});

		static StaticRegistrarStrategy s_staticRegistrarStrategy = StaticRegistrarStrategy.None;

		static PlatformType s_platformType = PlatformType.Defalut;

		static bool s_useMessagingCenter = true;

		public static event EventHandler<ViewInitializedEventArgs> ViewInitialized;

		public static IMauiContext MauiContext
		{
			get;
			internal set;
		}

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

		public static StaticRegistrarStrategy StaticRegistrarStrategy => s_staticRegistrarStrategy;

		public static PlatformType PlatformType => s_platformType;

		public static bool UseMessagingCenter => s_useMessagingCenter;

		public static bool UseSkiaSharp { get; private set; }

		public static bool UseFastLayout { get; private set; }

		public static DisplayResolutionUnit DisplayResolutionUnit { get; private set; } = DisplayResolutionUnit.Pixel();

		public static int ScreenDPI => s_dpi.Value;

		public static Size PhysicalScreenSize => DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();

		internal static TizenTitleBarVisibility TitleBarVisibility
		{
			get;
			private set;
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

		public static bool IsInitializedRenderers { get; private set; }

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

		[Obsolete]
		public static void Init(IActivationState activationState) => Init(activationState.Context);

		[Obsolete]
		public static void Init(IActivationState activationState, InitializationOptions options) => Init(activationState.Context, options);

		[Obsolete]
		public static void Init(IMauiContext context, InitializationOptions options = null)
		{
			if (options != null && options.DisplayResolutionUnit != null)
			{
				DisplayResolutionUnit = options.DisplayResolutionUnit;
			}
			SetupInit(context, options);
		}

		[Obsolete]
		static void SetupInit(IMauiContext context, InitializationOptions options = null)
		{
			MauiContext = context;
			Context = options?.Context ?? MauiApplication.Current;
			NativeParent = context.GetPlatformParent();
			Registrar.RegisterRendererToHandlerShim(RendererToHandlerShim.CreateShim);

			if (!IsInitialized)
			{
				if (System.Threading.SynchronizationContext.Current == null)
				{
					TizenSynchronizationContext.Initialize();
				}

				Elementary.Initialize();
				Elementary.ThemeOverlay();
				Utility.AppendGlobalFontPath(@"/usr/share/fonts");
			}

			Device.DefaultRendererAssembly = typeof(Forms).Assembly;

			if (options?.Flags.HasFlag(InitializationFlags.SkipRenderers) != true)
				RegisterCompatRenderers(options);

			if (options != null)
			{
				s_platformType = options.PlatformType;
				s_useMessagingCenter = options.UseMessagingCenter;
				UseSkiaSharp = options.UseSkiaSharp;
				UseFastLayout = options.UseFastLayout;
			}

			Application.AccentColor = GetAccentColor();
			ExpressionSearch.Default = new TizenExpressionSearch();

			if (Context is WatchApplication)
				s_platformType = PlatformType.Lightweight;

			IsInitialized = true;
		}

		[Obsolete]
		internal static void RegisterCompatRenderers(InitializationOptions maybeOptions = null)
		{
			if (!IsInitializedRenderers)
			{
				IsInitializedRenderers = true;
				if (maybeOptions != null)
				{
					var options = maybeOptions;
					var handlers = options.Handlers;
					var flags = options.Flags;
					var effectScopes = options.EffectScopes;

					//TODO: ExportCell?
					//TODO: ExportFont

					// renderers
					if (handlers != null)
					{
						Registrar.RegisterRenderers(handlers);
					}

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

		static Color GetAccentColor()
		{
			// On Windows Phone, this is the complementary color chosen by the user.
			// Good Windows Phone applications use this as part of their styling to provide a native look and feel.
			// On iOS and Android this instance is set to a contrasting color that is visible on the default
			// background but is not the same as the default text color.

			if (DeviceInfo.Idiom == DeviceIdiom.Phone)
				// [Tizen_3.0]Basic_Interaction_GUI_[REF].xlsx Theme 001 (Default) 1st HSB: 188 70 80
				return Color.FromRgba(61, 185, 204, 255);
			else if (DeviceInfo.Idiom == DeviceIdiom.TV)
				return Color.FromRgba(15, 15, 15, 230);
			else if (DeviceInfo.Idiom == DeviceIdiom.Watch)
				// Theme A (Default) 1st HSB: 207 75 16
				return Color.FromRgba(10, 27, 41, 255);
			else
				return Color.FromRgb(0, 0, 0);
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
			return (int)Math.Round(dp * s_scalingFactor.Value);
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
			if (pixel == int.MaxValue)
				return double.PositiveInfinity;
			return pixel / s_scalingFactor.Value;
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
			if (pixel == double.PositiveInfinity)
				return double.PositiveInfinity;
			return pixel / s_scalingFactor.Value;
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
			var window = new Microsoft.Maui.Controls.Compatibility.Platform.Tizen.PreloadedWindow();
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
