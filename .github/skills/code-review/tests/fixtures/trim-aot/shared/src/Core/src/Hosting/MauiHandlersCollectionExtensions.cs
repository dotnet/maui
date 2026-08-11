using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Maui.Hosting;

public static partial class MauiHandlersCollectionExtensions
{
	public static IMauiHandlersCollection AddHandler<TType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TTypeRender>(
		this IMauiHandlersCollection handlersCollection)
		where TType : IElement
		where TTypeRender : IElementHandler
	{
		handlersCollection.RegisterHandlerServiceType(typeof(TType));
		handlersCollection.AddTransient(typeof(TType), typeof(TTypeRender));
		return handlersCollection;
	}
}
