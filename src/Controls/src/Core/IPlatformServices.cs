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
		double GetNamedSize(NamedSize size, Type targetElementType, bool useOldSizes);

		OSAppTheme RequestedTheme { get; }

		void StartTimer(TimeSpan interval, Func<bool> callback);

		string RuntimePlatform { get; }

		SizeRequest GetNativeSize(VisualElement view, double widthConstraint, double heightConstraint);
	}
}