using System;

namespace Microsoft.Maui.Handlers;

internal class HybridWebViewInvokeJavaScriptException : Exception
{
	private readonly string? _stackTrace;

	public HybridWebViewInvokeJavaScriptException()
		: base()
	{
	}

	public HybridWebViewInvokeJavaScriptException(string? message)
		: base(message)
	{
	}

	public HybridWebViewInvokeJavaScriptException(string? message, Exception? innerException)
		: base(message, innerException)
	{
	}

	public HybridWebViewInvokeJavaScriptException(string? message, string? name, string? stackTrace)
		: base(message)
	{
		if (!string.IsNullOrWhiteSpace(name))
		{
			Data["JavaScriptErrorName"] = name;
		}

		_stackTrace = stackTrace;
	}

	public override string? StackTrace => _stackTrace ?? base.StackTrace;
}
