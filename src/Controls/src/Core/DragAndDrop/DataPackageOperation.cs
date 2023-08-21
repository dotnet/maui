// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

namespace Microsoft.Maui.Controls
{
	// These are copied from UWP so if you add additional values please use those
	// https://docs.microsoft.com/en-us/uwp/api/windows.applicationmodel.datatransfer.datapackageoperation?view=winrt-19041
	/// <include file="../../../docs/Microsoft.Maui.Controls/DataPackageOperation.xml" path="Type[@FullName='Microsoft.Maui.Controls.DataPackageOperation']/Docs/*" />
	[Flags]
	public enum DataPackageOperation
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/DataPackageOperation.xml" path="//Member[@MemberName='None']/Docs/*" />
		None = 0,
		/// <include file="../../../docs/Microsoft.Maui.Controls/DataPackageOperation.xml" path="//Member[@MemberName='Copy']/Docs/*" />
		Copy = 1
	}
}
