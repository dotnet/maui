using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Accessibility
{
	partial class SemanticScreenReaderImplementation : ISemanticScreenReader
	{
		static void PlatformAnnounce(string text) =>
			throw ExceptionUtils.NotSupportedOrImplementedException;
	}
}
