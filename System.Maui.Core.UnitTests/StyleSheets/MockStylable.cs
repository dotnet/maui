using System.Collections.Generic;
using NUnit.Framework;

namespace Xamarin.Forms.StyleSheets.UnitTests
{
	class MockStylable : IStyleSelectable
	{
		public IEnumerable<IStyleSelectable> Children { get; set; }
		public IList<string> Classes { get; set; }
		public string Id { get; set; }
		public string[] NameAndBases { get; set; }
		public IStyleSelectable Parent { get; set; }
	}
}