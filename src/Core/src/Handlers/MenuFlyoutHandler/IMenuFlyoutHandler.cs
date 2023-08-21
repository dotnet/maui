// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#if WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.MenuFlyout;
#else
using PlatformView = System.Object;
#endif

namespace Microsoft.Maui.Handlers
{
	public interface IMenuFlyoutHandler : IElementHandler
	{
		void Add(IMenuElement view);
		void Remove(IMenuElement view);
		void Clear();
		void Insert(int index, IMenuElement view);

		new PlatformView PlatformView { get; }
		new IMenuFlyout VirtualView { get; }
	}
}
