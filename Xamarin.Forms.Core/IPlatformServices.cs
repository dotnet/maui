using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Xamarin.Forms
{
	internal interface IPlatformServices
	{
		bool IsInvokeRequired { get; }

		void BeginInvokeOnMainThread(Action action);

		//this will go once Timer is included in Pcl profiles
		ITimer CreateTimer(Action<object> callback);
		ITimer CreateTimer(Action<object> callback, object state, int dueTime, int period);
		ITimer CreateTimer(Action<object> callback, object state, long dueTime, long period);
		ITimer CreateTimer(Action<object> callback, object state, TimeSpan dueTime, TimeSpan period);
		ITimer CreateTimer(Action<object> callback, object state, uint dueTime, uint period);

		Assembly[] GetAssemblies();

		string GetMD5Hash(string input);

		double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes);

		Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken);

		IIsolatedStorageFile GetUserStoreForApplication();

		void OpenUriAction(Uri uri);

		void StartTimer(TimeSpan interval, Func<bool> callback);
	}
}