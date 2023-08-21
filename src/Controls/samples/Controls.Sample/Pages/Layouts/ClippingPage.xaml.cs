// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Maui.Controls.Sample.Pages
{
	public partial class ClippingPage
	{
		public ClippingPage()
		{
			InitializeComponent();

			ToggleClip.Clicked += (sender, args) =>
			{
				Layout1.IsClippedToBounds = !Layout1.IsClippedToBounds;
				Layout2.IsClippedToBounds = !Layout2.IsClippedToBounds;
				Layout3.IsClippedToBounds = !Layout3.IsClippedToBounds;

				Status.Text = Layout1.IsClippedToBounds ? "Clipping" : "Not clipping";
			};
		}
	}
}