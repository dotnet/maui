using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.Maui.Controls.Compatibility.Platform.Tizen;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;
using Tizen.Applications;
using Color = Microsoft.Maui.Graphics.Color;
using NView = Tizen.NUI.BaseComponents.View;
using Size = Microsoft.Maui.Graphics.Size;
using TDeviceInfo = Tizen.UIExtensions.Common.DeviceInfo;

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
#pragma warning disable CA1815 // Override equals and operator equals on value types
	public class InitializationOptions
	{
		public CoreApplication Context { get; set; }
		public bool UseDeviceIndependentPixel { get; set; }
		public HandlerAttribute[] Handlers { get; set; }
		public Dictionary<Type, Func<IRegisterable>> CustomHandlers { get; set; } // for static registers
		public Assembly[] Assemblies { get; set; }
		public EffectScope[] EffectScopes { get; set; }
		public InitializationFlags Flags { get; set; }
		public StaticRegistrarStrategy StaticRegistarStrategy { get; set; }
		public PlatformType PlatformType { get; set; }
		public bool UseMessagingCenter { get; set; } = true;

		public bool UseSkiaSharp { get; set; }


		public DisplayResolutionUnit DisplayResolutionUnit { get; set; }

		public struct EffectScope
#pragma warning restore CA1815 // Override equals and operator equals on value types
		{
			public string Name;
			public ExportEffectAttribute[] Effects;
		}

		public InitializationOptions()
		{
		}

		public InitializationOptions(bool useDeviceIndependentPixel, HandlerAttribute[] handlers)
		{
			UseDeviceIndependentPixel = useDeviceIndependentPixel;
			Handlers = handlers;
		}

		public InitializationOptions(bool useDeviceIndependentPixel, params Assembly[] assemblies)
		{
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
		static IReadOnlyList<string> s_flags;

		public static event EventHandler<ViewInitializedEventArgs> ViewInitialized;

		public static bool IsInitialized { get; private set; }

		public static StaticRegistrarStrategy StaticRegistrarStrategy { get; private set; }

		public static PlatformType PlatformType { get; private set; }

		public static bool UseMessagingCenter { get; private set; }

		public static DisplayResolutionUnit DisplayResolutionUnit { get; private set; }

		public static int ScreenDPI => TDeviceInfo.DPI;

		public static Size PhysicalScreenSize => DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();

		public static IReadOnlyList<string> Flags => s_flags ?? (s_flags = Array.Empty<string>());

		public static IMauiContext MauiContext { get; internal set; }

		internal static TizenTitleBarVisibility TitleBarVisibility
		{
			get;
			private set;
		}

		internal static void SendViewInitialized(this VisualElement self, NView nativeView)
		{
			EventHandler<ViewInitializedEventArgs> viewInitialized = Forms.ViewInitialized;
			viewInitialized?.Invoke(self, new ViewInitializedEventArgs
			{
				View = self,
				NativeView = nativeView
			});
		}

		public static bool IsInitializedRenderers { get; private set; }

		public static void SetTitleBarVisibility(TizenTitleBarVisibility visibility)
		{
			TitleBarVisibility = visibility;
		}

		public static TOut GetHandler<TOut>(Type type, params object[] args) where TOut : class, IRegisterable
		{
			if (StaticRegistrarStrategy == StaticRegistrarStrategy.None)
			{
				// Find hander in internal registrar, that is using reflection (default).
				return Registrar.Registered.GetHandler<TOut>(type, args);
			}
			else
			{
				// 1. Find hander in static registrar first
				TOut ret = StaticRegistrar.Registered.GetHandler<TOut>(type, args);

				// 2. If there is no handler, try to find hander in internal registrar, that is using reflection.
				if (ret == null && StaticRegistrarStrategy == StaticRegistrarStrategy.All)
				{
					ret = Registrar.Registered.GetHandler<TOut>(type, args);
				}
				return ret;
			}
		}

		public static TOut GetHandlerForObject<TOut>(object obj) where TOut : class, IRegisterable
		{
			if (StaticRegistrarStrategy == StaticRegistrarStrategy.None)
			{
				// Find hander in internal registrar, that is using reflection (default).
				return Registrar.Registered.GetHandlerForObject<TOut>(obj);
			}
			else
			{
				// 1. Find hander in static registrar first
				TOut ret = StaticRegistrar.Registered.GetHandlerForObject<TOut>(obj);

				// 2. If there is no handler, try to find hander in internal registrar, that is using reflection.
				if (ret == null && StaticRegistrarStrategy == StaticRegistrarStrategy.All)
				{
					ret = Registrar.Registered.GetHandlerForObject<TOut>(obj);
				}
				return ret;
			}
		}

		public static TOut GetHandlerForObject<TOut>(object obj, params object[] args) where TOut : class, IRegisterable
		{
			if (StaticRegistrarStrategy == StaticRegistrarStrategy.None)
			{
				// Find hander in internal registrar, that is using reflection (default).
				return Registrar.Registered.GetHandlerForObject<TOut>(obj, args);
			}
			else
			{
				// 1. Find hander in static registrar first without fallback handler.
				TOut ret = StaticRegistrar.Registered.GetHandlerForObject<TOut>(obj, args);

				// 2. If there is no handler, try to find hander in internal registrar, that is using reflection.
				if (ret == null && StaticRegistrarStrategy == StaticRegistrarStrategy.All)
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
			Registrar.RegisterRendererToHandlerShim(RendererToHandlerShim.CreateShim);

			if (!IsInitialized)
			{
				if (System.Threading.SynchronizationContext.Current == null)
				{
					TizenSynchronizationContext.Initialize();
				}

				Tizen.NUI.FontClient.Instance.AddCustomFontDirectory(@"/usr/share/fonts");
			}

			Device.DefaultRendererAssembly = typeof(Forms).Assembly;

			if (options?.Flags.HasFlag(InitializationFlags.SkipRenderers) != true)
				RegisterCompatRenderers(options);

			if (options != null)
			{
				PlatformType = options.PlatformType;
				UseMessagingCenter = options.UseMessagingCenter;
			}

			Application.AccentColor = GetAccentColor();
			ExpressionSearch.Default = new TizenExpressionSearch();

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
			return (int)Math.Round(dp * TDeviceInfo.DPI / 160.0);
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
			return (int)Math.Round(dp * TDeviceInfo.ScalingFactor);
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
			return pixel / TDeviceInfo.ScalingFactor;
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
			return pixel / TDeviceInfo.ScalingFactor;
		}

		/// <summary>
		/// Converts the sp into EFL's font size metric (EFL point).
		/// </summary>
		/// <param name="sp"></param>
		/// <returns></returns>
		public static int ConvertToEflFontPoint(double sp)
		{
			if (sp == -1)
				return -1;
			return (int)sp.ToScaledPoint();
		}

		/// <summary>
		/// Convert the EFL's point into sp.
		/// </summary>
		/// <param name="eflPt"></param>
		/// <returns></returns>
		public static double ConvertToDPFont(int eflPt)
		{
			return eflPt.ToScaledDP();
		}

		/// <summary>
		/// Get the EFL's profile
		/// </summary>
		/// <returns></returns>
		public static string GetProfile()
		{
			return TDeviceInfo.Profile;
		}

		public static string GetDeviceType()
		{
			return TDeviceInfo.DeviceType.ToString();
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
