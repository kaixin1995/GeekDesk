using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace GeekDesk.Control.Other
{
    /// <summary>
    /// 支持关键字高亮的文本块 (PlainText 为原始文本, HighlightText 为命中关键字)
    /// </summary>
    public class HighlightTextBlock : TextBlock
    {
        public static readonly DependencyProperty PlainTextProperty = DependencyProperty.Register(
            nameof(PlainText), typeof(string), typeof(HighlightTextBlock),
            new PropertyMetadata(string.Empty, OnTextChanged));

        public static readonly DependencyProperty HighlightTextProperty = DependencyProperty.Register(
            nameof(HighlightText), typeof(string), typeof(HighlightTextBlock),
            new PropertyMetadata(string.Empty, OnTextChanged));

        public string PlainText
        {
            get => (string)GetValue(PlainTextProperty);
            set => SetValue(PlainTextProperty, value);
        }

        public string HighlightText
        {
            get => (string)GetValue(HighlightTextProperty);
            set => SetValue(HighlightTextProperty, value);
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((HighlightTextBlock)d).RebuildInlines();
        }

        private void RebuildInlines()
        {
            Inlines.Clear();
            string text = PlainText ?? string.Empty;
            string keyword = HighlightText;
            if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(keyword))
            {
                Inlines.Add(new Run(text));
                return;
            }

            Brush highlightBrush = new SolidColorBrush(Color.FromRgb(0x25, 0x63, 0xEB));
            int index = 0;
            int hit;
            while ((hit = text.IndexOf(keyword, index, StringComparison.OrdinalIgnoreCase)) >= 0)
            {
                if (hit > index)
                {
                    Inlines.Add(new Run(text.Substring(index, hit - index)));
                }
                Inlines.Add(new Run(text.Substring(hit, keyword.Length))
                {
                    Foreground = highlightBrush,
                    FontWeight = FontWeights.Bold
                });
                index = hit + keyword.Length;
            }
            if (index < text.Length)
            {
                Inlines.Add(new Run(text.Substring(index)));
            }
        }
    }
}
