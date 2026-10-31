using System;
using Tizen.NUI.BaseComponents;

namespace Microsoft.Maui.Handlers
{
	public partial class ContentButtonHandler
	{
		protected override View CreatePlatformView() =>
			throw new PlatformNotSupportedException("ContentButton is not implemented on Tizen.");

		public static partial void MapContent(IContentButtonHandler handler, IContentButton button) { }
		public static partial void MapBackground(IContentButtonHandler handler, IContentButton button) { }
		public static partial void MapIsEnabled(IContentButtonHandler handler, IContentButton button) { }
		public static partial void MapPadding(IContentButtonHandler handler, IContentButton button) { }
		public static partial void MapStrokeColor(IContentButtonHandler handler, IContentButton button) { }
		public static partial void MapStrokeThickness(IContentButtonHandler handler, IContentButton button) { }
		public static partial void MapCornerRadius(IContentButtonHandler handler, IContentButton button) { }
	}
}
