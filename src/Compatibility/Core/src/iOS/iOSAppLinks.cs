using System;
using System.Threading.Tasks;
using CoreSpotlight;
using Foundation;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	internal class IOSAppLinks : IAppLinks
	{
		public async void DeregisterLink(IAppLinkEntry appLink)
		{
			if (string.IsNullOrWhiteSpace(appLink.AppLinkUri?.ToString()))
				throw new ArgumentNullException("AppLinkUri");
			await RemoveLinkAsync(appLink.AppLinkUri?.ToString());
		}

		public async void DeregisterLink(Uri uri)
		{
			if (string.IsNullOrWhiteSpace(uri?.ToString()))
				throw new ArgumentNullException(nameof(uri));
			await RemoveLinkAsync(uri.ToString());
		}

		public async void RegisterLink(IAppLinkEntry appLink)
		{
			if (string.IsNullOrWhiteSpace(appLink.AppLinkUri?.ToString()))
				throw new ArgumentNullException("AppLinkUri");
			await AddLinkAsync(appLink);
		}

		public async void DeregisterAll()
		{
			await ClearIndexedDataAsync();
		}

		static async Task AddLinkAsync(IAppLinkEntry deepLinkUri)
		{
			var appDomain = NSBundle.MainBundle.BundleIdentifier;
			string contentType, associatedWebPage;
			bool shouldAddToPublicIndex;

			//user can provide associatedWebPage, contentType, and shouldAddToPublicIndex
			TryGetValues(deepLinkUri, out contentType, out associatedWebPage, out shouldAddToPublicIndex);

			//our unique identifier  will be the only content that is common to spotlight search result and a activity
			//this id allows us to avoid duplicate search results from CoreSpotlight api and NSUserActivity
			//https://developer.apple.com/library/ios/technotes/tn2416/_index.html
			var id = deepLinkUri.AppLinkUri.ToString();

			var searchableAttributeSet = await GetAttributeSet(deepLinkUri, contentType, id);
			var searchItem = new CSSearchableItem(id, appDomain, searchableAttributeSet);
			//we need to make sure we index the item in spotlight first or the RelatedUniqueIdentifier will not work
			await IndexItemAsync(searchItem);

			var activity = new NSUserActivity($"{appDomain}.{contentType}");
			activity.Title = deepLinkUri.Title;
			activity.EligibleForSearch = true;

			//help increase your website url index rating
			if (!string.IsNullOrEmpty(associatedWebPage))
				activity.WebPageUrl = new NSUrl(associatedWebPage);

			//make this search result available to Apple and to other users thatdon't have your app
			activity.EligibleForPublicIndexing = shouldAddToPublicIndex;

			activity.UserInfo = GetUserInfoForActivity(deepLinkUri);
			activity.ContentAttributeSet = searchableAttributeSet;

			//we don't need to track if the link is active iOS will call ResignCurrent
			if (deepLinkUri.IsLinkActive)
				activity.BecomeCurrent();

			var aL = deepLinkUri as AppLinkEntry;
			if (aL != null)
			{
				aL.PropertyChanged += (sender, e) =>
				{
					if (e.PropertyName == AppLinkEntry.IsLinkActiveProperty.PropertyName)
					{
						if (aL.IsLinkActive)
							activity.BecomeCurrent();
						else
							activity.ResignCurrent();
					}
				};
			}
		}

		static Task<bool> ClearIndexedDataAsync()
		{
			var tcs = new TaskCompletionSource<bool>();
			if (CSSearchableIndex.IsIndexingAvailable)
				CSSearchableIndex.DefaultSearchableIndex.DeleteAll(error => tcs.TrySetResult(error == null));
			else
				tcs.TrySetResult(false);
			return tcs.Task;
		}

		static async Task<CSSearchableItemAttributeSet> GetAttributeSet(IAppLinkEntry deepLinkUri, string contentType, string id)
		{
#pragma warning disable CA1416, CA1422  // TODO: 'CSSearchableItemAttributeSet' is unsupported on: 'ios' 14.0 and later
			var searchableAttributeSet = new CSSearchableItemAttributeSet(contentType)
			{
				RelatedUniqueIdentifier = id,
				Title = deepLinkUri.Title,
				ContentDescription = deepLinkUri.Description,
				Url = new NSUrl(deepLinkUri.AppLinkUri.ToString())
			};
#pragma warning restore CA1416, CA1422

			if (deepLinkUri.Thumbnail != null)
			{
				using (var uiimage = await deepLinkUri.Thumbnail.GetNativeImageAsync())
				{
					if (uiimage == null)
						throw new InvalidOperationException("AppLinkEntry Thumbnail must be set to a valid source");

					searchableAttributeSet.ThumbnailData = uiimage.AsPNG();
				}
			}

			return searchableAttributeSet;
		}

		static NSMutableDictionary GetUserInfoForActivity(IAppLinkEntry deepLinkUri)
		{
			//this info will only appear if not from a spotlight search
			var info = new NSMutableDictionary();
			info.Add(new NSString("link"), new NSString(deepLinkUri.AppLinkUri.ToString()));
			foreach (var item in deepLinkUri.KeyValues)
				info.Add(new NSString(item.Key), new NSString(item.Value));
			return info;
		}

		static Task<bool> IndexItemAsync(CSSearchableItem searchItem)
		{
			var tcs = new TaskCompletionSource<bool>();
			if (CSSearchableIndex.IsIndexingAvailable)
			{
				CSSearchableIndex.DefaultSearchableIndex.Index(new[] { searchItem }, error => tcs.TrySetResult(error == null));
			}
			else
				tcs.SetResult(false);
			return tcs.Task;
		}

		static Task<bool> RemoveLinkAsync(string identifier)
		{
			var tcs = new TaskCompletionSource<bool>();
			if (CSSearchableIndex.IsIndexingAvailable)
				CSSearchableIndex.DefaultSearchableIndex.Delete(new[] { identifier }, error => tcs.TrySetResult(error == null));
			else
				tcs.SetResult(false);
			return tcs.Task;
		}

		//Parse the KeyValues because user can provide associatedWebPage, contentType, and shouldAddToPublicIndex options
		static void TryGetValues(IAppLinkEntry deepLinkUri, out string contentType, out string associatedWebPage, out bool shouldAddToPublicIndex)
		{
			contentType = string.Empty;
			associatedWebPage = string.Empty;
			shouldAddToPublicIndex = false;
			var publicIndex = string.Empty;

			if (!deepLinkUri.KeyValues.TryGetValue(nameof(contentType), out contentType))
				contentType = "View";

			if (deepLinkUri.KeyValues.TryGetValue(nameof(publicIndex), out publicIndex))
				bool.TryParse(publicIndex, out shouldAddToPublicIndex);

			deepLinkUri.KeyValues.TryGetValue(nameof(associatedWebPage), out associatedWebPage);
		}
	}
}