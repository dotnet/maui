using System;

namespace Microsoft.Maui.Handlers;

internal abstract class ElementHandlerAttribute : Attribute
{
	public abstract IElementHandler CreateHandler();
	public abstract Type HandlerType { get; }
}

internal sealed class ElementHandlerAttribute<THandler> : ElementHandlerAttribute
	where THandler : IElementHandler, new()
{
	public override IElementHandler CreateHandler()
	{
		var handler = new THandler();
		return handler;
	}
	
	public override Type HandlerType => typeof(THandler);
}
