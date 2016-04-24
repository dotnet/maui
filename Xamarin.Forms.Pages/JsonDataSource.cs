using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Xamarin.Forms.Pages
{
	public class JsonDataSource : BaseDataSource
	{
		readonly ObservableCollection<IDataItem> _dataItems = new ObservableCollection<IDataItem>();
		Task _currentParseTask;
		bool _initialized;
		JsonSource _source;

		public JsonDataSource()
		{
		}

		internal JsonDataSource(JToken rootToken)
		{
			ParseJsonToken(rootToken);
		}

		[TypeConverter(typeof(JsonSourceConverter))]
		public JsonSource Source
		{
			get { return _source; }
			set
			{
				if (_source == value)
					return;
				_source = value;

				_dataItems.Clear();
				if (value != null && _initialized)
				{
					_currentParseTask = ParseJson();
					_currentParseTask.ContinueWith(t => { throw t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
				}
			}
		}

		protected override async Task<IList<IDataItem>> GetRawData()
		{
			if (!_initialized)
			{
				Task task = _currentParseTask = ParseJson();
				await task;
			}
			else if (_currentParseTask != null && _currentParseTask.IsCompleted == false)
				await _currentParseTask;
			return _dataItems;
		}

		protected override object GetValue(string key)
		{
			IDataItem target = _dataItems.FirstOrDefault(d => d.Name == key);
			return target?.Value;
		}

		protected override bool SetValue(string key, object value)
		{
			IDataItem target = _dataItems.FirstOrDefault(d => d.Name == key);
			if (target == null)
			{
				_dataItems.Add(new DataItem(key, value));
				return true;
			}
			if (target.Value == value)
				return false;
			target.Value = value;
			return true;
		}

		object GetValueForJToken(JToken token)
		{
			switch (token.Type)
			{
				case JTokenType.Object:
				case JTokenType.Array:
					return new JsonDataSource(token);
				case JTokenType.Constructor:
				case JTokenType.Property:
				case JTokenType.Comment:
					throw new NotImplementedException();
				case JTokenType.Integer:
					return (int)token;
				case JTokenType.Float:
					return (float)token;
				case JTokenType.Raw:
				case JTokenType.String:
					return (string)token;
				case JTokenType.Boolean:
					return (bool)token;
				case JTokenType.Date:
					return (DateTime)token;
				case JTokenType.Bytes:
					return (byte[])token;
				case JTokenType.Guid:
					return (Guid)token;
				case JTokenType.Uri:
					return (Uri)token;
				case JTokenType.TimeSpan:
					return (TimeSpan)token;
				default:
					return null;
			}
		}

		async Task ParseJson()
		{
			_initialized = true;

			if (Source == null)
				return;

			IsLoading = true;
			string json = await Source.GetJson();
			JToken jToken = JToken.Parse(json);
			ParseJsonToken(jToken);
			IsLoading = false;
		}

		void ParseJsonToken(JToken token)
		{
			var jArray = token as JArray;
			var jObject = token as JObject;
			if (jArray != null)
			{
				for (var i = 0; i < jArray.Count; i++)
				{
					JToken obj = jArray[i];
					_dataItems.Add(new DataItem(i.ToString(), GetValueForJToken(obj)));
				}
			}
			else if (jObject != null)
			{
				foreach (KeyValuePair<string, JToken> kvp in jObject)
				{
					_dataItems.Add(new DataItem(kvp.Key, GetValueForJToken(kvp.Value)));
				}
			}
		}
	}
}