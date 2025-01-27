using System;

namespace TestCases.Shared.CollectionViewTests
{
	// All the code in this file is included in all platforms.
	public class CollectionViewTests
	{
	}

	[AttributeUsage(AttributeTargets.Class)]
	public class CollectionViewTestPageAttribute : Attribute
	{
		public CollectionViewTestPageAttribute(string name = null) : base()
		{
		}
	}
}
