using AppKit;

namespace System.Maui.Platform
{
	public partial class SearchRenderer : AbstractViewRenderer<ISearch, NSSearchField>
	{

		protected override NSSearchField CreateView()
		{
			var searchBar = new NSSearchField();
			//_defaultTextColor = textView.TextColor;

			//textView.EditingDidEnd += TextViewEditingDidEnd;

			return searchBar;
		}
		public static void MapPropertyColor(IViewRenderer renderer, ITextInput entry)
		{
			(renderer as SearchRenderer)?.UpdateTextColor();
		}

		public static void MapPropertyPlaceholder(IViewRenderer renderer, ITextInput entry)
		{
			if (!(renderer is SearchRenderer searchRenderer))
				return;


			searchRenderer.UpdatePlaceholder();
		}

		public static void MapPropertyPlaceholderColor(IViewRenderer renderer, ITextInput entry)
		{
			if (!(renderer is SearchRenderer searchRenderer))
				return;

			searchRenderer.UpdatePlaceholder();
		}

		public static void MapPropertyText(IViewRenderer renderer, ITextInput view)
		{
			if (!(renderer.NativeView is NSSearchField searchField))
				return;

			if (!(renderer is SearchRenderer searchrenderer))
				return;

			searchField.StringValue = view.Text ?? "";
			searchrenderer.UpdateCancelButton();
		}

		public static void MapPropertyCancelColor(IViewRenderer renderer, ISearch view)
		{
			(renderer as SearchRenderer)?.UpdateCancelButton();
		}

		public static void MapPropertyMaxLength(IViewRenderer renderer, ITextInput view)
		{
			(renderer as SearchRenderer)?.UpdateMaxLength();
		}
		public static void MapPropertyBackgroundColor(IViewRenderer renderer, IView view)
		{

		}

		protected virtual void UpdateMaxLength()
		{

		}

		protected virtual void UpdateTextColor()
		{
			var color = VirtualView.Color;
			//TypedNativeView.TextColor = color.IsDefault ? _defaultTextColor : color.ToNativeColor();
		}

		protected virtual void UpdatePlaceholder()
		{
			var placeholder = VirtualView.Placeholder;

			if (placeholder == null)
			{
				return;
			}

			var targetColor = VirtualView.PlaceholderColor;
			//var color = targetColor.IsDefault ? ColorExtensions.SeventyPercentGrey : targetColor.ToNativeColor();

			//var attributedPlaceholder = new NSAttributedString(str: placeholder, foregroundColor: color);
			//TypedNativeView.AttributedPlaceholder = attributedPlaceholder;
		}

		protected virtual void UpdateCancelButton()
		{

		}
	}
}