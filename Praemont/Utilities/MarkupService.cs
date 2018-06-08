﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using NoteCore.Model;
using Praemont.Utilities.System;

namespace Praemont.Utilities
{
    public class MarkupService
    {
        public static readonly DependencyProperty MarkupNodesProperty = DependencyProperty.RegisterAttached(
            "MarkupNodes",
            typeof (IList<MarkupNode>),
            typeof (MarkupService),
            new PropertyMetadata(null, OnTextChanged)
            );

        public static IList<MarkupNode> GetMarkupNodes(DependencyObject d)
        {
            return d.GetValue(MarkupNodesProperty) as IList<MarkupNode>;
        }

        public static void SetMarkupNodes(DependencyObject d, IList<MarkupNode> value)
        {
            d.SetValue(MarkupNodesProperty, value);
        }

        private static void OnTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs ea)
        {
            MarkupToContent(sender as TextBlock, ea.NewValue as IList<MarkupNode>);
        }

        private static void MarkupToContent(TextBlock textBlock, IEnumerable<MarkupNode> nodes)
        {
            var inlines = new List<object>();
            foreach (var node in nodes)
            {
                switch (node.MarkupNodeType)
                {
                    case MarkupNodeType.Text:
                        inlines.Add(Run(node.Text));
                        break;
                    case MarkupNodeType.Url:
                        inlines.Add(Hyperlink("[link]", node.Text));
                        break;
                    case MarkupNodeType.Mention:
                        inlines.Add(Mention(node.Text));
                        break;
                    case MarkupNodeType.HashTag:
                        inlines.Add(Hashtag(node.Text));
                        break;
                    case MarkupNodeType.Media:
                        inlines.Add(Hyperlink("[link]", node.Text));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            textBlock.Inlines.Clear();
            textBlock.Inlines.AddRange(inlines);
            NativeMethods.keybd_event(0x28, 0, 0x0002, IntPtr.Zero);
        }

        private static global::System.Windows.Documents.Run Run(string text)
        {
            return new global::System.Windows.Documents.Run(ConvertXmlEntities(text));
        }

        private static string ConvertXmlEntities(string text)
        {
            return text.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&quot;", "\"").Replace("&apos;", "'").Replace("&amp;", "&");
        }

        private static Hyperlink Hyperlink(string text, string link)
        {
            var hyperlink = new Hyperlink(new global::System.Windows.Documents.Run(text))
            {
                //Command = OpenLinkCommand.Command, CommandParameter = link, ToolTip = link
            };
            ToolTipService.SetInitialShowDelay(hyperlink, 0);
            ToolTipService.SetShowDuration(hyperlink, 30000);
            //hyperlink.ToolTipOpening += async (s, e) => hyperlink.ToolTip = await LongUrl.Lookup(link);
            return hyperlink;
        }

        private static Hyperlink Mention(string text)
        {
            return new Hyperlink(new global::System.Windows.Documents.Run("@" + text))
            {
                //Command = ShowUserInformationCommand.Command, CommandParameter = text
            };
        }

        private static Hyperlink Hashtag(string text)
        {
            return new Hyperlink(new global::System.Windows.Documents.Run("#" + text))
            {
               // Command = SearchCommand.Command, CommandParameter = text
            };
        }
    }
}