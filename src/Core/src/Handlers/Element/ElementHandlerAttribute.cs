using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Handlers;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
internal class ElementHandlerAttribute(
	[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type handlerType)
	: Attribute
{
	[UnconditionalSuppressMessage("Trimming", "IL2026:Members annotated with 'RequiresUnreferencedCodeAttribute' require dynamic access otherwise can break functionality when trimming application code",
		Justification = "Handlers are expected to have parameterless constructors and the DynamicallyAccessedMembers annotation ensures the constructor is preserved during trimming.")]
	[UnconditionalSuppressMessage("AOT", "IL3050:RequiresDynamicCode",
		Justification = "Handlers are expected to have parameterless constructors and the DynamicallyAccessedMembers annotation ensures the constructor is preserved during trimming.")]
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
