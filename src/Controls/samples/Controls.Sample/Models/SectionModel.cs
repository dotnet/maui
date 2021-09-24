using System;

namespace Maui.Controls.Sample.Models
{
	public sealed class SectionModel
	{
		public SectionModel(Type type, string title, string description)
		{
			Type = type;
			Title = title;
			Description = description;
		}

		public Type Type { get; }

		public string Title { get; }

		public string Description { get; }
	}
}