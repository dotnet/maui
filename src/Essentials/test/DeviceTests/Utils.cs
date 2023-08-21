// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	public class Utils
	{
		public static void Unused(params object[] obj) { }

		public static Task OnMainThread(Action action) =>
			MainThread.InvokeOnMainThreadAsync(() => action());

		public static Task OnMainThread(Func<Task> action) =>
			MainThread.InvokeOnMainThreadAsync(() => action());
	}
}
