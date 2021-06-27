namespace Microsoft.Maui.Handlers
{
	public partial class WindowHandler
	{
		public static PropertyMapper<IWindow, WindowHandler> WindowMapper = new(ElementHandler.ElementMapper)
		{
			[nameof(IWindow.Title)] = MapTitle,
		};

		public WindowHandler()
			: base(WindowMapper)
		{
		}

		public WindowHandler(PropertyMapper? mapper = null)
			: base(mapper ?? WindowMapper)
		{
		}
	}
}