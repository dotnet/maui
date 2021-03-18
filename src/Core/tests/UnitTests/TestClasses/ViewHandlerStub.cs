using Microsoft.Maui.Handlers;
namespace Microsoft.Maui.Tests
{
	class ViewHandlerStub : AbstractViewHandler<IViewStub, NativeViewStub>
	{
		public static PropertyMapper<IViewStub, ViewHandlerStub> MockViewMapper = new PropertyMapper<IViewStub, ViewHandlerStub>(ViewHandler.ViewMapper)
		{

		};

		public ViewHandlerStub() : base(MockViewMapper)
		{

		}

		public ViewHandlerStub(PropertyMapper mapper = null) : base(mapper ?? MockViewMapper)
		{

		}

		protected override NativeViewStub CreateNativeView() => new NativeViewStub();
	}

}
