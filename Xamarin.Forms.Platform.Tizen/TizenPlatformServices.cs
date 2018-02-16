using ElmSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms.Internals;
using TAppControl = Tizen.Applications.AppControl;

namespace Xamarin.Forms.Platform.Tizen
{
	internal class TizenPlatformServices : IPlatformServices
	{
		static MD5 checksum = MD5.Create();

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
			// In case of TV profile The base named size sholud be lager than mobile profile
			if (Device.Idiom != TargetIdiom.Phone)
			{
				switch (size)
				{
					case NamedSize.Micro:
						return Forms.ConvertToDPFont(24);
					case NamedSize.Small:
						return Forms.ConvertToDPFont(26);
					case NamedSize.Default:
					case NamedSize.Medium:
						return Forms.ConvertToDPFont(28);
					case NamedSize.Large:
						return Forms.ConvertToDPFont(84);
					default:
						throw new ArgumentOutOfRangeException();
				}

			}

			double baseSize = Forms.ConvertToDPFont(19);
			double baseSizeSpan = 3;
			switch (size)
			{
				case NamedSize.Micro:
					return baseSize;
				case NamedSize.Small:
					return baseSize + baseSizeSpan;
				case NamedSize.Default:
				case NamedSize.Medium:
					return baseSize + (baseSizeSpan * 2);
				case NamedSize.Large:
					return baseSize + (baseSizeSpan * 4);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public void OpenUriAction(Uri uri)
		{
			if (uri == null || uri.AbsoluteUri == null)
			{
				throw new ArgumentNullException(nameof(uri));
			}
			TAppControl tAppControl = new TAppControl() { Operation = "%", Uri = uri.AbsoluteUri };
			var matchedApplications = TAppControl.GetMatchedApplicationIds(tAppControl);
			if (matchedApplications.Count() > 0)
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

		static readonly char[] HexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f' };
		public string GetMD5Hash(string input)
		{
			byte[] bin = checksum.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
			char[] hex = new char[32];
			for (var i = 0; i < 16; ++i)
			{
				hex[2 * i] = HexDigits[bin[i] >> 4];
				hex[2 * i + 1] = HexDigits[bin[i] & 0xf];
			}
			return new string(hex);
		}

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

			List<Assembly> _assemblies;

			public static bool IsTizenSpecificAvailable { get; private set; }

			static AppDomain()
			{
				CurrentDomain = new AppDomain();
			}

			AppDomain()
			{
				_assemblies = new List<Assembly>();

				// Add this renderer assembly to the list
				_assemblies.Add(GetType().GetTypeInfo().Assembly);
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
							if (refName.Name == "Xamarin.Forms.Core")
							{
								if (refAsm.GetType("Xamarin.Forms.PlatformConfiguration.TizenSpecific.VisualElement") != null)
								{
									IsTizenSpecificAvailable = true;
								}
							}
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
	}
}

