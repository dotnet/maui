using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Xamarin.Forms.Core.UnitTests
{
	[TestFixture]
	public class ShellElementCollection : ShellTestBase
	{
		[Test]
		public void ClearFiresOnlyOneRemovedEvent()
		{
			Shell shell = new Shell();
			shell.Items.Add(CreateShellItem());
			shell.Items.Add(CreateShellItem());
			shell.Items.Add(CreateShellItem());
			shell.Items.Add(CreateShellItem());
			var shellSection = shell.CurrentItem.CurrentItem;

			int firedCount = 0;

			(shellSection as IShellSectionController).ItemsCollectionChanged += (_, e) =>
			{
				if (e.OldItems != null)
					firedCount++;
			};

			shellSection.Items.Clear();
			Assert.AreEqual(1, firedCount);
		}
	}
}