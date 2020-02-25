using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Xamarin.Forms.Internals;
using Foundation;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;

#if __MOBILE__
using UIKit;
using Xamarin.Forms.Platform.iOS;
using TNativeView = UIKit.UIView;
#else
using AppKit;
using Xamarin.Forms.Platform.MacOS;
using TNativeView = AppKit.NSView;
#endif

namespace Xamarin.Forms
{
	public static class Forms
	{
		public static bool IsInitialized { get; private set; }

#if __MOBILE__
		static bool? s_isiOS9OrNewer;
		static bool? s_isiOS10OrNewer;
		static bool? s_isiOS11OrNewer;
		static bool? s_isiOS13OrNewer;
		static bool? s_respondsTosetNeedsUpdateOfHomeIndicatorAutoHidden;

		internal static bool IsiOS9OrNewer
		{
			get
			{
				if (!s_isiOS9OrNewer.HasValue)
					s_isiOS9OrNewer = UIDevice.CurrentDevice.CheckSystemVersion(9, 0);
				return s_isiOS9OrNewer.Value;
			}
		}


		internal static bool IsiOS10OrNewer
		{
			get
			{
				if (!s_isiOS10OrNewer.HasValue)
					s_isiOS10OrNewer = UIDevice.CurrentDevice.CheckSystemVersion(10, 0);
				return s_isiOS10OrNewer.Value;
			}
		}

		internal static bool IsiOS11OrNewer
		{
			get
			{
				if (!s_isiOS11OrNewer.HasValue)
					s_isiOS11OrNewer = UIDevice.CurrentDevice.CheckSystemVersion(11, 0);
				return s_isiOS11OrNewer.Value;
			}
		}

		internal static bool IsiOS13OrNewer
		{
			get
			{
				if (!s_isiOS13OrNewer.HasValue)
					s_isiOS13OrNewer = UIDevice.CurrentDevice.CheckSystemVersion(13, 0);
				return s_isiOS13OrNewer.Value;
			}
		}

		internal static bool RespondsToSetNeedsUpdateOfHomeIndicatorAutoHidden
		{
			get
			{
				if (!s_respondsTosetNeedsUpdateOfHomeIndicatorAutoHidden.HasValue)
					s_respondsTosetNeedsUpdateOfHomeIndicatorAutoHidden = new UIViewController().RespondsToSelector(new ObjCRuntime.Selector("setNeedsUpdateOfHomeIndicatorAutoHidden"));
				return s_respondsTosetNeedsUpdateOfHomeIndicatorAutoHidden.Value;
			}
		}
#else
		static bool? s_isMojaveOrNewer;

		internal static bool IsMojaveOrNewer
		{
			get
			{
				if (!s_isMojaveOrNewer.HasValue)
					s_isMojaveOrNewer = NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(new NSOperatingSystemVersion(10, 14, 0));
				return s_isMojaveOrNewer.Value;
			}
		}

#endif

		static IReadOnlyList<string> s_flags;
		public static IReadOnlyList<string> Flags => s_flags ?? (s_flags = new List<string>().AsReadOnly());

		public static void SetFlags(params string[] flags)
		{
			if (IsInitialized)
			{
				throw new InvalidOperationException($"{nameof(SetFlags)} must be called before {nameof(Init)}");
			}

			s_flags = flags.ToList().AsReadOnly();
		}

		public static void Init()
		{
			if (IsInitialized)
				return;
			IsInitialized = true;
			Color.SetAccent(Color.FromRgba(50, 79, 133, 255));

			Log.Listeners.Add(new DelegateLogListener((c, m) => Trace.WriteLine(m, c)));

#if __MOBILE__
			Device.SetIdiom(UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? TargetIdiom.Tablet : TargetIdiom.Phone);
			Device.SetFlowDirection(UIApplication.SharedApplication.UserInterfaceLayoutDirection.ToFlowDirection());
#else
			Device.SetIdiom(TargetIdiom.Desktop);
			Device.SetFlowDirection(NSApplication.SharedApplication.UserInterfaceLayoutDirection.ToFlowDirection());
			var mojave = new NSOperatingSystemVersion(10, 14, 0);
			if (NSProcessInfo.ProcessInfo.IsOperatingSystemAtLeastVersion(mojave) &&
				typeof(NSApplication).GetProperty("Appearance") is PropertyInfo appearance &&
				appearance != null)
			{
				var aquaAppearance = NSAppearance.GetAppearance(NSAppearance.NameAqua);
				appearance.SetValue(NSApplication.SharedApplication, aquaAppearance);
			}
#endif
			Device.SetFlags(s_flags);
			Device.PlatformServices = new IOSPlatformServices();
#if __MOBILE__
			Device.Info = new IOSDeviceInfo();
#else
			Device.Info = new Platform.macOS.MacDeviceInfo();
#endif

			Internals.Registrar.RegisterAll(new[]
				{ typeof(ExportRendererAttribute), typeof(ExportCellAttribute), typeof(ExportImageSourceHandlerAttribute), typeof(ExportFontAttribute) });
			ExpressionSearch.Default = new iOSExpressionSearch();
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

		class IOSPlatformServices : IPlatformServices
		{
			readonly double _fontScalingFactor = 1;
			public IOSPlatformServices()
			{
#if __MOBILE__
				//The standard accisibility size for a font is 18, we can get a
				//close aproximation to the new Size by multiplying by this scale factor
				_fontScalingFactor = (double)UIFont.PreferredBody.PointSize / 18f;
#endif
			}

			static readonly MD5CryptoServiceProvider s_checksum = new MD5CryptoServiceProvider();

			public void BeginInvokeOnMainThread(Action action)
			{
				NSRunLoop.Main.BeginInvokeOnMainThread(action.Invoke);
			}

			public Ticker CreateTicker()
			{
				return new CADisplayLinkTicker();
			}

			public Assembly[] GetAssemblies()
			{
				return AppDomain.CurrentDomain.GetAssemblies();
			}

			public string GetMD5Hash(string input)
			{
				var bytes = s_checksum.ComputeHash(Encoding.UTF8.GetBytes(input));
				var ret = new char[32];
				for (var i = 0; i < 16; i++)
				{
					ret[i * 2] = (char)Hex(bytes[i] >> 4);
					ret[i * 2 + 1] = (char)Hex(bytes[i] & 0xf);
				}
				return new string(ret);
			}

			public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
			{
				// We make these up anyway, so new sizes didn't really change
				// iOS docs say default button font size is 15, default label font size is 17 so we use those as the defaults.
				var scalingFactor = _fontScalingFactor;

				if (Application.Current?.On<iOS>().GetEnableAccessibilityScalingForNamedFontSizes() == false)
				{
					scalingFactor = 1;
				}

				switch (size)
				{
					//We multiply the fonts by the scale factor, and cast to an int, to make them whole numbers.
					case NamedSize.Default:
						return (int)((typeof(Button).IsAssignableFrom(targetElementType) ? 15 : 17) * scalingFactor);
					case NamedSize.Micro:
						return (int)(12 * scalingFactor);
					case NamedSize.Small:
						return (int)(14 * scalingFactor);
					case NamedSize.Medium:
						return (int)(17 * scalingFactor);
					case NamedSize.Large:
						return (int)(22 * scalingFactor);
#if __IOS__
					case NamedSize.Body:
						return (double)UIFont.PreferredBody.PointSize;
					case NamedSize.Caption:
						return (double)UIFont.PreferredCaption1.PointSize;
					case NamedSize.Header:
						return (double)UIFont.PreferredHeadline.PointSize;
					case NamedSize.Subtitle:
						return (double)UIFont.PreferredTitle2.PointSize;
					case NamedSize.Title:
						return (double)UIFont.PreferredTitle1.PointSize;
#else
					case NamedSize.Body:
						return 23;
					case NamedSize.Caption:
						return 18;
					case NamedSize.Header:
						return 23;
					case NamedSize.Subtitle:
						return 28;
					case NamedSize.Title:
						return 34;

#endif
					default:
						throw new ArgumentOutOfRangeException("size");
				}
			}

			public async Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken)
			{
				using (var client = GetHttpClient())
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

			public bool IsInvokeRequired => !NSThread.IsMain;

#if __MOBILE__
			public string RuntimePlatform => Device.iOS;
#else
			public string RuntimePlatform => Device.macOS;
#endif

			public void OpenUriAction(Uri uri)
			{
				NSUrl url;

				if (uri.Scheme == "tel" || uri.Scheme == "mailto")
					url = new NSUrl(uri.AbsoluteUri);
				else
					url = NSUrl.FromString(uri.OriginalString) ?? new NSUrl(uri.Scheme, uri.Host, uri.PathAndQuery);
#if __MOBILE__
				UIApplication.SharedApplication.OpenUrl(url);
#else
				NSWorkspace.SharedWorkspace.OpenUrl(url);
#endif
			}

			public void StartTimer(TimeSpan interval, Func<bool> callback)
			{
				NSTimer timer = NSTimer.CreateRepeatingTimer(interval, t =>
				{
					if (!callback())
						t.Invalidate();
				});
				NSRunLoop.Main.AddTimer(timer, NSRunLoopMode.Common);
			}

			HttpClient GetHttpClient()
			{
				var proxy = CoreFoundation.CFNetwork.GetSystemProxySettings();
				var handler = new HttpClientHandler();
				if (!string.IsNullOrEmpty(proxy.HTTPProxy))
				{
					handler.Proxy = CoreFoundation.CFNetwork.GetDefaultProxy();
					handler.UseProxy = true;
				}
				return new HttpClient(handler);
			}

			static int Hex(int v)
			{
				if (v < 10)
					return '0' + v;
				return 'a' + v - 10;
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

			public void QuitApplication()
			{
#if __MOBILE__
				Log.Warning(nameof(IOSPlatformServices), "Platform doesn't implement QuitApp");
#else
				NSApplication.SharedApplication.Terminate(new NSObject());
#endif
			}

			public SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
			{
#if __MOBILE__
				return Platform.iOS.Platform.GetNativeSize(view, widthConstraint, heightConstraint);
#else
				return Platform.MacOS.Platform.GetNativeSize(view, widthConstraint, heightConstraint);
#endif
			}
		}
	}
}
