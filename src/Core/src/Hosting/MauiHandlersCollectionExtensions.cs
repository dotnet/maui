using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Maui.Hosting
{
	public static partial class MauiHandlersCollectionExtensions
	{
		public static IMauiHandlersCollection AddHandler(
			this IMauiHandlersCollection handlersCollection,
			Type viewType,
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType)
		{
			handlersCollection.AddTransient(viewType, handlerType);
			return handlersCollection;
		}

		public static IMauiHandlersCollection AddHandler<TType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TTypeRender>(
			this IMauiHandlersCollection handlersCollection)
			where TType : IElement
			where TTypeRender : IElementHandler
		{
			handlersCollection.AddTransient(typeof(TType), typeof(TTypeRender));
			return handlersCollection;
		}

		public static IMauiHandlersCollection TryAddHandler(
			this IMauiHandlersCollection handlersCollection,
			Type viewType,
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType)
		{
			handlersCollection.TryAddTransient(viewType, handlerType);
			return handlersCollection;
		}

		public static IMauiHandlersCollection TryAddHandler<TType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TTypeRender>(
			this IMauiHandlersCollection handlersCollection)
			where TType : IView
			where TTypeRender : IViewHandler
		{
			handlersCollection.TryAddTransient(typeof(TType), typeof(TTypeRender));
			return handlersCollection;
		}
	}
}