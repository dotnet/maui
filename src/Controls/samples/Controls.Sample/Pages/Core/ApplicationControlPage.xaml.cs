// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class ApplicationControlPage
	{
		public ApplicationControlPage()
		{
			InitializeComponent();
		}

		void OnTerminateClicked(object sender, EventArgs e)
		{
			Application.Current.Quit();
		}
	}
}