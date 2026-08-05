using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows;

namespace TextPad
{
	internal class PersistantData
	{
		[JsonPropertyName("word wrap")]
		public TextWrapping WordWrap { get; set; } = TextWrapping.Wrap;

		
		[JsonPropertyName("font size")]
		public double FontSize { get; set; } = 16;

		[JsonPropertyName("recent files")]
		public ObservableCollection<string> RecentFiles { get; set; } = new ObservableCollection<string>();

	}
}
