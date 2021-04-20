namespace Microsoft.Maui.Graphics.Text.Immutable
{
	public class AttributedTextRun : IAttributedTextRun
	{
		public AttributedTextRun(
			int start,
			int length,
			ITextAttributes attributes)
		{
			Start = start;
			Length = length;
			Attributes = attributes;
		}

		public int Start { get; }

		public int Length { get; }

		public ITextAttributes Attributes { get; }

		public override string ToString()
		{
			return $"[AttributedTextRun: Start={Start}, Length={Length}, Attributes={Attributes}]";
		}
	}
}
