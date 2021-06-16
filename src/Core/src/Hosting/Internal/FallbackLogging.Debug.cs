#nullable enable
#define DEBUG

using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Microsoft.Maui.Hosting.Internal
{
	partial class FallbackLoggerFactory
	{
		partial class FallbackLogger : ILogger
		{
			void DebugWriteLine(string message)
			{
				Debug.WriteLine(message, category: _categoryName);
			}
		}
	}
}