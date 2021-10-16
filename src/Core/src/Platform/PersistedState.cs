using System.Collections.Generic;

namespace Microsoft.Maui
{
	public class PersistedState : Dictionary<string, string?>, IPersistedState
	{
#if __IOS__ || __MACCATALYST__
		internal Foundation.NSUserActivity ToUserActivity(string userActivityType)
		{
			var userInfo = new Foundation.NSMutableDictionary();
			foreach (var pair in this)
			{
				userInfo.SetValueForKey(new Foundation.NSString(pair.Value), new Foundation.NSString(pair.Key));
			}

			var userActivity = new Foundation.NSUserActivity(userActivityType);
			userActivity.AddUserInfoEntries(userInfo);

			return userActivity;
		}
#endif
	}
}