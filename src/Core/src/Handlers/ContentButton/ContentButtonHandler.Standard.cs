using System;

namespace Microsoft.Maui.Handlers
{
	public partial class ContentButtonHandler
	{
		protected override object CreatePlatformView() => throw new NotImplementedException();

		public static partial void MapContent(IContentButtonHandler handler, IContentButton button) { }
		public static partial void MapBackground(IContentButtonHandler handler, IContentButton button) { }
		public static partial void MapIsEnabled(IContentButtonHandler handler, IContentButton button) { }
		public static partial void MapPadding(IContentButtonHandler handler, IContentButton button) { }
		public static partial void MapStrokeColor(IContentButtonHandler handler, IContentButton button) { }
		public static partial void MapStrokeThickness(IContentButtonHandler handler, IContentButton button) { }
		public static partial void MapCornerRadius(IContentButtonHandler handler, IContentButton button) { }
	}
}
