// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class ShellElementCollection : ShellTestBase
	{
		[Fact]
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
			Assert.Equal(1, firedCount);
		}
	}
}
