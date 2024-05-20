using NeuralNetworkServer.NeuralNetworks;
using System;
using System.Collections.Generic;
using System.IO;
using Tensorflow.Keras.Engine;
using Tensorflow.NumPy;
using static Tensorflow.KerasApi;

namespace NeuralNetworkServer
{
    public class QLearningContext
    {
        public const string RootFolder = @"SaveData\";
        public const string NeuralNetworkFileName = "Best Neural Network.weights.h5";
        public const string ReplayMemoryFileName = "Replay Memory.txt";
        public const string TotalErrorFileName = "Total Errors.txt";
        public const string TotalCorrectFileName = "Total Correct.txt";

        public IModel? NeuralNetwork { get; private set; }
        public IModel? TargetNeuralNetwork { get; private set; }
        public ReplayMemory? ReplayMemory { get; private set; }
        public ErrorCalculator? ErrorCalculator { get; private set; }
        public List<int>? NeuralNetworkLayers { get; private set; }

        public IModel CreateNeuralNetwork(List<int> layers, float weightDecay, float dropoutPercentage, float mutationStrength, bool isDropoutActive)
        {
            if (layers == null || layers.Count <= 1)
            {
                throw new ArgumentException("Cannot create the Neural Network: There are not enough layers");
            }

            IModel neuralNetwork = CreateNeuralNetworkModel(layers, weightDecay, dropoutPercentage, "policy_model");

            //NeuralNetwork neuralNetwork = new(layers, weightDecay, dropoutPercentage);

            string filePath = Path.Combine(RootFolder, NeuralNetworkFileName);

            if (File.Exists(filePath))
            {
                neuralNetwork.load_weights(filePath);
            }
            else
            {
                //neuralNetwork.Mutate(100, mutationStrength);

                neuralNetwork.save_weights(filePath);

                //neuralNetwork.Save(filePath, 0);
            }

            //neuralNetwork.IsDropoutActive = isDropoutActive;

            NeuralNetwork = neuralNetwork;

            NeuralNetworkLayers = layers;

            return neuralNetwork;
        }

        public void RemoveNeuralNetwork()
        {
            if (NeuralNetwork == null) 
                return;

            NeuralNetwork = null;
        }

        public IModel CreateTargetNeuralNetwork()
        {
            if (NeuralNetwork == null)
                throw new ArgumentException("Cannot create the Target Neural Network from the Neural Network: Neural Network has not been created yet");

            if (NeuralNetworkLayers == null)
                throw new ArgumentException("Cannot create the Target Neural Network from the Neural Network: The NeuralNetworkLayers has not been assigned. This should be assigned when the Neural Network is created");

            IModel targetNeuralNetwork = CreateNeuralNetworkModel(NeuralNetworkLayers, 0, 0, "target_model");

            /*NeuralNetwork targetNeuralNetwork = new(NeuralNetwork.Layers, NeuralNetwork.WeightDecay, NeuralNetwork.DropoutPercentage)
            {
                IsDropoutActive = NeuralNetwork.IsDropoutActive
            };*/

            targetNeuralNetwork.set_weights(NeuralNetwork.get_weights());

            //targetNeuralNetwork.CopyTo(targetNeuralNetwork);

            TargetNeuralNetwork = targetNeuralNetwork;

            return targetNeuralNetwork;
        }

        public void RemoveTargetNeuralNetwork()
        {
            if (TargetNeuralNetwork == null)
                return;

            TargetNeuralNetwork = null;
        }

        public ReplayMemory CreateReplayMemory(int size, int inputCount, int outputCount)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size));

            if (inputCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(inputCount));

            if (outputCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(outputCount));

            ReplayMemory replayMemory = new(size, inputCount, outputCount);

            string filePath = Path.Combine(RootFolder, ReplayMemoryFileName);

            if (File.Exists(filePath))
            {
                try
                {
                    replayMemory.Load(filePath);
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Cannot load the Replay Memory: {exception.Message}");
                }
            }

            ReplayMemory = replayMemory;

            return replayMemory;
        }

        public void RemoveReplayMemory()
        {
            if (ReplayMemory == null)
                return;

            ReplayMemory = null;
        }

        public ErrorCalculator CreateErrorCalculator(int maxErrors)
        {
            if (maxErrors <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxErrors));

            ErrorCalculator errorCalculator = new(maxErrors);

            ErrorCalculator = errorCalculator;

            return errorCalculator;
        }

        public void RemoveErrorCalculator()
        {
            if (ErrorCalculator == null)
                return;

            ErrorCalculator = null;
        }

        private IModel CreateNeuralNetworkModel(List<int> layers, float weightDecay, float dropoutPercentage, string name)
        {
            var inputLayer = keras.layers.Input(
                layers[0],
                dtype: np.float32
            );

            var currentLayer = inputLayer;
            foreach (int layerSize in layers.GetRange(1, layers.Count - 2))
            {
                currentLayer = keras.layers.Dense(
                    layerSize,
                    activation: keras.activations.Relu
                    //kernel_initializer: new RandomNormal()
                ).Apply(currentLayer);
            }

            var outputLayer = keras.layers.Dense(
                layers[^1]
                //kernel_initializer: new RandomNormal()
            ).Apply(currentLayer);

            IModel model = keras.Model(inputLayer, outputLayer, name: name);

            model.summary();

            model.compile(
                loss: keras.losses.MeanSquaredError(),
                optimizer: keras.optimizers.AdamW(weight_decay: weightDecay),
                metrics: new[] { "accuracy" }
            );

            return model;
        }
    }
}
