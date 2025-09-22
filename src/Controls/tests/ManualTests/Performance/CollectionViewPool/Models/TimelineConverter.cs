using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PoolMath.Data;

public class TimelineConverter : JsonConverter
{
	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		var jObject = JToken.ReadFrom(reader);
		var type = jObject["type"].ToObject<string>();

		Log result;
		switch (type)
		{
			case TestLog.TYPE_NAME:
				result = new TestLog();
				break;
			case ChemicalLog.TYPE_NAME:
				result = new ChemicalLog();
				break;
			case MaintenanceLog.TYPE_NAME:
				result = new MaintenanceLog();
				break;
			case NoteLog.TYPE_NAME:
				result = new NoteLog();
				break;
			case WeatherLog.TYPE_NAME:
				result = new WeatherLog();
				break;
			case CostLog.TYPE_NAME:
				result = new CostLog();
				break;
			case PumpScheduleChangeLog.TYPE_NAME:
				result = new PumpScheduleChangeLog();
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		serializer.Populate(jObject.CreateReader(), result);

		return result;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		// We cannot directly serialize "value" here, as that would call our own converter once more
		throw new NotImplementedException();
	}

	public override bool CanConvert(Type objectType)
	{
		return (objectType == typeof(Log));
	}

	public override bool CanWrite
	{
		get { return false; }
	}


}
