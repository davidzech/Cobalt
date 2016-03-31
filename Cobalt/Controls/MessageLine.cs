using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Cobalt.Controls
{
    public enum MessageType
    {
        Notice,
        Join,
        ServerInfo,
        Client,
        Action,
        Ctcp,
        Info,
        Own,
        Default
    }

    [Flags]
    public enum MessageMarker
    {
        None = 0,
        OldMarker = 1,
        AttentionMarker = 2
    }

    [Flags]
    public enum MessageSpanFlags
    {
        None, 
        Bold = 1,
        Italic = 2,
        Underline = 4,
        Reverse = 8,
        Foreground = 16,
        Background = 32
    }

    public struct MessageLink
    {
        public int Start;
        public int End;
    }

    public struct MessageSpan
    {
        public int Start;
        public int End;
        public MessageSpanFlags Flags;
        public byte Foreground;
        public byte Background;
    }

    public enum MessageFormattingCode
    {
        Bold = 0x2,
        Color = 0x3,
        Italic = 0x1d,
        Underline = 0x1f,
        Reverse = 0x16,
        Reset = 0x0F
    }

    public interface ISpanProvider
    {
        MessageSpan GetSpan(int index);
    }

    public sealed class MessageLine : ISpanProvider
    {
        private static readonly Regex urlRegex = new Regex(@"(www\.|(http|https|ftp)+\:\/\/)[^\s\(\)]+", RegexOptions.IgnoreCase);

        public DateTime Time { get; set; }
        public MessageType Type { get; set; }
        public string NickName { get; set; }
        public string RawText { get; set; }
        public string Text { get; set; }
        public MessageMarker Marker { get; set; }
        public MessageSpan[] Spans { get; set; }
        public MessageLink[] Links { get; set; }

        public static MessageLine Process(MessageType type, MessageMarker marker, string nickName, string text, DateTime time)
        {
            MessageLine line = new MessageLine
            {
                Type = type,
                Marker = marker,
                NickName = nickName,
                RawText = text
            };

            var builder = new StringBuilder();
            var spans = new List<MessageSpan>();
            var span = new MessageSpan();

            int last = text.Length - 1;
            int index = 0;
            for (int i = 0; i < text.Length; i++)
            {
                MessageFormattingCode ichar = (MessageFormattingCode) text[i];
                if (ichar == MessageFormattingCode.Bold || ichar == MessageFormattingCode.Color || ichar == MessageFormattingCode.Reset || 
                    ichar == MessageFormattingCode.Italic || ichar == MessageFormattingCode.Underline || ichar == MessageFormattingCode.Reverse)
                {
                    span.End = index;
                    spans.Add(span);
                    span.Start = index;
                }
                switch (ichar)
                {
                    case MessageFormattingCode.Bold:
                        span.Flags ^= MessageSpanFlags.Bold;
                        break;
                    case MessageFormattingCode.Color:
                        if (i == last || text[i + 1] > '9' || text[i + 1] < '0')
                        {
                            span.Flags &= ~MessageSpanFlags.Foreground;
                            span.Flags &= ~MessageSpanFlags.Background;
                            break;
                        }
                        span.Flags |= MessageSpanFlags.Foreground;
                        int c = (int) (text[++i] - '0');
                        if (i < last &&
                            ((c == 0 && text[i + 1] >= '0' && text[i + 1] <= '9') ||
                             (c == 1 && text[i + 1] >= '0' && text[i + 1] <= '5')))
                        {
                            c *= 10;
                            c += (int) text[++i] - '0';
                        }
                        span.Foreground = (byte) Math.Min(15, c);
                        if (i == last || i + 1 == last || text[i + 1] != ',' || text[i + 2] < '0' || text[i + 2] > '9')
                        {
                            break;
                        }
                        span.Flags |= MessageSpanFlags.Background;
                        ++i;
                        c = (int) (text[++i] - '0');
                        if (i < last &&
                            ((c == 0 && text[i + 1] >= '0' && text[i + 1] <= '9') ||
                             (c == 1 && text[i + 1] >= '0' && text[i + 1] <= '5')))
                        {
                            c *= 10;
                            c += (int)text[++i] - '0';
                        }
                        span.Background = (byte) Math.Min(15, c);
                        break;
                    case MessageFormattingCode.Reset:
                        span.Flags = MessageSpanFlags.None;
                        break;
                    case MessageFormattingCode.Reverse:
                        span.Flags ^= MessageSpanFlags.Reverse;
                        break;
                    case MessageFormattingCode.Italic:
                        span.Flags ^= MessageSpanFlags.Italic;
                        break;
                    case MessageFormattingCode.Underline:
                        span.Flags ^= MessageSpanFlags.Underline;
                        break;
                    default:
                        builder.Append(text[i]);
                        index++;
                        break;
                }
            }
            span.End = index;
            spans.Add(span);
            line.Text = builder.ToString();
            line.Spans = spans.Where(s => s.End > s.Start).ToArray();
            line.Links =
                (from Match m in urlRegex.Matches(line.Text)
                    select new MessageLink {Start = m.Index, End = m.Index + m.Length}).ToArray();
            return line;
        }

        public MessageSpan GetSpan(int index)
        {
            return this.Spans.Where((s) => index >= s.Start && index < s.End).FirstOrDefault();
        }
    }
}