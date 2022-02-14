using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Essentials
{
	public partial class SemanticScreenReaderImplementation : ISemanticScreenReader
	{
		public void Announce(string text) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
