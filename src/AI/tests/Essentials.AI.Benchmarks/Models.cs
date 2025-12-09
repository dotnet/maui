using System.Collections.Generic;

namespace Maui.Controls.Sample.Models;

public class TravelItinerary
{
	public string? Title { get; set; }
	public string? Destination { get; set; }
	public string? Description { get; set; }
	public List<DayItinerary>? Days { get; set; }
}

public class DayItinerary
{
	public int Day { get; set; }
	public string? Date { get; set; }
	public string? Summary { get; set; }
	public List<Activity>? Activities { get; set; }
}

public class Activity
{
	public string? Time { get; set; }
	public string? Title { get; set; }
	public string? Description { get; set; }
	public ActivityType Type { get; set; }
}

public enum ActivityType
{
	Breakfast,
	Sightseeing,
	Lunch,
	Adventure,
	Dinner,
	Cultural,
	Leisure
}
