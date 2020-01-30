using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms.Internals;
using Xamarin.Forms.CustomAttributes;
using System.ComponentModel;

#if UITEST
using Xamarin.Forms.Core.UITests;
using Xamarin.UITest;
using NUnit.Framework;
using CategoryAttribute = NUnit.Framework.CategoryAttribute;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Issue(IssueTracker.Github, 9087, 
		"[Bug] Collection View items don't load bindable properties values inside OnElementChanged", PlatformAffected.All)]
	public class Issue9087 : TestContentPage
	{
		const string Success = "Success";

		protected override void Init()
		{
			var layout = new StackLayout();

			var instructions = new Label { Text = $"If you can see the text '{Success}' below, this test has passed." };

			var cv = new CollectionView();

			cv.ItemTemplate = new DataTemplate(() =>
			{
				var label = new _9087Label();

				label.SetBinding(Label.TextProperty, new Binding("Name"));

				return label;
			});

			layout.Children.Add(instructions);
			layout.Children.Add(cv);

			Content = layout;

			var list = new List<_9087Item>
			{
				new _9087Item() { Name = Success }
			};

			cv.ItemsSource = list;
		}

		public class _9087Label : Label { }

		public class _9087Item 
		{ 
			public string Name { get; set; }
		}

#if UITEST
		[Test, Category(UITestCategories.CollectionView)]
		public void BindablePropertiesAvailableAtOnElementChanged()
		{
			RunningApp.WaitForElement(Success);
		}
#endif
	}
}
