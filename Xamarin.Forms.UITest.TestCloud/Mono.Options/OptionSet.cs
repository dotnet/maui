using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Mono.Options
{
	public class OptionSet : KeyedCollection<string, Option>
	{
		const int OptionWidth = 29;
		const int DescriptionFirstWidth = 80 - OptionWidth;
		const int DescriptionRemWidth = 80 - OptionWidth - 2;

		readonly Regex _valueOption = new Regex(
			@"^(?<flag>--|-|/)(?<name>[^:=]+)((?<sep>[:=])(?<value>.*))?$");

		readonly List<ArgumentSource> _sources = new List<ArgumentSource>();

		public OptionSet()
			: this(delegate(string f) { return f; })
		{
		}

		public OptionSet(Converter<string, string> localizer)
		{
			MessageLocalizer = localizer;
			ArgumentSources = new ReadOnlyCollection<ArgumentSource>(_sources);
		}

		public Converter<string, string> MessageLocalizer { get; }

		public ReadOnlyCollection<ArgumentSource> ArgumentSources { get; }

		protected override string GetKeyForItem(Option item)
		{
			if (item == null)
				throw new ArgumentNullException("option");
			if (item.Names != null && item.Names.Length > 0)
				return item.Names[0];
			// This should never happen, as it's invalid for Option to be
			// constructed w/o any names.
			throw new InvalidOperationException("Option has no names!");
		}

		[Obsolete("Use KeyedCollection.this[string]")]
		protected Option GetOptionForName(string option)
		{
			if (option == null)
				throw new ArgumentNullException("option");
			try
			{
				return base[option];
			}
			catch (KeyNotFoundException)
			{
				return null;
			}
		}

		protected override void InsertItem(int index, Option item)
		{
			base.InsertItem(index, item);
			AddImpl(item);
		}

		protected override void RemoveItem(int index)
		{
			Option p = Items[index];
			base.RemoveItem(index);
			// KeyedCollection.RemoveItem() handles the 0th item
			for (int i = 1; i < p.Names.Length; ++i)
				Dictionary.Remove(p.Names[i]);
		}

		protected override void SetItem(int index, Option item)
		{
			base.SetItem(index, item);
			AddImpl(item);
		}

		void AddImpl(Option option)
		{
			if (option == null)
				throw new ArgumentNullException("option");
			List<string> added = new List<string>(option.Names.Length);
			try
			{
				// KeyedCollection.InsertItem/SetItem handle the 0th name.
				for (int i = 1; i < option.Names.Length; ++i)
				{
					Dictionary.Add(option.Names[i], option);
					added.Add(option.Names[i]);
				}
			}
			catch (Exception)
			{
				foreach (string name in added)
					Dictionary.Remove(name);
				throw;
			}
		}

		public OptionSet Add(string header)
		{
			if (header == null)
				throw new ArgumentNullException("header");
			Add(new Category(header));
			return this;
		}

		public new OptionSet Add(Option option)
		{
			base.Add(option);
			return this;
		}

		public OptionSet Add(string prototype, Action<string> action)
		{
			return Add(prototype, null, action);
		}

		public OptionSet Add(string prototype, string description, Action<string> action)
		{
			return Add(prototype, description, action, false);
		}

		public OptionSet Add(string prototype, string description, Action<string> action, bool hidden)
		{
			if (action == null)
				throw new ArgumentNullException("action");
			Option p = new ActionOption(prototype, description, 1,
				delegate(OptionValueCollection v) { action(v[0]); }, hidden);
			base.Add(p);
			return this;
		}

		public OptionSet Add(string prototype, OptionAction<string, string> action)
		{
			return Add(prototype, null, action);
		}

		public OptionSet Add(string prototype, string description, OptionAction<string, string> action)
		{
			return Add(prototype, description, action, false);
		}

		public OptionSet Add(string prototype, string description, OptionAction<string, string> action, bool hidden)
		{
			if (action == null)
				throw new ArgumentNullException("action");
			Option p = new ActionOption(prototype, description, 2,
				delegate(OptionValueCollection v) { action(v[0], v[1]); }, hidden);
			base.Add(p);
			return this;
		}

		public OptionSet Add<T>(string prototype, Action<T> action)
		{
			return Add(prototype, null, action);
		}

		public OptionSet Add<T>(string prototype, string description, Action<T> action)
		{
			return Add(new ActionOption<T>(prototype, description, action));
		}

		public OptionSet Add<TKey, TValue>(string prototype, OptionAction<TKey, TValue> action)
		{
			return Add(prototype, null, action);
		}

		public OptionSet Add<TKey, TValue>(string prototype, string description, OptionAction<TKey, TValue> action)
		{
			return Add(new ActionOption<TKey, TValue>(prototype, description, action));
		}

		public OptionSet Add(ArgumentSource source)
		{
			if (source == null)
				throw new ArgumentNullException("source");
			_sources.Add(source);
			return this;
		}

		protected virtual OptionContext CreateOptionContext()
		{
			return new OptionContext(this);
		}

		public List<string> Parse(IEnumerable<string> arguments)
		{
			if (arguments == null)
				throw new ArgumentNullException("arguments");
			OptionContext c = CreateOptionContext();
			c.OptionIndex = -1;
			bool process = true;
			List<string> unprocessed = new List<string>();
			Option def = Contains("<>") ? this["<>"] : null;
			ArgumentEnumerator ae = new ArgumentEnumerator(arguments);
			foreach (string argument in ae)
			{
				++c.OptionIndex;
				if (argument == "--")
				{
					process = false;
					continue;
				}
				if (!process)
				{
					Unprocessed(unprocessed, def, c, argument);
					continue;
				}
				if (AddSource(ae, argument))
					continue;
				if (!Parse(argument, c))
					Unprocessed(unprocessed, def, c, argument);
			}
			if (c.Option != null)
				c.Option.Invoke(c);
			return unprocessed;
		}

		bool AddSource(ArgumentEnumerator ae, string argument)
		{
			foreach (ArgumentSource source in _sources)
			{
				IEnumerable<string> replacement;
				if (!source.GetArguments(argument, out replacement))
					continue;
				ae.Add(replacement);
				return true;
			}
			return false;
		}

		static bool Unprocessed(ICollection<string> extra, Option def, OptionContext c, string argument)
		{
			if (def == null)
			{
				extra.Add(argument);
				return false;
			}
			c.OptionValues.Add(argument);
			c.Option = def;
			c.Option.Invoke(c);
			return false;
		}

		protected bool GetOptionParts(string argument, out string flag, out string name, out string sep, out string value)
		{
			if (argument == null)
				throw new ArgumentNullException("argument");

			flag = name = sep = value = null;
			Match m = _valueOption.Match(argument);
			if (!m.Success)
				return false;
			flag = m.Groups["flag"].Value;
			name = m.Groups["name"].Value;
			if (m.Groups["sep"].Success && m.Groups["value"].Success)
			{
				sep = m.Groups["sep"].Value;
				value = m.Groups["value"].Value;
			}
			return true;
		}

		protected virtual bool Parse(string argument, OptionContext c)
		{
			if (c.Option != null)
			{
				ParseValue(argument, c);
				return true;
			}

			string f, n, s, v;
			if (!GetOptionParts(argument, out f, out n, out s, out v))
				return false;

			Option p;
			if (Contains(n))
			{
				p = this[n];
				c.OptionName = f + n;
				c.Option = p;
				switch (p.OptionValueType)
				{
					case OptionValueType.None:
						c.OptionValues.Add(n);
						c.Option.Invoke(c);
						break;
					case OptionValueType.Optional:
					case OptionValueType.Required:
						ParseValue(v, c);
						break;
				}
				return true;
			}
			// no match; is it a bool option?
			if (ParseBool(argument, n, c))
				return true;
			// is it a bundled option?
			if (ParseBundledValue(f, string.Concat(n + s + v), c))
				return true;

			return false;
		}

		void ParseValue(string option, OptionContext c)
		{
			if (option != null)
			{
				foreach (string o in c.Option.ValueSeparators != null
					? option.Split(c.Option.ValueSeparators, c.Option.MaxValueCount - c.OptionValues.Count, StringSplitOptions.None)
					: new[] { option })
					c.OptionValues.Add(o);
			}
			if (c.OptionValues.Count == c.Option.MaxValueCount ||
			    c.Option.OptionValueType == OptionValueType.Optional)
				c.Option.Invoke(c);
			else if (c.OptionValues.Count > c.Option.MaxValueCount)
			{
				throw new OptionException(MessageLocalizer(string.Format(
					"Error: Found {0} option values when expecting {1}.",
					c.OptionValues.Count, c.Option.MaxValueCount)),
					c.OptionName);
			}
		}

		bool ParseBool(string option, string n, OptionContext c)
		{
			Option p;
			string rn;
			if (n.Length >= 1 && (n[n.Length - 1] == '+' || n[n.Length - 1] == '-') &&
			    Contains((rn = n.Substring(0, n.Length - 1))))
			{
				p = this[rn];
				string v = n[n.Length - 1] == '+' ? option : null;
				c.OptionName = option;
				c.Option = p;
				c.OptionValues.Add(v);
				p.Invoke(c);
				return true;
			}
			return false;
		}

		bool ParseBundledValue(string f, string n, OptionContext c)
		{
			if (f != "-")
				return false;
			for (int i = 0; i < n.Length; ++i)
			{
				Option p;
				string opt = f + n[i];
				string rn = n[i].ToString();
				if (!Contains(rn))
				{
					if (i == 0)
						return false;
					throw new OptionException(string.Format(MessageLocalizer(
						"Cannot bundle unregistered option '{0}'."), opt), opt);
				}
				p = this[rn];
				switch (p.OptionValueType)
				{
					case OptionValueType.None:
						Invoke(c, opt, n, p);
						break;
					case OptionValueType.Optional:
					case OptionValueType.Required:
					{
						string v = n.Substring(i + 1);
						c.Option = p;
						c.OptionName = opt;
						ParseValue(v.Length != 0 ? v : null, c);
						return true;
					}
					default:
						throw new InvalidOperationException("Unknown OptionValueType: " + p.OptionValueType);
				}
			}
			return true;
		}

		static void Invoke(OptionContext c, string name, string value, Option option)
		{
			c.OptionName = name;
			c.Option = option;
			c.OptionValues.Add(value);
			option.Invoke(c);
		}

		public void WriteOptionDescriptions(TextWriter o)
		{
			foreach (Option p in this)
			{
				int written = 0;

				if (p.Hidden)
					continue;

				Category c = p as Category;
				if (c != null)
				{
					WriteDescription(o, p.Description, "", 80, 80);
					continue;
				}

				if (!WriteOptionPrototype(o, p, ref written))
					continue;

				if (written < OptionWidth)
					o.Write(new string(' ', OptionWidth - written));
				else
				{
					o.WriteLine();
					o.Write(new string(' ', OptionWidth));
				}

				WriteDescription(o, p.Description, new string(' ', OptionWidth + 2),
					DescriptionFirstWidth, DescriptionRemWidth);
			}

			foreach (ArgumentSource s in _sources)
			{
				string[] names = s.GetNames();
				if (names == null || names.Length == 0)
					continue;

				int written = 0;

				Write(o, ref written, "  ");
				Write(o, ref written, names[0]);
				for (int i = 1; i < names.Length; ++i)
				{
					Write(o, ref written, ", ");
					Write(o, ref written, names[i]);
				}

				if (written < OptionWidth)
					o.Write(new string(' ', OptionWidth - written));
				else
				{
					o.WriteLine();
					o.Write(new string(' ', OptionWidth));
				}

				WriteDescription(o, s.Description, new string(' ', OptionWidth + 2),
					DescriptionFirstWidth, DescriptionRemWidth);
			}
		}

		void WriteDescription(TextWriter o, string value, string prefix, int firstWidth, int remWidth)
		{
			bool indent = false;
			foreach (string line in GetLines(MessageLocalizer(GetDescription(value)), firstWidth, remWidth))
			{
				if (indent)
					o.Write(prefix);
				o.WriteLine(line);
				indent = true;
			}
		}

		bool WriteOptionPrototype(TextWriter o, Option p, ref int written)
		{
			string[] names = p.Names;

			int i = GetNextOptionIndex(names, 0);
			if (i == names.Length)
				return false;

			if (names[i].Length == 1)
			{
				Write(o, ref written, "  -");
				Write(o, ref written, names[0]);
			}
			else
			{
				Write(o, ref written, "      --");
				Write(o, ref written, names[0]);
			}

			for (i = GetNextOptionIndex(names, i + 1);
				i < names.Length;
				i = GetNextOptionIndex(names, i + 1))
			{
				Write(o, ref written, ", ");
				Write(o, ref written, names[i].Length == 1 ? "-" : "--");
				Write(o, ref written, names[i]);
			}

			if (p.OptionValueType == OptionValueType.Optional ||
			    p.OptionValueType == OptionValueType.Required)
			{
				if (p.OptionValueType == OptionValueType.Optional)
					Write(o, ref written, MessageLocalizer("["));
				Write(o, ref written, MessageLocalizer("=" + GetArgumentName(0, p.MaxValueCount, p.Description)));
				string sep = p.ValueSeparators != null && p.ValueSeparators.Length > 0
					? p.ValueSeparators[0]
					: " ";
				for (int c = 1; c < p.MaxValueCount; ++c)
					Write(o, ref written, MessageLocalizer(sep + GetArgumentName(c, p.MaxValueCount, p.Description)));
				if (p.OptionValueType == OptionValueType.Optional)
					Write(o, ref written, MessageLocalizer("]"));
			}
			return true;
		}

		static int GetNextOptionIndex(string[] names, int i)
		{
			while (i < names.Length && names[i] == "<>")
				++i;
			return i;
		}

		static void Write(TextWriter o, ref int n, string s)
		{
			n += s.Length;
			o.Write(s);
		}

		static string GetArgumentName(int index, int maxIndex, string description)
		{
			if (description == null)
				return maxIndex == 1 ? "VALUE" : "VALUE" + (index + 1);
			string[] nameStart;
			if (maxIndex == 1)
				nameStart = new[] { "{0:", "{" };
			else
				nameStart = new[] { "{" + index + ":" };
			for (int i = 0; i < nameStart.Length; ++i)
			{
				int start, j = 0;
				do
				{
					start = description.IndexOf(nameStart[i], j);
				} while (start >= 0 && j != 0 ? description[j++ - 1] == '{' : false);
				if (start == -1)
					continue;
				int end = description.IndexOf("}", start);
				if (end == -1)
					continue;
				return description.Substring(start + nameStart[i].Length, end - start - nameStart[i].Length);
			}
			return maxIndex == 1 ? "VALUE" : "VALUE" + (index + 1);
		}

		static string GetDescription(string description)
		{
			if (description == null)
				return string.Empty;
			StringBuilder sb = new StringBuilder(description.Length);
			int start = -1;
			for (int i = 0; i < description.Length; ++i)
			{
				switch (description[i])
				{
					case '{':
						if (i == start)
						{
							sb.Append('{');
							start = -1;
						}
						else if (start < 0)
							start = i + 1;
						break;
					case '}':
						if (start < 0)
						{
							if ((i + 1) == description.Length || description[i + 1] != '}')
								throw new InvalidOperationException("Invalid option description: " + description);
							++i;
							sb.Append("}");
						}
						else
						{
							sb.Append(description.Substring(start, i - start));
							start = -1;
						}
						break;
					case ':':
						if (start < 0)
							goto default;
						start = i + 1;
						break;
					default:
						if (start < 0)
							sb.Append(description[i]);
						break;
				}
			}
			return sb.ToString();
		}

		static IEnumerable<string> GetLines(string description, int firstWidth, int remWidth)
		{
			return StringCoda.WrappedLines(description, firstWidth, remWidth);
		}

		internal sealed class Category : Option
		{
			// Prototype starts with '=' because this is an invalid prototype
			// (see Option.ParsePrototype(), and thus it'll prevent Category
			// instances from being accidentally used as normal options.
			public Category(string description)
				: base("=:Category:= " + description, description)
			{
			}

			protected override void OnParseComplete(OptionContext c)
			{
				throw new NotSupportedException("Category.OnParseComplete should not be invoked.");
			}
		}

		sealed class ActionOption : Option
		{
			readonly Action<OptionValueCollection> _action;

			public ActionOption(string prototype, string description, int count, Action<OptionValueCollection> action)
				: this(prototype, description, count, action, false)
			{
			}

			public ActionOption(string prototype, string description, int count, Action<OptionValueCollection> action,
				bool hidden)
				: base(prototype, description, count, hidden)
			{
				if (action == null)
					throw new ArgumentNullException("action");
				_action = action;
			}

			protected override void OnParseComplete(OptionContext c)
			{
				_action(c.OptionValues);
			}
		}

		sealed class ActionOption<T> : Option
		{
			readonly Action<T> _action;

			public ActionOption(string prototype, string description, Action<T> action)
				: base(prototype, description, 1)
			{
				if (action == null)
					throw new ArgumentNullException("action");
				_action = action;
			}

			protected override void OnParseComplete(OptionContext c)
			{
				_action(Parse<T>(c.OptionValues[0], c));
			}
		}

		sealed class ActionOption<TKey, TValue> : Option
		{
			readonly OptionAction<TKey, TValue> _action;

			public ActionOption(string prototype, string description, OptionAction<TKey, TValue> action)
				: base(prototype, description, 2)
			{
				if (action == null)
					throw new ArgumentNullException("action");
				_action = action;
			}

			protected override void OnParseComplete(OptionContext c)
			{
				_action(
					Parse<TKey>(c.OptionValues[0], c),
					Parse<TValue>(c.OptionValues[1], c));
			}
		}

		class ArgumentEnumerator : IEnumerable<string>
		{
			readonly List<IEnumerator<string>> _sources = new List<IEnumerator<string>>();

			public ArgumentEnumerator(IEnumerable<string> arguments)
			{
				_sources.Add(arguments.GetEnumerator());
			}

			public IEnumerator<string> GetEnumerator()
			{
				do
				{
					IEnumerator<string> c = _sources[_sources.Count - 1];
					if (c.MoveNext())
						yield return c.Current;
					else
					{
						c.Dispose();
						_sources.RemoveAt(_sources.Count - 1);
					}
				} while (_sources.Count > 0);
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			public void Add(IEnumerable<string> arguments)
			{
				_sources.Add(arguments.GetEnumerator());
			}
		}
	}
}