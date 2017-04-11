using System;
using System.Threading.Tasks;
using NUnit.Framework;

using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class NavigationMenuUnitTests : BaseTestFixture
	{
		[Test]
		public void TestTargets ()
		{
			var menu = new NavigationMenu ();

			Assert.That (menu.Targets, Is.Empty);

			bool signaled = false;
			menu.PropertyChanged += (sender, args) => {
				if (args.PropertyName == "Targets")
					signaled = true;
			};

			var newArray = new[] {
				new ContentPage { Content = new View (), Icon = "img1.jpg" },
				new ContentPage { Content = new View (), Icon = "img2.jpg" }
			};
			menu.Targets = newArray;

			Assert.AreEqual (newArray, menu.Targets);
			Assert.True (signaled);
		}

		[Test]
		public void TestTargetsDoubleSet ()
		{
			var menu = new NavigationMenu ();

			bool signaled = false;
			menu.PropertyChanged += (sender, args) => {
				if (args.PropertyName == "Targets")
					signaled = true;
			};

			menu.Targets = menu.Targets;

			Assert.False (signaled);
		}

		[Test]
		public void TestAdd ()
		{
			var menu = new NavigationMenu ();

			bool signaled = false;
			menu.PropertyChanged += (sender, args) => {
				switch (args.PropertyName) {
				case "Targets":
					signaled = true;
					break;
				}
			};

			var child = new ContentPage {
				Content = new View (),
				Icon = "img.jpg"
			};
			menu.Add (child);
			Assert.True (menu.Targets.Contains (child));
			Assert.True (signaled);
		}

		[Test]
		public void IconNotSet ()
		{
			var menu = new NavigationMenu ();
			var childWithoutIcon = new ContentPage { Title = "I have no image" };

			var ex = Assert.Throws<Exception> (() => menu.Add (childWithoutIcon));
			Assert.That (ex.Message, Is.EqualTo ("Icon must be set for each page before adding them to a Navigation Menu"));
		}

		[Test]
		public void TestDoubleAdd ()
		{
			var menu = new NavigationMenu ();

			var child = new ContentPage {
				Icon = "img.img",
				Content = new View ()
			};

			menu.Add (child);

			bool signaled = false;
			menu.PropertyChanged += (sender, args) => {
				switch (args.PropertyName) {
				case "Targets":
					signaled = true;
					break;
				}
			};

			menu.Add (child);
			
			Assert.True (menu.Targets.Contains (child));
			Assert.False (signaled);
		}

		[Test]
		public void TestRemove ()
		{
			var menu = new NavigationMenu ();

			var child = new ContentPage {
				Icon = "img.img",
				Content = new View ()
			};
			menu.Add (child);

			bool signaled = false;
			menu.PropertyChanged += (sender, args) => {
				switch (args.PropertyName) {
				case "Targets":
					signaled = true;
					break;
				}
			};

			menu.Remove (child);

			Assert.False (menu.Targets.Contains (child));
			Assert.True (signaled);
		}

		[Test]
		public void TestDoubleRemove ()
		{
			var menu = new NavigationMenu ();

			var child = new ContentPage {
				Icon = "jpg.jpg",
				Content = new View ()
			};
			menu.Add (child);
			menu.Remove (child);

			bool signaled = false;
			menu.PropertyChanged += (sender, args) => {
				switch (args.PropertyName) {
				case "Targets":
					signaled = true;
					break;
				}
			};

			menu.Remove (child);

			Assert.False (menu.Targets.Contains (child));
			Assert.False (signaled);
		}

		[Test]
		public async Task TestSendTargetSelected ()
		{
			var menu = new NavigationMenu ();
			var navForm = new NavigationPage ();

			await navForm.PushAsync (new ContentPage {
				Title = "Menu", 
				Content = menu
			});

			bool pushed = false;
			navForm.Pushed += (sender, arg) => pushed = true;

			var child = new ContentPage {
				Icon = "img.jpg",
				Content = new View ()
			};
			menu.Add (child);

			menu.SendTargetSelected (child);

			Assert.True (pushed);
			Assert.AreEqual (child, navForm.CurrentPage);
		}
	}
}
