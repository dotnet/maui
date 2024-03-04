namespace Microsoft.Maui.Handlers
{

	public partial class ViewHandler
	{

		[MissingMapper]
		public static void MapTranslationX(IViewHandler handler, IView view) { }

		[MissingMapper]
		public static void MapTranslationY(IViewHandler handler, IView view) { }

		[MissingMapper]
		public static void MapScale(IViewHandler handler, IView view)
		{
			//handler.PlatformView.UpdateScale(view.Scale);
		}

		[MissingMapper]
		public static void MapScaleX(IViewHandler handler, IView view) { }

		[MissingMapper]
		public static void MapScaleY(IViewHandler handler, IView view) { }

		[MissingMapper]
		public static void MapRotation(IViewHandler handler, IView view) { }

		[MissingMapper]
		public static void MapRotationX(IViewHandler handler, IView view) { }

		[MissingMapper]
		public static void MapRotationY(IViewHandler handler, IView view) { }

		[MissingMapper]
		public static void MapAnchorX(IViewHandler handler, IView view) { }

		[MissingMapper]
		public static void MapAnchorY(IViewHandler handler, IView view) { }

		public static void MapToolbar(IViewHandler handler, IView view)
		{
			if (view is IToolbarElement tb)
				MapToolbar(handler, tb);
		}

		internal static void MapToolbar(IElementHandler handler, IToolbarElement tb)
		{
			if (handler.MauiContext is not null)
			{
				var toolbarContainer = handler.MauiContext.GetToolBarContainer();
				if (toolbarContainer is not null)
				{
					toolbarContainer.SetToolbar(tb.Toolbar?.ToPlatform(handler.MauiContext) as MauiToolbar);
				}
			}
		}
	}

}