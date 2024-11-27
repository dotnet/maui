#nullable enable
using System;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.VisualRunner
{
	class DiagnosticMessageSink : LongLivedMarshalByRefObject, Xunit.Abstractions.IMessageSink
	{
		string _assemblyDisplayName;
		Action<string> _logger;
		bool _show;
		public DiagnosticMessageSink(Action<string> logger, string assemblyDisplayName, bool showDiagnostics)
		{
			_logger = logger;
			_assemblyDisplayName = assemblyDisplayName;
			_show = showDiagnostics;
		}

		public bool OnMessage(IMessageSinkMessage message)
		{
			if (_show && _logger is not null)
			{
				_logger($"{_assemblyDisplayName}: {(message as DiagnosticMessage)?.Message}");
			}
			return true;
		}
	}
}