#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/RoutingEffect.xml" path="Type[@FullName='Microsoft.Maui.Controls.RoutingEffect']/Docs/*" />
	public class RoutingEffect : Effect
	{
		internal readonly Effect Inner;

		protected RoutingEffect(string effectId)
		{
			Inner = Resolve(effectId);
		}

		protected RoutingEffect()
		{
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
			PlatformEffect?.ClearEffect();
		}

		internal override void SendAttached()
		{
			Inner?.SendAttached();
			PlatformEffect?.SendAttached();
		}

		internal override void SendDetached()
		{
			Inner?.SendDetached();
			PlatformEffect?.SendDetached();
		}

		internal override void SendOnElementPropertyChanged(PropertyChangedEventArgs args)
		{
			Inner?.SendOnElementPropertyChanged(args);
			PlatformEffect?.SendOnElementPropertyChanged(args);
		}
	}
}