// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Maui.Controls.Sample.Pages.ScrollViewPages
{
	public partial class ScrollToFromConstructorPage
	{
		public ScrollToFromConstructorPage()
		{
			InitializeComponent();

			ScrollToAsync();
		}

		public async void ScrollToAsync()
		{
			await ScrollView.ScrollToAsync(0, 1000, false);
		}
	}
}