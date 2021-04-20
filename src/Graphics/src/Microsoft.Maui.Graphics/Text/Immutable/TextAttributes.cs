using System.Collections.Generic;

namespace Microsoft.Maui.Graphics.Text.Immutable
{
	public class TextAttributes : Dictionary<TextAttribute, string>, ITextAttributes
	{
		public TextAttributes()
		{
		}

		public TextAttributes(IDictionary<TextAttribute, string> dictionary) : base(dictionary)
		{
		}

		public TextAttributes(
			IReadOnlyDictionary<TextAttribute, string> first,
			IReadOnlyDictionary<TextAttribute, string> second)
		{
			foreach (var entry in first)
				this[entry.Key] = entry.Value;

			foreach (var entry in second)
				this[entry.Key] = entry.Value;
		}
	}
}
