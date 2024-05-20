using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace NeuralNetworkServer.NeuralNetworks
{
    public class NeuralNetwork
    {
        public List<int> Layers { get; set; }
        public List<double[]> Neurons { get; set; }
        public List<double[]> NeuronsWithoutActivation { get; set; }
        public List<double[]> Biases { get; set; }
        public List<double[][]> Weights { get; set; }
        public bool IsDropoutActive { get; set; }
        public double Fitness { get; set; }
        public float WeightDecay { get; set; }
        public float DropoutPercentage { get; set; }

        public NeuralNetwork(List<int> layers, float weightDecay, float dropoutPercentage)
        {
            Layers = new List<int>(layers);
            WeightDecay = weightDecay;
            DropoutPercentage = dropoutPercentage;

            Neurons = new List<double[]>();
            NeuronsWithoutActivation = new List<double[]>();
            Biases = new List<double[]>();
            Weights = new List<double[][]>();

            Fitness = 0;

            InitNeurons();
            InitBiases();
            InitWeights();
        }

        public static double Activate(double value)
        {
            return Math.Tanh(value);
        }

        public static double ActivateDerivative(double value)
        {
            double activation = Activate(value);

            return 1 - (activation * activation);
        }

        public double[] FeedForward(double[] inputs)
        {
            lock (this)
            {
                // Update inputs
                for (int inputIndex = 0; inputIndex < inputs.Length; inputIndex++)
                {
                    Neurons[0][inputIndex] = inputs[inputIndex];
                }

                // Pre-compute constant values
                double dropoutFraction = DropoutPercentage / 100.0;
                double dropoutScaler = 1.0 / (1 - dropoutFraction);

                // Initialize Random object
                Random? rand = IsDropoutActive ? new Random() : null;

                // Update neurons based on weights, biases, and activation
                for (int layerIndex = 1; layerIndex < Layers.Count; layerIndex++)
                {
                    int neuronCount = Neurons[layerIndex].Length;
                    int prevNeuronCount = Neurons[layerIndex - 1].Length;
                    double[][] layerWeights = Weights[layerIndex - 1];
                    double[] layerBiases = Biases[layerIndex];
                    double[] layerNeurons = Neurons[layerIndex - 1];
                    double[] layerNeuronsWithoutActivation = NeuronsWithoutActivation[layerIndex];
                    bool applyDropout = layerIndex != Layers.Count - 1 && IsDropoutActive && rand != null;

                    if (applyDropout)
                    {
                        Parallel.For(0, neuronCount, neuronIndex =>
                        {
                            double value = 0;
                            double[] layerWeightsNeuron = layerWeights[neuronIndex];

                            for (int weightIndex = 0; weightIndex < prevNeuronCount; weightIndex++)
                            {
                                value += layerWeightsNeuron[weightIndex] * layerNeurons[weightIndex];
                            }

                            double activationValue = value + layerBiases[neuronIndex];

                            double dropoutMask = rand.NextDouble();
                            double neuronValue = dropoutMask < dropoutFraction ? 0 : activationValue * dropoutScaler;

                            //Neurons[layerIndex][neuronIndex] = Activate(activationValue);

                            if (layerIndex != Layers.Count - 1)
                            {
                                Neurons[layerIndex][neuronIndex] = Activate(neuronValue);
                            }
                            else
                            {
                                Neurons[layerIndex][neuronIndex] = neuronValue; // No activation for output layer
                            }

                            layerNeuronsWithoutActivation[neuronIndex] = activationValue;
                        });
                    }
                    else
                    {
                        Parallel.For(0, neuronCount, neuronIndex =>
                        {
                            double value = 0;
                            double[] layerWeightsNeuron = layerWeights[neuronIndex];

                            for (int weightIndex = 0; weightIndex < prevNeuronCount; weightIndex++)
                            {
                                value += layerWeightsNeuron[weightIndex] * layerNeurons[weightIndex];
                            }

                            double activationValue = value + layerBiases[neuronIndex];

                            //Neurons[layerIndex][neuronIndex] = Activate(activationValue);

                            if (layerIndex != Layers.Count - 1)
                            {
                                Neurons[layerIndex][neuronIndex] = Activate(activationValue);
                            }
                            else
                            {
                                Neurons[layerIndex][neuronIndex] = activationValue; // No activation for output layer
                            }

                            layerNeuronsWithoutActivation[neuronIndex] = activationValue;
                        });
                    }
                }

                return Neurons[Layers.Count - 1];
            }
        }

        public void BackPropagate(List<double> targetOutputs, double learningRate)
        {
            int outputLayerIndex = Layers.Count - 1;
            double[] outputLayer = Neurons[outputLayerIndex];
            double[] outputLayerWithoutActivation = NeuronsWithoutActivation[outputLayerIndex];
            double[] outputDeltas = new double[outputLayer.Length];

            for (int i = 0; i < outputLayer.Length; i++)
            {
                double output = outputLayer[i];
                double target = targetOutputs[i];
                double outputDerivative = ActivateDerivative(outputLayerWithoutActivation[i]);
                outputDeltas[i] = (target - output) * outputDerivative;
            }

            // Backpropagate the error to the previous layers
            for (int layer = outputLayerIndex - 1; layer >= 0; layer--)
            {
                double[] currentLayer = Neurons[layer];
                double[] currentLayerWithoutActivation = NeuronsWithoutActivation[layer];
                double[][] currentLayerWeights = Weights[layer];
                double[] nextLayerDeltas = outputDeltas;

                if (layer < outputLayerIndex)
                {
                    nextLayerDeltas = new double[currentLayer.Length];

                    for (int i = 0; i < currentLayer.Length; i++)
                    {
                        double outputDerivative = ActivateDerivative(currentLayerWithoutActivation[i]);

                        double weightedSumDeltas = 0;
                        for (int j = 0; j < outputDeltas.Length; j++)
                        {
                            weightedSumDeltas += currentLayerWeights[j][i] * outputDeltas[j];
                        }

                        nextLayerDeltas[i] = outputDerivative * weightedSumDeltas;
                    }
                }

                outputDeltas = nextLayerDeltas;
            }

            // Update weights and biases using calculated deltas
            for (int layer = 0; layer < Layers.Count - 1; layer++)
            {
                double[] currentLayer = Neurons[layer];

                for (int i = 0; i < currentLayer.Length; i++)
                {
                    for (int j = 0; j < Weights[layer][i].Length; j++)
                    {
                        double deltaWeight = learningRate * outputDeltas[i] * currentLayer[j];
                        Weights[layer][i][j] += deltaWeight;
                    }

                    // Update biases
                    double deltaBias = learningRate * outputDeltas[i];
                    Biases[layer][i] += deltaBias;
                }
            }
        }



        public void Mutate(int chance, float value)
        {
            lock (this)
            {
                Random random = new();

                // Reset all neurons to 0
                foreach (double[] neuronLayer in Neurons)
                {
                    for (int i = 0; i < neuronLayer.Length; i++)
                    {
                        neuronLayer[i] = 0;
                    }
                }

                // Mutate biases
                foreach (double[] biasLayer in Biases)
                {
                    for (int i = 0; i < biasLayer.Length; i++)
                    {
                        if (random.Next(1, 101) <= chance)
                        {
                            biasLayer[i] += (random.NextDouble() * 2 - 1) * value / 100;
                            biasLayer[i] = Math.Clamp(biasLayer[i], -1, 1);
                        }
                    }
                }

                // Mutate weights
                foreach (double[][] weightMatrix in Weights)
                {
                    for (int i = 0; i < weightMatrix.Length; i++)
                    {
                        for (int j = 0; j < weightMatrix[i].Length; j++)
                        {
                            if (random.Next(1, 101) <= chance)
                            {
                                weightMatrix[i][j] += (random.NextDouble() * 2 - 1) * value / 100;
                                weightMatrix[i][j] = Math.Clamp(weightMatrix[i][j], -1, 1);
                            }
                        }
                    }
                }
            }
        }

        public NeuralNetwork CopyTo(NeuralNetwork neuralNetwork)
        {
            lock (this)
            {
                for (int i = 0; i < Neurons.Count; i++)
                {
                    for (int j = 0; j < Neurons[i].Length; j++)
                    {
                        neuralNetwork.Neurons[i][j] = 0;
                        neuralNetwork.NeuronsWithoutActivation[i][j] = 0;
                    }
                }

                for (int i = 0; i < Biases.Count; i++)
                {
                    for (int j = 0; j < Biases[i].Length; j++)
                    {
                        neuralNetwork.Biases[i][j] = Biases[i][j];
                    }
                }

                for (int i = 0; i < Weights.Count; i++)
                {
                    for (int j = 0; j < Weights[i].Length; j++)
                    {
                        for (int k = 0; k < Weights[i][j].Length; k++)
                        {
                            neuralNetwork.Weights[i][j][k] = Weights[i][j][k];
                        }
                    }
                }

                return neuralNetwork;
            }
        }

        public void Save(string filePath, double bestFitness)
        {
            lock (this)
            {
                try
                {
                    SaveInternal(filePath, bestFitness);
                }
                catch (Exception exception)
                {
                    throw new Exception($"Cannot save the Neural Network: {exception}");
                }
            }
        }

        public double Load(string filePath)
        {
            double bestFitness;

            lock (this)
            {
                try
                {
                    bestFitness = LoadInternal(filePath);
                }
                catch (Exception exception)
                {
                    throw new Exception($"Cannot load the Neural Network: {exception}");
                }

                return bestFitness;
            }
        }

        private void InitNeurons()
        {
            Neurons.Clear();
            NeuronsWithoutActivation.Clear();

            for (int i = 0; i < Layers.Count; i++)
            {
                Neurons.Add(new double[Layers[i]]);
                NeuronsWithoutActivation.Add(new double[Layers[i]]);

                for (int j = 0; j < Layers[i]; j++)
                {
                    Neurons[i][j] = 0;
                    NeuronsWithoutActivation[i][j] = 0;
                }
            }
        }

        private void InitBiases()
        {
            Biases.Clear();

            for (int i = 0; i < Layers.Count; i++)
            {
                Biases.Add(new double[Layers[i]]);

                for (int j = 0; j < Layers[i]; j++)
                {
                    Biases[i][j] = 0;
                }
            }
        }

        private void InitWeights()
        {
            Weights.Clear();

            for (int i = 1; i < Layers.Count; i++)
            {
                Weights.Add(new double[Layers[i]][]);
                int neuronsInPreviousLayer = Layers[i - 1];

                for (int j = 0; j < Neurons[i].Length; j++)
                {
                    Weights[i - 1][j] = new double[neuronsInPreviousLayer];

                    for (int k = 0; k < neuronsInPreviousLayer; k++)
                    {
                        Weights[i - 1][j][k] = 0;
                    }
                }
            }
        }

        private void SaveInternal(string filePath, double bestFitness)
        {
            lock (this)
            {
                using StreamWriter sw = new(filePath);

                sw.WriteLine(bestFitness.ToString(CultureInfo.InvariantCulture));

                foreach (var layer in Biases)
                {
                    foreach (var bias in layer)
                    {
                        sw.WriteLine(bias.ToString(CultureInfo.InvariantCulture));
                    }
                }

                foreach (var layer in Weights)
                {
                    foreach (var neuronWeights in layer)
                    {
                        foreach (var weight in neuronWeights)
                        {
                            sw.WriteLine(weight.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                }
            }
        }

        private double LoadInternal(string filePath)
        {
            lock (this)
            {
                StreamReader? sr = null;
                bool isFileLoaded = false;

                while (!isFileLoaded)
                {
                    try
                    {
                        sr = new(@filePath);

                        isFileLoaded = true;
                    }
                    catch (IOException) { }
                }

                double bestFitness = double.Parse(sr.ReadLine(), CultureInfo.InvariantCulture);

                InitNeurons();
                InitWeights();
                InitBiases();

                for (int i = 0; i < Biases.Count; i++)
                {
                    for (int j = 0; j < Biases[i].Length; j++)
                    {
                        Biases[i][j] = double.Parse(sr.ReadLine(), CultureInfo.InvariantCulture);
                    }
                }

                for (int i = 0; i < Weights.Count; i++)
                {
                    for (int j = 0; j < Weights[i].Length; j++)
                    {
                        for (int k = 0; k < Weights[i][j].Length; k++)
                        {
                            Weights[i][j][k] = double.Parse(sr.ReadLine(), CultureInfo.InvariantCulture);
                        }
                    }
                }

                return bestFitness;
            }
        }
    }
}
