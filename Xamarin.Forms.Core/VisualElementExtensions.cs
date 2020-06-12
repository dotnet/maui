// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace Xamarin.Forms
{
	public static class VisualElementExtensions
	{
		public static void SetOnAppTheme<T>(this VisualElement self, BindableProperty targetProperty, T light, T dark)
		{
			ExperimentalFlags.VerifyFlagEnabled(nameof(BindableObjectExtensions), ExperimentalFlags.AppThemeExperimental, nameof(BindableObjectExtensions), nameof(SetOnAppTheme));
			self.SetBinding(targetProperty, new AppThemeBinding { Light = light, Dark = dark});
		}

		public static void SetAppThemeColor(this VisualElement self, BindableProperty targetProperty, Color light, Color dark) => SetOnAppTheme(self, targetProperty, light, dark);
	}
}