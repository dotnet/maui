// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui8149View : ContentView
{
	private Controls.DataTemplate _itemTemplate;
	public string Text { get; set; }
	public Microsoft.Maui.Controls.DataTemplate ItemTemplate
	{
		get => _itemTemplate;
		set
		{
			_itemTemplate = value;
			Content = _itemTemplate.CreateContent() as View;
			Text = ((Content as Maui8149Item).Content as Label).Text;
		}
	}

	public Maui8149View()
	{
		InitializeComponent();
	}
}