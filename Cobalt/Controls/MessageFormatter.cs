using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;
using CobaltSettings.Annotations;

namespace Cobalt.Controls
{
    public static class MessageFormatter
    {
        private class MessageTextRunProperties : TextRunProperties
        {
            private Typeface _typeface;
            private double _fontSize;
            private Brush _foreground;
            private Brush _background;
            private TextDecorationCollection _decorations;

            public override Typeface Typeface => _typeface;
            public override double FontRenderingEmSize => _fontSize;
            public override double FontHintingEmSize => _fontSize;
            public override TextDecorationCollection TextDecorations => _decorations;
            public override Brush ForegroundBrush => _foreground;
            public override Brush BackgroundBrush => _background;
            public override CultureInfo CultureInfo => CultureInfo.InvariantCulture;
            public override TextEffectCollection TextEffects => null;

            public MessageTextRunProperties(Typeface typeface, double fontSize, Brush foreground, Brush background, bool underline)
            {
                _typeface = typeface;
                _fontSize = fontSize;
                _foreground = foreground;
                _background = background;
                if (underline)
                {
                    _decorations = new TextDecorationCollection(1);
                    _decorations.Add(System.Windows.TextDecorations.Underline);
                }
            }
        }

        private class MessageParagraphProperties : TextParagraphProperties
        {
            public override FlowDirection FlowDirection => FlowDirection.LeftToRight;
            public override TextAlignment TextAlignment => TextAlignment.Left;
            public override double LineHeight => 0.0;
            public override bool FirstLineInParagraph => false;
            public override TextRunProperties DefaultTextRunProperties { get; }

            public override TextWrapping TextWrapping => TextWrapping.Wrap;
            public override TextMarkerProperties TextMarkerProperties => null;
            public override double Indent => 0.0;

            public MessageParagraphProperties(TextRunProperties defaultTextRunProperties)
            {
                DefaultTextRunProperties = defaultTextRunProperties;
            }
        }

        private class MessageTextSource : TextSource
        {
            public readonly ISpanProvider Spans;
            public readonly string Text;
            public readonly MessageTextRunProperties DefaultRunProperties;
            public readonly IDictionary<string, Brush> Pallete;
            public readonly Brush Foreground;
            public readonly Brush Background;

            public MessageTextSource(string text, ISpanProvider spans, MessageTextRunProperties runProperties)
            {
                Text = text;
                Spans = spans;
                DefaultRunProperties = runProperties;
            }

            public override TextRun GetTextRun(int textSourceCharacterIndex)
            {
                if (textSourceCharacterIndex >= Text.Length)
                {
                    return new TextEndOfLine(1);
                }
                var props = DefaultRunProperties;
                int end = Text.Length;
                if (Spans != null)
                {
                    var span = Spans.GetSpan(textSourceCharacterIndex);
                    if (span.Flags > 0)
                    {
                        props = new MessageTextRunProperties(
                            new Typeface(DefaultRunProperties.Typeface.FontFamily,
                                DefaultRunProperties.Typeface.Style,
                                (span.Flags & MessageSpanFlags.Bold) > 0 ? FontWeights.Bold : FontWeights.Normal,
                                DefaultRunProperties.Typeface.Stretch),
                                DefaultRunProperties.FontRenderingEmSize,
                                (span.Flags & MessageSpanFlags.Reverse) > 0 ? Background :
                                ((span.Flags & MessageSpanFlags.Foreground) > 0 ? Pallete["Color" + span.Foreground] : props.ForegroundBrush),
                                (span.Flags & MessageSpanFlags.Reverse) > 0 ? props.ForegroundBrush :
                                ((span.Flags & MessageSpanFlags.Background) > 0 ? Pallete["Color" + span.Background] : props.BackgroundBrush),
                                (span.Flags & MessageSpanFlags.Underline) > 0);
                    }
                    end = span.End;
                }
                return new TextCharacters(Text, textSourceCharacterIndex, end - textSourceCharacterIndex, props);
            }

            public override TextSpan<CultureSpecificCharacterBufferRange> GetPrecedingText(int textSourceCharacterIndexLimit)
            {
                return new TextSpan<CultureSpecificCharacterBufferRange>(0, null);
            }

            public override int GetTextEffectCharacterIndexFromTextSourceCharacterIndex(int textSourceCharacterIndex)
            {
                return textSourceCharacterIndex;
            }
        }

        private static readonly TextFormatter Formatter = TextFormatter.Create(TextFormattingMode.Display);

        /*
         * Returns a list of TextLines to render, given the span provider and rendering width.
         */
        public static IList<TextLine> Format([NotNull]string text, ISpanProvider spans, double width, 
            Typeface typeface, double fontSize, Brush foreground, Brush background, TextWrapping wrapping = TextWrapping.NoWrap)
        {
            IList<TextLine> lines = new List<TextLine>();
            if (width < 0)
            {
                width = 0;                
            }
            int index = 0;

            while (index < text.Length)
            {
                MessageTextRunProperties properties = new MessageTextRunProperties(typeface, fontSize, foreground,
                    background, false);
                MessageTextSource src = new MessageTextSource(text, spans, properties);
                var formattedLine = Formatter.FormatLine(src, index, width, new MessageParagraphProperties(properties),
                    null);
                index += formattedLine.Length;                
                lines.Add(formattedLine);
            }
            return lines;
        }
    }
}