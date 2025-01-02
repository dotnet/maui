using System;
using System.Collections;
using System.Linq;
using Microsoft.Maui.Graphics;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests;

public class FrameUnitTests : BaseTestFixture
{
	[Fact]
	public void TestPackWithoutChild()
	{
		Frame frame = new Frame();

		var parent = new NaiveLayout();

		bool thrown = false;
		try
		{
			parent.Children.Add(frame);
		}
		catch
		{
			thrown = true;
		}

		Assert.False(thrown);
	}

	[Fact]
	public void TestPackWithChild()
	{
		Frame frame = new Frame
		{
			Content = new View()
		};

		var parent = new NaiveLayout();

		bool thrown = false;
		try
		{
			parent.Children.Add(frame);
		}
		catch
		{
			thrown = true;
		}

		Assert.False(thrown);
	}
}