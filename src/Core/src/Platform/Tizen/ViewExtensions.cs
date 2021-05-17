using ElmSharp;
using ElmSharp.Accessible;

namespace Microsoft.Maui
{
	public static class ViewExtensions
	{
		public static void UpdateIsEnabled(this EvasObject nativeView, IView view)
		{
			if (!(nativeView is Widget widget))
				return;

			widget.IsEnabled = view.IsEnabled;
		}

		public static void UpdateVisibility(this EvasObject nativeView, IView view)
		{
			if (view.Visibility.ToNativeVisibility())
			{
				nativeView.Show();
			}
			else
			{
				nativeView.Hide();
			}
		}

		public static bool ToNativeVisibility(this Visibility visibility)
		{
			return visibility switch
			{
				Visibility.Hidden => false,
				Visibility.Collapsed => false,
				_ => true,
			};
		}

		public static void UpdateBackground(this EvasObject nativeView, IView view)
		{
			//TODO: Need to implement
		}

		public static void UpdateAutomationId(this EvasObject nativeView, IView view)
		{
			{
				//TODO: EvasObject.AutomationId is supported from tizen60.
				//nativeView.AutomationId = view.AutomationId;
			}
		}

		public static void UpdateSemantics(this EvasObject nativeView, IView view)
		{
			var semantics = view.Semantics;
			var accessibleObject = nativeView as IAccessibleObject;

			if (semantics == null || accessibleObject == null)
				return;

			accessibleObject.Name = semantics.Description;
			accessibleObject.Description = semantics.Hint;
		}

		public static void InvalidateMeasure(this EvasObject nativeView, IView view)
		{
			nativeView.MarkChanged();
		}

		public static void UpdateWidth(this EvasObject nativeView, IView view)
		{
			if (view.Width == -1)
			{
				// Ignore the initial set of the height; the initial layout will take care of it
				return;
			}

			UpdateGeometry(nativeView, view);
		}

		public static void UpdateHeight(this EvasObject nativeView, IView view)
		{
			if (view.Height == -1)
			{
				// Ignore the initial set of the height; the initial layout will take care of it
				return;
			}

			UpdateGeometry(nativeView, view);
		}

		public static void UpdateGeometry(EvasObject nativeView, IView view)
		{
			// Updating the frame (assuming it's an actual change) will kick off a layout update
			// Handling of the default (-1) width/height will be taken care of by GetDesiredSize
			var currentGeometry = nativeView.Geometry;
			nativeView.Geometry = new Rect(currentGeometry.X, currentGeometry.Y, (int)view.Width, (int)view.Height);
		}
	}
}
