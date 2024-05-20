using Tensorflow.Keras.Engine;

namespace NeuralNetworkServer.NeuralNetworks
{
    public class TrainingParameters
    {
        public required ReplayMemory ReplayMemory { get; set; }
        public required IModel NeuralNetwork { get; set; }
        public required IModel TargetNeuralNetwork { get; set; }
        public required ErrorCalculator ErrorCalculator { get; set; }
        public int BatchSize { get; set; }
        public float DiscountFactor { get; set; }
        public float BellmanLearningRate { get; set; }
        public float NeuralNetworkLearningRate { get; set; }
        public double StartReplayPriority { get; set; }
    }
}
