// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui.Controls.Xaml
{
	[ProvideCompiled("Microsoft.Maui.Controls.Build.Tasks.NullExtension")]
	[AcceptEmptyServiceProvider]
	public class NullExtension : IMarkupExtension
	{
		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return null;
		}
	}
}
