using Android.Widget;
using AScrollView = Android.Widget.ScrollView;
using AButton = Android.Widget.Button;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android
{
	public class ToolbarRenderer : ViewRenderer
	{
		public ToolbarRenderer()
		{
			AutoPackage = false;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<View> e)
		{
			base.OnElementChanged(e);

			if (e.OldElement == null)
			{
				var layout = new LinearLayout(Context);
				layout.SetBackgroundColor(new Color(0.2, 0.2, 0.2, 0.5).ToAndroid());

				layout.Orientation = Orientation.Horizontal;

				SetNativeControl(layout);
			}
			else
			{
				var oldToolbar = (Toolbar)e.OldElement;
				oldToolbar.ItemAdded -= OnToolbarItemsChanged;
				oldToolbar.ItemRemoved -= OnToolbarItemsChanged;
			}

			UpdateChildren();

			var toolbar = (Toolbar)e.NewElement;
			toolbar.ItemAdded += OnToolbarItemsChanged;
			toolbar.ItemRemoved += OnToolbarItemsChanged;
		}

		void OnToolbarItemsChanged(object sender, ToolbarItemEventArgs e)
		{
			UpdateChildren();
		}

		void UpdateChildren()
		{
			RemoveAllViews();

			foreach (ToolbarItem child in ((Toolbar)Element).Items)
			{
				AView view = null;

				if (!string.IsNullOrEmpty(child.Icon))
					view = new ToolbarImageButton(Context, child);
				else
					view = new AButton(Context);

				using(var param = new LinearLayout.LayoutParams(LayoutParams.WrapContent, (int)Context.ToPixels(48), 1))
					((LinearLayout)Control).AddView(view, param);
			}
		}
	}
}