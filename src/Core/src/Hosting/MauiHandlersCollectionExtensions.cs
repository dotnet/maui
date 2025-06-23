using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Hosting.Internal;

namespace Microsoft.Maui.Hosting
{
	public static partial class MauiHandlersCollectionExtensions
	{
		/// <summary>
		/// Registers a handler with the underlying service container via AddTransient.
		/// </summary>
		/// <param name="handlersCollection">The element collection</param>
		/// <param name="viewType">The type of view to register</param>
		/// <param name="handlerType">The handler type that represents the element</param>
		/// <returns>The handler collection</returns>
		public static IMauiHandlersCollection AddHandler(
			this IMauiHandlersCollection handlersCollection,
			Type viewType,
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType)
		{
			if (!typeof(IElement).IsAssignableFrom(viewType) || !typeof(IElementHandler).IsAssignableFrom(handlerType))
				throw new InvalidOperationException($"Unable to add handler mapping for {viewType} and {handlerType}. Please ensure that {viewType} implements {nameof(IElement)} and {handlerType} implements {nameof(IElementHandler)}.");

			handlersCollection.RegisterHandlerServiceType(viewType);
#pragma warning disable RS0030 // Do not use banned APIs, the current method is also banned
			handlersCollection.AddTransient(viewType, handlerType);
#pragma warning restore RS0030 // Do not use banned APIs
			return handlersCollection;
		}

		/// <summary>
		/// Registers a handler with the underlying service container via AddTransient.
		/// </summary>
		/// <typeparam name="TType">The type of element to register</typeparam>
		/// <typeparam name="TTypeRender">The handler type that represents the element</typeparam>
		/// <param name="handlersCollection">The handler collection</param>
		/// <returns>The handler collection</returns>
		public static IMauiHandlersCollection AddHandler<TType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TTypeRender>(
			this IMauiHandlersCollection handlersCollection)
			where TType : IElement
			where TTypeRender : IElementHandler
		{
			handlersCollection.RegisterHandlerServiceType(typeof(TType));
#pragma warning disable RS0030 // Do not use banned APIs, the current method is also banned
			handlersCollection.AddTransient(typeof(TType), typeof(TTypeRender));
#pragma warning restore RS0030 // Do not use banned APIs
			return handlersCollection;
		}

		/// <summary>
		/// Registers a handler with the underlying service container via AddTransient.
		/// </summary>
		/// <typeparam name="TType">The type of element to register</typeparam>
		/// <param name="handlersCollection">The handler collection</param>
		/// <param name="handlerImplementationFactory">A factory method to create the handler</param>
		/// <returns>The handler collection</returns>
		public static IMauiHandlersCollection AddHandler<TType>(
			this IMauiHandlersCollection handlersCollection,
			Func<IServiceProvider, IElementHandler> handlerImplementationFactory)
			where TType : IElement
		{
			handlersCollection.RegisterHandlerServiceType(typeof(TType));
			handlersCollection.AddTransient(typeof(TType), handlerImplementationFactory);
			return handlersCollection;
		}

		/// <summary>
		/// Registers a handler with the underlying service container via AddTransient.
		/// </summary>
		/// <param name="handlersCollection">The handler collection</param>
		/// <param name="viewType">The type of element to register</param>
		/// <param name="handlerType">The handler type that represents the element</param>
		/// <returns>The handler collection</returns>
		public static IMauiHandlersCollection TryAddHandler(
			this IMauiHandlersCollection handlersCollection,
			Type viewType,
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type handlerType)
		{
			if (!typeof(IElement).IsAssignableFrom(viewType) || !typeof(IElementHandler).IsAssignableFrom(handlerType))
				throw new InvalidOperationException($"Unable to add handler mapping for {viewType} and {handlerType}. Please ensure that {viewType} implements {nameof(IElement)} and {handlerType} implements {nameof(IElementHandler)}.");

			handlersCollection.RegisterHandlerServiceType(viewType);
#pragma warning disable RS0030 // Do not use banned APIs, the current method is also banned
			handlersCollection.TryAddTransient(viewType, handlerType);
#pragma warning restore RS0030 // Do not use banned APIs
			return handlersCollection;
		}

		/// <summary>
		/// Registers a handler with the underlying service container via AddTransient.
		/// </summary>
		/// <typeparam name="TType">The type of element to register</typeparam>
		/// <typeparam name="TTypeRender">The handler type that represents the element</typeparam>
		/// <param name="handlersCollection">The handler collection</param>
		/// <returns>The handler collection</returns>
		public static IMauiHandlersCollection TryAddHandler<TType, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TTypeRender>(
			this IMauiHandlersCollection handlersCollection)
			where TType : IView
			where TTypeRender : IViewHandler
		{
			handlersCollection.RegisterHandlerServiceType(typeof(TType));
#pragma warning disable RS0030 // Do not use banned APIs, the current method is also banned
			handlersCollection.TryAddTransient(typeof(TType), typeof(TTypeRender));
#pragma warning restore RS0030 // Do not use banned APIs
			return handlersCollection;
		}

		/// <summary>
		/// Registers a handler with the underlying service container via AddTransient.
		/// </summary>
		/// <typeparam name="TType">The type of element to register</typeparam>
		/// <param name="handlersCollection">The handler collection</param>
		/// <param name="handlerImplementationFactory">A factory method to create the handler</param>
		/// <returns>The handler collection</returns>
		public static IMauiHandlersCollection TryAddHandler<TType>(
			this IMauiHandlersCollection handlersCollection,
			Func<IServiceProvider, IElementHandler> handlerImplementationFactory)
			where TType : IElement
		{
			handlersCollection.RegisterHandlerServiceType(typeof(TType));
			handlersCollection.TryAddTransient(typeof(TType), handlerImplementationFactory);
			return handlersCollection;
		}

		private static void RegisterHandlerServiceType(this IMauiHandlersCollection handlersCollection, Type virtualViewType)
		{
			RegisteredHandlerServiceTypeSet.GetInstance(handlersCollection).Add(virtualViewType);
		}
	}
}