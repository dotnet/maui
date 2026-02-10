using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

/// <summary>
/// Tests for JsonMerger - a utility for merging JSON documents.
/// </summary>
public partial class JsonMergerTests
{
	private static JsonSerializerOptions SerializerOptions =>
		field ??= new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			Converters = { new JsonStringEnumConverter() }
		};

	public class SimpleModel
	{
		public string? Name { get; set; }
		public int Age { get; set; }
		public string? City { get; set; }
	}

	public class NestedModel
	{
		public string? Title { get; set; }
		public PersonInfo? Person { get; set; }
	}

	public class PersonInfo
	{
		public string? FirstName { get; set; }
		public string? LastName { get; set; }
		public int Age { get; set; }
	}

	public class ArrayModel
	{
		public string? Title { get; set; }
		public List<ItemInfo>? Items { get; set; }
	}

	public class ItemInfo
	{
		public string? Name { get; set; }
		public string? Description { get; set; }
		public decimal Price { get; set; }
	}

	public class ItineraryModel
	{
		public string? Destination { get; set; }
		public int Days { get; set; }
		public List<DayPlan>? Schedule { get; set; }
	}

	public class DayPlan
	{
		public int Day { get; set; }
		public string? Title { get; set; }
		public string? Description { get; set; }
		public List<Activity>? Activities { get; set; }
	}

	public class Activity
	{
		public string? Time { get; set; }
		public string? Name { get; set; }
		public string? Details { get; set; }
	}
}
