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

		new TextAlignment HorizontalTextAlignment { get; set; }
		
		new TextAlignment VerticalTextAlignment { get; set; }
	}
}
