using Microsoft.Maui;

namespace Microsoft.Maui.Controls
{
#pragma warning disable RS0016 // Add public types and members to the declared API
	public static class HandlerBehavior
	{
        public static readonly BindableProperty DisconnectPolicyProperty = BindableProperty.CreateAttached(
            "DisconnectPolicy",
            typeof(HandlerDisconnectPolicy),
            typeof(HandlerBehavior),
            HandlerDisconnectPolicy.Automatic);

        public static void SetDisconnectPolicy(BindableObject target, HandlerDisconnectPolicy value)
        {
            target.SetValue(DisconnectPolicyProperty, value);
        }

        public static HandlerDisconnectPolicy GetDisconnectPolicy(BindableObject target)
        {
            return (HandlerDisconnectPolicy)target.GetValue(DisconnectPolicyProperty);
        }
    }

    
#pragma warning restore RS0016 // Add public types and members to the declared API
}