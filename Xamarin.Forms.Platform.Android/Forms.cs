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

namespace Xamarin.Forms
{
	public static class Forms
	{
		const int TabletCrossover = 600;

		static bool? s_supportsProgress;

		static bool? s_isLollipopOrNewer;

		public static Context Context { get; internal set; }

		public static bool IsInitialized { get; private set; }

		internal static bool IsLollipopOrNewer
		{
			get
			{
				if (!s_isLollipopOrNewer.HasValue)
					s_isLollipopOrNewer = (int)Build.VERSION.SdkInt >= 21;
				return s_isLollipopOrNewer.Value;
			}
		}

		internal static bool SupportsProgress
		{
			get
			{
				var activity = Context as Activity;
				if (!s_supportsProgress.HasValue)
				{
					int progressCircularId = Context.Resources.GetIdentifier("progress_circular", "id", "android");
					if (progressCircularId > 0 && activity != null)
						s_supportsProgress = activity.FindViewById(progressCircularId) != null;
					else
						s_supportsProgress = true;
				}
				return s_supportsProgress.Value;
			}
		}

		internal static AndroidTitleBarVisibility TitleBarVisibility { get; set; }

		// Provide backwards compat for Forms.Init and AndroidActivity
		// Why is bundle a param if never used?
		public static void Init(Context activity, Bundle bundle)
		{
			Assembly resourceAssembly = Assembly.GetCallingAssembly();
			SetupInit(activity, resourceAssembly);
		}

		public static void Init(Context activity, Bundle bundle, Assembly resourceAssembly)
		{
			SetupInit(activity, resourceAssembly);
		}

		/// <summary>
		/// Sets title bar visibility programmatically. Must be called after Xamarin.Forms.Forms.Init() method
		/// </summary>
		/// <param name="visibility">Title bar visibility enum</param>
		public static void SetTitleBarVisibility(AndroidTitleBarVisibility visibility)
		{
			if((Activity)Context == null)
				throw new NullReferenceException("Must be called after Xamarin.Forms.Forms.Init() method");

			TitleBarVisibility = visibility;

			if (TitleBarVisibility == AndroidTitleBarVisibility.Never)
			{
				if (!((Activity)Context).Window.Attributes.Flags.HasFlag(WindowManagerFlags.Fullscreen))
					((Activity)Context).Window.AddFlags(WindowManagerFlags.Fullscreen);
			}
			else
			{
				if (((Activity)Context).Window.Attributes.Flags.HasFlag(WindowManagerFlags.Fullscreen))
					((Activity)Context).Window.ClearFlags(WindowManagerFlags.Fullscreen);
			}
		}

		public static event EventHandler<ViewInitializedEventArgs> ViewInitialized;

		internal static void SendViewInitialized(this VisualElement self, global::Android.Views.View nativeView)
		{
			EventHandler<ViewInitializedEventArgs> viewInitialized = ViewInitialized;
			if (viewInitialized != null)
				viewInitialized(self, new ViewInitializedEventArgs { View = self, NativeView = nativeView });
		}

		static void SetupInit(Context activity, Assembly resourceAssembly)
		{
			Context = activity;

			ResourceManager.Init(resourceAssembly);

			Color.SetAccent(GetAccentColor());

			if (!IsInitialized)
				Internals.Log.Listeners.Add(new DelegateLogListener((c, m) => Trace.WriteLine(m, c)));

			Device.PlatformServices = new AndroidPlatformServices();

			// use field and not property to avoid exception in getter
			if (Device.info != null)
			{
				((AndroidDeviceInfo)Device.info).Dispose();
				Device.info = null;
			}

			Device.Info = new AndroidDeviceInfo(activity);

			var ticker = Ticker.Default as AndroidTicker;
			if (ticker != null)
				ticker.Dispose();
			Ticker.SetDefault(new AndroidTicker());

			if (!IsInitialized)
			{
				Registrar.RegisterAll(new[] { typeof(ExportRendererAttribute), typeof(ExportCellAttribute), typeof(ExportImageSourceHandlerAttribute) });
			}

			int minWidthDp = Context.Resources.Configuration.SmallestScreenWidthDp;

			Device.SetIdiom(minWidthDp >= TabletCrossover ? TargetIdiom.Tablet : TargetIdiom.Phone);

			if (ExpressionSearch.Default == null)
				ExpressionSearch.Default = new AndroidExpressionSearch();

			IsInitialized = true;
		}

		static Color GetAccentColor()
		{
			Color rc;
			using (var value = new TypedValue())
			{
				if (Context.Theme.ResolveAttribute(global::Android.Resource.Attribute.ColorAccent, value, true) && Forms.IsLollipopOrNewer)	// Android 5.0+
				{
					rc = Color.FromUint((uint)value.Data);
				}
				else if(Context.Theme.ResolveAttribute(Context.Resources.GetIdentifier("colorAccent", "attr", Context.PackageName), value, true))	// < Android 5.0
				{
					rc = Color.FromUint((uint)value.Data);
				}
				else                    // fallback to old code if nothing works (don't know if that ever happens)
				{
					// Detect if legacy device and use appropriate accent color
					// Hardcoded because could not get color from the theme drawable
					var sdkVersion = (int)Build.VERSION.SdkInt;
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

		class AndroidDeviceInfo : DeviceInfo
		{
			bool disposed;
			readonly Context _formsActivity;
			readonly Size _pixelScreenSize;
			readonly double _scalingFactor;

			Orientation _previousOrientation = Orientation.Undefined;

			public AndroidDeviceInfo(Context formsActivity)
			{
				_formsActivity = formsActivity;
				CheckOrientationChanged(_formsActivity.Resources.Configuration.Orientation);
				// This will not be an implementation of IDeviceInfoProvider when running inside the context
				// of layoutlib, which is what the Android Designer does.
				if (_formsActivity is IDeviceInfoProvider)
					((IDeviceInfoProvider) _formsActivity).ConfigurationChanged += ConfigurationChanged;

				using (DisplayMetrics display = formsActivity.Resources.DisplayMetrics)
				{
					_scalingFactor = display.Density;
					_pixelScreenSize = new Size(display.WidthPixels, display.HeightPixels);
					ScaledScreenSize = new Size(_pixelScreenSize.Width / _scalingFactor, _pixelScreenSize.Width / _scalingFactor);
				}
			}

			public override Size PixelScreenSize
			{
				get { return _pixelScreenSize; }
			}

			public override Size ScaledScreenSize { get; }

			public override double ScalingFactor
			{
				get { return _scalingFactor; }
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing && !disposed) {
					disposed = true;
					if (_formsActivity is IDeviceInfoProvider)
						((IDeviceInfoProvider) _formsActivity).ConfigurationChanged -= ConfigurationChanged;
				}
				base.Dispose(disposing);
			}

			void CheckOrientationChanged(Orientation orientation)
			{
				if (!_previousOrientation.Equals(orientation))
					CurrentOrientation = orientation.ToDeviceOrientation();

				_previousOrientation = orientation;
			}

			void ConfigurationChanged(object sender, EventArgs e)
			{
				CheckOrientationChanged(_formsActivity.Resources.Configuration.Orientation);
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

		class AndroidPlatformServices : IPlatformServices
		{
			static readonly MD5CryptoServiceProvider Checksum = new MD5CryptoServiceProvider();
			double _buttonDefaultSize;
			double _editTextDefaultSize;
			double _labelDefaultSize;
			double _largeSize;
			double _mediumSize;

			double _microSize;
			double _smallSize;

			static Handler s_handler;

			public void BeginInvokeOnMainThread(Action action)
			{
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

			public string GetMD5Hash(string input)
			{
				byte[] bytes = Checksum.ComputeHash(Encoding.UTF8.GetBytes(input));
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
				if (_smallSize == 0)
				{
					_smallSize = ConvertTextAppearanceToSize(Resource.Attribute.TextAppearanceSmall, Resource.Style.TextAppearanceDeviceDefaultSmall, 12);
					_mediumSize = ConvertTextAppearanceToSize(Resource.Attribute.TextAppearanceMedium, Resource.Style.TextAppearanceDeviceDefaultMedium, 14);
					_largeSize = ConvertTextAppearanceToSize(Resource.Attribute.TextAppearanceLarge, Resource.Style.TextAppearanceDeviceDefaultLarge, 18);
					_buttonDefaultSize = ConvertTextAppearanceToSize(Resource.Attribute.TextAppearanceButton, Resource.Style.TextAppearanceDeviceDefaultWidgetButton, 14);
					_editTextDefaultSize = ConvertTextAppearanceToSize(Resource.Style.TextAppearanceWidgetEditText, Resource.Style.TextAppearanceDeviceDefaultWidgetEditText, 18);
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
					default:
						throw new ArgumentOutOfRangeException("size");
				}
			}

			public async Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken)
			{
				using (var client = new HttpClient())
				using (HttpResponseMessage response = await client.GetAsync(uri, cancellationToken))
				{
					if (!response.IsSuccessStatusCode)
					{
						Internals.Log.Warning("HTTP Request", $"Could not retrieve {uri}, status code {response.StatusCode}");
						return null;
					}
					return await response.Content.ReadAsStreamAsync();
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
				Context.StartActivity(intent);
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

				if (TryGetTextAppearance(themeDefault, out myValue))
					return myValue;
				if (TryGetTextAppearance(deviceDefault, out myValue))
					return myValue;
				return defaultValue;
			}

			static int Hex(int v)
			{
				if (v < 10)
					return '0' + v;
				return 'a' + v - 10;
			}

			static bool TryGetTextAppearance(int appearance, out double val)
			{
				val = 0;
				try
				{
					using (var value = new TypedValue())
					{
						if (Context.Theme.ResolveAttribute(appearance, value, true))
						{
							var textSizeAttr = new[] { Resource.Attribute.TextSize };
							const int indexOfAttrTextSize = 0;
							using (TypedArray array = Context.ObtainStyledAttributes(value.Data, textSizeAttr))
							{
								val = Context.FromPixels(array.GetDimensionPixelSize(indexOfAttrTextSize, -1));
								return true;
							}
						}
					}
				}
				catch (Exception ex)
				{
					Internals.Log.Warning("Xamarin.Forms.Platform.Android.AndroidPlatformServices", "Error retrieving text appearance: {0}", ex);
				}
				return false;
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

				public Task<Stream> OpenFileAsync(string path, Internals.FileMode mode, Internals.FileAccess access)
				{
					Stream stream = _isolatedStorageFile.OpenFile(path, (System.IO.FileMode)mode, (System.IO.FileAccess)access);
					return Task.FromResult(stream);
				}

				public Task<Stream> OpenFileAsync(string path, Internals.FileMode mode, Internals.FileAccess access, Internals.FileShare share)
				{
					Stream stream = _isolatedStorageFile.OpenFile(path, (System.IO.FileMode)mode, (System.IO.FileAccess)access, (System.IO.FileShare)share);
					return Task.FromResult(stream);
				}
			}
		}
	}
}