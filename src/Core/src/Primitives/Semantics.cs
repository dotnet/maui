namespace Microsoft.Maui
{
	public struct Semantics
	{
		public string Description { get; set; }
		
		public string Hint { get; set; }

		bool Equals(Semantics other)
		{
			return string.Equals(Description, other.Description) && string.Equals(Hint, other.Hint);
		}

		public static Semantics FromLabel(string label)
        {
			return new Semantics() { Description = label };
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
			return string.Format("Description: {0}, Hint: {1}", Description, Hint);
		}
	}
}