using System.ComponentModel;

namespace Xamarin.Forms
{
	public class RoutingEffect : Effect
	{
		internal readonly Effect Inner;

		protected RoutingEffect(string effectId)
		{
			Inner = Resolve(effectId);
		}

		protected override void OnAttached()
		{
		}

		protected override void OnDetached()
		{
		}

		internal override void ClearEffect()
		{
			Inner?.ClearEffect();
		}

		internal override void SendAttached()
		{
			Inner?.SendAttached();
		}

		internal override void SendDetached()
		{
			Inner?.SendDetached();
		}

		internal override void SendOnElementPropertyChanged(PropertyChangedEventArgs args)
		{
			Inner?.SendOnElementPropertyChanged(args);
		}
	}
}