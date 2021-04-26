using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	public static class Device
	{
		public const string iOS = "iOS";
		public const string Android = "Android";
		public const string UWP = "UWP";
		public const string macOS = "macOS";
		public const string GTK = "GTK";
		public const string Tizen = "Tizen";
		public const string WPF = "WPF";

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static DeviceInfo info;

		static IPlatformServices s_platformServices;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetIdiom(TargetIdiom value) => Idiom = value;
		public static TargetIdiom Idiom { get; internal set; }

		//TODO: Why are there two of these? This is never used...?
		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetTargetIdiom(TargetIdiom value) => Idiom = value;

		public static string RuntimePlatform => PlatformServices.RuntimePlatform;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static DeviceInfo Info
		{
			get
			{
				if (info == null)
					throw new InvalidOperationException("You must call Microsoft.Maui.Controls.Forms.Init(); prior to using this property.");
				return info;
			}
			set { info = value; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetFlowDirection(FlowDirection value) => FlowDirection = value;
		public static FlowDirection FlowDirection { get; internal set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static bool IsInvokeRequired
		{
			get { return PlatformServices.IsInvokeRequired; }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IPlatformServices PlatformServices
		{
			get
			{
				if (s_platformServices == null)
					throw new InvalidOperationException("You must call Microsoft.Maui.Controls.Forms.Init(); prior to using this property.");
				return s_platformServices;
			}
			set { s_platformServices = value; }
		}

		public static IPlatformInvalidate PlatformInvalidator { get; set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static IReadOnlyList<string> Flags { get; private set; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static void SetFlags(IReadOnlyList<string> flags)
		{
			Flags = flags;
		}

		public static void BeginInvokeOnMainThread(Action action)
		{
			PlatformServices.BeginInvokeOnMainThread(action);
		}

		public static Task<T> InvokeOnMainThreadAsync<T>(Func<T> func)
		{
			var tcs = new TaskCompletionSource<T>();
			BeginInvokeOnMainThread(() =>
			{
				try
				{
					var result = func();
					tcs.SetResult(result);
				}
				catch (Exception ex)
				{
					tcs.SetException(ex);
				}
			});
			return tcs.Task;
		}

		public static Task InvokeOnMainThreadAsync(Action action)
		{
			object wrapAction()
			{ action(); return null; }
			return InvokeOnMainThreadAsync((Func<object>)wrapAction);
		}

		public static Task<T> InvokeOnMainThreadAsync<T>(Func<Task<T>> funcTask)
		{
			var tcs = new TaskCompletionSource<T>();
			BeginInvokeOnMainThread(
				async () =>
				{
					try
					{
						var ret = await funcTask().ConfigureAwait(false);
						tcs.SetResult(ret);
					}
					catch (Exception e)
					{
						tcs.SetException(e);
					}
				}
			);

			return tcs.Task;
		}

		public static Task InvokeOnMainThreadAsync(Func<Task> funcTask)
		{
			async Task<object> wrapFunction()
			{ await funcTask().ConfigureAwait(false); return null; }
			return InvokeOnMainThreadAsync(wrapFunction);
		}

		public static async Task<SynchronizationContext> GetMainThreadSynchronizationContextAsync()
		{
			SynchronizationContext ret = null;
			await InvokeOnMainThreadAsync(() =>
				ret = SynchronizationContext.Current
			).ConfigureAwait(false);
			return ret;
		}

		public static double GetNamedSize(NamedSize size, Element targetElement)
		{
			return GetNamedSize(size, targetElement.GetType());
		}

		public static double GetNamedSize(NamedSize size, Type targetElementType)
		{
			return GetNamedSize(size, targetElementType, false);
		}

		public static void StartTimer(TimeSpan interval, Func<bool> callback)
		{
			PlatformServices.StartTimer(interval, callback);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Assembly[] GetAssemblies()
		{
			return PlatformServices.GetAssemblies();
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
		{
			return PlatformServices.GetNamedSize(size, targetElementType, useOldSizes);
		}

		public static Color GetNamedColor(string name)
		{
			return PlatformServices.GetNamedColor(name);
		}

		internal static Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken)
		{
			return PlatformServices.GetStreamAsync(uri, cancellationToken);
		}

		public static class Styles
		{
			public static readonly string TitleStyleKey = "TitleStyle";

			public static readonly string SubtitleStyleKey = "SubtitleStyle";

			public static readonly string BodyStyleKey = "BodyStyle";

			public static readonly string ListItemTextStyleKey = "ListItemTextStyle";

			public static readonly string ListItemDetailTextStyleKey = "ListItemDetailTextStyle";

			public static readonly string CaptionStyleKey = "CaptionStyle";

			public static readonly Style TitleStyle = new Style(typeof(Label)) { BaseResourceKey = TitleStyleKey };

			public static readonly Style SubtitleStyle = new Style(typeof(Label)) { BaseResourceKey = SubtitleStyleKey };

			public static readonly Style BodyStyle = new Style(typeof(Label)) { BaseResourceKey = BodyStyleKey };

			public static readonly Style ListItemTextStyle = new Style(typeof(Label)) { BaseResourceKey = ListItemTextStyleKey };

			public static readonly Style ListItemDetailTextStyle = new Style(typeof(Label)) { BaseResourceKey = ListItemDetailTextStyleKey };

			public static readonly Style CaptionStyle = new Style(typeof(Label)) { BaseResourceKey = CaptionStyleKey };
		}

		public static void Invalidate(VisualElement visualElement)
		{
			PlatformInvalidator?.Invalidate(visualElement);
		}
	}
}
