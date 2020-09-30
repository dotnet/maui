using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 5749, "Disable horizontal scroll in the custom listview in android")]
	public partial class Issue5749 : TestContentPage
	{
#if UITEST
		[Test]
		public void DisableScrollingOnCustomHorizontalListView()
		{
			RunningApp.WaitForElement("Button");
			RunningApp.WaitForElement(q => q.Marked("True"), timeout: TimeSpan.FromSeconds(2));
			RunningApp.Screenshot("Custom HorizontalListView Scrolling Enabled Default");
			RunningApp.Tap(q => q.Marked("Toggle ScrollView.IsEnabled"));
			RunningApp.WaitForElement(q => q.Marked("False"), timeout: TimeSpan.FromSeconds(2));
			RunningApp.Screenshot("Custom HorizontalListView Scrolling Disabled");
		}
#endif
		public Issue5749()
		{
#if APP
			InitializeComponent();
			listViewHorizontal.ItemsSource = new string[] { "item1... ", "item2... ", "item3... ", "item4... ", "item5... ", "item6... ", "item7... ", "item8... ", "item9... ", "item10... " };
#endif
		}

		protected override void Init()
		{

		}

		void ToggleScrollViewIsEnabled(object sender, EventArgs args)
		{
#if APP
			listViewHorizontal.IsEnabled = !listViewHorizontal.IsEnabled;
#endif
		}
	}

	[Preserve(AllMembers = true)]
	public class CustomHorizontalListview : ScrollView
	{
		public static readonly BindableProperty ItemsSourceProperty =
			BindableProperty.Create("ItemsSource", typeof(IEnumerable), typeof(CustomHorizontalListview), default(IEnumerable));

		public IEnumerable ItemsSource
		{
			get { return (IEnumerable)GetValue(ItemsSourceProperty); }
			set { SetValue(ItemsSourceProperty, value); }
		}

		public static readonly BindableProperty ItemTemplateProperty =
			BindableProperty.Create("ItemTemplate", typeof(DataTemplate), typeof(CustomHorizontalListview), default(DataTemplate));

		public DataTemplate ItemTemplate
		{
			get { return (DataTemplate)GetValue(ItemTemplateProperty); }
			set { SetValue(ItemTemplateProperty, value); }
		}

		public static readonly BindableProperty SelectedCommandParameterProperty =
			BindableProperty.Create("SelectedCommandParameter", typeof(object), typeof(CustomHorizontalListview), null);

		public object SelectedCommandParameter
		{
			get { return GetValue(SelectedCommandParameterProperty); }
			set { SetValue(SelectedCommandParameterProperty, value); }
		}
		public void Render()
		{
			if (ItemTemplate == null || ItemsSource == null)
				return;

			var layout = new StackLayout();
			layout.Padding = 20;
			layout.Orientation = Orientation == ScrollOrientation.Vertical ? StackOrientation.Vertical : StackOrientation.Horizontal;

			foreach (var item in ItemsSource)
			{
				var viewCell = ItemTemplate.CreateContent() as ViewCell;
				viewCell.View.BindingContext = item;
				layout.Children.Add(viewCell.View);
			}

			Content = layout;
		}
	}
}