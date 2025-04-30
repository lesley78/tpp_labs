using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace WpfNotepad
{
    public partial class MainWindow : Window
    {
        private string _currentFilePath;
        private string _lastSavedText = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            TextEditor.TextChanged += TextEditor_TextChanged;
        }

        private void TextEditor_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            
            if (!Title.EndsWith("*") && HasUnsavedChanges())
            {
                Title += "*";
            }
            else if (Title.EndsWith("*") && !HasUnsavedChanges())
            {
                Title = Title.TrimEnd('*');
            }
        }

        private bool HasUnsavedChanges()
        {
            return _lastSavedText != TextEditor.Text;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (HasUnsavedChanges())
            {
                var result = MessageBox.Show("Документ содержит несохраненные изменения. Сохранить?",
                                           "Блокнот",
                                           MessageBoxButton.YesNoCancel,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveCommand_Executed(null, null);
                   
                    if (HasUnsavedChanges())
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                    return;
                }
            }

            base.OnClosing(e);
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (HasUnsavedChanges())
            {
                var result = MessageBox.Show("Сохранить изменения в текущем файле?",
                                           "Блокнот",
                                           MessageBoxButton.YesNoCancel,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveCommand_Executed(null, null);
                    if (HasUnsavedChanges()) return; 
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            TextEditor.Clear();
            _currentFilePath = null;
            _lastSavedText = string.Empty;
            Title = "Блокнот - Новый файл";
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (HasUnsavedChanges())
            {
                var result = MessageBox.Show("Сохранить изменения в текущем файле?",
                                           "Блокнот",
                                           MessageBoxButton.YesNoCancel,
                                           MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    SaveCommand_Executed(null, null);
                    if (HasUnsavedChanges()) return; 
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            var openFileDialog = new OpenFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                DefaultExt = ".txt"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    TextEditor.Text = File.ReadAllText(openFileDialog.FileName);
                    _currentFilePath = openFileDialog.FileName;
                    _lastSavedText = TextEditor.Text;
                    Title = $"Блокнот - {Path.GetFileName(_currentFilePath)}";
                }
                catch (IOException ex)
                {
                    MessageBox.Show($"Ошибка при открытии файла:\n{ex.Message}",
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }

        private void SaveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                SaveAs();
            }
            else
            {
                SaveToFile(_currentFilePath);
            }
        }

        private void SaveAs()
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                DefaultExt = ".txt"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                _currentFilePath = saveFileDialog.FileName;
                SaveToFile(_currentFilePath);
            }
        }

        private void SaveToFile(string filePath)
        {
            try
            {
                File.WriteAllText(filePath, TextEditor.Text);
                _lastSavedText = TextEditor.Text;
                Title = $"Блокнот - {Path.GetFileName(filePath)}";
            }
            catch (IOException ex)
            {
                MessageBox.Show($"Ошибка при сохранении файла:\n{ex.Message}",
                              "Ошибка",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
    }
}