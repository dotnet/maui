// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Components.WebView.WebView2
{
    public interface IWebViewManager : IDisposable
    {
        void Navigate(string url);
        Task AddRootComponentAsync(Type componentType, string selector, ParameterView parameters);
        Task RemoveRootComponentAsync(string selector);
		Dispatcher Dispatcher { get; }
	}
}
