using System;

using Xamarin.Forms.CustomAttributes;
using System.Text;
using Xamarin.Forms.Internals;
#if UITEST
using Xamarin.UITest;
using NUnit.Framework;
#endif

namespace Xamarin.Forms.Controls.Issues
{
	[Preserve (AllMembers = true)]
	[Issue (IssueTracker.Bugzilla, 27642, "[Windows Phone] Adding a ScrollView control to a ContentView, remove it and re-add it will cause an exception on Windows Phone")]
	public class Bugzilla27642 : TestContentPage // or TestMasterDetailPage, etc ...
	{
		ContentView _mainContent;
		protected override void Init ()
		{
			var rootGrid = new Grid {
				RowDefinitions = new RowDefinitionCollection
														  {
															 new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
															 new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) },
														 },
			};


			_mainContent = new ContentView { Content = new ScrollView { Content = new Label { Text = Description } } };
			rootGrid.AddChild (_mainContent, 0, 0);


			var buttons = new StackLayout { Orientation = StackOrientation.Horizontal };

			var button1A = new Button { Text = "View 1A" };
			button1A.Clicked += (sender, args) => ShowView (_view1A);
			buttons.Children.Add (button1A);

			var button1B = new Button { Text = "View 1B" };
			button1B.Clicked += (sender, args) => ShowView (_view1B);
			buttons.Children.Add (button1B);

			var button2 = new Button { Text = "View 2" };
			button2.Clicked += (sender, args) => ShowView (_view2);
			buttons.Children.Add (button2);

			rootGrid.AddChild (buttons, 0, 1);


			Content = rootGrid;
		}

		const string Description = "A view containing a ScrollView cannot be re-used (same instance, Singleton) \n\n\n"
								+ "Steps to reproduce: \n\n" + "View1a contains a ScrollView \n"
								+ "Click: View1A -> View2 -> View1A => Exception\n\n"
								+ "View1b also contains a ScrollView, but its Content (including ScrollView!) will be re-generated during activation.\n"
								+ "Click: View1B -> View2 -> View1B => Exception\n\n"
								+ "View2 doesn't contain a ScrollView and therefore can be called again without problems.\n\n"
								+ "The Error-Message-View contains a ScrollView, too but will be re-created every time.";

		readonly View1A _view1A = new View1A ();  // always same instance, simulates Singleton from IoC
		readonly View1B _view1B = new View1B ();  // -"-
		readonly View2 _view2 = new View2 ();     // -"-

		void ShowView (ExtendedContentView view)
		{
			try {
				view.Activating ();    // implemented only for View1B
				_mainContent.Content = view;
			}
			catch (Exception ex) {
				_mainContent.Content = new ErrorView (ex);
			}
		}

		[Preserve (AllMembers = true)]
		public class ExtendedContentView : ContentView
		{
			public virtual void Activating ()
			{
			}
		}
	
		[Preserve (AllMembers = true)]
		public class View1A : ExtendedContentView
		{
			public View1A ()
			{

				BackgroundColor = Color.Olive;
				var scrollView = new ScrollView ();
				var sb = new StringBuilder ();
				for (var i = 0; i < 100; i++)
					sb.Append ("View 1a with ScrollView +++ ");

#pragma warning disable 618
				var label = new Label { Text = sb.ToString (), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, XAlign = TextAlignment.Center, };
#pragma warning restore 618

				scrollView.Content = label;

				Content = scrollView;

			}

		}

		[Preserve (AllMembers = true)]
		public class View1B : ExtendedContentView
		{
			public View1B ()
			{
				BackgroundColor = Color.Navy;
			}

			public override void Activating ()
			{
				var scrollView = new ScrollView ();
				var sb = new StringBuilder ();
				for (var i = 0; i < 50; i++)
					sb.Append ("View 1b with ScrollView and recreation of content +++++ ");

#pragma warning disable 618
				var label = new Label { Text = sb.ToString (), HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, XAlign = TextAlignment.Center, };
#pragma warning restore 618

				scrollView.Content = label;

				Content = scrollView;

			}
		}

		public class View2 : ExtendedContentView
		{
			public View2 ()
			{
				BackgroundColor = Color.Teal;
#pragma warning disable 618
				Content = new Label { Text = "View 2", HorizontalOptions = LayoutOptions.Center, VerticalOptions = LayoutOptions.Center, XAlign = TextAlignment.Center, };
#pragma warning restore 618
			}
		}

		[Preserve (AllMembers = true)]
		public class ErrorView : ExtendedContentView
		{
			public ErrorView (Exception ex)
			{
				BackgroundColor = Color.Maroon;
				Content = new ScrollView { Content = new Label { Text = ex.ToString () } };
			}
		}

	}
}
