using System;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen
{
	/// <summary>
	/// Renderer of ContentPage.
	/// </summary>
	public class PageRenderer : VisualElementRenderer<Page>
	{
		/// <summary>
		/// Native control which holds the contents.
		/// </summary>
		Native.Page _page;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public PageRenderer()
		{
			RegisterPropertyHandler(Page.BackgroundImageProperty, UpdateBackgroundImage);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
		{
			if (null == _page)
			{
				_page = new Native.Page(Forms.NativeParent);
				_page.LayoutUpdated += OnLayoutUpdated;
				SetNativeView(_page);
			}
			base.OnElementChanged(e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (_page != null)
				{
					_page.LayoutUpdated -= OnLayoutUpdated;
				}
			}
			base.Dispose(disposing);
		}

		protected override void UpdateBackgroundColor(bool initialize)
		{
			if (initialize && Element.BackgroundColor.IsDefault)
				return;

			// base.UpdateBackgroundColor() is not called on purpose, we don't want the regular background setting
			if (Element.BackgroundColor.IsDefault || Element.BackgroundColor.A == 0)
				_page.Color = EColor.Transparent;
			else
				_page.Color = Element.BackgroundColor.ToNative();
		}

		protected override void UpdateLayout()
		{
			// empty on purpose
		}

		void UpdateBackgroundImage(bool initiaize)
		{
			if (initiaize && string.IsNullOrWhiteSpace(Element.BackgroundImage))
				return;

			if (string.IsNullOrWhiteSpace(Element.BackgroundImage))
				_page.File = null;
			else
				_page.File = ResourcePath.GetPath(Element.BackgroundImage);
		}

		void OnLayoutUpdated(object sender, Native.LayoutEventArgs e)
		{
			Element.Layout(e.Geometry.ToDP());
		}
	}
}
