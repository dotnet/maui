using Microsoft.Maui.Handlers;
namespace Microsoft.Maui.UnitTests
{
	class ViewHandlerStub : ViewHandler<IViewStub, NativeViewStub>
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
