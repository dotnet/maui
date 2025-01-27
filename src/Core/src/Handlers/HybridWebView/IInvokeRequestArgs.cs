using System;
using System.Text.Json.Serialization.Metadata;

namespace Microsoft.Maui.Handlers;

#pragma warning disable RS0016 // Add public types and members to the declared API

public interface IInvokeRequestArgs
{
	T? GetArgument<T>(int index, JsonTypeInfo<T> info);

	void SetReturn(object value, JsonTypeInfo info);

	void SetException(Exception exception);
}
