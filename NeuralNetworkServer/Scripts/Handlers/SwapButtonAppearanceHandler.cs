using System.Windows.Controls;
using System.Windows.Media;

namespace NeuralNetworkServer.Handlers
{
    public class SwapButtonAppearanceHandler
    {
        private readonly Button _button;
        private readonly Brush _startColor;
        private readonly Brush _swappedColor;
        private readonly string _startText;
        private readonly string _swappedText;

        private bool _isSwapped;

        public SwapButtonAppearanceHandler(Button button, Brush startColor, Brush swappedColor, string startText, string swappedText)
        {
            _button = button;
            _startColor = startColor;
            _swappedColor = swappedColor;
            _startText = startText;
            _swappedText = swappedText;
        }

        public void Swap()
        {
            _isSwapped = !_isSwapped;

            _button.Dispatcher.Invoke(() =>
            {
                _button.Background = _isSwapped ? _swappedColor : _startColor;
                _button.Content = _isSwapped ? _swappedText : _startText;
            });
        }
    }
}
