using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Input.Platform;
using Avalonia.Media;

namespace AvaloniaApplication3.Views
{
    public partial class SegmentedTextBox : UserControl
    {
        public static readonly StyledProperty<int> SegmentCountProperty =
            AvaloniaProperty.Register<SegmentedTextBox, int>(nameof(SegmentCount), 4);

        public static readonly StyledProperty<bool> OnlyNumbersProperty =
            AvaloniaProperty.Register<SegmentedTextBox, bool>(nameof(OnlyNumbers));

        public bool OnlyNumbers
        {
            get => GetValue(OnlyNumbersProperty);
            set => SetValue(OnlyNumbersProperty, value);
        }
        
        public int SegmentCount
        {
            get => GetValue(SegmentCountProperty);
            set => SetValue(SegmentCountProperty, value);
        }

        private StackPanel? _stackPanel;
        private List<TextBox> _textBoxes = new();


        private void AttachHandlers(TextBox textBox)
        {
            textBox.TextChanged += OnTextChangedFilter;
        }
        

        private void OnTextChangedFilter(object? sender, EventArgs e)
        {
            if (OnlyNumbers && sender is TextBox tb)
            {
                if (!string.IsNullOrEmpty(tb.Text) && !tb.Text.All(char.IsDigit))
                {
                    tb.Text = new string(tb.Text.Where(char.IsDigit).ToArray());
                    tb.CaretIndex = tb.Text.Length;
                }
            }
        }
        
        private async Task HandlePasteOperation()
        {
            try
            {
                if (TopLevel.GetTopLevel(this)?.Clipboard is not { } clipboard) return;

                var text = await clipboard.GetTextAsync();
                if (string.IsNullOrEmpty(text)) return;

                // Trim and validate the text
                text = text.Trim();
                if (text.Length != SegmentCount || (OnlyNumbers && !text.All(char.IsDigit)))
                    return;

                // Update textboxes
                for (int i = 0; i < Math.Min(text.Length, _textBoxes.Count); i++)
                {
                    _textBoxes[i].Text = text[i].ToString();
                }
            }
            catch (Exception)
            {
                // Handle clipboard access errors
            }
        }
        
        public SegmentedTextBox()
        {
            InitializeComponent();
            this.AttachedToVisualTree += OnAttachedToVisualTree;
            this.GetObservable(SegmentCountProperty).Subscribe(Observer.Create<int>(_ => BuildSegments()));
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            _stackPanel = this.FindControl<StackPanel>("PART_StackPanel");
            BuildSegments();
        }

        private void BuildSegments()
        {
            if (_stackPanel == null)
                return;
        
            _stackPanel.Children.Clear();
            _textBoxes.Clear();
        
            for (int i = 0; i < SegmentCount; i++)
            {
                var tb = new TextBox
                {
                };
        
                AttachHandlers(tb);
                tb.TextChanged += (_, _) => OnTextChanged(tb);
                tb.AddHandler(KeyDownEvent, TextBox_KeyDown, RoutingStrategies.Tunnel);
                _textBoxes.Add(tb);
                _stackPanel.Children.Add(tb);
            }
        
            if (_textBoxes.Count > 0)
            {
                _textBoxes[0].Focus();
            }
        }

        private async void TextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (sender is not TextBox currentBox) return;

            // Block Tab and Space
            if (e.Key == Key.Tab || e.Key == Key.Space)
            {
                e.Handled = true;
                return;
            }

            // Handle paste operation
            if (e.Key == Key.V && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            {
                e.Handled = true;
                await HandlePasteOperation();
                return;
            }

            // Handle backspace
            if (e.Key == Key.Back)
            {
                e.Handled = true;
                int idx = _textBoxes.IndexOf(currentBox);
                currentBox.Text = string.Empty;
        
                if (idx > 0)
                {
                    var previousBox = _textBoxes[idx - 1];
                    previousBox.Focus();
                    previousBox.CaretIndex = previousBox.Text?.Length ?? 0;
                }
                return;
            }

            // Handle numeric input
            if (OnlyNumbers && e.Key >= Key.D0 && e.Key <= Key.D9)
            {
                e.Handled = true;
                int digit = (int)e.Key - (int)Key.D0;
                currentBox.Text = digit.ToString();
        
                int idx = _textBoxes.IndexOf(currentBox);
                if (idx < _textBoxes.Count - 1)
                {
                    _textBoxes[idx + 1].Focus();
                }
            }
        }

        public event EventHandler? CombinedTextChanged;

        private void OnTextChanged(TextBox sender)
        {
            if (sender.Text?.Length == sender.MaxLength)
            {
                int idx = _textBoxes.IndexOf(sender);
                if (idx < _textBoxes.Count - 1)
                {
                    _textBoxes[idx + 1].Focus();
                }
            }
            CombinedTextChanged?.Invoke(this, EventArgs.Empty);
        }
        
        
        public string GetCombinedText()
        {
            return string.Concat(_textBoxes.Select(tb => tb.Text ?? ""));
        }
    }
}
