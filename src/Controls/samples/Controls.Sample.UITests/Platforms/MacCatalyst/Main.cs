// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using ObjCRuntime;
using UIKit;

namespace Maui.Controls.Sample.Platform
{
	public class Application
	{
		static void Main(string[] args) => UIApplication.Main(args, null, typeof(AppDelegate));
	}
}