using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/Element.xml" path="Type[@FullName='Microsoft.Maui.Controls.Element']/Docs/*" />
	public partial class Element : Maui.IElement, IEffectControlProvider, IToolTipElement, IContextFlyoutElement
	{
		IElementHandler _handler;
		EffectsFactory _effectsFactory;

		Maui.IElement Maui.IElement.Parent => Parent;
		EffectsFactory EffectsFactory => _effectsFactory ??= Handler.MauiContext.Services.GetRequiredService<EffectsFactory>();

		public IElementHandler Handler
		{
			get => _handler;
			set => SetHandler(value);
		}

		public event EventHandler<HandlerChangingEventArgs> HandlerChanging;
		public event EventHandler HandlerChanged;

		protected virtual void OnHandlerChanging(HandlerChangingEventArgs args) { }

		protected virtual void OnHandlerChanged() { }

		private protected virtual void OnHandlerChangedCore()
		{
			EffectControlProvider = (Handler != null) ? this : null;
			HandlerChanged?.Invoke(this, EventArgs.Empty);

			OnHandlerChanged();
		}

		private protected virtual void OnHandlerChangingCore(HandlerChangingEventArgs args)
		{
			HandlerChanging?.Invoke(this, args);
			OnHandlerChanging(args);
		}

		IElementHandler _previousHandler;
		void SetHandler(IElementHandler newHandler)
		{
			if (newHandler == _handler)
				return;

			try
			{
				// If a handler is getting changed before the end of this method
				// Something is wired up incorrectly
				if (_previousHandler != null)
					throw new InvalidOperationException("Handler is already being set elsewhere");

				_previousHandler = _handler;

				OnHandlerChangingCore(new HandlerChangingEventArgs(_previousHandler, newHandler));

				_handler = newHandler;

				// Only call disconnect if the previous handler is still connected to this virtual view.
				// If a handler is being reused for a different VirtualView then the virtual
				// view would have already rolled 
				if (_previousHandler?.VirtualView == this)
					_previousHandler?.DisconnectHandler();

				if (_handler?.VirtualView != this)
					_handler?.SetVirtualView(this);

				OnHandlerChangedCore();
			}
			finally
			{
				_previousHandler = null;
			}
		}

		void IEffectControlProvider.RegisterEffect(Effect effect)
		{
			if (effect is RoutingEffect re && re.Inner != null)
			{
				re.Element = this;
				re.Inner.Element = this;
				return;
			}

			var platformEffect = EffectsFactory.CreateEffect(effect);

			if (platformEffect != null)
			{
				platformEffect.Element = this;
				effect.PlatformEffect = platformEffect;
			}
			else
			{
				effect.Element = this;
			}
		}

		ToolTip IToolTipElement.ToolTip => ToolTipProperties.GetToolTip(this);
		IFlyout IContextFlyoutElement.ContextFlyout => FlyoutBase.GetContextFlyout(this);
	}
}
