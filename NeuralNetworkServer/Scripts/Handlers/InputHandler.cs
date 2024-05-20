using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Controls;

namespace NeuralNetworkServer.Handlers
{
    public class InputHandler
    {
        public bool ResetOnStart
        {
            get
            {
                return _mainWindow.cbReset.IsChecked ?? false;
            }
            set
            {
                _mainWindow.cbReset.IsChecked = value;
            }
        }

        public List<int> Layers
        {
            get
            {
                string layersAsString = _mainWindow.tbLayers.Text;

                if (string.IsNullOrEmpty(layersAsString))
                {
                    throw new InvalidOperationException("Cannot get the layers input: Layers input is empty");
                }

                List<string> layersAsStringArray = new(layersAsString.Split(','));
                List<int> layers = layersAsStringArray.ConvertAll(layerString => int.Parse(layerString, CultureInfo.InvariantCulture));

                return layers;
            }
        }

        public string LayersText
        {
            get
            {
                return _mainWindow.tbLayers.Text;
            }
            set
            {
                _mainWindow.tbLayers.Text = value;
            }
        }

        public float MutationStrength
        {
            get
            {
                return float.Parse(_mainWindow.tbMutationStrength.Text, CultureInfo.InvariantCulture);
            }
        }

        public string MutationStrengthText
        {
            get
            {
                return _mainWindow.tbMutationStrength.Text;
            }
            set
            {
                _mainWindow.tbMutationStrength.Text = value;
            }
        }

        public float WeightDecay
        {
            get
            {
                return float.Parse(_mainWindow.tbWeightDecay.Text, CultureInfo.InvariantCulture);
            }
        }

        public string WeightDecayText
        {
            get
            {
                return _mainWindow.tbWeightDecay.Text;
            }
            set
            {
                _mainWindow.tbWeightDecay.Text = value;
            }
        }

        public int DropoutPercentage
        {
            get
            {
                return int.Parse(_mainWindow.tbDropoutPercentage.Text, CultureInfo.InvariantCulture);
            }
        }

        public string DropoutPercentageText
        {
            get
            {
                return _mainWindow.tbDropoutPercentage.Text;
            }
            set
            {
                _mainWindow.tbDropoutPercentage.Text = value;
            }
        }

        public bool IsDropoutActive
        {
            get
            {
                return _mainWindow.cbDropoutPercentage.IsChecked ?? false;
            }
            set
            {
                _mainWindow.cbDropoutPercentage.IsChecked = value;
            }
        }

        public int ReplayMemorySize
        {
            get
            {
                return int.Parse(_mainWindow.tbReplayMemorySize.Text, CultureInfo.InvariantCulture);
            }
        }

        public string ReplayMemorySizeText
        {
            get
            {
                return _mainWindow.tbReplayMemorySize.Text;
            }
            set
            {
                _mainWindow.tbReplayMemorySize.Text = value;
            }
        }

        public int BatchSize
        {
            get
            {
                return int.Parse(_mainWindow.tbBatchSize.Text, CultureInfo.InvariantCulture);
            }
        }

        public string BatchSizeText
        {
            get
            {
                return _mainWindow.tbBatchSize.Text;
            }
            set
            {
                _mainWindow.tbBatchSize.Text = value;
            }
        }

        public double StartReplayPriority
        {
            get
            {
                return double.Parse(_mainWindow.tbStartReplayPriority.Text, CultureInfo.InvariantCulture);
            }
        }

        public string StartReplayPriorityText
        {
            get
            {
                return _mainWindow.tbStartReplayPriority.Text;
            }
            set
            {
                _mainWindow.tbStartReplayPriority.Text = value;
            }
        }

        public int MaxErrors
        {
            get
            {
                return int.Parse(_mainWindow.tbMaxErrors.Text, CultureInfo.InvariantCulture);
            }
        }

        public string MaxErrorsText
        {
            get
            {
                return _mainWindow.tbMaxErrors.Text;
            }
            set
            {
                _mainWindow.tbMaxErrors.Text = value;
            }
        }

        public float LearningRate
        {
            get
            {
                return float.Parse(_mainWindow.tbLearningRate.Text, CultureInfo.InvariantCulture);
            }
        }

        public string LearningRateText
        {
            get
            {
                return _mainWindow.tbLearningRate.Text;
            }
            set
            {
                _mainWindow.tbLearningRate.Text = value;
            }
        }

        public float DiscountFactor
        {
            get
            {
                return float.Parse(_mainWindow.tbDiscountFactor.Text, CultureInfo.InvariantCulture);
            }
        }

        public string DiscountFactorText
        {
            get
            {
                return _mainWindow.tbDiscountFactor.Text;
            }
            set
            {
                _mainWindow.tbDiscountFactor.Text = value;
            }
        }

        public float BellmanLearningRate
        {
            get
            {
                return float.Parse(_mainWindow.tbBellmanLearningRate.Text, CultureInfo.InvariantCulture);
            }
        }

        public string BellmanLearningRateText
        {
            get
            {
                return _mainWindow.tbBellmanLearningRate.Text;
            }
            set
            {
                _mainWindow.tbBellmanLearningRate.Text = value;
            }
        }

        public Button StartTrainingButton
        {
            get
            {
                return _mainWindow.btnStart;
            }
        }

        public Button HostServerButton
        {
            get
            {
                return _mainWindow.btnHostServer;
            }
        }

        private readonly MainWindow _mainWindow;

        public InputHandler(MainWindow mainWindow)
        {
            _mainWindow = mainWindow;
        }

#pragma warning disable CA1822

        public void EnableButton(Button button, bool enabled = true)
        {
            button.IsEnabled = enabled;
        }

#pragma warning restore CA1822
    }
}
