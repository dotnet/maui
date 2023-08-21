// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Maui.Controls.Core.UnitTests
{

	public class AnimationTests : BaseTestFixture
	{
		[Fact]
		//https://bugzilla.xamarin.com/show_bug.cgi?id=51424
		public async Task AnimationRepeats()
		{
			var box = AnimationReadyHandler.Prepare(new BoxView());
			Assert.Equal(0d, box.Rotation);
			var sb = new Animation();
			var animcount = 0;
			var rot45 = new Animation(d =>
			{
				box.Rotation = d;
				if (d > 44)
					animcount++;
			}, box.Rotation, box.Rotation + 45);
			sb.Add(0, .5, rot45);
			Assert.Equal(0d, box.Rotation);

			var i = 0;
			sb.Commit(box, "foo", length: 100, repeat: () => ++i < 2);

			await Task.Delay(1000);
			Assert.Equal(2, animcount);
		}
	}
}