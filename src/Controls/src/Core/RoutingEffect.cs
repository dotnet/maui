#nullable disable
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <summary>Platform-independent effect that wraps an inner effect, which is usually platform-specific.</summary>
	public class RoutingEffect : Effect
	{
		internal readonly Effect Inner;

		/// <summary>Creates a new routing effect with the specified effect ID.</summary>
		/// <param name="effectId">The ID for the effect.</param>
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