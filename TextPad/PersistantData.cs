using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Shell;

namespace TextPad
{
	internal class PersistantData
	{
		[JsonPropertyName("word wrap")]
		public TextWrapping WordWrap { get; set; } = TextWrapping.Wrap;

		
		[JsonPropertyName("font size")]
		public double FontSize { get; set; } = 16;

		[JsonPropertyName("recent files")]
		public List<JumpTask> RecentFiles { get; set; } = new List<JumpTask>();

		[JsonPropertyName("window size")]
		public Size WindowSize { get; set; }

		[JsonPropertyName("window position")]
		public Point WindowPosition { get; set; }

	}
}
