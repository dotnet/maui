using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Maui.Graphics.Text;
using XmlNames = Microsoft.Maui.Graphics.Text.XmlAttributedTextNames;

namespace Microsoft.Maui.Graphics.Text
{
	public class XmlAttributedTextReader
	{
		private XmlReader _reader;

		private StringWriter _writer;
		private readonly List<IAttributedTextRun> _runs = new List<IAttributedTextRun>();
		private bool _inContent;
		private bool _contentEncoded;

		public IAttributedText Read(string text)
		{
			using (var stringReader = new StringReader(text))
			{
				return Read(stringReader);
			}
		}

		public IAttributedText Read(TextReader reader)
		{
			_writer = new StringWriter();
			_runs.Clear();

			using (_reader = XmlReader.Create(reader))
			{
				try
				{
					while (_reader.Read())
					{
						switch (_reader.NodeType)
						{
							case XmlNodeType.CDATA:
								HandleText();
								break;
							case XmlNodeType.Text:
								HandleText();
								break;
							case XmlNodeType.Element:
								ElementStarted();
								break;
							case XmlNodeType.EndElement:
								ElementEnded();
								break;
						}
					}
				}
				catch (XmlException)
				{
					_writer.Write(reader.ReadToEnd());
				}
			}

			var text = _writer.ToString();
			var textLength = text.Length;
			_runs.Optimize(textLength);

			return new AttributedText(text, _runs, true);
		}

		protected void ElementStarted()
		{
			string elementName = _reader.Name;

			if (XmlNames.Content.Equals(elementName))
			{
				_inContent = true;
				_contentEncoded = ReadBool(XmlNames.Encoded);
			}
			else if (XmlNames.Run.Equals(elementName))
			{
				ReadRun();
			}
		}

		protected void ElementEnded()
		{
			string elementName = _reader.Name;
			if (XmlNames.Content.Equals(elementName))
				_inContent = false;
		}

		private void HandleText()
		{
			if (_inContent)
			{
				if (_contentEncoded)
				{
					byte[] bytes = Convert.FromBase64String(_reader.Value);
					string text = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
					_writer.Write(text);
				}
				else
				{
					_writer.Write(_reader.Value);
				}
			}
		}

		private void ReadRun()
		{
			TextAttributes attributes = new TextAttributes();

			var start = ReadInt(XmlNames.Start, 0);
			var length = ReadInt(XmlNames.Length, 0);

			if (_reader.HasAttributes)
			{
				_reader.MoveToElement();
				while (_reader.MoveToNextAttribute())
				{
					var attributeName = _reader.Name;
					var attributeValue = _reader.Value;

					if (!(XmlNames.Start.Equals(attributeName) || XmlNames.Length.Equals(attributeName)))
					{
						if (Enum.TryParse(attributeName, out TextAttribute key))
							attributes[key] = attributeValue;
					}
				}

				_reader.MoveToElement();
			}

			var run = new AttributedTextRun(start, length, attributes);
			_runs.Add(run);
		}

		private bool ReadBool(string attribute)
		{
			var value = _reader.GetAttribute(attribute);
			if (value != null)
				return ParseBool(value);

			return false;
		}

		private bool ParseBool(string value)
		{
			if (value != null)
			{
				if (bool.TryParse(value, out var boolValue))
					return boolValue;
			}

			return false;
		}

		private int ReadInt(string attribute, int defaultValue)
		{
			var value = _reader.GetAttribute(attribute);
			if (value != null)
				return ParseInt(value, defaultValue);

			return defaultValue;
		}

		private int ParseInt(string value, int defaultValue)
		{
			if (value != null)
			{
				if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var intValue))
					return intValue;
			}

			return defaultValue;
		}
	}
}
