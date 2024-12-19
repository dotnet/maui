using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 99999, "CompressedLayout is not working", PlatformAffected.All)]

public partial class Issue99999 : ContentPage
{
	int _count = 6;
	
	public Issue99999()
	{
		InitializeComponent();
	}
	
	private void OnInsOut(object sender, EventArgs e)
	{
		var index = int.Parse(IndexEntry.Text);
		OuterLayout.Insert(index, new Label { Text = $"Text{++_count}" });
	}
	
	private void OnRmOut(object sender, EventArgs e)
	{
		var index = int.Parse(IndexEntry.Text);
		OuterLayout.RemoveAt(index);
	}
	
	private void OnInsIn(object sender, EventArgs e)
	{
		var index = int.Parse(IndexEntry.Text);
		InnerLayout.Insert(index, new Label { Text = $"Text{++_count}" });
	}
	
	private void OnRmIn(object sender, EventArgs e)
	{
		var index = int.Parse(IndexEntry.Text);
		InnerLayout.RemoveAt(index);
	}
}
