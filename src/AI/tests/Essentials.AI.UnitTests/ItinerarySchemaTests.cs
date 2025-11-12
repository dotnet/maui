using System;
using System.Linq;
using System.Text.Json;
using Maui.Controls.Sample.Models;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public class ItinerarySchemaTests
{
	private readonly ITestOutputHelper _output;

	private static readonly string[] TestLandmarks = new[]
	{
		"Sahara Desert",
		"Serengeti",
		"Deadvlei",
		"Grand Canyon",
		"Niagara Falls",
		"Joshua Tree",
		"Rocky Mountains",
		"Monument Valley",
		"Muir Woods",
		"Amazon Rainforest",
		"Lençóis Maranhenses",
		"Uyuni Salt Flat",
		"White Cliffs of Dover",
		"Alps",
		"Mount Fuji",
		"Wulingyuan",
		"Mount Everest",
		"Great Barrier Reef",
		"South Shetland Islands",
	};

	public ItinerarySchemaTests(ITestOutputHelper output)
	{
		_output = output;
	}

	[Fact]
	public void GenerateSchema_PrintForInspection()
	{
		// Act
		var schema = Itinerary.ToJsonSchema(TestLandmarks);
		var json = JsonSerializer.Serialize(schema, new JsonSerializerOptions { WriteIndented = true });

		// Output for inspection
		_output.WriteLine("=== Generated JSON Schema ===");
		_output.WriteLine(json);
		_output.WriteLine("=============================");

		// Assert - basic validation
		Assert.Equal(JsonValueKind.Object, schema.ValueKind);
	}

	[Fact]
	public void GenerateSchema_HasAdditionalPropertiesFalse()
	{
		// Act
		var schema = Itinerary.ToJsonSchema(TestLandmarks);

		// Assert - root level
		Assert.True(schema.TryGetProperty("additionalProperties", out var additionalProps));
		Assert.False(additionalProps.GetBoolean());
	}

	[Fact]
	public void GenerateSchema_HasXOrder()
	{
		// Act
		var schema = Itinerary.ToJsonSchema(TestLandmarks);

		// Assert - root level has x-order
		Assert.True(schema.TryGetProperty("x-order", out var xOrder));
		Assert.Equal(JsonValueKind.Array, xOrder.ValueKind);

		var orderArray = xOrder.EnumerateArray().Select(e => e.GetString()).ToList();
		Assert.Contains("title", orderArray);
		Assert.Contains("destinationName", orderArray);
		Assert.Contains("description", orderArray);
		Assert.Contains("rationale", orderArray);
		Assert.Contains("days", orderArray);
	}

	[Fact]
	public void GenerateSchema_DestinationName_HasEnumConstraint()
	{
		// Act
		var schema = Itinerary.ToJsonSchema(TestLandmarks);

		// Assert
		Assert.True(schema.TryGetProperty("properties", out var properties));
		Assert.True(properties.TryGetProperty("destinationName", out var destNameProperty));
		Assert.True(destNameProperty.TryGetProperty("enum", out var enumValues));

		var enumArray = enumValues.EnumerateArray().Select(e => e.GetString()).ToList();
		Assert.Contains("Sahara Desert", enumArray);
		Assert.Contains("Mount Fuji", enumArray);
		Assert.Equal(TestLandmarks.Length, enumArray.Count);
	}

	[Fact]
	public void GenerateSchema_Days_HasMinMaxItems()
	{
		// Act
		var schema = Itinerary.ToJsonSchema(TestLandmarks);

		// Assert
		Assert.True(schema.TryGetProperty("properties", out var properties));
		Assert.True(properties.TryGetProperty("days", out var daysProperty));
		
		Assert.True(daysProperty.TryGetProperty("minItems", out var minItems));
		Assert.Equal(3, minItems.GetInt32());
		
		Assert.True(daysProperty.TryGetProperty("maxItems", out var maxItems));
		Assert.Equal(3, maxItems.GetInt32());
	}

	[Fact]
	public void GenerateSchema_NestedObjects_HaveAdditionalPropertiesFalse()
	{
		// Act
		var schema = Itinerary.ToJsonSchema(TestLandmarks);

		// Navigate to DayPlan (nested in days.items)
		Assert.True(schema.TryGetProperty("properties", out var properties));
		Assert.True(properties.TryGetProperty("days", out var daysProperty));
		Assert.True(daysProperty.TryGetProperty("items", out var dayPlanDef));
		
		// Check DayPlan has additionalProperties: false
		Assert.True(dayPlanDef.TryGetProperty("additionalProperties", out var dayPlanAdditionalProps));
		Assert.False(dayPlanAdditionalProps.GetBoolean());

		// Navigate to Activity (nested in activities.items)
		Assert.True(dayPlanDef.TryGetProperty("properties", out var dayPlanProps));
		Assert.True(dayPlanProps.TryGetProperty("activities", out var activitiesProperty));
		Assert.True(activitiesProperty.TryGetProperty("items", out var activityDef));
		
		// Check Activity has additionalProperties: false
		Assert.True(activityDef.TryGetProperty("additionalProperties", out var activityAdditionalProps));
		Assert.False(activityAdditionalProps.GetBoolean());
	}

	[Fact]
	public void GenerateSchema_NestedObjects_HaveXOrder()
	{
		// Act
		var schema = Itinerary.ToJsonSchema(TestLandmarks);

		// Navigate to DayPlan
		Assert.True(schema.TryGetProperty("properties", out var properties));
		Assert.True(properties.TryGetProperty("days", out var daysProperty));
		Assert.True(daysProperty.TryGetProperty("items", out var dayPlanDef));
		
		// Check DayPlan has x-order
		Assert.True(dayPlanDef.TryGetProperty("x-order", out var dayPlanOrder));
		var dayPlanOrderArray = dayPlanOrder.EnumerateArray().Select(e => e.GetString()).ToList();
		Assert.Contains("title", dayPlanOrderArray);
		Assert.Contains("subtitle", dayPlanOrderArray);
		Assert.Contains("destination", dayPlanOrderArray);
		Assert.Contains("activities", dayPlanOrderArray);

		// Navigate to Activity
		Assert.True(dayPlanDef.TryGetProperty("properties", out var dayPlanProps));
		Assert.True(dayPlanProps.TryGetProperty("activities", out var activitiesProperty));
		Assert.True(activitiesProperty.TryGetProperty("items", out var activityDef));
		
		// Check Activity has x-order
		Assert.True(activityDef.TryGetProperty("x-order", out var activityOrder));
		var activityOrderArray = activityOrder.EnumerateArray().Select(e => e.GetString()).ToList();
		Assert.Contains("type", activityOrderArray);
		Assert.Contains("title", activityOrderArray);
		Assert.Contains("description", activityOrderArray);
	}

	[Fact]
	public void GenerateSchema_ActivityKind_IsEnum()
	{
		// Act
		var schema = Itinerary.ToJsonSchema(TestLandmarks);

		// Navigate to Activity type property
		Assert.True(schema.TryGetProperty("properties", out var properties));
		Assert.True(properties.TryGetProperty("days", out var daysProperty));
		Assert.True(daysProperty.TryGetProperty("items", out var dayPlanDef));
		Assert.True(dayPlanDef.TryGetProperty("properties", out var dayPlanProps));
		Assert.True(dayPlanProps.TryGetProperty("activities", out var activitiesProperty));
		Assert.True(activitiesProperty.TryGetProperty("items", out var activityDef));
		Assert.True(activityDef.TryGetProperty("properties", out var activityProps));
		Assert.True(activityProps.TryGetProperty("type", out var typeProperty));
		
		// Verify enum values
		Assert.True(typeProperty.TryGetProperty("enum", out var enumValues));
		var enumArray = enumValues.EnumerateArray().Select(e => e.GetString()).ToList();
		
		Assert.Contains("Sightseeing", enumArray);
		Assert.Contains("FoodAndDining", enumArray);
		Assert.Contains("Shopping", enumArray);
		Assert.Contains("HotelAndLodging", enumArray);
		Assert.Equal(4, enumArray.Count);
	}
}
