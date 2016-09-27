using System;
using System.Linq;

using NUnit.Framework;
using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	public class ConvertibleToView
	{
		public static implicit operator View(ConvertibleToView source)
		{
			return new Button();
		}
	}

	public class MockViewWithValues : View
	{ 
		public UInt16 UShort { get; set; }
		public decimal ADecimal { get; set; }
	}

	public partial class SetValue : ContentPage
	{	
		public SetValue ()
		{
			InitializeComponent ();
		}

		public SetValue (bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		bool fired;
		void onButtonClicked (object sender, EventArgs args)
		{
			fired = true;
		}

		[TestFixture]
		public class Tests
		{
			[SetUp]
			public void Setup()
			{
				Device.PlatformServices = new MockPlatformServices();
			}

			[TearDown]
			public void TearDown()
			{
				Device.PlatformServices = null;
			}

			[TestCase (false)]
			[TestCase (true)]
			public void SetValueToBP (bool useCompiledXaml)
			{
				var page = new SetValue (useCompiledXaml);
				Assert.AreEqual ("Foo", page.label0.Text);
			}

			[TestCase (false)]
			[TestCase (true)]
			public void SetBindingToBP (bool useCompiledXaml)
			{
				var page = new SetValue (useCompiledXaml);
				Assert.AreEqual (Label.TextProperty.DefaultValue, page.label1.Text);

				page.label1.BindingContext = new {labeltext="Foo"};
				Assert.AreEqual ("Foo", page.label1.Text);
			}

			[TestCase (false)]
			[TestCase (true)]
			public void SetBindingWithImplicitPath (bool useCompiledXaml)
			{
				var page = new SetValue (useCompiledXaml);
				Assert.AreEqual (Label.TextProperty.DefaultValue, page.label11.Text);

				page.label11.BindingContext = new {labeltext="Foo"};
				Assert.AreEqual ("Foo", page.label11.Text);
			}

			[TestCase (false)]
			[TestCase (true)]
			public void SetEvent (bool useCompiledXaml)
			{
				var page = new SetValue (useCompiledXaml);
				Assert.False (page.fired);
				(page.button0 as IButtonController).SendClicked ();
				Assert.True (page.fired);
			}

			[TestCase (false)]
			[TestCase (true)]
			public void SetBoolValue (bool useCompiledXaml)
			{
				var page = new SetValue (useCompiledXaml);
				Assert.True (page.image0.IsOpaque);
			}

			//TODO test all value conversions

			[TestCase (false)]
			[TestCase (true)]
			public void SetAttachedBP (bool useCompiledXaml)
			{
				var page = new SetValue (useCompiledXaml);
				Assert.AreEqual (1, Grid.GetColumn (page.label2));
				Assert.AreEqual (2, Grid.GetRow (page.label2));
			}

			[TestCase (false)]
			[TestCase (true)]
			public void SetContent (bool useCompiledXaml)
			{
				var page = new SetValue (useCompiledXaml);
				Assert.AreSame (page.label3, page.contentview0.Content);
			}

			[TestCase (false)]
			[TestCase (true)]
			public void SetImplicitContent (bool useCompiledXaml)
			{
				var page = new SetValue (useCompiledXaml);
				Assert.AreSame (page.label4, page.contentview1.Content);
			}

			[TestCase (false)]
			[TestCase (true)]
			public void SetCollectionContent (bool useCompiledXaml)
			{
				var page = new SetValue (useCompiledXaml);
				Assert.True (page.stack0.Children.Contains(page.label5));
				Assert.True (page.stack0.Children.Contains(page.label6));
			}

			[TestCase (false)]
			[TestCase (true)]
			public void SetImplicitCollectionContent (bool useCompiledXaml)
			{
				var page = new SetValue (useCompiledXaml);
				Assert.True (page.stack1.Children.Contains(page.label7));
				Assert.True (page.stack1.Children.Contains(page.label8));
			}

			[TestCase (false)]
			[TestCase (true)]
			public void SetSingleCollectionContent (bool useCompiledXaml)
			{
				var page = new SetValue (useCompiledXaml);
				Assert.True (page.stack2.Children.Contains(page.label9));
			}

			[TestCase (false)]
			[TestCase (true)]
			public void SetImplicitSingleCollectionContent (bool useCompiledXaml)
			{
				var page = new SetValue (useCompiledXaml);
				Assert.True (page.stack3.Children.Contains(page.label10));
			}

			[TestCase (false)]
			[TestCase (true)]
			public void SetPropertyDefinedOnGenericType (bool useCompiledXaml)
			{
				var page = new SetValue (useCompiledXaml);
				Assert.AreEqual (2, page.listView.ItemsSource.Cast<object>().Count ());
			}

			[TestCase (false)]
			[TestCase (true)]
			public void SetConvertibleProperties (bool useCompiledXaml)
			{
				var page = new SetValue (useCompiledXaml);
				Assert.AreEqual (Color.Red, page.label12.TextColor);
			}

			[TestCase (false)]
			[TestCase (true)]
			public void SetValueTypeProperties (bool useCompiledXaml)
			{
				var page = new SetValue (useCompiledXaml);
				Assert.AreEqual (Color.Pink, page.label13.TextColor);
			}

			[TestCase (false)]
			[TestCase (true)]
			public void CreateValueTypes (bool useCompiledXaml)
			{
				var page = new SetValue (useCompiledXaml);
				Assert.AreEqual (Color.Purple, page.Resources ["purple"]);
			}

			[TestCase (false)]
			[TestCase (true)]
			public void DefCollections (bool useCompiledXaml)
			{
				var page = new SetValue (useCompiledXaml);
				Assert.AreEqual (2, page.grid0.RowDefinitions.Count);
				Assert.AreEqual (1, page.grid0.ColumnDefinitions.Count);
			}

			[TestCase (false)]
			[TestCase (true)]
			public void FlagsAreApplied (bool useCompiledXaml)
			{
				var page = new SetValue (useCompiledXaml);
				Assert.AreEqual (AbsoluteLayoutFlags.PositionProportional | AbsoluteLayoutFlags.WidthProportional, AbsoluteLayout.GetLayoutFlags (page.label14));
			}

			[TestCase(false)]
			[TestCase(true)]
			public void ConversionsAreAppliedOnSet(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.That(page.content0.Content, Is.TypeOf<Button>());
			}

			[TestCase(false)]
			[TestCase(true)]
			public void ConversionsAreAppliedOnAdd(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.That(page.stack4.Children[0], Is.TypeOf<Button>());
			}

			[TestCase(false)]
			[TestCase(true)]
			public void ListsAreSimplified(bool useCompiledXaml)
			{
				var page = new SetValue(useCompiledXaml);
				Assert.That(page.contentview2.Content, Is.TypeOf<Label>());
			}

			[TestCase(false)]
			[TestCase(true)]
			public void MorePrimitiveTypes(bool useCompiledXaml)
			{ 
				var page = new SetValue(useCompiledXaml);
				Assert.AreEqual((ushort)32, page.mockView0.UShort);
				Assert.AreEqual((decimal)42, page.mockView0.ADecimal);
			}
		}
	}
}