﻿using System.Diagnostics;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Microsoft.Maui.Controls.ControlGallery.Issues
{
#if UITEST
	[NUnit.Framework.Category(Compatibility.UITests.UITestCategories.Bugzilla)]
#endif
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 44955, "[WinRT/UWP] Setting Entry BackgroundColor via Behavior results in sticky unfocused background color", PlatformAffected.WinRT)]
	public class Bugzilla44955 : TestContentPage
	{
		Entry _validationEntry;
		protected override void Init()
		{
			_validationEntry = new Entry();
			_validationEntry.Behaviors.Add(new NonEmptyStringValidator());
			Content = new StackLayout
			{
				Children =
				{
					new Label
					{
						Text = "The first entry should have a red background only when it is empty, regardless of focus (due to an attached behavior). The second has a set background color, the third is default, and the last is default, but disabled."
					},
					_validationEntry,
					new Entry
					{
						BackgroundColor = Colors.MediumPurple
					},
					new Entry(),
					new Entry
					{
						IsEnabled = false
					},
					new Button
					{
						Text = "Change background of first label to yellow",
						Command = new Command(() => _validationEntry.BackgroundColor = Colors.Yellow)
					}
				}
			};
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			_validationEntry.Behaviors.Clear();
		}

		class NonEmptyStringValidator : Behavior<Entry>
		{
			protected override void OnAttachedTo(Entry bindable)
			{
				bindable.TextChanged += HandleTextChanged;
				Validate(bindable, bindable.Text);
			}

			protected override void OnDetachingFrom(Entry bindable)
			{
				bindable.TextChanged -= HandleTextChanged;
			}

			void HandleTextChanged(object sender, TextChangedEventArgs e)
			{
				Validate((Entry)sender, e.NewTextValue);
			}

			void Validate(Entry entry, string text)
			{
				if (text == null)
					entry.BackgroundColor = Colors.Red;
				else
					entry.BackgroundColor = text.Trim() != "" ? null : Colors.Red;
			}
		}
	}
}
