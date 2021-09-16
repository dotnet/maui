using System;

namespace Microsoft.Maui
{
	public partial class ActivationState : IActivationState
	{
		public ActivationState(IMauiContext context, Foundation.NSUserActivity? userActivity)
		{
			Context = context ?? throw new ArgumentNullException(nameof(context));
			UserActivity = userActivity;
		}

		public Foundation.NSUserActivity? UserActivity { get; }

		public IMauiContext Context { get; }
	}

	public partial class RestoredState : IRestoredState
	{
		public RestoredState(IMauiContext context, Foundation.NSUserActivity? userActivity)
		{
			Context = context ?? throw new ArgumentNullException(nameof(context));
			UserActivity = userActivity;
		}

		public Foundation.NSUserActivity? UserActivity { get; }

		public IMauiContext Context { get; }

		public bool TryGet(string key, out string? value)
		{
			value = UserActivity?.UserInfo?.ValueForKey(new Foundation.NSString(key))?.ToString();
			return value != null;
		}
	}

	public partial class SaveableState : ISaveableState
	{
		public SaveableState(IMauiContext context, Foundation.NSUserActivity? userActivity)
		{
			Context = context ?? throw new ArgumentNullException(nameof(context));
			UserActivity = userActivity;
		}

		public Foundation.NSUserActivity? UserActivity { get; }

		public IMauiContext Context { get; }

		public bool TryGet(string key, out string? value)
		{
			value = UserActivity?.UserInfo?.ValueForKey(new Foundation.NSString(key))?.ToString();
			return value != null;
		}

		public void Set(string key, string? value)
		{
			UserActivity?.UserInfo?.SetValueForKey(value == null ? default : new Foundation.NSString(value), new Foundation.NSString(key));
		}

	}
}