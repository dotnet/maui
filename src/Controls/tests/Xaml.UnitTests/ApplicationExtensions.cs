// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	static class ApplicationExtensions
	{
		public static Window LoadPage(this Application app, Page page)
		{
			app.MainPage = page;

			return ((IApplication)app).CreateWindow(null) as Window;
		}
	}
}