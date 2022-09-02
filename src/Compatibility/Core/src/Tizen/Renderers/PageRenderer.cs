using System;
using Microsoft.Maui.Controls.Platform;
using ViewGroup = Tizen.UIExtensions.NUI.ViewGroup;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	/// <summary>
	/// Renderer of ContentPage.
	/// </summary>
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class PageRenderer : VisualElementRenderer<Page>
	{
		ViewGroup _page;

		protected override void OnElementChanged(ElementChangedEventArgs<Page> e)
		{
			if (null == _page)
			{
				_page = new ViewGroup();
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

		protected override void UpdateLayout()
		{
			// empty on purpose
		}


		void OnLayoutUpdated(object sender, global::Tizen.UIExtensions.Common.LayoutEventArgs e)
		{
			Element.Layout(e.Geometry.ToDP());
		}

	}
}
