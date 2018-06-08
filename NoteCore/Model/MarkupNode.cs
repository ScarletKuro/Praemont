﻿namespace NoteCore.Model
{
    public enum MarkupNodeType
    {
        Text,
        Url,
        Mention,
        HashTag,
        Media
    }

    public class MarkupNode
    {
        public MarkupNode(MarkupNodeType markupNodeType, string text)
        {
            MarkupNodeType = markupNodeType;
            Text = text;
        }

        public MarkupNodeType MarkupNodeType { get; private set; }
        public string Text { get; private set; }
    }
}