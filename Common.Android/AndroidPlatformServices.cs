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
using AndroidX.Core.Content;

#if __MAUI__
using Label = System.Maui.ILabel;
using Entry = System.Maui.ITextInput;
using Button = System.Maui.IButton;
using Editor = System.Maui.IEditor;
using SearchBar = System.Maui.ISearch;
#endif

namespace Xamarin.Forms
{
	class AndroidPlatformServices : IPlatformServices
	{
		double _buttonDefaultSize;
		double _editTextDefaultSize;
		double _labelDefaultSize;
		double _largeSize;
		double _mediumSize;

		double _microSize;
		double _smallSize;

		static Handler s_handler;

		Context Context
		{
			get
			{
				Context context;
				if (_weakContext.TryGetTarget(out context))
					return context;

				return null;
			}
		}

		readonly WeakReference<Context> _weakContext;

		public AndroidPlatformServices(Context context)
		{
			_weakContext = new WeakReference<Context>(context);
		}

		public void BeginInvokeOnMainThread(Action action)
		{
			if (Context.IsDesignerContext())
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
					color = ContextCompat.GetColor(Context, Resource.Color.BackgroundDark);
					break;
				case NamedPlatformColor.BackgroundLight:
					color = ContextCompat.GetColor(Context, Resource.Color.BackgroundLight);
					break;
				case NamedPlatformColor.Black:
					color = ContextCompat.GetColor(Context, Resource.Color.Black);
					break;
				case NamedPlatformColor.DarkerGray:
					color = ContextCompat.GetColor(Context, Resource.Color.DarkerGray);
					break;
				case NamedPlatformColor.HoloBlueBright:
					color = ContextCompat.GetColor(Context, Resource.Color.HoloBlueBright);
					break;
				case NamedPlatformColor.HoloBlueDark:
					color = ContextCompat.GetColor(Context, Resource.Color.HoloBlueDark);
					break;
				case NamedPlatformColor.HoloBlueLight:
					color = ContextCompat.GetColor(Context, Resource.Color.HoloBlueLight);
					break;
				case NamedPlatformColor.HoloGreenDark:
					color = ContextCompat.GetColor(Context, Resource.Color.HoloGreenDark);
					break;
				case NamedPlatformColor.HoloGreenLight:
					color = ContextCompat.GetColor(Context, Resource.Color.HoloGreenLight);
					break;
				case NamedPlatformColor.HoloOrangeDark:
					color = ContextCompat.GetColor(Context, Resource.Color.HoloOrangeDark);
					break;
				case NamedPlatformColor.HoloOrangeLight:
					color = ContextCompat.GetColor(Context, Resource.Color.HoloOrangeLight);
					break;
				case NamedPlatformColor.HoloPurple:
					color = ContextCompat.GetColor(Context, Resource.Color.HoloPurple);
					break;
				case NamedPlatformColor.HoloRedDark:
					color = ContextCompat.GetColor(Context, Resource.Color.HoloRedDark);
					break;
				case NamedPlatformColor.HoloRedLight:
					color = ContextCompat.GetColor(Context, Resource.Color.HoloRedLight);
					break;
				case NamedPlatformColor.TabIndicatorText:
					color = ContextCompat.GetColor(Context, Resource.Color.TabIndicatorText);
					break;
				case NamedPlatformColor.Transparent:
					color = ContextCompat.GetColor(Context, Resource.Color.Transparent);
					break;
				case NamedPlatformColor.White:
					color = ContextCompat.GetColor(Context, Resource.Color.White);
					break;
				case NamedPlatformColor.WidgetEditTextDark:
					color = ContextCompat.GetColor(Context, Resource.Color.WidgetEditTextDark);
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

		public void QuitApplication()
		{
			Internals.Log.Warning(nameof(AndroidPlatformServices), "Platform doesn't implement QuitApp");
		}


#if !__MAUI__
		// This is Obsolete at the platform level so we should probably delete this
		public SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			return Platform.Android.Platform.GetNativeSize(view, widthConstraint, heightConstraint);
		}
#endif

		public OSAppTheme RequestedTheme
		{
			get
			{
				var nightMode = Context.Resources.Configuration.UiMode & UiMode.NightMask;
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
