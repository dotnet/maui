using System;
using System.Threading.Tasks;
using Android.Content;
using Android.Gms.AppIndexing;
using Android.Gms.Common.Apis;
using Android.Runtime;
using IndexingAction = Android.Gms.AppIndexing.Action;
using Android.App;

namespace Xamarin.Forms.Platform.Android.AppLinks
{
	[Preserve(AllMembers = true)]
	public class AndroidAppLinks : IAppLinks, IDisposable
	{
		readonly GoogleApiClient _client;

		bool _disposed;

		public static bool IsInitialized { get; private set; }

		public static Context Context { get; private set; }

		public static void Init(Activity activity)
		{
			if (IsInitialized)
				return;
			IsInitialized = true;

			Context = activity;
		}

		public AndroidAppLinks(Context context)
		{
			_client = new GoogleApiClient.Builder(context).AddApi(AppIndex.API).Build();
			_client.Connect();
		}

		public void DeregisterLink(IAppLinkEntry appLink)
		{
			RemoveFromIndexItemAsync(appLink.AppLinkUri.ToString());
		}

		public void DeregisterLink(Uri appLinkUri)
		{
			RemoveFromIndexItemAsync(appLinkUri.ToString());
		}

		public async void RegisterLink(IAppLinkEntry appLink)
		{
			await IndexItemAsync(appLink);
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool isDisposing)
		{
			if (isDisposing && !_disposed)
			{
				_disposed = true;
				_client.Disconnect();
				_client.Dispose();
			}
		}

		static IndexingAction BuildIndexAction(IAppLinkEntry appLink)
		{
			Thing item = new Thing.Builder().SetName(appLink.Title).SetDescription(appLink.Description).SetUrl(global::Android.Net.Uri.Parse(appLink.AppLinkUri.AbsoluteUri)).Build();
			Thing thing = new IndexingAction.Builder(IndexingAction.TypeView).SetObject(item).SetActionStatus(IndexingAction.StatusTypeCompleted).Build();
			var indexAction = thing.JavaCast<IndexingAction>();
			return indexAction;
		}

		async Task IndexItemAsync(IAppLinkEntry appLink)
		{
			IndexingAction indexAction = BuildIndexAction(appLink);

			if (_client.IsConnected && appLink.IsLinkActive)
			{
				Statuses resultStart = await AppIndex.AppIndexApi.StartAsync(_client, indexAction);
				if (resultStart.IsSuccess)
				{
					var aL = appLink as AppLinkEntry;
					if (aL != null)
					{
						aL.PropertyChanged += async (sender, e) =>
						{
							if (e.PropertyName == AppLinkEntry.IsLinkActiveProperty.PropertyName)
							{
								if (appLink.IsLinkActive)
								{
									await AppIndex.AppIndexApi.StartAsync(_client, indexAction);
								}
								else
								{
									await AppIndex.AppIndexApi.EndAsync(_client, indexAction);
								}
							}
						};
					}
				}
			}
		}

		void RemoveFromIndexItemAsync(string identifier)
		{
			if (_client.IsConnected)
			{
			}
		}
	}
}

