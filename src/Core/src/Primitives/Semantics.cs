namespace Microsoft.Maui
{
	public enum SemanticHeadingLevel
	{
		Default = -1,
		Level1 = 1,
		Level2 = 2,
		Level3 = 3,
		Level4 = 4,
		Level5 = 5,
		Level6 = 6,
		Level7 = 7,
		Level8 = 8,
		Level9 = 9,
		None = 0
	}

	public class Semantics
	{
		bool _isHeading;

		public string? Description { get; set; }

		public string? Hint { get; set; }

		public SemanticHeadingLevel HeadingLevel { get; set; }

		public Semantics()
		{
			HeadingLevel = SemanticHeadingLevel.Default;
		}

		public bool IsHeading 
		{ 
			get
			{
				if (HeadingLevel == SemanticHeadingLevel.None)
					return false;
				else if (HeadingLevel != SemanticHeadingLevel.Default)
					return true;

				return _isHeading;
			}
			
			set => _isHeading = value; 
		}

		bool Equals(Semantics other)
		{
			return string.Equals(Description, other.Description)
				&& string.Equals(Hint, other.Hint)
				&& string.Equals(IsHeading, other.IsHeading);
		}

		public override bool Equals(object? obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}
			if (obj.GetType() != GetType())
			{
				return false;
			}
			return Equals((Semantics)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = Description != null ? Description.GetHashCode() : 0;
				hashCode = Hint != null ? (hashCode * 397) ^ Hint.GetHashCode() : 0;
				hashCode = (hashCode * 397) ^ IsHeading.GetHashCode();

				return hashCode;
			}
		}

		public static bool operator ==(Semantics left, Semantics right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(Semantics left, Semantics right)
		{
			return !left.Equals(right);
		}

		public override string ToString()
		{
			return string.Format("Description: {0}, Hint: {1}, IsHeading: {2}", Description, Hint, IsHeading);
		}
	}
}