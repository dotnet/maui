namespace Microsoft.Maui
{
	public partial class Semantics
	{
		bool _isHeading;

		public string? Description { get; set; }

		public string? Hint { get; set; }

		public Semantics()
		{
		}

		public bool IsHeading 
		{ 
			get
			{
#if WINDOWS
				if (HeadingLevel == SemanticHeadingLevel.None)
					return false;
				else if (HeadingLevel != SemanticHeadingLevel.Default)
					return true;
#endif
				return _isHeading;
			}
			
			set => _isHeading = value; 
		}

		public override string ToString()
		{
			return string.Format("Description: {0}, Hint: {1}, IsHeading: {2}", Description, Hint, IsHeading);
		}
	}
}