using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public partial class PickerStub : StubBase, IPicker
	{
		public string Title { get; set; }

		public IList<string> Items { get; set; } = new List<string>();

		public IList ItemsSource { get; set; }

		public int SelectedIndex { get; set; } = -1;

		public object SelectedItem { get; set; }

		public double CharacterSpacing { get; set; }
	}
}