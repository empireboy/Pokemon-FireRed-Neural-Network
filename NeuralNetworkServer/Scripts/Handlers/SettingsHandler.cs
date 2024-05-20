using NeuralNetworkServer;

namespace NeuralNetworkServer.Handlers
{
    public class SettingsHandler
    {
        private readonly InputHandler _inputHandler;

        public SettingsHandler(InputHandler inputHandler)
        {
            _inputHandler = inputHandler;
        }

        public void Save()
        {
            Settings.Default.Layers = _inputHandler.LayersText;
            Settings.Default.MutationStrength = _inputHandler.MutationStrengthText;
            Settings.Default.WeightDecay = _inputHandler.WeightDecayText;
            Settings.Default.DropoutPercentage = _inputHandler.DropoutPercentageText;
            Settings.Default.ReplayMemorySize = _inputHandler.ReplayMemorySizeText;
            Settings.Default.BatchSize = _inputHandler.BatchSizeText;
            Settings.Default.StartReplayPriority = _inputHandler.StartReplayPriorityText;
            Settings.Default.MaxErrors = _inputHandler.MaxErrorsText;
            Settings.Default.LearningRate = _inputHandler.LearningRateText;
            Settings.Default.DiscountFactor = _inputHandler.DiscountFactorText;
            Settings.Default.BellmanLearningRate = _inputHandler.BellmanLearningRateText;

            Settings.Default.Save();
        }

        public void Load()
        {
            _inputHandler.LayersText = Settings.Default.Layers;
            _inputHandler.MutationStrengthText = Settings.Default.MutationStrength;
            _inputHandler.WeightDecayText = Settings.Default.WeightDecay;
            _inputHandler.DropoutPercentageText = Settings.Default.DropoutPercentage;
            _inputHandler.ReplayMemorySizeText = Settings.Default.ReplayMemorySize;
            _inputHandler.BatchSizeText = Settings.Default.BatchSize;
            _inputHandler.StartReplayPriorityText = Settings.Default.StartReplayPriority;
            _inputHandler.MaxErrorsText = Settings.Default.MaxErrors;
            _inputHandler.LearningRateText = Settings.Default.LearningRate;
            _inputHandler.DiscountFactorText = Settings.Default.DiscountFactor;
            _inputHandler.BellmanLearningRateText = Settings.Default.BellmanLearningRate;
        }
    }
}
