using Android.Views;
using AView = Android.Views.View;

namespace Xamarin.Forms.Platform.Android.FastRenderers
{
	internal class EffectControlProvider : IEffectControlProvider
	{
		readonly AView _control;
		readonly ViewGroup _container;

		public EffectControlProvider(AView control)
		{
			_control = control;
			_container = null;
		}

		public EffectControlProvider(AView control, ViewGroup container)
		{
			_control = control;
			_container = container;
		}

		public void RegisterEffect(Effect effect)
		{
			var platformEffect = effect as PlatformEffect;
			if (platformEffect == null)
			{
				return;
			}

			platformEffect.SetControl(_control);
			platformEffect.SetContainer(_container);
		}
	}
}