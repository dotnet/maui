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
using CoreFoundation;
using Foundation;
using UIKit;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;

namespace Xamarin.Forms
{
	public static class Forms
	{
		//Preserve GetCallingAssembly
		static readonly bool nevertrue = false;

		static bool? s_isiOS7OrNewer;

		static bool? s_isiOS8OrNewer;

		static bool? s_isiOS9OrNewer;

		static Forms()
		{
			if (nevertrue)
				Assembly.GetCallingAssembly();
		}

		public static bool IsInitialized { get; private set; }

		internal static bool IsiOS7OrNewer
		{
			get
			{
				if (!s_isiOS7OrNewer.HasValue)
					s_isiOS7OrNewer = UIDevice.CurrentDevice.CheckSystemVersion(7, 0);
				return s_isiOS7OrNewer.Value;
			}
		}

		internal static bool IsiOS8OrNewer
		{
			get
			{
				if (!s_isiOS8OrNewer.HasValue)
					s_isiOS8OrNewer = UIDevice.CurrentDevice.CheckSystemVersion(8, 0);
				return s_isiOS8OrNewer.Value;
			}
		}

		internal static bool IsiOS9OrNewer
		{
			get
			{
				if (!s_isiOS9OrNewer.HasValue)
					s_isiOS9OrNewer = UIDevice.CurrentDevice.CheckSystemVersion(9, 0);
				return s_isiOS9OrNewer.Value;
			}
		}

		public static void Init()
		{
			if (IsInitialized)
				return;
			IsInitialized = true;
			Color.Accent = Color.FromRgba(50, 79, 133, 255);

			Log.Listeners.Add(new DelegateLogListener((c, m) => Trace.WriteLine(m, c)));

			Device.OS = TargetPlatform.iOS;
			Device.PlatformServices = new IOSPlatformServices();
			Device.Info = new IOSDeviceInfo();

			Registrar.RegisterAll(new[] { typeof(ExportRendererAttribute), typeof(ExportCellAttribute), typeof(ExportImageSourceHandlerAttribute) });

			Device.Idiom = UIDevice.CurrentDevice.UserInterfaceIdiom == UIUserInterfaceIdiom.Pad ? TargetIdiom.Tablet : TargetIdiom.Phone;

			ExpressionSearch.Default = new iOSExpressionSearch();
		}

		public static event EventHandler<ViewInitializedEventArgs> ViewInitialized;

		internal static void SendViewInitialized(this VisualElement self, UIView nativeView)
		{
			var viewInitialized = ViewInitialized;
			if (viewInitialized != null)
				viewInitialized(self, new ViewInitializedEventArgs { View = self, NativeView = nativeView });
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

		internal class IOSDeviceInfo : DeviceInfo
		{
			readonly NSObject _notification;
			readonly Size _scaledScreenSize;
			readonly double _scalingFactor;

			public IOSDeviceInfo()
			{
				_notification = UIDevice.Notifications.ObserveOrientationDidChange((sender, args) => CurrentOrientation = UIDevice.CurrentDevice.Orientation.ToDeviceOrientation());

				_scalingFactor = UIScreen.MainScreen.Scale;
				_scaledScreenSize = new Size(UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height);
				PixelScreenSize = new Size(_scaledScreenSize.Width * _scalingFactor, _scaledScreenSize.Height * _scalingFactor);
			}

			public override Size PixelScreenSize { get; }

			public override Size ScaledScreenSize
			{
				get { return _scaledScreenSize; }
			}

			public override double ScalingFactor
			{
				get { return _scalingFactor; }
			}

			protected override void Dispose(bool disposing)
			{
				_notification.Dispose();
				base.Dispose(disposing);
			}
		}

		class IOSPlatformServices : IPlatformServices
		{
			static readonly MD5CryptoServiceProvider Checksum = new MD5CryptoServiceProvider();

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
				var bytes = Checksum.ComputeHash(Encoding.UTF8.GetBytes(input));
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
				switch (size)
				{
					case NamedSize.Default:
						return typeof(Button).IsAssignableFrom(targetElementType) ? 15 : 17;
					case NamedSize.Micro:
						return 12;
					case NamedSize.Small:
						return 14;
					case NamedSize.Medium:
						return 17;
					case NamedSize.Large:
						return 22;
					default:
						throw new ArgumentOutOfRangeException("size");
				}
			}

			public async Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken)
			{
				using (var client = GetHttpClient())
				using (var response = await client.GetAsync(uri, cancellationToken))
					return await response.Content.ReadAsStreamAsync();
			}

			public IIsolatedStorageFile GetUserStoreForApplication()
			{
				return new _IsolatedStorageFile(IsolatedStorageFile.GetUserStoreForApplication());
			}

			public bool IsInvokeRequired
			{
				get { return !NSThread.IsMain; }
			}

			public void OpenUriAction(Uri uri)
			{
				UIApplication.SharedApplication.OpenUrl(new NSUrl(uri.AbsoluteUri));
			}

			public void StartTimer(TimeSpan interval, Func<bool> callback)
			{
				NSTimer timer = null;
				timer = NSTimer.CreateRepeatingScheduledTimer(interval, t =>
				{
					if (!callback())
						t.Invalidate();
				});
				NSRunLoop.Main.AddTimer(timer, NSRunLoopMode.Common);
			}

			HttpClient GetHttpClient()
			{
				var proxy = CFNetwork.GetSystemProxySettings();
				var handler = new HttpClientHandler();
				if (!string.IsNullOrEmpty(proxy.HTTPProxy))
				{
					handler.Proxy = CFNetwork.GetDefaultProxy();
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

			public class _Timer : ITimer
			{
				readonly Timer _timer;

				public _Timer(Timer timer)
				{
					_timer = timer;
				}

				public void Change(int dueTime, int period)
				{
					_timer.Change(dueTime, period);
				}

				public void Change(long dueTime, long period)
				{
					_timer.Change(dueTime, period);
				}

				public void Change(TimeSpan dueTime, TimeSpan period)
				{
					_timer.Change(dueTime, period);
				}

				public void Change(uint dueTime, uint period)
				{
					_timer.Change(dueTime, period);
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
					Stream stream = _isolatedStorageFile.OpenFile(path, (System.IO.FileMode)mode, (System.IO.FileAccess)access);
					return Task.FromResult(stream);
				}

				public Task<Stream> OpenFileAsync(string path, FileMode mode, FileAccess access, FileShare share)
				{
					Stream stream = _isolatedStorageFile.OpenFile(path, (System.IO.FileMode)mode, (System.IO.FileAccess)access, (System.IO.FileShare)share);
					return Task.FromResult(stream);
				}
			}
		}
	}
}