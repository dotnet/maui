using System;
using System.Collections.Generic;
using System.IO;

namespace Xamarin.Forms.StyleSheets
{
	sealed class CssReader : TextReader
	{
		readonly TextReader _reader;

		public CssReader(TextReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException(nameof(reader));

			_reader = reader;
		}

		readonly Queue<char> _cache = new Queue<char>();

		//skip comments
		//TODO unescape escaped sequences
		public override int Peek()
		{
			if (_cache.Count > 0)
				return _cache.Peek();

			int p = _reader.Peek();
			if (p <= 0)
				return p;
			if (unchecked((char)p) != '/')
				return p;

			_cache.Enqueue(unchecked((char)_reader.Read()));
			p = _reader.Peek();
			if (p <= 0)
				return _cache.Peek();
			if (unchecked((char)p) != '*')
				return _cache.Peek();

			_cache.Clear();
			_reader.Read(); //consume the '*'

			bool hasStar = false;
			while (true)
			{
				var next = _reader.Read();
				if (next <= 0)
					return next;
				if (unchecked((char)next) == '*')
					hasStar = true;
				else if (hasStar && unchecked((char)next) == '/')
					return Peek(); //recursively call self for comments following comments
				else
					hasStar = false;
			}
		}

		//skip comments
		//TODO unescape escaped sequences
		public override int Read()
		{
			if (_cache.Count > 0)
				return _cache.Dequeue();

			int p = _reader.Read();
			if (p <= 0)
				return p;
			var c = unchecked((char)p);
			if (c != '/')
				return p;

			_cache.Enqueue(c);
			p = _reader.Read();
			if (p <= 0)
				return _cache.Dequeue();
			c = unchecked((char)p);
			if (c != '*')
				return _cache.Dequeue();

			_cache.Clear();
			_reader.Read(); //consume the '*'

			bool hasStar = false;
			while (true)
			{
				var next = _reader.Read();
				if (next <= 0)
					return next;
				if (unchecked((char)next) == '*')
					hasStar = true;
				else if (hasStar && unchecked((char)next) == '/')
					return Read(); //recursively call self for comments following comments
				else
					hasStar = false;
			}
		}
	}
}