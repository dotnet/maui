using System.ComponentModel;
using Microsoft.Maui.Controls.Platform;
using NViewGroup = Tizen.UIExtensions.NUI.ViewGroup;
using TLayoutEventArgs = Tizen.UIExtensions.Common.LayoutEventArgs;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	/// <summary>
	/// Renderer of a Layout.
	/// </summary>
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class LayoutRenderer : ViewRenderer<Layout, NViewGroup>, ILayoutRenderer
	{
		bool _layoutUpdatedRegistered = false;

		public void RegisterOnLayoutUpdated()
		{
			if (!_layoutUpdatedRegistered)
			{
				Control.LayoutUpdated += OnLayoutUpdated;
				_layoutUpdatedRegistered = true;
			}
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Layout> e)
		{
			if (null == Control)
			{
				SetNativeControl(new NViewGroup());
			}
			base.OnElementChanged(e);

			RegisterOnLayoutUpdated();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);
			if (e.PropertyName == Layout.CascadeInputTransparentProperty.PropertyName)
			{
				UpdateInputTransparent(false);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_layoutUpdatedRegistered)
				{
					Control.LayoutUpdated -= OnLayoutUpdated;
					_layoutUpdatedRegistered = false;
				}
			}
			base.Dispose(disposing);
		}

		protected override void UpdateInputTransparent(bool initialize)
		{
			if (initialize && Element.InputTransparent == default(bool))
			{
				return;
			}


			if (initialize)
			{
				// On initialize time, GestureDetector was not created even if GestureRecognizer was existed
				Application.Current.Dispatcher.Dispatch(() => UpdateInputTransparent(false));
				return;
			}

			if (Element.InputTransparent)
			{
				// Disabling event
				if (Element.CascadeInputTransparent)
				{
					// disabling all event including child
					NativeView.Sensitive = false;
				}
				else
				{
					// Child can get event, but layout blocking all event
					// acutally, it allow event on view, so we need to manually block a event.
					NativeView.Sensitive = true;
				}
				// Disabling gesture detecting on layout
				if (GestureDetector != null)
					GestureDetector.IsEnabled = false;
			}
			else
			{
				if (GestureDetector != null)
					GestureDetector.IsEnabled = true;
				NativeView.Sensitive = true;
			}
		}

		protected override void UpdateLayout()
		{
			if (!_layoutUpdatedRegistered)
			{
				base.UpdateLayout();
			}
			else
			{
				ApplyTransformation();
			}
		}

		void OnLayoutUpdated(object sender, TLayoutEventArgs e)
		{
			var bound = e.Geometry.ToDP();
			bound.X = Element.X;
			bound.Y = Element.Y;
			Element.Layout(bound);
		}
	}
}
