using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.ExternalBackend
{
	/// <summary>
	/// Compile-time proof that an assembly outside of dotnet/maui can participate in the strongly typed
	/// handler contracts using its own native types.
	/// </summary>
	/// <remarks>
	/// Every conversion below is an implicit reference conversion. If any of them stopped compiling, an
	/// external platform backend would have lost the ability to be recognized by .NET MAUI.
	/// </remarks>
	public static class ExternalBackendContracts
	{
		/// <summary>
		/// The backend's own native type flows out of the contract, strongly typed.
		/// </summary>
		public static IViewHandler<ILabel, FakeNativeLabel> TypedLabelHandler(FakeLabelHandler handler) =>
			handler;

		/// <summary>
		/// Covariance lets consumers match any backend's label handler platform-neutrally.
		/// </summary>
		public static IViewHandler<ILabel, object> NeutralLabelHandler(FakeLabelHandler handler) =>
			handler;

		public static IViewHandler<IContentView, FakeNativeContentView> TypedContentViewHandler(FakeContentViewHandler handler) =>
			handler;

		public static IViewHandler<IContentView, object> NeutralContentViewHandler(FakeContentViewHandler handler) =>
			handler;

		public static ILayoutHandler<FakeNativeLayoutView> TypedLayoutHandler(FakeLayoutHandler handler) =>
			handler;

		public static ILayoutHandler<object> NeutralLayoutHandler(FakeLayoutHandler handler) =>
			handler;

		/// <summary>
		/// A window is an element, not a view, so it uses the element-level contract.
		/// </summary>
		public static IElementHandler<IWindow, FakeNativeWindow> TypedWindowHandler(FakeWindowHandler handler) =>
			handler;

		public static IElementHandler<IWindow, object> NeutralWindowHandler(FakeWindowHandler handler) =>
			handler;

		/// <summary>
		/// The neutral contracts still expose the base handler surface.
		/// </summary>
		public static IViewHandler AsViewHandler(IViewHandler<ILabel, object> handler) => handler;

		/// <summary>
		/// The neutral contracts still expose the element handler surface.
		/// </summary>
		public static IElementHandler AsElementHandler(IElementHandler<IWindow, object> handler) => handler;

		/// <summary>
		/// Reads the strongly typed members without any cast or ambiguity.
		/// </summary>
		public static string? ReadThroughContract(IViewHandler<ILabel, FakeNativeLabel> handler)
		{
			ILabel virtualView = handler.VirtualView;
			FakeNativeLabel platformView = handler.PlatformView;

			return virtualView.Text ?? platformView.Text;
		}

		/// <summary>
		/// Reads the platform-neutral members without any cast or ambiguity.
		/// </summary>
		public static object ReadThroughNeutralContract(IViewHandler<ILabel, object> handler)
		{
			ILabel virtualView = handler.VirtualView;

			return virtualView.Text is { Length: > 0 } ? virtualView : handler.PlatformView;
		}

		/// <summary>
		/// The in-box mapper infrastructure accepts an external backend's handler type.
		/// </summary>
		public static IPropertyMapper<ILabel, IViewHandler<ILabel, object>> NeutralMapper() =>
			new PropertyMapper<ILabel, IViewHandler<ILabel, object>>(ViewHandler.ViewMapper);
	}
}
