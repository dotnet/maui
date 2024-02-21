using System;

namespace Maui.Controls.Sample.Models
{
	public sealed class SectionModel
	{
		public SectionModel(Type type, string title, string description, object? viewModel = null)
		{
			Type = type;
			Title = title;
			Description = description;
			ViewModel = viewModel;
		}

		public Type Type { get; }

		public string Title { get; }

		public string Description { get; }

		public object? ViewModel { get; }
	}
}