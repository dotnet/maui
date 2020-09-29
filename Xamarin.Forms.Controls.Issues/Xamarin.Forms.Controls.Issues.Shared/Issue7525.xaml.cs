using System;
using System.Collections.Generic;
using Xamarin.Forms.CustomAttributes;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Xaml;

#if UITEST
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.Forms.Core.UITests;
#endif

namespace Xamarin.Forms.Controls.Issues
{
#if APP
	[Preserve(AllMembers = true)]
	public class Test1View : ContentView
	{
		public Test1View()
		{
			BackgroundColor = Color.Red;
		}
	}
	[Preserve(AllMembers = true)]
	public class Test2View : ContentView
	{
		public Test2View()
		{
			BackgroundColor = Color.Blue;
		}
	}
	[Preserve(AllMembers = true)]
	public class Test3View : ContentView
	{
		public Test3View()
		{
			BackgroundColor = Color.Green;
		}
	}

	[Preserve(AllMembers = true)]
	public class CustomViewSelector : DataTemplateSelector
	{
		private DataTemplate view1 = new DataTemplate(typeof(Test1View));
		private DataTemplate view2 = new DataTemplate(typeof(Test2View));
		private DataTemplate view3 = new DataTemplate(typeof(Test3View));

		protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
		{
			Type currentView = item as Type;

			if (currentView == typeof(Test1View))
				return view1;
			else if (currentView == typeof(Test2View))
				return view2;
			return view3;
		}
	}

#endif

	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Github, 7525, "Carousel Position property doesn't change the position on page constructor", PlatformAffected.Android)]
	public partial class Issue7525 : TestContentPage
	{
#if APP
		private int _position;

		public Issue7525()
		{
			InitializeComponent();

			MainCarousel.Position = 1;
		}

		public List<Type> AvailableViews { get; set; }
		public int Position { get { return _position; } set { _position = value; OnPropertyChanged(); } }

		protected override void Init()
		{
			Device.SetFlags(new List<string>(Device.Flags ?? new List<string>()) { "CollectionView_Experimental" });

			AvailableViews = new List<Type>() { typeof(Test1View), typeof(Test2View), typeof(Test3View) };

			BindingContext = this;
		}
#else
		protected override void Init()
		{
		}
#endif
	}
}