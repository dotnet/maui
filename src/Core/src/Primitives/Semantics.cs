namespace Microsoft.Maui
{
	public enum SemanticHeadingLevel
	{
		None = 0,
		Level1 = 1,
		Level2 = 2,
		Level3 = 3,
		Level4 = 4,
		Level5 = 5,
		Level6 = 6,
		Level7 = 7,
		Level8 = 8,
		Level9 = 9
	}

	public partial class Semantics
	{
		public string? Description { get; set; }

		public string? Hint { get; set; }

		public bool IsHeading => HeadingLevel != SemanticHeadingLevel.None;

		public Semantics()
		{
		}

		public SemanticHeadingLevel HeadingLevel { get; set; } = SemanticHeadingLevel.Default;

		public override string ToString()
		{
			return string.Format("Description: {0}, Hint: {1}, HeadingLevel: {2}", Description, Hint, HeadingLevel);
		}
	}
}