using System;
using NUnit.Framework;

using Xamarin.Forms.Core.UnitTests;

namespace Xamarin.Forms.Xaml.UnitTests
{
	[TestFixture]
	public class FactoryMethodTests : BaseTestFixture
	{
		[Test]
		public void ThrowOnMissingCtor ()
		{
			var xaml = @"
			<local:MockView
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
				xmlns:local=""clr-namespace:Xamarin.Forms.Xaml.UnitTests;assembly=Xamarin.Forms.Xaml.UnitTests"" >
				<local:MockView.Content>
					<local:MockFactory>
						<x:Arguments>
							<x:Object/>
							<x:String>bar</x:String>
							<x:Int32>42</x:Int32>
						</x:Arguments>
					</local:MockFactory>
				</local:MockView.Content>
			</local:MockView>";
			Assert.Throws<MissingMethodException> (()=>new MockView ().LoadFromXaml (xaml));
		}

		[Test]
		public void ThrowOnMissingMethod ()
		{
			var xaml = @"
			<local:MockView
				xmlns=""http://xamarin.com/schemas/2014/forms""
				xmlns:x=""http://schemas.microsoft.com/winfx/2009/xaml""
				xmlns:local=""clr-namespace:Xamarin.Forms.Xaml.UnitTests;assembly=Xamarin.Forms.Xaml.UnitTests"" >
				<local:MockView.Content>
					<local:MockFactory x:FactoryMethod=""Factory"">
						<x:Arguments>
							<x:Object/>
							<x:String>bar</x:String>
							<x:Int32>42</x:Int32>
						</x:Arguments>
					</local:MockFactory>
				</local:MockView.Content>
			</local:MockView>";
			Assert.Throws<MissingMemberException> (()=>new MockView ().LoadFromXaml (xaml));
		}
	}
}
