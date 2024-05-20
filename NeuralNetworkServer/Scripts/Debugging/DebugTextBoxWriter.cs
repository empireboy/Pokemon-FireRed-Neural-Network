using System;
using System.IO;
using System.Windows.Controls;

namespace NeuralNetworkServer.Debugging
{
    internal class DebugTextBoxWriter : TextWriter
    {
        private readonly TextBox _textBox;
        private readonly ScrollViewer _scrollViewer;

        public DebugTextBoxWriter(TextBox textBox, ScrollViewer scrollViewer)
        {
            _textBox = textBox;
            _scrollViewer = scrollViewer;
        }

        public void Clear()
        {
            _textBox.Clear();
        }

        public override void Write(string? value)
        {
            _textBox.Dispatcher.Invoke(() =>
            {
                _textBox.AppendText(value);
                _textBox.AppendText(Environment.NewLine);

                if (_scrollViewer.VerticalOffset + _scrollViewer.ViewportHeight >= _scrollViewer.ExtentHeight)
                    _scrollViewer.ScrollToBottom();
            });
        }

        public override void WriteLine(string? value)
        {
            Write(value + Environment.NewLine);
        }

        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;
    }
}