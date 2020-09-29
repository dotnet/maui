using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using ElmSharp;
using Xamarin.Forms.Internals;
using TAppControl = Tizen.Applications.AppControl;

namespace Xamarin.Forms.Platform.Tizen
{
	internal class TizenPlatformServices : IPlatformServices
	{
		static SynchronizationContext s_context;

		public TizenPlatformServices()
		{
			s_context = SynchronizationContext.Current;
		}

		public class TizenTicker : Ticker
		{
			readonly Timer _timer;

			public TizenTicker()
			{
				_timer = new Timer((object o) => HandleElapsed(o), this, Timeout.Infinite, Timeout.Infinite);
			}

			protected override void EnableTimer()
			{
				_timer.Change(16, 16);
			}

			protected override void DisableTimer()
			{
				_timer.Change(-1, -1);
			}

			void HandleElapsed(object state)
			{
				s_context.Post((o) => SendSignals(-1), null);
			}
		}
		#region IPlatformServices implementation

		public double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes)
		{
			int pt;
			// Actual font size depends on the target idiom.
			switch (size)
			{
				case NamedSize.Micro:
					pt = Device.Idiom == TargetIdiom.TV || Device.Idiom == TargetIdiom.Watch ? 24 : 19;
					break;
				case NamedSize.Small:
					pt = Device.Idiom == TargetIdiom.TV ? 26 : (Device.Idiom == TargetIdiom.Watch ? 30 : 22);
					break;
				case NamedSize.Default:
				case NamedSize.Medium:
					pt = Device.Idiom == TargetIdiom.TV ? 28 : (Device.Idiom == TargetIdiom.Watch ? 32 : 25);
					break;
				case NamedSize.Large:
					pt = Device.Idiom == TargetIdiom.TV ? 32 : (Device.Idiom == TargetIdiom.Watch ? 36 : 31);
					break;
				case NamedSize.Body:
					pt = Device.Idiom == TargetIdiom.TV ? 30 : (Device.Idiom == TargetIdiom.Watch ? 32 : 28);
					break;
				case NamedSize.Caption:
					pt = Device.Idiom == TargetIdiom.TV ? 26 : (Device.Idiom == TargetIdiom.Watch ? 24 : 22);
					break;
				case NamedSize.Header:
					pt = Device.Idiom == TargetIdiom.TV ? 84 : (Device.Idiom == TargetIdiom.Watch ? 36 : 138);
					break;
				case NamedSize.Subtitle:
					pt = Device.Idiom == TargetIdiom.TV ? 30 : (Device.Idiom == TargetIdiom.Watch ? 30 : 28);
					break;
				case NamedSize.Title:
					pt = Device.Idiom == TargetIdiom.TV ? 42 : (Device.Idiom == TargetIdiom.Watch ? 36 : 40);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(size));
			}
			return Forms.ConvertToDPFont(pt);
		}

		public Color GetNamedColor(string name)
		{
			// Not supported on this platform
			return Color.Default;
		}

		public void OpenUriAction(Uri uri)
		{
			if (uri == null || uri.AbsoluteUri == null)
			{
				throw new ArgumentNullException(nameof(uri));
			}
			TAppControl tAppControl = new TAppControl() { Operation = "%", Uri = uri.AbsoluteUri };
			var matchedApplications = TAppControl.GetMatchedApplicationIds(tAppControl);
			if (matchedApplications.Any())
			{
				TAppControl.SendLaunchRequest(tAppControl);
				return;
			}
			throw new PlatformNotSupportedException();
		}

		public void BeginInvokeOnMainThread(Action action)
		{
			s_context.Post((o) => action(), null);
		}

		public Ticker CreateTicker()
		{
			return new TizenTicker();
		}

		public void StartTimer(TimeSpan interval, Func<bool> callback)
		{
			Timer timer = null;
			bool invoking = false;
			TimerCallback onTimeout = o =>
			{
				if (!invoking)
				{
					invoking = true;
					BeginInvokeOnMainThread(() =>
						{
							if (!callback())
							{
								timer.Dispose();
							}
							invoking = false;
						}
					);
				}
			};
			timer = new Timer(onTimeout, null, Timeout.Infinite, Timeout.Infinite);
			// set interval separarately to prevent calling onTimeout before `timer' is assigned
			timer.Change(interval, interval);
		}

		public async Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken)
		{
			using (var client = new HttpClient())
			using (HttpResponseMessage response = await client.GetAsync(uri, cancellationToken))
				return await response.Content.ReadAsStreamAsync();
		}

		public Assembly[] GetAssemblies()
		{
			return AppDomain.CurrentDomain.GetAssemblies();
		}

		public IIsolatedStorageFile GetUserStoreForApplication()
		{
			return new TizenIsolatedStorageFile();
		}

		public string GetHash(string input) => Crc64.GetHash(input);

		string IPlatformServices.GetMD5Hash(string input) => GetHash(input);

		public void QuitApplication()
		{
			Forms.Context.Exit();
		}

		public bool IsInvokeRequired
		{
			get
			{
				return !EcoreMainloop.IsMainThread;
			}
		}

		public string RuntimePlatform => Device.Tizen;

		#endregion

		// In .NETCore, AppDomain is not supported. The list of the assemblies should be generated manually.
		internal class AppDomain
		{
			public static AppDomain CurrentDomain { get; private set; }

			readonly HashSet<Assembly> _assemblies;

			static AppDomain()
			{
				CurrentDomain = new AppDomain();
			}

			AppDomain()
			{
				_assemblies = new HashSet<Assembly>();

				// Add this renderer assembly to the list
				_assemblies.Add(GetType().GetTypeInfo().Assembly);
			}

			public void AddAssembly(Assembly assembly)
			{
				if (!_assemblies.Contains(assembly))
				{
					_assemblies.Add(assembly);
				}
			}

			public void AddAssemblies(Assembly[] assemblies)
			{
				foreach (var asm in assemblies)
				{
					AddAssembly(asm);
				}
			}

			internal void RegisterAssemblyRecursively(Assembly asm)
			{
				if (_assemblies.Contains(asm))
					return;

				_assemblies.Add(asm);

				foreach (var refName in asm.GetReferencedAssemblies())
				{
					if (!refName.Name.StartsWith("System.") && !refName.Name.StartsWith("Microsoft.") && !refName.Name.StartsWith("mscorlib"))
					{
						try
						{
							Assembly refAsm = Assembly.Load(refName);
							RegisterAssemblyRecursively(refAsm);
						}
						catch
						{
							Log.Warn("Reference Assembly can not be loaded. {0}", refName.FullName);
						}
					}
				}
			}

			public Assembly[] GetAssemblies()
			{
				return _assemblies.ToArray();
			}
		}

		public SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			return Platform.GetNativeSize(view, widthConstraint, heightConstraint);
		}

		public OSAppTheme RequestedTheme => OSAppTheme.Unspecified;

		static MD5 CreateChecksum()
		{
			return MD5.Create();
		}
	}
}