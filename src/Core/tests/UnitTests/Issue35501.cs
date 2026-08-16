using System;
using System.Runtime.CompilerServices;
using Microsoft.CSharp.RuntimeBinder;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core)]
	public class Issue35501
	{
		[Fact]
		public void ContainerIsPubliclyAccessible()
		{
			var getContainer = CallSite<Func<CallSite, Type, object>>.Create(
				Binder.GetMember(
					CSharpBinderFlags.None,
					"Container",
					typeof(object),
					new[]
					{
						CSharpArgumentInfo.Create(
							CSharpArgumentInfoFlags.UseCompileTimeType | CSharpArgumentInfoFlags.IsStaticType,
							null)
					}));

			object container = null;
			try
			{
				container = getContainer.Target(getContainer, typeof(SafeAreaEdges));
			}
			catch (RuntimeBinderException)
			{
			}

			Assert.True(container is SafeAreaEdges, "SafeAreaEdges.Container should be publicly accessible from code-behind.");
		}
	}
}
