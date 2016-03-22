using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class ViewRenderer<TElement, TNativeElement> : VisualElementRenderer<TElement, TNativeElement> where TElement : View where TNativeElement : FrameworkElement
	{
		protected override void OnElementChanged(ElementChangedEventArgs<TElement> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				UpdateBackgroundColor();
			}
		}

		protected override void SetAutomationId(string id)
		{
			if (Control == null)
			{
				base.SetAutomationId(id);
			}
			else
			{
				SetValue(AutomationProperties.AutomationIdProperty, $"{id}_Container");
				Control.SetValue(AutomationProperties.AutomationIdProperty, id);
			}
		}
	}
}