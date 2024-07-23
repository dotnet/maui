using Microsoft.Maui;

namespace Microsoft.Maui.Controls
{
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
}