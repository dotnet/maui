namespace Xamarin.Platform.Handlers.Tests
{
	class ViewHandlerStub : AbstractViewHandler<IViewStub, NativeViewStub>
	{
		public static PropertyMapper<IViewStub, ViewHandlerStub> MockViewMapper = new PropertyMapper<IViewStub, ViewHandlerStub>(ViewHandler.ViewMapper)
		{

		};

		public ViewHandlerStub() : base(MockViewMapper)
		{

		}

		public ViewHandlerStub(PropertyMapper mapper) : base(mapper ?? MockViewMapper)
		{

		}

		protected override NativeViewStub CreateNativeView() => new NativeViewStub();
	}

}
