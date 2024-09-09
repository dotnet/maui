using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 21711, "NullReferenceException from FlexLayout.InitItemProperties", PlatformAffected.iOS)]
	public class Issue21711 : TestContentPage
	{
		protected override void Init()
		{
			FlexLayout flex = null!;
			Content = new VerticalStackLayout
			{
				new Button
				{
					Text = "Add",
					AutomationId = "Add",
					Command = new Command(() =>
					{
						flex.Clear();
						flex.Add(NewLabel(0));
						flex.Add(NewLabel(1));

						flex.Clear();
						flex.Add(NewLabel(2));
						flex.Add(NewLabel(3));
					})
				},
				new Button
				{
					Text = "Insert",
					AutomationId = "Insert",
					Command = new Command(() =>
					{
						flex.Clear();
						flex.Insert(0, NewLabel(1));
						flex.Insert(0, NewLabel(0));

						flex.Clear();
						flex.Insert(0, NewLabel(3));
						flex.Insert(0, NewLabel(2));
					})
				},
				new Button
				{
					Text = "Update",
					AutomationId = "Update",
					Command = new Command(() =>
					{
						flex.Clear();
						flex.Add(NewLabel(0));
						flex[0] = NewLabel(1);

						flex.Clear();
						flex.Add(NewLabel(2));
						flex[0] = NewLabel(3);
					})
				},
				new Button
				{
					Text = "Remove",
					AutomationId = "Remove",
					Command = new Command(() =>
					{
						flex.Clear();
						var label = NewLabel(0);
						flex.Add(label);
						flex.Remove(label);

						flex.Clear();
						label = NewLabel(1);
						flex.Add(label);
						flex.Remove(label);

						flex.Add(NewLabel(2));
					})
				},
				(flex = new FlexLayout { }),
			};
		}

		Label NewLabel(int count) =>
			new Label
			{
				Text = $"Item{count}",
				AutomationId = $"Item{count}",
				Background = Brush.Yellow,
				TextType = TextType.Html
			};
	}
}