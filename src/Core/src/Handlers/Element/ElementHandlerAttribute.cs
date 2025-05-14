using System;

namespace Microsoft.Maui.Handlers;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, Inherited = true, AllowMultiple = false)]
internal abstract class ElementHandlerAttribute : Attribute
{
	public abstract IElementHandler CreateHandler(IMauiContext context);
	public abstract Type HandlerType { get; }
}

internal sealed class ElementHandlerAttribute<THandler> : ElementHandlerAttribute
	where THandler : IElementHandler, new()
{
	public override IElementHandler CreateHandler(IMauiContext context) => new THandler();
	public override Type HandlerType => typeof(THandler);
}

#if ANDROID
internal sealed class ElementHandlerWithAndroidContextAttribute<THandler> : ElementHandlerAttribute
	where THandler : IElementHandler, IElementHandlerWithAndroidContext<THandler>
{
	public override IElementHandler CreateHandler(IMauiContext context)
		=> THandler.CreateHandler(context.Context);

	public override Type HandlerType => typeof(THandler);
}
#endif
