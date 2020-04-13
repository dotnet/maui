using System;

namespace Xamarin.Forms
{
	public class AppThemeColor : BindableObject
	{
		public AppThemeColor()
		{
			ExperimentalFlags.VerifyFlagEnabled(nameof(AppThemeColor), ExperimentalFlags.AppThemeExperimental, nameof(AppThemeColor));

			Application.Current.RequestedThemeChanged += RequestedThemeChanged;
		}

		public static readonly BindableProperty LightProperty = BindableProperty.Create(nameof(Light), typeof(Color), typeof(AppThemeColor), default(Color), propertyChanged: (bo, __, ___) => UpdateActualValue(bo));

		public Color Light
		{
			get => (Color)GetValue(LightProperty);
			set => SetValue(LightProperty, value);
		}

		public static readonly BindableProperty DarkProperty = BindableProperty.Create(nameof(Dark), typeof(Color), typeof(AppThemeColor), default(Color), propertyChanged: (bo, __, ___) => UpdateActualValue(bo));

		public Color Dark
		{
			get => (Color)GetValue(DarkProperty);
			set => SetValue(DarkProperty, value);
		}

		public static readonly BindableProperty DefaultProperty = BindableProperty.Create(nameof(Default), typeof(Color), typeof(AppThemeColor), default(Color), propertyChanged: (bo, __, ___) => UpdateActualValue(bo));

		public Color Default
		{
			get => (Color)GetValue(DefaultProperty);
			set => SetValue(DefaultProperty, value);
		}

		private Color _value;
		public Color Value
		{
			get => _value;
			private set
			{
				_value = value;
				OnPropertyChanged();
			}
		}

		public static implicit operator Color(AppThemeColor appThemeColor)
		{
			switch (Application.Current?.RequestedTheme)
			{
				default:
				case OSAppTheme.Light:
					return appThemeColor.IsSet(LightProperty) ? appThemeColor.Light : (appThemeColor.IsSet(DefaultProperty) ? appThemeColor.Default : default(Color));
				case OSAppTheme.Dark:
					return appThemeColor.IsSet(DarkProperty) ? appThemeColor.Dark : (appThemeColor.IsSet(DefaultProperty) ? appThemeColor.Default : default(Color));
			}
		}

		static void UpdateActualValue(BindableObject bo)
		{
			var appThemeColor = bo as AppThemeColor;
			switch (Application.Current?.RequestedTheme)
			{
				default:
				case OSAppTheme.Light:
					appThemeColor.Value = appThemeColor.IsSet(LightProperty) ? appThemeColor.Light : (appThemeColor.IsSet(DefaultProperty) ? appThemeColor.Default : default(Color));
					break;
				case OSAppTheme.Dark:
					appThemeColor.Value = appThemeColor.IsSet(DarkProperty) ? appThemeColor.Dark : (appThemeColor.IsSet(DefaultProperty) ? appThemeColor.Default : default(Color));
					break;
			}
		}

		void RequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
		{
			UpdateActualValue(this);
		}
	}
}