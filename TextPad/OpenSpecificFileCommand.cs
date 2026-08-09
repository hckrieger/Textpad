using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace TextPad
{
	internal class OpenSpecificFileCommand : ICommand
	{
		private MainWindow mainWindow;
		public OpenSpecificFileCommand(MainWindow mainWindow)
		{
			this.mainWindow = mainWindow;
		}

		public event EventHandler? CanExecuteChanged;

		public bool CanExecute(object? parameter)
		{
			return true;
		}

		public void Execute(object? parameter)
		{
			if (parameter is string fileName)
			{
				mainWindow.CurrentFile = fileName;
				if (!File.Exists(fileName))
				{
					MessageBox.Show($"File '{fileName}' does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
					return;
				}
					
				mainWindow.mainTextBox.Text = File.ReadAllText(fileName);
				mainWindow.AdjustRecentMenuItems(fileName);
			}
		}


	}
}
