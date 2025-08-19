using System;
using System.Diagnostics.CodeAnalysis;

namespace Maui.Controls.Sample.Models
{
	public sealed class SectionModel
	{
		public SectionModel(
			[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] Type type, 
			string title, 
			string description, 
			object? viewModel = null)
		{
			Type = type;
			Title = title;
			Description = description;
			ViewModel = viewModel;
		}

		// Add DynamicallyAccessedMembers attribute to Type property to fix IL2072 error
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
		public Type Type { get; }

		public string Title { get; }

		public string Description { get; }

		public object? ViewModel { get; }
	}
}