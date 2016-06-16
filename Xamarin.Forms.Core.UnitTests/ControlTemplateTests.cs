using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Xamarin.Forms;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ControlTemplateTests : BaseTestFixture
	{
		[SetUp]
		public override void Setup()
		{
			base.Setup ();
			Device.PlatformServices = new MockPlatformServices ();
		}

		[TearDown]
		public override void TearDown()
		{
			base.TearDown ();
			Device.PlatformServices = null;
		}

		public class ContentControl : StackLayout
		{
			public ContentControl ()
			{
				var label = new Label ();
				label.SetBinding (Label.TextProperty, new TemplateBinding ("Name"));

				Children.Add (label);
				Children.Add (new ContentPresenter ());
			}
		}

		public class PresenterWrapper : ContentView
		{
			public PresenterWrapper ()
			{
				Content = new ContentPresenter ();
			}
		}

		public class TestView : ContentView
		{
			public static readonly BindableProperty NameProperty =
				BindableProperty.Create (nameof (Name), typeof (string), typeof (TestView), default(string));

			public string Name
			{
				get { return (string)GetValue (NameProperty); }
				set { SetValue (NameProperty, value); }
			}

			public TestView ()
			{
				ControlTemplate = new ControlTemplate(typeof (ContentControl));
			}
		}

		[Test]
		public void ResettingControlTemplateNullsPresenterContent ()
		{
			var testView = new TestView {
				Platform = new UnitPlatform (),
				ControlTemplate = new ControlTemplate (typeof (PresenterWrapper))
			};

			var label = new Label ();
			testView.Content = label;

			var child1 = ((IElementController)testView).LogicalChildren[0];
			var child2 = ((IElementController)child1).LogicalChildren[0];

			var originalPresenter = (ContentPresenter)child2;

			Assert.AreEqual (label, originalPresenter.Content);

			testView.ControlTemplate = new ControlTemplate (typeof (PresenterWrapper));

			Assert.IsNull (originalPresenter.Content);
		}

		[Test]
		public void NestedTemplateBindings ()
		{
			var testView = new TestView ();

			var child1 = ((IElementController)testView).LogicalChildren[0];
			var child2 = ((IElementController)child1).LogicalChildren[0];

			var label = (Label)child2;

			testView.Platform = new UnitPlatform ();
			Assert.IsNull (label.Text);

			testView.Name = "Bar";
			Assert.AreEqual ("Bar", label.Text);
		}

		[Test]
		public void ParentControlTemplateDoesNotClearChildTemplate ()
		{
			var parentView = new TestView ();
			var childView = new TestView ();
			parentView.Platform = new UnitPlatform ();

			parentView.Content = childView;
			childView.Content = new Button ();

			var child1 = ((IElementController)childView).LogicalChildren[0];
			var child2 = ((IElementController)child1).LogicalChildren[1];

			var childPresenter = (ContentPresenter)child2;

			parentView.ControlTemplate = new ControlTemplate (typeof (ContentControl));
			Assert.IsNotNull (childPresenter.Content);
		}

		[Test]
		public void NullConstructor ()
		{
			Assert.Throws<ArgumentNullException> (() => new ControlTemplate (null));
		}

		class TestPage : ContentPage
		{
			public static readonly BindableProperty NameProperty =
				BindableProperty.Create (nameof (Name), typeof (string), typeof (TestPage), null);

			public string Name
			{
				get { return (string)GetValue (NameProperty); }
				set { SetValue (NameProperty, value); }
			}
		}

		class TestContent : ContentView
		{
			public TestContent ()
			{
				Content = new Entry ();
				Content.SetBinding (Entry.TextProperty, new TemplateBinding ("Name", BindingMode.TwoWay));
			}
		}

		class ViewModel : INotifyPropertyChanged
		{
			string name;

			public string Name
			{
				get { return name; }
				set
				{
					if (name == value)
						return;
					name = value;
					OnPropertyChanged ();
				}
			}

			public event PropertyChangedEventHandler PropertyChanged;

			protected virtual void OnPropertyChanged ([CallerMemberName] string propertyName = null)
			{
				PropertyChanged?.Invoke (this, new PropertyChangedEventArgs (propertyName));
			}
		}

		[Test]
		public void DoubleTwoWayBindingWorks ()
		{
			var page = new TestPage ();
			page.Platform = new UnitPlatform ();
			var viewModel = new ViewModel {
				Name = "Jason"
			};
			page.BindingContext = viewModel;

			page.ControlTemplate = new ControlTemplate (typeof (TestContent));
			page.SetBinding (TestPage.NameProperty, "Name");

			var entry = ((ContentView)((IElementController)page).LogicalChildren[0]).Content as Entry;
			((IElementController)entry).SetValueFromRenderer (Entry.TextProperty, "Bar");
			viewModel.Name = "Raz";

			Assert.AreEqual ("Raz", entry.Text);
		}
	}
}