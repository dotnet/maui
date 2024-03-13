using System;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public partial class ListViewHandler : ViewHandler<ListView, MauiListView>
	{
		public static IPropertyMapper<ListView, ListViewHandler> Mapper = new PropertyMapper<ListView, ListViewHandler>(
			ViewRenderer.VisualElementRendererMapper) { [VisualElement.BackgroundProperty.PropertyName] = MapBackground, [ListView.SelectedItemProperty.PropertyName] = MapSelectedItem, };

		public static CommandMapper<ListView, ListViewHandler> CommandMapper = new(ViewRenderer.VisualElementRendererCommandMapper) { };

		public ListViewHandler() : base(Mapper, CommandMapper) { }

		public ListViewHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper) { }

		public ListViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

		[MissingMapper]
		protected override MauiListView CreatePlatformView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a {nameof(ListView)}");

			var view = new MauiListView() { };

			return view;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
		}

		[MissingMapper]
		public static void MapSelectedItem(ListViewHandler handler, IView view)
		{
			if (handler.PlatformView is not { } platformView)
				return;

			UpdateSelectedItem(handler);
		}

		[MissingMapper]
		static void UpdateSelectedItem(ListViewHandler handler)
		{
			var platformView = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			var mauiContext = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			var virtualView = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
		}
	}
}