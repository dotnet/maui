// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using Microsoft.AspNetCore.Components.Web;

namespace Microsoft.AspNetCore.Components.WebView.Wpf
{
	/// <summary>
	/// A collection of <see cref="RootComponent"/> items.
	/// </summary>
	public class RootComponentsCollection : ObservableCollection<RootComponent>, IJSComponentConfiguration
	{
		/// <inheritdoc />
		public JSComponentConfigurationStore JSComponents { get; } = new();
	}
}
