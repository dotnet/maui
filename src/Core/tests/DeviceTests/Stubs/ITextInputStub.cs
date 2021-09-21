using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.DeviceTests.Stubs
{
	public interface ITextInputStub : ITextInput
	{
		public event EventHandler<StubPropertyChangedEventArgs<string>> TextChanged;
	}
}
