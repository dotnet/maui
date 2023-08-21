// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable
using System;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/DragEventArgs.xml" path="Type[@FullName='Microsoft.Maui.Controls.DragEventArgs']/Docs/*" />
	public class DragEventArgs : EventArgs
	{
		/// <include file="../../../docs/Microsoft.Maui.Controls/DragEventArgs.xml" path="//Member[@MemberName='.ctor']/Docs/*" />
		public DragEventArgs(DataPackage dataPackage)
		{
			Data = dataPackage;
			AcceptedOperation = DataPackageOperation.Copy;
		}

		/// <include file="../../../docs/Microsoft.Maui.Controls/DragEventArgs.xml" path="//Member[@MemberName='Data']/Docs/*" />
		public DataPackage Data { get; }
		/// <include file="../../../docs/Microsoft.Maui.Controls/DragEventArgs.xml" path="//Member[@MemberName='AcceptedOperation']/Docs/*" />
		public DataPackageOperation AcceptedOperation { get; set; }
	}
}
