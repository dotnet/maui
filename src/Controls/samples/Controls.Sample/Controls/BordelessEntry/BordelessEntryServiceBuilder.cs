using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace Maui.Controls.Sample.Controls
{
	internal class BorderlessEntryRegistration
	{
		private readonly Action<BordelessEntryServiceBuilder> _builderAction;

		public BorderlessEntryRegistration(Action<BordelessEntryServiceBuilder> builderAction)
		{
			_builderAction = builderAction;
		}

		internal void RunBuilderAction(BordelessEntryServiceBuilder builder)
		{
			_builderAction(builder);
		}
	}

	class BordelessEntryServiceBuilder
	{
		internal static IMauiHandlersCollection? HandlersCollection;
		internal static readonly Dictionary<Type, Type> PendingHandlers = new();

		public static void TryAddHandler<TType, TTypeRender>()
			where TType : IView
			where TTypeRender : IViewHandler
		{
			if (HandlersCollection == null)
				PendingHandlers[typeof(TType)] = typeof(TTypeRender);
			else
				HandlersCollection.TryAddHandler<TType, TTypeRender>();
		}
	}

	class BorderlessEntryInitializer : IMauiInitializeService
	{
		private readonly IEnumerable<BorderlessEntryRegistration> _borderlessEntryRegistrations;

		public BorderlessEntryInitializer(IEnumerable<BorderlessEntryRegistration> borderlessEntryRegistrations)
		{
			_borderlessEntryRegistrations = borderlessEntryRegistrations;
		}

		public void Initialize(IServiceProvider services)
		{
			var essentialsBuilder = new BordelessEntryServiceBuilder();
			if (_borderlessEntryRegistrations != null)
			{
				foreach (var essentialsRegistration in _borderlessEntryRegistrations)
				{
					essentialsRegistration.RunBuilderAction(essentialsBuilder);
				}
			}

			BordelessEntryServiceBuilder.HandlersCollection ??= services.GetRequiredService<IMauiHandlersFactory>().GetCollection();

			if (BordelessEntryServiceBuilder.PendingHandlers.Count > 0)
			{
				foreach (var pair in BordelessEntryServiceBuilder.PendingHandlers)
				{
					BordelessEntryServiceBuilder.HandlersCollection.TryAddHandler(pair.Key, pair.Value);
				}
				BordelessEntryServiceBuilder.PendingHandlers.Clear();
			}
		}
	}
}
