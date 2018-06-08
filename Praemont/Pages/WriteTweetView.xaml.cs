﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using NoteCore.Service;
using NoteCore.Twitter.Model;

namespace Praemont.Pages
{
    /// <summary>
    /// Interaction logic for WriteTweetView.xaml
    /// </summary>
    public partial class WriteTweetView : UserControl, INotifyPropertyChanged
    {
        private string _inReplyToId;
        private bool _directMessage;
        private string _directMessageRecipient;
        private string _image;
        private IInputElement _previousFocusedElement;
        private bool _isSending;
        public WriteTweetView()
        {
            InitializeComponent();
            ComposeTitle.Text = "Compose a tweet";
            SendButtonText.Text = "Tweet";
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (TextBox.IsVisible)
            {
                TextBox.Focus();
                TextBox.SelectionStart = TextBox.Text.Length;
            }
        }
        private void Hide()
        {
            TextBox.Clear();
            Visibility = Visibility.Collapsed;
            Image = null;
            ComposeTitle.Text = "Compose a tweet";
            Keyboard.Focus(_previousFocusedElement);
        }
        public string Image
        {
            get { return _image; }
            set
            {
                if (_image == value) return;
                _image = value;
                OnPropertyChanged();
            }
        }
        public void Show(string message = "", string inReplyToId = null)
        {
            _previousFocusedElement = Keyboard.FocusedElement;
            ComposeTitle.Text = "Compose a tweet";
            TextBox.Text = message;
            _directMessage = false;
            _directMessageRecipient = null;
            _inReplyToId = inReplyToId;
            SendButtonText.Text = "Tweet";
            Image = null;
            Visibility = Visibility.Visible;
        }
        public void Toggle()
        {
            if (IsVisible) Hide();
            else Show();
        }
        public void ShowDirectMessage(string screenName)
        {
            _previousFocusedElement = Keyboard.FocusedElement;
            ComposeTitle.Text = "@" + screenName;
            TextBox.Text = string.Empty;
            _directMessage = true;
            _directMessageRecipient = screenName;
            _inReplyToId = null;
            SendButtonText.Text = "Send";
            Image = null;
            Visibility = Visibility.Visible;
        }
        private void TextBoxOnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
                e.Handled = true;
            }
            if (e.Key == Key.Return && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                OnSend(this, null);
                e.Handled = true;
            }
        }
        private async void OnSend(object sender, RoutedEventArgs e)
        {
            if (_isSending) return;
            _isSending = true;
            try
            {
                SendButtonText.Visibility = Visibility.Collapsed;
                SendButtonProgress.IsIndeterminate = true;
                SendButtonProgress.Visibility = Visibility.Visible;
                var text = TextBox.Text;
                string json;

                if (_directMessage)
                {
                    json = await Twitter.SendDirectMessage(text, _directMessageRecipient);
                }
                else
                {
                    json = string.IsNullOrWhiteSpace(Image)
                        ? await Twitter.UpdateStatus(text, _inReplyToId)
                        : await Twitter.UpdateStatusWithMedia(text, Image);
                }

                if (json.Contains("id_str"))
                {
                    Hide();
                    //var status = Status.ParseJson("[" + json + "]");
                    //UpdateStatusHomeTimelineCommand.Command.Execute(status, this);
                }
            }
            catch (Exception ex)
            {
                ComposeTitle.Text = "Error";
                Trace.TraceError(ex.ToString());
            }
            finally
            {
                _isSending = false;
                SendButtonText.Visibility = Visibility.Visible;
                SendButtonProgress.Visibility = Visibility.Collapsed;
                SendButtonProgress.IsIndeterminate = false;
            }
        }

        private void OnShorten(object sender, RoutedEventArgs e)
        {
            try
            {
                Shorten.IsEnabled = false;
                TextBox.Text = ShortUrl(TextBox.Text);
            }
            catch (Exception)
            {
                ComposeTitle.Text = "Error shortening urls";
            }
            finally
            {
                Shorten.IsEnabled = true;
            }
        }
        private static string ShortUrl(string link)
        {
            var url = "http://is.gd/create.php?format=simple&url=" + OAuth.UrlEncode(link);
            using (var client = new WebClient())
            {
                var shortUrl = client.DownloadString(url);
                return shortUrl;
            }
        }
        private void OnPhoto(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                Filter = "Images (*.png, *.jpg, *.jpeg, *.gif)|*.png;*.jpg;*.jpeg;*.gif"
            };
            if (dialog.ShowDialog() == true)
            {
                Image = dialog.FileName;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
