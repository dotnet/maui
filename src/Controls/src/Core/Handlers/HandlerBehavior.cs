using Microsoft.Maui;

namespace Microsoft.Maui.Controls
{
#pragma warning disable RS0016 // Add public types and members to the declared API
	public static class HandlerBehavior
	{
        public static readonly BindableProperty DisconnectionPolicyProperty = BindableProperty.CreateAttached(
            "DisconnectionPolicy",
            typeof(HandlerDisconnectPolicy),
            typeof(HandlerBehavior),
            HandlerDisconnectPolicy.Automatic);

        public static void SetDisconnectionPolicy(BindableObject target, HandlerDisconnectPolicy value)
        {
            target.SetValue(DisconnectionPolicyProperty, value);
        }

        public static HandlerDisconnectPolicy GetDisconnectionPolicy(BindableObject target)
        {
            return (HandlerDisconnectPolicy)target.GetValue(DisconnectionPolicyProperty);
        }
    }

    
#pragma warning restore RS0016 // Add public types and members to the declared API
}