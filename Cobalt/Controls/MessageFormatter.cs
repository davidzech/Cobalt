using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.TextFormatting;

namespace Cobalt.Controls
{
    public class MessageFormatter
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
            private readonly TextRunProperties _defaultProperties;

            public override FlowDirection FlowDirection => FlowDirection.LeftToRight;
            public override TextAlignment TextAlignment => TextAlignment.Left;
            public override double LineHeight => 0.0;
            public override bool FirstLineInParagraph => false;
            public override TextRunProperties DefaultTextRunProperties => _defaultProperties;
            public override TextWrapping TextWrapping => TextWrapping.Wrap;
            public override TextMarkerProperties TextMarkerProperties => null;
            public override double Indent => 0.0;

            public MessageParagraphProperties(TextRunProperties defaultTextRunProperties)
            {
                _defaultProperties = defaultTextRunProperties;
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
                                (span.Flags & MessageSpanFlags.Reverse) > 0 ? _background :
                                ((span.Flags & MessageSpanFlags.Foreground) > 0 ? _palette["Color" + span.Foreground] : _runProperties.ForegroundBrush),
                                (span.Flags & MessageSpanFlags.Reverse) > 0 ? _runProperties.ForegroundBrush :
                                ((span.Flags & MessageSpanFlags.Background) > 0 ? _palette["Color" + span.Background] : _runProperties.BackgroundBrush),
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

        public static readonly MessageFormatter Instance = new MessageFormatter();
        public static MessageFormatter GetInstance() => Instance;
        private readonly TextFormatter _formatter = TextFormatter.Create(TextFormattingMode.Display);
        protected MessageFormatter()
        {
            
        }

        /*
         * Returns a list of TextLines to render, given the span provider and rendering width.
         */
        public IEnumerable<TextLine> Format(MessageLine line, double width)
        {
            if (width < 0)
            {
                width = 0;                
            }
            int index = 0;

            while (index < line.Text.Length)
            {
                var formatted = _formatter.FormatLine(this, index, width, _paragraphProperties, null);
                index += line.;
                yield return line;
            }
        }
    }
}