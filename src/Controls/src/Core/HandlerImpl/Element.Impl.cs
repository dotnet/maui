﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls.Hosting;

namespace Microsoft.Maui.Controls
{
	public partial class Element : Maui.IElement, IEffectControlProvider
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

		void SetHandler(IElementHandler newHandler)
		{
			if (newHandler == _handler)
				return;

			var previousHandler = _handler;

			OnHandlerChangingCore(new HandlerChangingEventArgs(previousHandler, newHandler));

			_handler = newHandler;

			if (_handler?.VirtualView != this)
				_handler?.SetVirtualView(this);

			OnHandlerChangedCore();
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
	}
}