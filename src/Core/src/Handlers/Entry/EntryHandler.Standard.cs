using System;

namespace Microsoft.Maui.Handlers
{
	public partial class EntryHandler : AbstractViewHandler<IEntry, object>
	{
		protected override object CreateNativeView() => throw new NotImplementedException();

		public static void MapText(IViewHandler handler, IEntry entry) { }
		public static void MapTextColor(IViewHandler handler, IEntry entry) { }
		public static void MapIsPassword(IViewHandler handler, IEntry entry) { }
	}
}