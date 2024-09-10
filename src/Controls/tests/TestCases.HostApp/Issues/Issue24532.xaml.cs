using System;
using System.Collections.ObjectModel;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 24532, "Rendering performance", PlatformAffected.All)]
public partial class Issue24532 : ContentPage
{
	public List<Model> Models { get; set; }
	
	public Issue24532()
	{
		Models = GenerateItems("TheFirstOne");

		BindingContext = this;
		InitializeComponent();
	}

	private void ButtonClicked(object sender, EventArgs e)
	{
		Models = GenerateItems("TheSecondOne");
		OnPropertyChanged(nameof(Models));
	}
	
	static List<Model> GenerateItems(string automationId)
	{
		return [
			..new Model[]
			{
				new() { Content = automationId, Header = automationId, AutomationId = automationId }
			},
			..Enumerable.Range(0, 40).Select(i => new Model { Content = $"Content {i}", Header = $"Header {i}" })
		];
	}
	
	public class Model : SubModel
	{
		public SubModel[] SubModels { get; } = Enumerable.Range(0, 10).Select(i => new SubModel
		{
			Content = $"SubContent {i}", Header = $"SubHeader {i}"
		}).ToArray();
	}
	
	public class SubModel
	{
		public string Header { get; set; }
		public string Content { get; set; }
		public string AutomationId { get; set; }
	}
}
