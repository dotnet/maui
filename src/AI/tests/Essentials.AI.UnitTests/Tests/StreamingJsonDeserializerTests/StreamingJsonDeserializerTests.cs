namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class StreamingJsonDeserializerTests
{
	public class SimpleModel
	{
		public string? Text { get; set; }
		public int Score { get; set; }
	}

	public class NestedModel
	{
		public string? Text { get; set; }
		public MetaData? Meta { get; set; }
	}

	public class MetaData
	{
		public int Score { get; set; }
		public string? Author { get; set; }
	}

	public class ComplexModel
	{
		public string? Title { get; set; }
		public List<Item>? Items { get; set; }
		public Dictionary<string, string>? Tags { get; set; }
	}

	public class Item
	{
		public string? Name { get; set; }
		public decimal Price { get; set; }
		public ItemCategory Category { get; set; }
	}

	public enum ItemCategory
	{
		Electronics,
		Clothing,
		Food
	}

	public class NumericModel
	{
		public int Integer { get; set; }
		public double Decimal { get; set; }
		public bool IsActive { get; set; }
		public bool IsComplete { get; set; }
	}

	public class EmotionalResponse
	{
		public double Anger { get; set; }
		public double Happiness { get; set; }
		public string? Reply { get; set; }
	}
}
