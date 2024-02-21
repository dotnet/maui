using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public interface ITextInputStub : ITextInput, ITextAlignment
	{
		public event EventHandler<(string OldValue, string NewValue)> TextChanged;

		// The use of 'new' here was necessary to write generic tests that look at TextAlignment without implementing a TextInput stub
		new TextAlignment HorizontalTextAlignment { get; set; }

		new TextAlignment VerticalTextAlignment { get; set; }
	}
}
