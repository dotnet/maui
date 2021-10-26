using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Internals
{
	[EditorBrowsable(EditorBrowsableState.Never)]
	public interface IPlatformServices
	{
		bool IsInvokeRequired { get; }

		void BeginInvokeOnMainThread(Action action);

		double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes);

		Color GetNamedColor(string name);

		OSAppTheme RequestedTheme { get; }

		Task<Stream> GetStreamAsync(Uri uri, CancellationToken cancellationToken);

		IIsolatedStorageFile GetUserStoreForApplication();

		void StartTimer(TimeSpan interval, Func<bool> callback);

		string RuntimePlatform { get; }

		SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint);
	}
}