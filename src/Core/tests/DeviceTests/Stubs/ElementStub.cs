namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class ElementStub : IElement
	{
		public IElement Parent { get; set; }

		public IElementHandler Handler { get; set; }

		public IElementHandler GetElementHandler(IMauiContext context) => throw new System.NotImplementedException();

		public System.Type GetElementHandlerType() => throw new System.NotImplementedException();
	}
}
