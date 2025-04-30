using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Input;

namespace wpf_calculator_v3
{
    public partial class MainWindow : Window
    {
        private StringBuilder _history = new StringBuilder();
        private MediaPlayer _soundPlayer = new MediaPlayer();

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Добавляем обработчики анимаций для всех кнопок
            AddButtonAnimations();
        }

        private void AddButtonAnimations()
        {
            foreach (var child in ((Grid)Content).Children)
            {
                if (child is Button button)
                {
                    // Анимация при наведении
                    button.MouseEnter += (s, e) =>
                    {
                        var scale = new ScaleTransform(1, 1);
                        button.RenderTransform = scale;
                        var animation = new DoubleAnimation(1.1, TimeSpan.FromMilliseconds(200));
                        scale.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                        scale.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
                    };

                    // Анимация при уходе курсора
                    button.MouseLeave += (s, e) =>
                    {
                        var scale = button.RenderTransform as ScaleTransform ?? new ScaleTransform(1, 1);
                        var animation = new DoubleAnimation(1, TimeSpan.FromMilliseconds(200));
                        scale.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                        scale.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
                    };

                    // Анимация нажатия
                    button.PreviewMouseDown += (s, e) =>
                    {
                        PlayClickSound();
                        var scale = button.RenderTransform as ScaleTransform ?? new ScaleTransform(1, 1);
                        var animation = new DoubleAnimation(0.9, TimeSpan.FromMilliseconds(100));
                        scale.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                        scale.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
                    };

                    button.PreviewMouseUp += (s, e) =>
                    {
                        var scale = button.RenderTransform as ScaleTransform ?? new ScaleTransform(1, 1);
                        var animation = new DoubleAnimation(1.1, TimeSpan.FromMilliseconds(100));
                        scale.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                        scale.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
                    };
                }
            }
        }

        private void PlayClickSound()
        {
            try
            {
                _soundPlayer.Open(new Uri("pack://siteoforigin:,,,/click.wav"));
                _soundPlayer.Play();
            }
            catch
            {
                // Если файл звука не найден, просто игнорируем
            }
        }

        public int LastOperatorIndex()
        {
            int lastOperatorIndex = Math.Max(
                    Display.Text.LastIndexOf('+'),
                    Math.Max(
                        Display.Text.LastIndexOf('-'),
                        Math.Max(
                            Display.Text.LastIndexOf('*'),
                            Math.Max(
                                Display.Text.LastIndexOf('/'),
                                Math.Max(
                                    Display.Text.LastIndexOf('('),
                                    Display.Text.LastIndexOf(')')
                                )
                            )
                        )
                    )
                );

            return lastOperatorIndex;
        }

        private void ButtonNum_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            Display.Text += button.Content.ToString();
        }

        private void ButtonOperation_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            Display.Text += button.Content.ToString();
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            // Анимация очистки дисплея
            var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
            fadeOut.Completed += (s, _) =>
            {
                Display.Text = string.Empty;
                var fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(200));
                Display.BeginAnimation(OpacityProperty, fadeIn);
            };
            Display.BeginAnimation(OpacityProperty, fadeOut);
        }

        private void ButtonDecimal_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Display.Text) || (!char.IsDigit(Display.Text[Display.Text.Length - 1]) && (char)Display.Text[Display.Text.Length - 1] != '.'))
            {
                Display.Text += "0.";
            }
            else
            {
                int lastOperatorIndex = LastOperatorIndex();

                string currentNumber = lastOperatorIndex >= 0
                    ? Display.Text.Substring(lastOperatorIndex + 1)
                    : Display.Text;

                if (!currentNumber.Contains("."))
                {
                    Display.Text += ".";
                }
            }
        }

        private void ButtonEquals_Click(object sender, RoutedEventArgs e)
        {
            int lastOperatorIndex = LastOperatorIndex();
            if (string.IsNullOrEmpty(Display.Text) || (lastOperatorIndex == -1)) return;

            try
            {
                if (Display.Text.Contains("/0") && !Display.Text.Contains("/0."))
                {
                    throw new DivideByZeroException();
                }

                var result = new DataTable().Compute(Display.Text, null);

                _history.AppendLine($"{Display.Text} = {result}");
                HistoryText.Text = _history.ToString();

                HistoryScrollViewer.ScrollToEnd();

                var fadeOut = new DoubleAnimation(0, TimeSpan.FromMilliseconds(200));
                fadeOut.Completed += (s, _) =>
                {
                    Display.Text = Convert.ToString(result, CultureInfo.InvariantCulture);
                    var fadeIn = new DoubleAnimation(1, TimeSpan.FromMilliseconds(200));
                    Display.BeginAnimation(OpacityProperty, fadeIn);
                };
                Display.BeginAnimation(OpacityProperty, fadeOut);
            }
            catch (DivideByZeroException)
            {
                Display.Text = "Ошибка на /0";
            }
            catch (Exception ex)
            {
                Display.Text = $"Ошибка: {ex.Message}";
            }
        }
    }
}