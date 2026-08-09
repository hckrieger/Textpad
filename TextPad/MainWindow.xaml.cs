using Microsoft.Win32;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Printing;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shell;
using System.Windows.Xps;
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
		int minFontSize = 4, maxFontSize = 64;
		int startingIndexOfFindText = 0;
		int findInstanceIndex = 0;
		int numberOfInstancesReplaced = 0;
		
		JumpList jumpList = new JumpList();
		List<JumpTask> jumpTasks = new List<JumpTask>();

		PersistantData? persistantData;

		private MenuItem? recentMenuItem;
		public string? CurrentFile
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




			InputBindings.Add(new KeyBinding(NavigationCommands.Zoom, new KeyGesture(Key.D0, ModifierKeys.Control)));

			InputBindings.Add(new KeyBinding(NavigationCommands.IncreaseZoom, new KeyGesture(Key.OemPlus, ModifierKeys.Control)));
			InputBindings.Add(new KeyBinding(NavigationCommands.DecreaseZoom, new KeyGesture(Key.OemMinus, ModifierKeys.Control)));


			if (!File.Exists("persistantData.json"))
			{
				persistantData = new PersistantData();
				SerializeJson();
			}
			else
			{
				var json = File.ReadAllText("persistantData.json");


				persistantData = JsonSerializer.Deserialize<PersistantData>(json);
			}



			

			if (persistantData != null)
			{
				

				mainTextBox.TextWrapping = persistantData.WordWrap;

				if (persistantData.WordWrap == TextWrapping.Wrap)
				{
					wordWrapMenuItem.IsChecked = true;
				}
				else
				{
					wordWrapMenuItem.IsChecked = false;
				}
				//DisplayTasks();
				mainTextBox.FontSize = persistantData.FontSize;
				fontSizePercentageLabel.Content = $"Font Size: {persistantData.FontSize}";

				this.Width = (persistantData.WindowSize.Width == 0) ? Width : persistantData.WindowSize.Width;
				this.Height = (persistantData.WindowSize.Height == 0) ? Height : persistantData.WindowSize.Height;

				this.Left = (persistantData.WindowPosition.X == 0) ? Left : persistantData.WindowPosition.X;
				this.Top = (persistantData.WindowPosition.Y == 0) ? Top : persistantData.WindowPosition.Y;

				if (persistantData.RecentFiles != null && persistantData.RecentFiles.Count > 0 && recentMenuItem == null)
				{

					recentMenuItem = new MenuItem() { Header = "Recent" };

		
					
					mainMenu.Items.Add(recentMenuItem);
					
					if (recentMenuItem != null)
					{
						foreach (var recentFile in persistantData.RecentFiles)
						{
							MenuItem newFilemenuItem = new MenuItem()
							{
								Header = Path.GetFileName(recentFile),
								Command = new OpenSpecificFileCommand(this),
								CommandParameter = recentFile
							};

							AddRightClickDeleteHandler(newFilemenuItem, recentFile);

							recentMenuItem.Items.Add(newFilemenuItem);
						}

					}

				}



			}

		}


		private void SerializeJson()
		{

			
			var json = JsonSerializer.Serialize(persistantData, new JsonSerializerOptions { WriteIndented = true });
			File.WriteAllText("persistantData.json", json);

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

				AdjustRecentMenuItems(fileName);



				//AddTask();
			}

		}

		public void AddRightClickDeleteHandler(MenuItem menuItem, string fileName)
		{
			menuItem.PreviewMouseRightButtonDown += (s, e) =>
			{
				if (MessageBox.Show(
					$"Do you want to clear '{fileName}' from recent history? \nThis will not delete the file, only remove from history",
					"Remove from Recent History",
					MessageBoxButton.YesNo,
					MessageBoxImage.Question) == MessageBoxResult.Yes)
				{
					persistantData?.RecentFiles.Remove(fileName);
					recentMenuItem?.Items.Remove(menuItem);
					SerializeJson();
				}
				if (recentMenuItem?.Items.Count <= 0)
				{
					mainMenu.Items.Remove(recentMenuItem);
					recentMenuItem = null;
				}
			};
		}

		public void AdjustRecentMenuItems(string fileName)
		{
			if (persistantData != null)
			{
				if (persistantData.RecentFiles.Count == 0)
				{
					recentMenuItem = new MenuItem() { Header = "Recent" };
					mainMenu.Items.Add(recentMenuItem);
				}



				if (!persistantData.RecentFiles.Contains(fileName))
				{
					persistantData.RecentFiles.Insert(0, fileName);

					MenuItem? newFileMenuItem = new MenuItem()
					{
						Header = Path.GetFileName(fileName),
						Command = new OpenSpecificFileCommand(this),
						CommandParameter = fileName
					};
					recentMenuItem?.Items.Insert(0, newFileMenuItem);

					AddRightClickDeleteHandler(newFileMenuItem, fileName);
				}
				else
				{
					persistantData.RecentFiles.Remove(fileName);
					persistantData.RecentFiles.Insert(0, fileName);

					MenuItem? existingMenuItem = recentMenuItem?.Items.OfType<MenuItem>().FirstOrDefault(item => item.Header.ToString() == Path.GetFileName(fileName));
					if (existingMenuItem != null)
					{
						recentMenuItem?.Items.RemoveAt(recentMenuItem.Items.IndexOf(existingMenuItem));
						recentMenuItem?.Items.Insert(0, existingMenuItem);

						AddRightClickDeleteHandler(existingMenuItem, fileName);
					}
				}

				if (persistantData.RecentFiles.Count > 7)
				{
		
					persistantData.RecentFiles.RemoveAt(persistantData.RecentFiles.Count - 1);
					recentMenuItem?.Items.RemoveAt(recentMenuItem.Items.Count - 1);

		
				}




				SerializeJson();
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

			if (!File.Exists(currentFile))
			{
				e.CanExecute = false;
				return;
			}

			bool condition2 = currentFile != null && mainTextBox.Text == File.ReadAllText(currentFile);
			e.CanExecute = (condition1 || condition2) ? false : true;
		}

		private void wordWrapMenuItem_Click(object sender, RoutedEventArgs e)
		{
			MenuItem? item = sender as MenuItem;

			

			mainTextBox.TextWrapping = (!wordWrapMenuItem.IsChecked) ? TextWrapping.Wrap : TextWrapping.NoWrap;

			persistantData?.WordWrap = mainTextBox.TextWrapping; 

			SerializeJson();

			item?.IsChecked = !item.IsChecked;



		}



		private void Window_Closing(object sender, CancelEventArgs e)
		{
			if ((currentFile == null && mainTextBox.Text.Length > 0) || currentFile != null && mainTextBox.Text != File.ReadAllText(currentFile))
			{
				MessageBoxResult result = MessageBox.Show("Do you want to save changes?", "TextPad", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
				if (result == MessageBoxResult.Yes)
				{
					if (currentFile != null)
					{
						File.WriteAllText(currentFile, mainTextBox.Text);
					} else
					{
						SaveFileProcess();
						if (currentFile == null)
							e.Cancel = true;
					}
					
				}
				else if (result == MessageBoxResult.Cancel)
				{
					e.Cancel = true;
				}
				//Close();
			}
		}

		private void ExitCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			Close();
		}

		private void SaveAsCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			var saveCondition = !string.IsNullOrEmpty(mainTextBox.Text);
			e.CanExecute = (saveCondition) ? true : false;
		}

		private void SaveFileProcess()
		{
			SaveFileDialog saveFileDialog = new SaveFileDialog()
			{
				Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*",
				DefaultExt = ".txt",
				DefaultDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
			};

			if (!string.IsNullOrEmpty(currentFile))
			{
				saveFileDialog.FileName = Path.GetFileNameWithoutExtension(currentFile);
			}

			if (saveFileDialog.ShowDialog() == true)
			{
				CurrentFile = saveFileDialog.FileName;
				File.WriteAllText(currentFile, mainTextBox.Text);

				AdjustRecentMenuItems(CurrentFile);
			}
		}

		private void SaveAsCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{

			SaveFileProcess();
			
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

		private void FindFocus()
		{
			findSearchBox.Focus();
			if (!string.IsNullOrEmpty(mainTextBox.SelectedText))
			{
				findSearchBox.Text = mainTextBox.SelectedText;
				findSearchBox.SelectionStart = 0;
				findSearchBox.SelectionLength = findSearchBox.Text.Length;
			}
		}
		
		private void FindCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			replacePanel.Visibility = Visibility.Collapsed;
			findPanel.Visibility = Visibility.Visible;
			FindFocus();
			
		}

		private void closeFindPanelButton_Click(object sender, RoutedEventArgs e)
		{
			findPanel.Visibility = Visibility.Collapsed;
			replacePanel.Visibility = Visibility.Collapsed;
			dropDownForReplaceButton.Content = "v";
		}



		private void replaceButton_Click(object sender, RoutedEventArgs e)
		{

			if (string.IsNullOrEmpty(replaceBox.Text))
				return;


			if (string.IsNullOrEmpty(mainTextBox.SelectedText))
			{
				FindNext();
			} else
			{
				ReplaceAction();
			}
		}

		private void ReplaceAction()
		{
			if (!string.IsNullOrEmpty(mainTextBox.SelectedText))
			{
					mainTextBox.SelectedText = replaceBox.Text;
				numberOfInstancesReplaced++;
			} 
				
			FindNext();
		}

		private void replaceAllButton_Click(object sender, RoutedEventArgs e)
		{
			numberOfInstancesReplaced = 0;


			while (mainTextBox.Text.Contains(findSearchBox.Text, matchedCaseCheckBox.IsChecked == true
					? StringComparison.Ordinal
					: StringComparison.OrdinalIgnoreCase))
			{
				ReplaceAction();
				
			}

			if (string.IsNullOrEmpty(replaceBox.Text))
				return;


			if (numberOfInstancesReplaced > 0)
			{
				
				MessageBox.Show($"{numberOfInstancesReplaced} instances of '{findSearchBox.Text}' replaced with '{replaceBox.Text}'");
				numberOfInstancesReplaced = 0;
				mainTextBox.SelectionLength = 0;
			}
				
		}

		

		private void dropDownForReplace_Click(object sender, RoutedEventArgs e)
		{
			if (replacePanel.Visibility == Visibility.Collapsed)
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
			FindFocus();
		}



	


		private void DecreaseZoomCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			persistantData.FontSize = Math.Max(minFontSize, --persistantData.FontSize);
			mainTextBox.FontSize = persistantData.FontSize;
			fontSizePercentageLabel.Content = $"Font Size: {persistantData.FontSize}";
			SerializeJson();
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


		}


		private void ChangeLineAndRowCount()
		{
			string textBeforeCaret = mainTextBox.Text[..mainTextBox.CaretIndex];

			int line = textBeforeCaret.Count(c => c == '\n');
			int lastNewLineIndex = textBeforeCaret.LastIndexOf('\n');
			int column = mainTextBox.CaretIndex - lastNewLineIndex - 1;

			linesAndColumnsLabel.Content = $"Ln {line + 1}, Col {column + 1}";
			characterCountLabel.Content = $"{mainTextBox.Text.Length} Characters";
		}

		private void mainTextBox_TextChanged(object sender, TextChangedEventArgs e)
		{
			ChangeLineAndRowCount();
		}

		private void PrintCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			e.CanExecute = true;
		}

		private void PrintCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{ 
			PrintDialog printDialog = new PrintDialog();
			if (printDialog.ShowDialog() == true)
			{
				FlowDocument document = new FlowDocument(new Paragraph(new Run(mainTextBox.Text)));
				document.PagePadding = new Thickness(50);
				document.FontSize = mainTextBox.FontSize;

				printDialog.PrintDocument(
					((IDocumentPaginatorSource)document).DocumentPaginator, "Print TextBox Content"
				);

			}

		
		}

		private void mainTextBox_SelectionChanged(object sender, RoutedEventArgs e)
		{
			findInstanceIndex = 0;
			startingIndexOfFindText = 0;

			ChangeLineAndRowCount();
		}

		private void IncreaseZoomCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			persistantData.FontSize = Math.Min(++persistantData.FontSize, maxFontSize);
			mainTextBox.FontSize = persistantData.FontSize;
			fontSizePercentageLabel.Content = $"Font Size: {persistantData.FontSize}";
			SerializeJson();
		}

		private void FindNext()
		{

			if (string.IsNullOrEmpty(findSearchBox.Text))
				return;

			var stringComparison = matchedCaseCheckBox.IsChecked == true ?
				StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

			var startSelect = mainTextBox.SelectionStart + mainTextBox.SelectionLength;

			startingIndexOfFindText = mainTextBox.Text.IndexOf(findSearchBox.Text, startSelect, stringComparison);

			// Wrap around to the beginning.
			if (startingIndexOfFindText == -1 && startSelect > 0)
			{
				startingIndexOfFindText = mainTextBox.Text.IndexOf(
					findSearchBox.Text,
					0,
					stringComparison);

				
			}

			if (startingIndexOfFindText == -1)
				return;


			SelectFindText();

		}

		private void FindPrevious()
		{
			if (string.IsNullOrEmpty(findSearchBox.Text))
				return;

			StringComparison comparison =
				matchedCaseCheckBox.IsChecked == true
					? StringComparison.Ordinal
					: StringComparison.OrdinalIgnoreCase;

			// Search immediately before the current selection.
			int searchStart = mainTextBox.SelectionStart - 1;

			if (searchStart >= 0)
			{
				startingIndexOfFindText = mainTextBox.Text.LastIndexOf(
					findSearchBox.Text,
					searchStart,
					comparison);
			}
			else
			{
				startingIndexOfFindText = -1;
			}

			// Wrap around to the end.
			if (startingIndexOfFindText == -1 && mainTextBox.Text.Length > 0)
			{
				startingIndexOfFindText = mainTextBox.Text.LastIndexOf(
					findSearchBox.Text,
					mainTextBox.Text.Length - 1,
					comparison);
			}

			if (startingIndexOfFindText == -1)
				return;

			SelectFindText();
		}

		private void findNextRadio_Checked(object sender, RoutedEventArgs e)
		{
			startingIndexOfFindText = 0;
			findInstanceIndex = 0;
		}

		private void findPreviousRadio_Checked(object sender, RoutedEventArgs e)
		{
			startingIndexOfFindText = mainTextBox.CaretIndex; 
			findInstanceIndex = mainTextBox.CaretIndex;
		}

		int defaultFontSize = 16;
		private void DefaultZoomCmd_CanExecute(object sender, CanExecuteRoutedEventArgs e)
		{
			var fontSize = mainTextBox.FontSize;
			if (fontSize != defaultFontSize)
				e.CanExecute = true;
		}

		private void DefaultZoomCmd_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			persistantData.FontSize = defaultFontSize;
			mainTextBox.FontSize = persistantData.FontSize;
			fontSizePercentageLabel.Content = $"Font Size: {persistantData.FontSize}";
			SerializeJson();
		}

		private void SelectFindText()
		{
			int lengthOfFindText = findSearchBox.Text.Length;

			mainTextBox.Focus();
			mainTextBox.SelectionStart = startingIndexOfFindText;
			mainTextBox.SelectionLength = lengthOfFindText;


		}

		private void findButton_Click(object sender, RoutedEventArgs e)
		{

			
			bool findNextIsChecked = findNextRadio.IsChecked != null && (bool)findNextRadio.IsChecked;
			bool findPrevIsChecked = findPreviousRadio.IsChecked != null && (bool)findPreviousRadio.IsChecked;



			if (findNextIsChecked)
				FindNext();
			else 
				FindPrevious();
			

		}

		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			
			persistantData?.WindowSize = new Size(e.NewSize.Width, e.NewSize.Height);
			SerializeJson();
			

		}

		private void Window_LocationChanged(object sender, EventArgs e)
		{
			persistantData?.WindowPosition = new Point(this.Left, this.Top);
			SerializeJson();
		}

		//private void AddTask()
		//{
			
		//	JumpTask jumpTask = new JumpTask();

			
		//	jumpTask.Title = Path.GetFileName(currentFile);
		//	jumpTask.Description = currentFile;
		//	jumpTask.ApplicationPath = currentFile;





		//	if (jumpTasks.Count(m => m.Title == jumpTask.Title) == 0)
		//	{
		//		jumpTasks.Insert(0, jumpTask);
		//	} else
		//	{
		//		var index = jumpTasks.FindIndex(m => m.Title == jumpTask.Title);
		//		var selectedTask = jumpTasks[index];
		//		jumpTasks.RemoveAt(index);
		//		jumpTasks.Insert(0, jumpTask);
				
		//	}

		//	jumpList.JumpItems.Clear();

			

		//	DisplayTasks();

		//	persistantData?.RecentFiles = jumpTasks;

		//	SerializeJson();
		//}

		//private void DisplayTasks()
		//{
		//	jumpList = JumpList.GetJumpList(App.Current);

		//	jumpList.JumpItems.Clear();

		//	jumpTasks = persistantData?.RecentFiles ?? new List<JumpTask>();
		//	foreach (JumpTask jump in jumpTasks)
		//	{
		//		jumpList.JumpItems.Add(jump);
		//	}
		//	jumpList.JumpItems.Take(12);
		//	jumpList.Apply();	
		//}
		
	}
}