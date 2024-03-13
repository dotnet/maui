using System;

namespace Microsoft.Maui.Controls.Handlers.Compatibility
{
	public partial class TableViewHandler : ViewHandler<TableView, MauiTableView>
	{
		public static IPropertyMapper<TableView, TableViewHandler> Mapper = new PropertyMapper<TableView, TableViewHandler>(ViewRenderer.VisualElementRendererMapper) { [VisualElement.BackgroundProperty.PropertyName] = MapBackground, [TableView.RowHeightProperty.PropertyName] = MapRowHeight, };

		public static CommandMapper<TableView, TableViewHandler> CommandMapper = new(ViewRenderer.VisualElementRendererCommandMapper) { };

		public TableViewHandler() : base(Mapper, CommandMapper) { }

		public TableViewHandler(IPropertyMapper? mapper)
			: base(mapper ?? Mapper, CommandMapper) { }

		public TableViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
			: base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

		[MissingMapper]
		protected override MauiTableView CreatePlatformView()
		{
			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a {nameof(TableView)}");

			var view = new MauiTableView() { };

			return view;
		}

		public override void SetVirtualView(IView view)
		{
			base.SetVirtualView(view);

			_ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
			_ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
		}


		[MissingMapper]
		public static void MapRowHeight(TableViewHandler handler, IView view)
		{
			if (handler.PlatformView is not { } platformView)
				return;

			UpdateRowHeight(handler);
		}

		[MissingMapper]
		static void UpdateRowHeight(TableViewHandler handler)
		{
			var platformView = handler.PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
			var mauiContext = handler.MauiContext ?? throw new InvalidOperationException($"{nameof(MauiContext)} should have been set by base class.");
			var virtualView = handler.VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");
		}
	}
}