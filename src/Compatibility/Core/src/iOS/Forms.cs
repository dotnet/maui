using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Foundation;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Animations;
using Microsoft.Extensions.DependencyInjection;
using System.Runtime.Versioning;

#if __MOBILE__
using ObjCRuntime;
using UIKit;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using TNativeView = UIKit.UIView;
#else
using AppKit;
using Microsoft.Maui.Controls.Compatibility.Platform.MacOS;
using TNativeView = AppKit.NSView;
#endif

namespace Microsoft.Maui.Controls.Compatibility
{
	[Obsolete]
#pragma warning disable CA1815 // Override equals and operator equals on value types
	public struct InitializationOptions
#pragma warning restore CA1815 // Override equals and operator equals on value types
	{
		public InitializationFlags Flags;
	}

	public static class Forms
	{
		internal static IMauiContext MauiContext { get; private set; }

		public static bool IsInitialized { get; private set; }

#if __MOBILE__
		static bool? s_isiOS11OrNewer;
		static bool? s_isiOS12OrNewer;
		static bool? s_isiOS13OrNewer;
		static bool? s_isiOS14OrNewer;
		static bool? s_isiOS15OrNewer;

		[SupportedOSPlatformGuard("ios11.0")]
		//[SupportedOSPlatformGuard("tvos11.0")] TODO: the block guarded by this property calling API unsupported on TvOS or version not supported
		internal static bool IsiOS11OrNewer
		{
			get
			{
				if (!s_isiOS11OrNewer.HasValue)
					s_isiOS11OrNewer = OperatingSystem.IsIOSVersionAtLeast(11, 0) || OperatingSystem.IsTvOSVersionAtLeast(11, 0);
				return s_isiOS11OrNewer.Value;
			}
		}

		internal static bool IsiOS12OrNewer
		{
			get
			{
				if (!s_isiOS12OrNewer.HasValue)
					s_isiOS12OrNewer = OperatingSystem.IsIOSVersionAtLeast(12, 0) || OperatingSystem.IsTvOSVersionAtLeast(12, 0);
				return s_isiOS12OrNewer.Value;
			}
		}

		[SupportedOSPlatformGuard("ios13.0")]
		//[SupportedOSPlatformGuard("tvos13.0")] TODO: the block guarded by this property calling API unsupported on TvOS or version not supported
		internal static bool IsiOS13OrNewer
		{
			get
			{
				if (!s_isiOS13OrNewer.HasValue)
					s_isiOS13OrNewer = OperatingSystem.IsIOSVersionAtLeast(13, 0) || OperatingSystem.IsTvOSVersionAtLeast(13, 0);
				return s_isiOS13OrNewer.Value;
			}
		}

		[SupportedOSPlatformGuard("ios14.0")]
		//[SupportedOSPlatformGuard("tvos14.0")] TODO: the block guarded by this property calling API unsupported on TvOS
		internal static bool IsiOS14OrNewer
		{
			get
			{
				if (!s_isiOS14OrNewer.HasValue)
					s_isiOS14OrNewer = OperatingSystem.IsIOSVersionAtLeast(14, 0) || OperatingSystem.IsTvOSVersionAtLeast(14, 0);
				return s_isiOS14OrNewer.Value;
			}
		}

		[SupportedOSPlatformGuard("ios15.0")]
		[SupportedOSPlatformGuard("tvos15.0")]
		internal static bool IsiOS15OrNewer
		{
			get
			{
				if (!s_isiOS15OrNewer.HasValue)
					s_isiOS15OrNewer = OperatingSystem.IsIOSVersionAtLeast(15, 0) || OperatingSystem.IsTvOSVersionAtLeast(15, 0);
				return s_isiOS15OrNewer.Value;
			}
		}

		// Once we get essentials/cg converted to using startup.cs
		// we will delete all the renderer code inside this file
		[Obsolete]
		internal static void RenderersRegistered()
		{
			IsInitializedRenderers = true;
		}

#else

		static bool? s_isMojaveOrNewer;

		internal static bool IsMojaveOrNewer
		{
			get
			{
				if (!s_isMojaveOrNewer.HasValue)
					s_isMojaveOrNewer = OperatingSystem.IsMacOSVersionAtLeast(10, 14);
				return s_isMojaveOrNewer.Value;
			}
		}

#endif

		[Obsolete]
		public static bool IsInitializedRenderers { get; private set; }

		[Obsolete]
		public static void Init(IActivationState activationState, InitializationOptions? options = null) =>
			SetupInit(activationState.Context, options);

		[Obsolete]
		static void SetupInit(IMauiContext context, InitializationOptions? maybeOptions = null)
		{
			MauiContext = context;

			Microsoft.Maui.Controls.Internals.Registrar.RegisterRendererToHandlerShim(RendererToHandlerShim.CreateShim);

			Application.AccentColor = Color.FromRgba(50, 79, 133, 255);

			Device.DefaultRendererAssembly = typeof(Forms).Assembly;

			if (maybeOptions?.Flags.HasFlag(InitializationFlags.SkipRenderers) != true)
				RegisterCompatRenderers(context);

			ExpressionSearch.Default = new iOSExpressionSearch();

			IsInitialized = true;
		}

		[Obsolete]
		internal static void RegisterCompatRenderers(IMauiContext context)
		{
			if (!IsInitializedRenderers)
			{
				IsInitializedRenderers = true;

				// Only need to do this once
				Controls.Internals.Registrar.RegisterAll(new[]
				{
					typeof(ExportRendererAttribute),
					typeof(ExportCellAttribute),
					typeof(ExportImageSourceHandlerAttribute),
					typeof(ExportFontAttribute)
				}, context?.Services?.GetService<IFontRegistrar>());
			}
		}

		public static event EventHandler<ViewInitializedEventArgs> ViewInitialized;

		internal static void SendViewInitialized(this VisualElement self, TNativeView nativeView)
		{
			ViewInitialized?.Invoke(self, new ViewInitializedEventArgs { View = self, NativeView = nativeView });
		}

		class iOSExpressionSearch : ExpressionVisitor, IExpressionSearch
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
}
