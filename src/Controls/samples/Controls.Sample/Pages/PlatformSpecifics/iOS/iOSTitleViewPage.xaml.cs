// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class iOSTitleViewPage : ContentPage
	{
		ICommand _returnToPlatformSpecificsPage;

		public iOSTitleViewPage(ICommand restore)
		{
			InitializeComponent();

			_returnToPlatformSpecificsPage = restore;
			_searchBar.Effects.Add(Effect.Resolve("XamarinDocs.SearchBarEffect"));
		}

		void OnReturnButtonClicked(object sender, EventArgs e)
		{
			_returnToPlatformSpecificsPage.Execute(null);
		}
	}
}
