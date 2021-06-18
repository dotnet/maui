using System;

namespace Xunit.Runners.Sinks
{
    class DiagnosticMessageSink : DiagnosticEventSink
    {
        public DiagnosticMessageSink(Action<string> logger, string assemblyDisplayName, bool showDiagnostics)
        {
            if (showDiagnostics && logger != null)
                DiagnosticMessageEvent += args => logger(($"{assemblyDisplayName}: {args.Message.Message}"));
        }
    }
}
