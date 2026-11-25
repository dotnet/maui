using System.Runtime.CompilerServices;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace Microsoft.Maui.Platform
{
	public static class ProgressBarExtensions
	{
		static readonly ConditionalWeakTable<ProgressBar, object> s_originalForegrounds = new();

		public static void UpdateProgress(this ProgressBar platformProgressBar, IProgress progress)
		{
			platformProgressBar.Value = progress.Progress;
		}

		public static void UpdateProgressColor(this ProgressBar platformProgressBar, IProgress progress)
		{
			Color progressColor = progress.ProgressColor;

			if (progressColor is null)
			{
				if (s_originalForegrounds.TryGetValue(platformProgressBar, out var originalForeground))
				{
					s_originalForegrounds.Remove(platformProgressBar);
					platformProgressBar.RestoreForeground(originalForeground);
				}
			}
			else
			{
				s_originalForegrounds.GetValue(platformProgressBar, static pb => pb.ReadLocalValue(Control.ForegroundProperty));
				platformProgressBar.Foreground = progressColor.ToPlatform();
			}
		}

		static void RestoreForeground(this ProgressBar platformProgressBar, object originalForeground)
		{
			if (originalForeground == DependencyProperty.UnsetValue)
				platformProgressBar.ClearValue(Control.ForegroundProperty);
			else if (originalForeground is BindingExpression bindingExpression)
				platformProgressBar.SetBinding(Control.ForegroundProperty, bindingExpression.ParentBinding);
			else
				platformProgressBar.SetValue(Control.ForegroundProperty, originalForeground);
		}
	}
}
