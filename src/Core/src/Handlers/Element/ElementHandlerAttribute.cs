using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Handlers;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
internal class ElementHandlerAttribute(
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type handlerType)
	: Attribute
{
	[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "The DynamicallyAccessedMembers annotation ensures constructor availability.")]
	public virtual IElementHandler CreateHandler(IMauiContext context)
		=> (IElementHandler)Activator.CreateInstance(handlerType)!;

	public virtual Type HandlerType => handlerType;
}

#if ANDROID
internal sealed class ElementHandlerWithAndroidContextAttribute<THandler> : ElementHandlerAttribute
	where THandler : IElementHandler, IElementHandlerWithAndroidContext<THandler>
{
	public ElementHandlerWithAndroidContextAttribute() : base(typeof(THandler)) { }

	public override IElementHandler CreateHandler(IMauiContext context)
		=> THandler.CreateHandler(context.Context!); // TODO: revisit nullability

	public override Type HandlerType => typeof(THandler);
}
#endif
