using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Graphics;

[assembly: Dependency(typeof(MockDeserializer))]
[assembly: Dependency(typeof(MockResourcesProvider))]
[assembly: Dependency(typeof(MockFontNamedSizeService))]

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	internal class MockPlatformServices : Internals.IPlatformServices
	{
		readonly IDispatcher _dispatcher;
		Func<VisualElement, double, double, SizeRequest> getNativeSizeFunc;
		readonly bool useRealisticLabelMeasure;

		public MockPlatformServices(
			IDispatcher dispatcher = null,
			Func<VisualElement, double, double, SizeRequest> getNativeSizeFunc = null,
			bool useRealisticLabelMeasure = false)
		{
			_dispatcher = dispatcher ?? new MockDispatcher();
			this.getNativeSizeFunc = getNativeSizeFunc;
			this.useRealisticLabelMeasure = useRealisticLabelMeasure;
		}

		public void StartTimer(TimeSpan interval, Func<bool> callback)
		{
			Timer timer = null;
			TimerCallback onTimeout = o => _dispatcher.Dispatch(() =>
			{
				if (callback())
					return;

				timer.Dispose();
			});
			timer = new Timer(onTimeout, null, interval, interval);
		}

		public SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint)
		{
			if (getNativeSizeFunc != null)
				return getNativeSizeFunc(view, widthConstraint, heightConstraint);
			// EVERYTHING IS 100 x 20

			var label = view as Label;
			if (label != null && useRealisticLabelMeasure)
			{
				var letterSize = new Size(5, 10);
				var w = label.Text.Length * letterSize.Width;
				var h = letterSize.Height;
				if (!double.IsPositiveInfinity(widthConstraint) && w > widthConstraint)
				{
					h = ((int)w / (int)widthConstraint) * letterSize.Height;
					w = widthConstraint - (widthConstraint % letterSize.Width);

				}
				return new SizeRequest(new Size(w, h), new Size(Math.Min(10, w), h));
			}

			return new SizeRequest(new Size(100, 20));
		}

		public OSAppTheme RequestedTheme { get; set; }
	}

	internal class MockDeserializer : Internals.IDeserializer
	{
		public Task<IDictionary<string, object>> DeserializePropertiesAsync()
		{
			return Task.FromResult<IDictionary<string, object>>(new Dictionary<string, object>());
		}

		public Task SerializePropertiesAsync(IDictionary<string, object> properties)
		{
			return Task.FromResult(false);
		}
	}

	internal class MockResourcesProvider : Internals.ISystemResourcesProvider
	{
		public Internals.IResourceDictionary GetSystemResources()
		{
			var dictionary = new ResourceDictionary();
			Style style;
			style = new Style(typeof(Label));
			dictionary[Device.Styles.BodyStyleKey] = style;

			style = new Style(typeof(Label));
			style.Setters.Add(Label.FontSizeProperty, 50);
			dictionary[Device.Styles.TitleStyleKey] = style;

			style = new Style(typeof(Label));
			style.Setters.Add(Label.FontSizeProperty, 40);
			dictionary[Device.Styles.SubtitleStyleKey] = style;

			style = new Style(typeof(Label));
			style.Setters.Add(Label.FontSizeProperty, 30);
			dictionary[Device.Styles.CaptionStyleKey] = style;

			style = new Style(typeof(Label));
			style.Setters.Add(Label.FontSizeProperty, 20);
			dictionary[Device.Styles.ListItemTextStyleKey] = style;

			style = new Style(typeof(Label));
			style.Setters.Add(Label.FontSizeProperty, 10);
			dictionary[Device.Styles.ListItemDetailTextStyleKey] = style;

			return dictionary;
		}
	}

	public class MockFontNamedSizeService : IFontNamedSizeService
	{
		public double GetNamedSize(NamedSize size, Type targetElement, bool useOldSizes)
		{
			switch (size)
			{
				case NamedSize.Default:
					return new MockFontManager().DefaultFontSize;
				case NamedSize.Micro:
					return 4;
				case NamedSize.Small:
					return 8;
				case NamedSize.Medium:
					return 12;
				case NamedSize.Large:
					return 16;
				default:
					throw new ArgumentOutOfRangeException(nameof(size));
			}
		}
	}

	public class MockApplication : Application
	{
		public static UnitTestLogger MockLogger;

		public MockApplication()
		{
		}
	}

	internal class MockTicker : Ticker
	{
		bool _enabled;

		public override void Start()
		{
			_enabled = true;

			while (_enabled)
			{
				this.Fire?.Invoke();
			}
		}
		public override void Stop()
		{
			_enabled = false;
		}
	}

	class MockDispatcher : IDispatcher
	{
		public bool IsDispatchRequired => false;

		public bool Dispatch(Action action)
		{
			action();
			return true;
		}
	}

	class MockDeviceInfo : IDeviceInfo
	{
		DeviceIdiom _deviceIdiom;
		TargetIdiom _targetIdiom;
		DevicePlatform _devicePlatform;
		string _runtimePlatform;

		public MockDeviceInfo()
		{
			Platform = DevicePlatform.Unknown;
			Idiom = DeviceIdiom.Unknown;
			DeviceType = DeviceType.Unknown;
		}

		public MockDeviceInfo(DevicePlatform? platform = null, DeviceIdiom? idiom = null, DeviceType? deviceType = null)
		{
			Platform = platform ?? DevicePlatform.Unknown;
			Idiom = idiom ?? DeviceIdiom.Unknown;
			DeviceType = deviceType ?? DeviceType.Unknown;
		}

		public MockDeviceInfo(string platform = null, TargetIdiom idiom = TargetIdiom.Unsupported, DeviceType? deviceType = null)
		{
			RuntimePlatform = platform;
			TargetIdiom = idiom;
			DeviceType = deviceType ?? DeviceType.Unknown;
		}

		public string Model { get; set; }

		public string Manufacturer { get; set; }

		public string Name { get; set; }

		public string VersionString { get; set; }

		public Version Version { get; set; }

		public DevicePlatform Platform
		{
			get => _devicePlatform;
			set
			{
				if (_devicePlatform == value)
					return;

				_devicePlatform = value;
				_runtimePlatform = value.ToString();
			}
		}

		public string RuntimePlatform
		{
			get => _runtimePlatform;
			set
			{
				if (_runtimePlatform == value)
					return;

				_runtimePlatform = value;
				_devicePlatform = DevicePlatform.Create(value);
			}
		}

		public DeviceIdiom Idiom
		{
			get => _deviceIdiom;
			set
			{
				if (_deviceIdiom == value)
					return;

				_deviceIdiom = value;
				if (value == DeviceIdiom.Tablet)
					_targetIdiom = TargetIdiom.Tablet;
				else if (value == DeviceIdiom.Phone)
					_targetIdiom = TargetIdiom.Phone;
				else if (value == DeviceIdiom.Desktop)
					_targetIdiom = TargetIdiom.Desktop;
				else if (value == DeviceIdiom.TV)
					_targetIdiom = TargetIdiom.TV;
				else if (value == DeviceIdiom.Watch)
					_targetIdiom = TargetIdiom.Watch;
				else
					_targetIdiom = TargetIdiom.Unsupported;
			}
		}

		public TargetIdiom TargetIdiom
		{
			get => _targetIdiom;
			set
			{
				if (_targetIdiom == value)
					return;

				_targetIdiom = value;
				_deviceIdiom = value switch
				{
					TargetIdiom.Phone => DeviceIdiom.Phone,
					TargetIdiom.Tablet => DeviceIdiom.Tablet,
					TargetIdiom.Desktop => DeviceIdiom.Desktop,
					TargetIdiom.Watch => DeviceIdiom.Watch,
					TargetIdiom.TV => DeviceIdiom.TV,
					_ => DeviceIdiom.Unknown,
				};
			}
		}

		public DeviceType DeviceType { get; set; }
	}
}