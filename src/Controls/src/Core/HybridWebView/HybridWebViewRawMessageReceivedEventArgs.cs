using System;

namespace Microsoft.Maui.Controls
{
	public class HybridWebViewRawMessageReceivedEventArgs : EventArgs
	{
		public HybridWebViewRawMessageReceivedEventArgs(string? message)
		{
			Message = message;
		}

		public string? Message { get; }
	}
}
