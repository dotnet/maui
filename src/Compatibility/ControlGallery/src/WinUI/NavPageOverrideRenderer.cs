using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues;
using Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI;
using Microsoft.Maui.Controls.Compatibility.Platform.UWP;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.UI.Xaml.Media.Animation;

[assembly: ExportRenderer(typeof(NavPageOverrideUWP.CustomNavPageForOverride), typeof(NavPageOverrideRenderer))]
namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.WinUI
{
	public class NavPageOverrideRenderer : NavigationPageRenderer
	{
		protected override void OnElementChanged(VisualElementChangedEventArgs e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
				global::System.Diagnostics.Debug.WriteLine($"{e.NewElement.GetType()} is replaced by NavPageOverrideRenderer");
		}

		protected override void SetupPageTransition(Transition transition, bool isAnimated, bool isPopping)
		{
			var newTransition = new EntranceThemeTransition { FromVerticalOffset = 0 };

			if (isPopping)
			{
				newTransition.FromHorizontalOffset = -ContainerElement.ActualWidth;
			}
			else
			{
				newTransition.FromHorizontalOffset = ContainerElement.ActualWidth;
			}

			base.SetupPageTransition(newTransition, isAnimated, isPopping);
		}

		protected override void OnPushRequested(object sender, NavigationRequestedEventArgs e)
		{
			base.OnPushRequested(sender, e);
			global::System.Diagnostics.Debug.WriteLine($"NavPageOverrideRenderer - OnPushRequested");
		}

		protected override void OnPopRequested(object sender, NavigationRequestedEventArgs e)
		{
			base.OnPopRequested(sender, e);
			global::System.Diagnostics.Debug.WriteLine($"NavPageOverrideRenderer - OnPopRequested");
		}

		protected override void OnPopToRootRequested(object sender, NavigationRequestedEventArgs e)
		{
			base.OnPopToRootRequested(sender, e);
			global::System.Diagnostics.Debug.WriteLine($"NavPageOverrideRenderer - OnPopToRootRequested");
		}
	}
}
