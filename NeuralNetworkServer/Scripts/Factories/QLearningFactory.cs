using NeuralNetworkServer.Handlers;
using NeuralNetworkServer.NeuralNetworks;
using System;
using System.Collections.Generic;

namespace NeuralNetworkServer.Factories
{
    public class QLearningFactory
    {
        private readonly QLearningContext _context;
        private readonly InputHandler _inputHandler;

        public QLearningFactory(QLearningContext context, InputHandler inputHandler)
        {
            _context = context;
            _inputHandler = inputHandler;
        }

        public void CreateNeuralNetwork()
        {
            _context.CreateNeuralNetwork(
                _inputHandler.Layers,
                _inputHandler.WeightDecay,
                _inputHandler.DropoutPercentage,
                _inputHandler.MutationStrength,
                _inputHandler.IsDropoutActive
            );
        }

        public void CreateNeuralNetworkIfNull()
        {
            if (_context.NeuralNetwork != null)
                return;

            CreateNeuralNetwork();
        }

        public void RemoveNeuralNetwork()
        {
            if (_context.NeuralNetwork == null)
                return;

            _context.RemoveNeuralNetwork();
        }

        public void CreateTargetNeuralNetwork()
        {
            _context.CreateTargetNeuralNetwork();
        }

        public void CreateTargetNeuralNetworkIfNull()
        {
            if (_context.TargetNeuralNetwork != null)
                return;

            CreateTargetNeuralNetwork();
        }

        public void RemoveTargetNeuralNetwork()
        {
            if (_context.TargetNeuralNetwork == null)
                return;

            _context.RemoveTargetNeuralNetwork();
        }

        public void CreateReplayMemory()
        {
            List<int> layers = _inputHandler.Layers;

            _context.CreateReplayMemory(
                _inputHandler.ReplayMemorySize,
                layers[0],
                layers[^1]
            );
        }

        public void CreateReplayMemoryIfNull()
        {
            if (_context.ReplayMemory != null)
                return;

            CreateReplayMemory();
        }

        public void RemoveReplayMemory()
        {
            if (_context.ReplayMemory == null)
                return;

            _context.RemoveReplayMemory();
        }

        public void CreateErrorCalculator()
        {
            _context.CreateErrorCalculator(_inputHandler.MaxErrors);
        }

        public void RemoveErrorCalculator()
        {
            if (_context.ErrorCalculator == null)
                return;

            _context.RemoveErrorCalculator();
        }

        public TrainingParameters CreateTrainingParameters()
        {
            if (_context.ReplayMemory == null)
                throw new ArgumentException("Cannot create the Target Training Parameters: Replay Memory has not been created yet");

            if (_context.NeuralNetwork == null)
                throw new ArgumentException("Cannot create the Target Training Parameters: Neural Network has not been created yet");

            if (_context.TargetNeuralNetwork == null)
                throw new ArgumentException("Cannot create the Target Training Parameters: Target Neural Network has not been created yet");

            if (_context.ErrorCalculator == null)
                throw new ArgumentException("Cannot create the Target Training Parameters: Error Calculator has not been created yet");

            TrainingParameters trainingParameters = new()
            {
                ReplayMemory = _context.ReplayMemory,
                NeuralNetwork = _context.NeuralNetwork,
                TargetNeuralNetwork = _context.TargetNeuralNetwork,
                ErrorCalculator = _context.ErrorCalculator,
                BatchSize = _inputHandler.BatchSize,
                DiscountFactor = _inputHandler.DiscountFactor,
                BellmanLearningRate = _inputHandler.BellmanLearningRate,
                NeuralNetworkLearningRate = _inputHandler.LearningRate,
                StartReplayPriority = _inputHandler.StartReplayPriority
            };

            return trainingParameters;
        }

#pragma warning disable CA1822

        public BatchTrainer CreateBatchTrainer()
        {
            return new BatchTrainer(_inputHandler.LearningRate);
        }

#pragma warning restore CA1822
    }
}
