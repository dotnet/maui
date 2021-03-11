namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class SearchBarStub : StubBase, ISearchBar
	{
		private string _text;

		public string Text { get => _text; set => SetProperty(ref _text, value); }
	}
}