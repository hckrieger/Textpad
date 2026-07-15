using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Xps.Packaging;

namespace TextPad
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		string? currentFile;
		bool justReplaced = false;
		int minFontSize = 4, maxFontSize = 48;
		private string? CurrentFile
		{
			get => currentFile;
			set
			{
				
				currentFile = value;


				if (Title.Contains("|"))
				{

					int index = Title.IndexOf("  |");

					Title = Title.Remove(index);
				
					
				}

				if (currentFile != null)
					Title += "  |  " + Path.GetFileName(currentFile);
			}
		}

		int findIndex, findInstances;
	
		public MainWindow()
		{
			
			InitializeComponent();
			findIndex = 0;
			findInstances = 0;
	
		}

		

		private void OpenCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			string defaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			OpenFileDialog openFileDialog = new OpenFileDialog()
			{
				Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
				InitialDirectory = defaultDirectory
			};
			var fileName = string.Empty;
			if (openFileDialog.ShowDialog() == true)
			{

				fileName = openFileDialog.FileName;
				CurrentFile = fileName;
				var text = File.ReadAllText(fileName);

				mainTextBox.Text = text;
			}

		}

		private void OpenCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void SaveCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			
			
			File.WriteAllText(currentFile, mainTextBox.Text);
			
		}


		private void SaveCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{

			bool condition1 = string.IsNullOrEmpty(currentFile) && currentFile == null;
			bool condition2 = currentFile != null && mainTextBox.Text == File.ReadAllText(currentFile);
			e.CanExecute = (condition1 || condition2) ? false : true;
		}

		private void wordWrapMenuItem_Click(object sender, RoutedEventArgs e)
		{
			MenuItem? item = sender as MenuItem;

			mainTextBox.TextWrapping = (!wordWrapMenuItem.IsChecked) ? TextWrapping.Wrap : TextWrapping.NoWrap;


			item?.IsChecked = !item.IsChecked;



		}

		private void Window_Closing(object sender, EventArgs e)
		{
			if ((currentFile == null && mainTextBox.Text.Length > 0) || currentFile != null && mainTextBox.Text != File.ReadAllText(currentFile))
			{
				MessageBoxResult result = MessageBox.Show("Do you want to save changes?", "TextPad", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
				if (result == MessageBoxResult.Yes)
				{
					File.WriteAllText(currentFile, mainTextBox.Text);
				}
				else if (result == MessageBoxResult.Cancel)
				{
					return;
				}
			}
		}

		private void ExitCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Window_Closing(sender, e);
			Close();
		}

		private void SaveAsCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			var saveCondition = !string.IsNullOrEmpty(mainTextBox.Text);
			e.CanExecute = (saveCondition) ? true : false;
		}

		private void SaveAsCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			
			SaveFileDialog saveFileDialog = new SaveFileDialog()
			{
				Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
				DefaultExt = ".txt",
				DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
			};

			if (!string.IsNullOrEmpty(currentFile))
			{
				saveFileDialog.FileName =  Path.GetFileNameWithoutExtension(currentFile);
			}

			if (saveFileDialog.ShowDialog() == true)
			{
				CurrentFile = saveFileDialog.FileName;
				File.WriteAllText(currentFile, mainTextBox.Text);
			}
			
		}

		private void NewCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(currentFile) || !string.IsNullOrEmpty(mainTextBox.Text))
			{
				e.CanExecute = true;
			} else
			{
				e.CanExecute = false;
			}
		}

		private void NewCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			mainTextBox.Clear();
			CurrentFile = null;
		}

		private void IfHasText_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			bool executeCondition = mainTextBox.Text.Length > 0 || !string.IsNullOrEmpty(mainTextBox.SelectedText);
			e.CanExecute = (executeCondition) ? true : false;
		}

		private void DeleteCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(mainTextBox.SelectedText))
			{
				mainTextBox.SelectedText = string.Empty;
			}
		}



		private void FindCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			findPanel.Visibility = Visibility.Visible;
			
			findSearchBox.Focus();
			
		}

		private void closeFindPanelButton_Click(object sender, RoutedEventArgs e)
		{
			findPanel.Visibility = Visibility.Collapsed;
			replacePanel.Visibility = Visibility.Collapsed;
			dropDownForReplaceButton.Content = "v";

		}



		private void replaceButton_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(mainTextBox.SelectedText))
			{
				FindNext();
				return;
			}
			{
				mainTextBox.SelectedText = replaceBox.Text;
				FindNext();
			}

			

		}

		private void replaceAllButton_Click(object sender, RoutedEventArgs e)
		{

		}

		private void findSearchBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			findIndex = 0;
		}

		private void dropDownForReplace_Click(object sender, RoutedEventArgs e)
		{
			if (replacePanel.Visibility != Visibility.Visible)
			{
				replacePanel.Visibility = Visibility.Visible;
				dropDownForReplaceButton.Content = "^";
			} else
			{
				replacePanel.Visibility = Visibility.Collapsed;
				dropDownForReplaceButton.Content = "v";
			}

		}



		private void ReplaceCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			findPanel.Visibility = Visibility.Visible;
			replacePanel.Visibility = Visibility.Visible;
			dropDownForReplaceButton.Content = "^";

			if (mainTextBox.SelectedText.Length > 0)
			{
				findSearchBox.Text = mainTextBox.SelectedText;
			} 
		}

		private void FindNextText()
		{

		}

		private void FindNext()
		{
			string searchText = findSearchBox.Text;
			if (searchText.Length == 0)
				return;

			//remember to reset the findIndex if the search text and check box has changed

			StringComparison comparison = matchedCaseCheckBox.IsChecked == true
				? StringComparison.OrdinalIgnoreCase
				: StringComparison.Ordinal;

			void findFromIndex(int index)
			{
				findIndex = mainTextBox.Text.IndexOf(searchText, index, comparison);
			}

		   if (!string.IsNullOrEmpty(mainTextBox.SelectedText))
		   {
				while (findIndex <= mainTextBox.CaretIndex && findIndex >= 0 && !justReplaced)
					findFromIndex(findIndex + searchText.Length);
		   }



			if (findIndex == -1)
				findFromIndex(0);

			if (findIndex == -1)
			{
				Debug.WriteLine("Cannot find it");
				findSearchBox.Background = Brushes.PaleVioletRed;
				return;
			}

			mainTextBox.Focus();
			mainTextBox.SelectionStart = findIndex;
			mainTextBox.SelectionLength = searchText.Length;
			findSearchBox.Background = Brushes.White;
			justReplaced = false;
		}

	


		private void DecreaseZoomCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			mainTextBox.FontSize = Math.Max(minFontSize, mainTextBox.FontSize - 4);
		}

		private void DecreaseZoomCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (mainTextBox.FontSize > minFontSize) ? true : false;
		}

		private void IncreaseZoomCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = (mainTextBox.FontSize < maxFontSize) ? true : false;
		}

		//Make Command later
		private void defaultZoom_Click(object sender, RoutedEventArgs e)
		{
			var fontSize = mainTextBox.FontSize;
			var defaultFontSize = 16;
			if (fontSize != defaultFontSize)
				mainTextBox.FontSize = defaultFontSize;

		}

		private void mainTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			int caretIndex = mainTextBox.CaretIndex;

			int line = mainTextBox.GetLineIndexFromCharacterIndex(caretIndex);
			int lineStartCharIndex = mainTextBox.GetCharacterIndexFromLineIndex(line);

			int column = caretIndex - lineStartCharIndex;

			linesAndColumnsLabel.Content = $"Ln {line+1}, Col {column+1}";
			characterCountLabel.Content = $"{mainTextBox.Text.Length} Characters";
		}

		private void PrintCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void PrintCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			PrintDialog printDialog = new PrintDialog
			{
				PageRangeSelection = PageRangeSelection.AllPages,
				UserPageRangeEnabled = true,
			};

			Nullable<Boolean> print = printDialog.ShowDialog();

			if (print.Value)
			{
				XpsDocument xpsDocument = new XpsDocument($"{AppDomain.CurrentDomain.BaseDirectory}\\Document.xps", FileAccess.ReadWrite);
				FixedDocumentSequence fixedDocSeq = xpsDocument.GetFixedDocumentSequence();
				printDialog.PrintDocument(fixedDocSeq.DocumentPaginator, "Print job");
			}

		}

		private void IncreaseZoomCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			mainTextBox.FontSize = Math.Min(mainTextBox.FontSize + 4, maxFontSize);
		}

		private void findButton_Click(object sender, RoutedEventArgs e)
		{

			FindNext();

		}


	}
}