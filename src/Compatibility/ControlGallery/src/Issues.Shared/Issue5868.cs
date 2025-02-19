using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5868, "[Bug] VisualElement.Unfocus does not work on WPF", PlatformAffected.WPF)]
	public class Issue5868 : TestContentPage
	{
		readonly Entry _entry = new Entry();
		readonly Label _isFocusedLabel = new Label();
		readonly Label _infoLabel = new Label();

		protected override void Init()
		{
			_infoLabel.Text = "Gets unfocused after 3 seconds";
			Content = new StackLayout()
			{
				Children =
			{
				_infoLabel,
				_entry,
				_isFocusedLabel
			}
			};
			_entry.Unfocused += (sender, e) =>
			{
				_isFocusedLabel.Text = "Is entry focused: no";
			};
			_entry.Focused += (sender, e) =>
			{
				_isFocusedLabel.Text = "Is entry focused: yes";
			};
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			_entry.Focus();
			Task.Run(async () =>
			{
				await Task.Delay(3000);
				Device.BeginInvokeOnMainThread(() =>
				{
					_entry.Unfocus();
					_infoLabel.Text = "Entry should be unfocused.";
				});
			});
		}
	}
}