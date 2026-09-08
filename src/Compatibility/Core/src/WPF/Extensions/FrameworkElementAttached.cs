using System.Windows;


namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF.Extensions
{
	public class FrameworkElementAttached : FrameworkElement
	{
		public static readonly DependencyProperty PriorityProperty = DependencyProperty.RegisterAttached(
			"Priority",
			typeof(int),
			typeof(FrameworkElementAttached),
			new FrameworkPropertyMetadata(0)
		);
		public static void SetPriority(FrameworkElement element, int value)
		{
			element.SetValue(PriorityProperty, value);
		}
		public static int GetPriority(FrameworkElement element)
		{
			return (int)element.GetValue(PriorityProperty);
		}
	}
}
