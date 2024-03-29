﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Github5000)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 1665, "ListView full width separators on iOS", PlatformAffected.iOS)]
	public class Issue1665 : TestContentPage
	{
		ListView _list;
		Button _button;
		List<string> _items;
		Random _random = new Random(new DateTime().Millisecond);
		SeparatorStyle _separatorStyle = SeparatorStyle.FullWidth;

		protected override void Init()
		{
			_button = new Button();
			_button.Margin = new Thickness { Top = 50 };
			_button.Clicked += ToggleSeparatorStyle;

			UpdateButtonAndRefreshList();

		}

		void UpdateButtonAndRefreshList()
		{
			_button.Text = $"SeparatorStyle: {_separatorStyle}. Click to toggle.";

			var dataTemplate = new DataTemplate(typeof(TextCell));
			dataTemplate.SetBinding(TextCell.TextProperty, new Binding("."));
			_list = new ListView
			{
				ItemTemplate = dataTemplate
			};
			_items = new List<string>() { "John", "Paul", "George", "Ringo", "John", "Paul", "George", "Ringo", "John", "Paul", "George", "Ringo", "John", "Paul", "George", "Ringo", "John", "Paul", "George", "Ringo" };
			_list.ItemsSource = _items.OrderBy(i => _random.Next());
			_list.On<iOS>().SetSeparatorStyle(_separatorStyle);

			Content = new StackLayout
			{
				Children = {
					_button,
					_list
				}
			};
		}

		void ToggleSeparatorStyle(object sender, EventArgs e)
		{
			_separatorStyle = _separatorStyle == SeparatorStyle.FullWidth ? SeparatorStyle.Default : SeparatorStyle.FullWidth;
			UpdateButtonAndRefreshList();
		}
	}
}
