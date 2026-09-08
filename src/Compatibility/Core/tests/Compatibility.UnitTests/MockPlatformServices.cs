using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Animations;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Graphics;
using NSubstitute;
using NSubstitute.Extensions;

#pragma warning disable CS0612 // Type or member is obsolete
[assembly: Dependency(typeof(MockResourcesProvider))]
[assembly: Dependency(typeof(MockFontNamedSizeService))]
#pragma warning restore CS0612 // Type or member is obsolete

namespace Microsoft.Maui.Controls.Core.UnitTests
{
	[Obsolete]
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

	[Obsolete]
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

		public void NotifyThemeChanged() =>
			(this as IApplication).ThemeChanged();
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

		public bool DispatchDelayed(TimeSpan delay, Action action) =>
			throw new NotImplementedException();

		public IDispatcherTimer CreateTimer() =>
			throw new NotImplementedException();
	}

	class MockDeviceInfo : IDeviceInfo
	{
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

		public string Model { get; set; }

		public string Manufacturer { get; set; }

		public string Name { get; set; }

		public string VersionString { get; set; }

		public Version Version { get; set; }

		public DevicePlatform Platform { get; set; }

		public DeviceIdiom Idiom { get; set; }

		public DeviceType DeviceType { get; set; }
	}

	class MockAppInfo : IAppInfo
	{
		public string PackageName { get; set; }

		public string Name { get; set; }

		public string VersionString { get; set; }

		public Version Version { get; set; }

		public string BuildString { get; set; }

		public LayoutDirection RequestedLayoutDirection { get; set; }

		public void ShowSettingsUI()
		{
		}

		public AppTheme RequestedTheme { get; set; }

		public AppPackagingModel PackagingModel { get; set; }
	}

	static class MockPlatformSizeService
	{
		public static T Sub<T>(
			bool useRealisticLabelMeasure = false,
			LayoutOptions? horizOpts = default,
			LayoutOptions? vertOpts = default,
			bool? visible = default,
			double? width = default,
			double? height = default,
			Thickness? margin = default,
			string text = default,
			TextAlignment? horizText = default,
			LineBreakMode? lineBreak = default,
			Color bgColor = default)
			where T : View
		{
			return Sub<T>(
				(v, w, h) => DefaultLogic(v, w, h, useRealisticLabelMeasure),
				horizOpts,
				vertOpts,
				visible,
				width,
				height,
				margin,
				text,
				horizText,
				lineBreak,
				bgColor);
		}

		public static T Sub<T>(
			Func<VisualElement, double, double, SizeRequest> func,
			LayoutOptions? horizOpts = default,
			LayoutOptions? vertOpts = default,
			bool? visible = default,
			double? width = default,
			double? height = default,
			Thickness? margin = default,
			string text = default,
			TextAlignment? horizText = default,
			LineBreakMode? lineBreak = default,
			Color bgColor = default)
			where T : View
		{
			var element = Substitute.ForPartsOf<T>();
			element.IsPlatformEnabled = true;

			if (horizOpts.HasValue)
				element.HorizontalOptions = horizOpts.Value;
			if (vertOpts.HasValue)
				element.VerticalOptions = vertOpts.Value;
			if (visible.HasValue)
				element.IsVisible = visible.Value;
			if (width.HasValue)
				element.WidthRequest = width.Value;
			if (height.HasValue)
				element.HeightRequest = height.Value;
			if (margin.HasValue)
				element.Margin = margin.Value;
			if (text != null)
			{
				if (element is Label labelElement)
					labelElement.Text = text;
				else if (element is Button buttonElement)
					buttonElement.Text = text;
				else
					throw new ArgumentException("Only Label is supported for text");
			}
			if (horizText.HasValue)
			{
				if (element is Label labelElement)
					labelElement.HorizontalTextAlignment = horizText.Value;
				else
					throw new ArgumentException("Only Label is supported for text");
			}
			if (lineBreak.HasValue)
			{
				if (element is Label labelElement)
					labelElement.LineBreakMode = lineBreak.Value;
				else
					throw new ArgumentException("Only Label is supported for text");
			}
			if (bgColor != default)
				element.BackgroundColor = bgColor;

			var onMeasure = typeof(VisualElement)
				.GetMethod("OnMeasure", BindingFlags.NonPublic | BindingFlags.Instance);

			var configure = element.Configure();

			onMeasure
				.Invoke(configure, new object[] { Arg.Any<double>(), Arg.Any<double>() })
				.Returns(i => func(element, i.ArgAt<double>(0), i.ArgAt<double>(1)));

			return element;
		}

		static SizeRequest DefaultLogic(VisualElement view, double widthConstraint, double heightConstraint, bool useRealisticLabelMeasure)
		{
			if (view is not Label label || !useRealisticLabelMeasure)
				return new SizeRequest(new Size(100, 20));

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
	}
}