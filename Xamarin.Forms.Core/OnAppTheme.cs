using System;

namespace Xamarin.Forms
{
	public class OnAppTheme<T> : BindableObject, IDisposable
	{
		public OnAppTheme()
		{
			Application.Current.RequestedThemeChanged += RequestedThemeChanged;
		}

		public static readonly BindableProperty LightProperty = BindableProperty.Create(nameof(Light), typeof(T), typeof(OnAppTheme<T>), default(T), propertyChanged: (bo, __, ___) => UpdateActualValue(bo));

		public T Light
		{
			get => (T)GetValue(LightProperty);
			set => SetValue(LightProperty, value);
		}

		public static readonly BindableProperty DarkProperty = BindableProperty.Create(nameof(Dark), typeof(T), typeof(OnAppTheme<T>), default(T), propertyChanged: (bo, __, ___) => UpdateActualValue(bo));

		public T Dark
		{
			get => (T)GetValue(DarkProperty);
			set => SetValue(DarkProperty, value);
		}

		public static readonly BindableProperty DefaultProperty = BindableProperty.Create(nameof(Default), typeof(T), typeof(OnAppTheme<T>), default(T), propertyChanged: (bo, __, ___) => UpdateActualValue(bo));

		public T Default
		{
			get => (T)GetValue(DefaultProperty);
			set => SetValue(DefaultProperty, value);
		}

		public static implicit operator T(OnAppTheme<T> onAppTheme)
		{
			switch (Application.Current?.RequestedTheme)
			{
				default:
				case AppTheme.Light:
					return onAppTheme.IsSet(LightProperty) ? onAppTheme.Light : (onAppTheme.IsSet(DefaultProperty) ? onAppTheme.Default : default(T));
				case AppTheme.Dark:
					return onAppTheme.IsSet(DarkProperty) ? onAppTheme.Dark : (onAppTheme.IsSet(DefaultProperty) ? onAppTheme.Default : default(T));
			}
		}

		private T _actualValue;
		public T ActualValue
		{
			get => _actualValue;
			private set
			{
				_actualValue = value;
				OnPropertyChanged();
			}
		}

		public void Dispose()
		{
			Application.Current.RequestedThemeChanged -= RequestedThemeChanged;
		}

		static void UpdateActualValue(BindableObject bo)
		{
			var appThemeColor = bo as OnAppTheme<T>;
			switch (Application.Current?.RequestedTheme)
			{
				default:
				case AppTheme.Light:
					appThemeColor.ActualValue = appThemeColor.IsSet(LightProperty) ? appThemeColor.Light : (appThemeColor.IsSet(DefaultProperty) ? appThemeColor.Default : default(T));
					break;
				case AppTheme.Dark:
					appThemeColor.ActualValue = appThemeColor.IsSet(DarkProperty) ? appThemeColor.Dark : (appThemeColor.IsSet(DefaultProperty) ? appThemeColor.Default : default(T));
					break;
			}
		}

		void RequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
		{
			UpdateActualValue(this);
		}
	}
}