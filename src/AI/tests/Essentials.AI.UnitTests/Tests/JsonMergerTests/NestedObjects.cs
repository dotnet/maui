using Maui.Controls.Sample.Services;
using Xunit;

namespace Microsoft.Maui.Essentials.AI.UnitTests;

public partial class JsonMergerTests
{
	public class NestedObjects
	{
		[Fact]
		public void MergeOverlay_NestedObject_MergesRecursively()
		{
			var baseJson = @"{
				""title"": ""Profile"",
				""person"": {
					""firstName"": ""John"",
					""lastName"": ""Doe"",
					""age"": 30
				}
			}";
			var overlay = @"{
				""person"": {
					""firstName"": ""Jane""
				}
			}";
			var merger = new JsonMerger(baseJson);

			var result = merger.MergeOverlay(overlay);
			var parsed = merger.Deserialize<NestedModel>(SerializerOptions);

			Assert.NotNull(parsed);
			Assert.Equal("Profile", parsed.Title);         // Base preserved
			Assert.NotNull(parsed.Person);
			Assert.Equal("Jane", parsed.Person.FirstName); // Overlay replaced
			Assert.Equal("Doe", parsed.Person.LastName);   // Base preserved
			Assert.Equal(30, parsed.Person.Age);           // Base preserved
		}

		[Fact]
		public void MergeOverlay_NestedObject_OverlayAddsNewNestedProperty()
		{
			var baseJson = @"{""title"": ""Profile""}";
			var overlay = @"{
				""person"": {
					""firstName"": ""Jane"",
					""lastName"": ""Smith"",
					""age"": 25
				}
			}";
			var merger = new JsonMerger(baseJson);

			var result = merger.MergeOverlay(overlay);
			var parsed = merger.Deserialize<NestedModel>(SerializerOptions);

			Assert.NotNull(parsed);
			Assert.Equal("Profile", parsed.Title);
			Assert.NotNull(parsed.Person);
			Assert.Equal("Jane", parsed.Person.FirstName);
			Assert.Equal("Smith", parsed.Person.LastName);
			Assert.Equal(25, parsed.Person.Age);
		}

		[Fact]
		public void MergeOverlay_DeeplyNested_MergesAllLevels()
		{
			var baseJson = @"{
				""level1"": {
					""level2"": {
						""level3"": {
							""value"": ""original""
						}
					}
				}
			}";
			var overlay = @"{
				""level1"": {
					""level2"": {
						""level3"": {
							""value"": ""modified""
						}
					}
				}
			}";
			var merger = new JsonMerger(baseJson);

			var result = merger.MergeOverlay(overlay);

			Assert.Contains(@"""value"":""modified""", result.Replace(" ", "", StringComparison.Ordinal), StringComparison.Ordinal);
		}
	}
}
