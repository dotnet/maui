using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using PoolMath.Data;

namespace PoolMath.Data;

public class PumpDailySchedule
{
	[JsonProperty("segments")]
	[System.Text.Json.Serialization.JsonPropertyName("segments")]
	public List<PumpScheduleSegment> Segments { get; set; } = new List<PumpScheduleSegment>();

	public double? CalculateFreeChlorinePpm(Pool pool)
	{
		if (pool == null || !pool.SwgLbsPerDay.HasValue)
			return null;

		return Segments.Sum(s => s.CalculateFreeChlorinePpm(pool) ?? 0);
	}

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public bool HasOverlaps
	{
		get
		{
			for (var i = 0; i < Segments.Count; i++)
			{
				var seg = Segments[i];

				foreach (var nr in seg.GetNormalizedRanges())
				{
					for (var j = 0; j < Segments.Count; j++)
					{
						// Don't check ourselves
						if (j == i)
							continue;

						var seg2 = Segments[j];

						foreach (var nr2 in seg.GetNormalizedRanges())
						{
							// Check if nr overlaps nr2
							if ((nr2.start >= nr.start && nr2.start <= nr.end)
								|| (nr2.end >= nr.start && nr2.end <= nr.end))
								return true;
						}

					}
				}
			}

			return false;
		}
	}
}

public class PumpScheduleSegment
{
	[JsonProperty("name")]
	[System.Text.Json.Serialization.JsonPropertyName("name")]
	public string Name { get; set; }

	[JsonProperty("startHour")]
	[System.Text.Json.Serialization.JsonPropertyName("startHour")]
	public int StartHour { get; set; } = 0;

	[JsonProperty("startMinute")]
	[System.Text.Json.Serialization.JsonPropertyName("startMinute")]
	public int StartMinute { get; set; } = 0;

	[JsonProperty("endHour")]
	[System.Text.Json.Serialization.JsonPropertyName("endHour")]
	public int EndHour { get; set; } = 0;

	[JsonProperty("endMinute")]
	[System.Text.Json.Serialization.JsonPropertyName("endMinute")]
	public int EndMinute { get; set; } = 0;

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public TimeSpan StartTime
	{
		get => new TimeSpan(StartHour, StartMinute, 0);
		set
		{
			StartHour = value.Hours;
			StartMinute = value.Minutes;
		}
	}

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public TimeSpan EndTime
	{
		get => new TimeSpan(EndHour, EndMinute, 0);
		set
		{
			EndHour = value.Hours;
			EndMinute = value.Minutes;
		}
	}

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public TimeSpan RunTime
	{
		get
		{
			if (EndTime > StartTime)
				return EndTime - StartTime;
			else
			{
				var total = (new TimeSpan(0, 24, 0, 0, 0) - StartTime) + EndTime;
				return total;
			}
		}
		set => EndTime = StartTime.Add(value);
	}

	public IEnumerable<(TimeSpan start, TimeSpan end)> GetNormalizedRanges()
	{
		if (StartTime > EndTime)
		{
			return new[] {
				(StartTime, new TimeSpan(0, 23, 59, 59, 999)),
				(new TimeSpan(0, 0, 0, 0, 0), EndTime)
			};
		}

		return new[] { (StartTime, EndTime) };
	}

	[JsonProperty("speed")]
	[System.Text.Json.Serialization.JsonPropertyName("speed")]
	public int Speed { get; set; }

	[JsonProperty("swgPercent")]
	[System.Text.Json.Serialization.JsonPropertyName("swgPercent")]
	public double? SwgPercent { get; set; }

	[JsonProperty("heat")]
	[System.Text.Json.Serialization.JsonPropertyName("heat")]
	public bool Heat { get; set; } = false;

	[JsonProperty("heatTemp")]
	[System.Text.Json.Serialization.JsonPropertyName("heatTemp")]
	public double? HeatTemp { get; set; }

	[JsonProperty("heatTempUnit")]
	[System.Text.Json.Serialization.JsonPropertyName("heatTempUnit")]
	public Units HeatTempUnit { get; set; } = Units.US;

	public double? CalculateFreeChlorinePpm(Pool pool)
	{
		if (pool == null || !pool.SwgLbsPerDay.HasValue)
			return null;

		var runtimeHours = RunTime.TotalHours;

		var ppm = 1d;
		//var ppm = 1; Calculator.CalculateSwgFcPpm(
		//			pool.SwgLbsPerDay.Value,
		//			SwgPercent ?? 0,
		//			runtimeHours,
		//			pool.Volume,
		//			pool.PoolVolumeUnit);

		return Math.Round(ppm, 1);
	}
}

public class PumpScheduleChangeLog : Log
{
	public const string API_ROUTE = "pumpschedules";

	public const string TYPE_NAME = "pumpschedule";

	[System.Text.Json.Serialization.JsonPropertyName("type")]
	public override string Type => TYPE_NAME;

	[JsonIgnore]
	[System.Text.Json.Serialization.JsonIgnore]
	public override string ApiRoute => API_ROUTE;

	[JsonProperty("isOverride")]
	[System.Text.Json.Serialization.JsonPropertyName("isOverride")]
	public bool IsOverride { get; set; } = false;

	[JsonProperty("overrideSegment")]
	[System.Text.Json.Serialization.JsonPropertyName("overrideSegment")]
	public PumpScheduleSegment OverrideSegment { get; set; }

	[JsonProperty("dailySchedule")]
	[System.Text.Json.Serialization.JsonPropertyName("dailySchedule")]
	public PumpDailySchedule DailySchedule { get; set; } = new PumpDailySchedule();
}

public class ScheduleDayCalculation
{
	[System.Text.Json.Serialization.JsonPropertyName("Day")]
	public DateTime Day { get; set; } = DateTime.UtcNow;

	[System.Text.Json.Serialization.JsonPropertyName("FreeChlorinePpm")]
	public double FreeChlorinePpm { get; set; } = 0;

	[System.Text.Json.Serialization.JsonPropertyName("PumpRunTime")]
	public TimeSpan PumpRunTime { get; set; } = TimeSpan.Zero;

	[System.Text.Json.Serialization.JsonPropertyName("HeatTime")]
	public TimeSpan HeatTime { get; set; } = TimeSpan.Zero;
}

public enum ScheduleDayType
{
	SameEveryDay,
	DifferentWeekVsWeekend,
	DifferentEveryDay
}

public class PumpScheduling
{
	static PumpScheduleChangeLog GetScheduleChangeLogForDay(DateTime day, IEnumerable<PumpScheduleChangeLog> logs)
	{
		var maxOfDay = new DateTime(day.Year, day.Month, day.Day, 23, 59, 59, 999, DateTimeKind.Utc);

		return logs
			.Where(l => l.Type == PumpScheduleChangeLog.TYPE_NAME && !l.IsOverride && l.LogTimestamp <= maxOfDay)
			.OrderByDescending(l => l.LogTimestamp)
			.FirstOrDefault() as PumpScheduleChangeLog;
	}

	public static ScheduleDayCalculation CalculateSchedule(DateTime day, Pool pool, IEnumerable<PumpScheduleChangeLog> logs)
	{
		var calc = new ScheduleDayCalculation();

		var scheduleLog = GetScheduleChangeLogForDay(day, logs);

		var schedule = scheduleLog.DailySchedule; // scheduleLog.ScheduleForDay(day);

		var overrideLogs = logs.Where(l => l.IsOverride && l.LogTimestamp.Year == day.Year
			&& l.LogTimestamp.Month == day.Month && l.LogTimestamp.Day == day.Day);

		AddScheduleSegment(day, pool, calc, schedule, overrideLogs);

		return calc;
	}

	public static IEnumerable<ScheduleDayCalculation> CalculateSchedules(DateTime start, DateTime end, Pool pool, IEnumerable<PumpScheduleChangeLog> logs)
	{
		var scheduleLogs = logs.Where(l => l.Type == PumpScheduleChangeLog.TYPE_NAME && !l.IsOverride && l.LogTimestamp >= start && l.LogTimestamp <= end)
			.ToList();

		// For the first day in our range, likely there's a schedule log change before the date that is used for it
		// so go fetch it and add to the list of possible schedules
		var first = GetScheduleChangeLogForDay(start, logs);
		if (first != null)
			scheduleLogs.Insert(0, first);


		PumpScheduleChangeLog GetDailyScheduleLog(DateTime day)
		{
			var maxOfDay = new DateTime(day.Year, day.Month, day.Day, 23, 59, 59, 999, DateTimeKind.Utc);

			return scheduleLogs.Where(l => l.LogTimestamp <= maxOfDay && !l.IsOverride)
				.OrderByDescending(l => l.LogTimestamp)
				.FirstOrDefault() as PumpScheduleChangeLog;
		}

		var results = new List<ScheduleDayCalculation>();

		for (var day = start.Date; day.Date <= end.Date; day = day.AddDays(1))
		{
			var scheduleLog = GetDailyScheduleLog(day);

			if (scheduleLog == null)
				continue;

			var calc = new ScheduleDayCalculation();

			var schedule = scheduleLog.DailySchedule; //.ScheduleForDay(day);

			var overrideLogs = logs.Where(l => l.IsOverride && l.LogTimestamp.Year == day.Year
				&& l.LogTimestamp.Month == day.Month && l.LogTimestamp.Day == day.Day);

			AddScheduleSegment(day, pool, calc, schedule, overrideLogs);

			results.Add(calc);
		}

		return results;
	}

	static void AddScheduleSegment(DateTime day, Pool pool, ScheduleDayCalculation calculation, PumpDailySchedule schedule, IEnumerable<PumpScheduleChangeLog> overrideLogs)
	{
		// If overrides are found, we need to  step through each minute of the day
		// and find the applicable override or segment

		void AddSegmentForHours(PumpScheduleSegment segment, TimeSpan length)
		{
			if (pool.ChemistryType == PoolChemistryType.SWG)
			{
				var fcPpm = 1;// Calculator.CalculateSwgFcPpm(pool.SwgLbsPerDay ?? 0, segment.SwgPercent ?? 0, length.TotalHours, pool.Volume, pool.PoolVolumeUnit);

				calculation.FreeChlorinePpm += fcPpm;
			}

			if (segment.Heat)
				calculation.HeatTime += length;

			calculation.PumpRunTime += length;
		}

		// Override logs make things trickier
		// right now I'm just looping through every minute of the day in the case where overrides exist
		// to easily detect for every minute which schedule to follow, an override, or the daily schedule
		if (overrideLogs.Any())
		{
			bool DoRangesOverlapMinute(TimeSpan minute, IEnumerable<(TimeSpan start, TimeSpan end)> ranges)
			{
				if (ranges?.Any() ?? false)
				{
					foreach (var r in ranges)
					{
						if (r.start <= minute && r.end > minute)
							return true;
					}
				}

				return false;
			}

			var segmentMinutes = new Dictionary<PumpScheduleSegment, int>();

			for (var m = 0; m < 24 * 60; m++)
			{
				var minute = TimeSpan.FromMinutes(m);

				var usedOverride = false;

				Console.WriteLine(minute);
				foreach (var ol in overrideLogs)
				{
					var olRanges = ol.OverrideSegment.GetNormalizedRanges();

					if (DoRangesOverlapMinute(minute, olRanges))
					{
						usedOverride = true;
						if (segmentMinutes.ContainsKey(ol.OverrideSegment))
							segmentMinutes[ol.OverrideSegment]++;
						else
							segmentMinutes.Add(ol.OverrideSegment, 1);
						break;
					}
				}

				if (usedOverride)
					continue;

				foreach (var segment in schedule.Segments)
				{
					var nRanges = segment.GetNormalizedRanges();

					if (DoRangesOverlapMinute(minute, nRanges))
					{
						if (segmentMinutes.ContainsKey(segment))
							segmentMinutes[segment]++;
						else
							segmentMinutes.Add(segment, 1);
						break;
					}
				}
			}

			foreach (var smKvp in segmentMinutes)
			{
				if (smKvp.Value > 0)
					AddSegmentForHours(smKvp.Key, TimeSpan.FromMinutes(smKvp.Value));
			}
		}
		else
		{
			foreach (var segment in schedule.Segments)
			{
				AddSegmentForHours(segment, segment.RunTime);
			}
		}
	}
}