using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Accessibility
{
	partial class SemanticScreenReaderImplementation : ISemanticScreenReader
	{
		public void Announce(string text) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
