namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class ElementStub : IElement
	{
		public IElement Parent { get; set; }

		public IElementHandler Handler { get; set; }
	}
}
