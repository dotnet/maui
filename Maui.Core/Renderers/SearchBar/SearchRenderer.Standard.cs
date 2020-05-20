using System;
using System.Collections.Generic;
using System.Text;

namespace System.Maui.Platform
{
	public partial class SearchRenderer : AbstractViewRenderer<ISearch, object>
	{
		protected override object CreateView() => throw new NotImplementedException();

		public static void MapPropertyColor(IViewRenderer renderer, ITextInput entry) { }
		public static void MapPropertyPlaceholder(IViewRenderer renderer, ITextInput entry) { }
		public static void MapPropertyPlaceholderColor(IViewRenderer renderer, ITextInput entry) { }
		public static void MapPropertyText(IViewRenderer renderer, ITextInput entry) { }
		public static void MapPropertyCancelColor(IViewRenderer renderer, ISearch search) { }
		public static void MapPropertyMaxLength(IViewRenderer renderer, ITextInput view) { }
		public static void MapPropertyBackgroundColor(IViewRenderer renderer, IView view) { }
	}
}
