using System.Collections.Generic;

namespace Microsoft.Maui.Graphics.Text
{
	public interface IAttributedText
	{
		string Text { get; }
		IReadOnlyList<IAttributedTextRun> Runs { get; }
	}
}
