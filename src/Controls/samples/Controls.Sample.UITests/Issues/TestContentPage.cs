// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	public abstract class TestContentPage : ContentPage
	{
		protected TestContentPage()
		{
			Init();
		}

		protected abstract void Init();
	}
}
