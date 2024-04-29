namespace Microsoft.Maui.DeviceTests.Stubs
{
	public class ElementStub : IElement
	{
		bool IElement.IsPlatformViewNew { get; set; }
		public IElement Parent { get; set; }

		public IElementHandler Handler { get; set; }
	}
}
