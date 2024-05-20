using NeuralNetworkServer.Factories;
using NeuralNetworkServer.NeuralNetworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace NeuralNetworkServer.Handlers
{
    public class TestTrainingHandler : ITrainingHandler
    {
        public bool IsEnabled { get; set; }

        public event ITrainingHandler.TrainingEventHandler? OnTrainingStarted;
        public event ITrainingHandler.TrainingEventHandler? OnTrainingStopped;

        private readonly QLearningFactory _factory;
        private readonly InputHandler _inputHandler;

        private bool _isTrainingRunning;
        private bool _isResetAfterTrainingStopped;

        public TestTrainingHandler(QLearningFactory factory, InputHandler inputHandler)
        {
            _factory = factory;
            _inputHandler = inputHandler;
        }

        public void StartTraining(bool skipFileDeletion = false)
        {
            if (_inputHandler.ResetOnStart && !skipFileDeletion)
            {
                FileRemover fileRemover = new();

                if (!fileRemover.RemoveAllFiles())
                    return;

                Console.WriteLine("Removed all Neural Network save files");

                _factory.CreateNeuralNetwork();
                _factory.CreateTargetNeuralNetwork();
                _factory.CreateReplayMemory();
            }
            else
            {
                _factory.CreateNeuralNetworkIfNull();
                _factory.CreateTargetNeuralNetworkIfNull();
                _factory.CreateReplayMemoryIfNull();
            }

            BatchTrainer batchTrainer = _factory.CreateBatchTrainer();
            
            _factory.CreateErrorCalculator();

            TrainingParameters trainingParameters = _factory.CreateTrainingParameters();

            try
            {
                StartTrainingInternal(batchTrainer, trainingParameters);

                IsEnabled = true;

                _isTrainingRunning = true;

                Console.WriteLine("Training started");

                OnTrainingStarted?.Invoke();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Failed to run the training: {exception.Message}");
            }
        }

        public void StopTraining(bool reset = false)
        {
            _isTrainingRunning = false;

            _isResetAfterTrainingStopped = reset;
        }

        private void StartTrainingInternal(BatchTrainer batchTrainer, TrainingParameters trainingParameters)
        {
            if (batchTrainer == null)
                throw new NullReferenceException(nameof(batchTrainer));

            if (trainingParameters.ReplayMemory == null)
                throw new NullReferenceException(nameof(trainingParameters.ReplayMemory));

            if (trainingParameters.NeuralNetwork == null)
                throw new NullReferenceException(nameof(trainingParameters.NeuralNetwork));

            if (trainingParameters.TargetNeuralNetwork == null)
                throw new NullReferenceException(nameof(trainingParameters.TargetNeuralNetwork));

            if (trainingParameters.ErrorCalculator == null)
                throw new NullReferenceException(nameof(trainingParameters.ErrorCalculator));

            Task.Run(() => TrainingLoopQLearning(batchTrainer, trainingParameters));
        }

        private void TrainingLoopQLearning(BatchTrainer batchTrainer, TrainingParameters trainingParameters)
        {
            /*List<List<double>> outputTables = new();
            List<List<double>> inputTables = new();
            int testDataCount = 20;
            Random random = new();

            for (int i = 0; i < testDataCount; i++)
            {
                var t = new List<double>();

                for (int j = 0; j < trainingParameters.NeuralNetwork.Layers[^1]; j++)
                {
                    t.Add(random.NextDouble() * 2 - 1);
                }

                outputTables.Add(t);
            }

            for (int i = 0; i < testDataCount; i++)
            {
                var t = new List<double>();

                for (int j = 0; j < trainingParameters.NeuralNetwork.Layers[0]; j++)
                {
                    t.Add(random.NextDouble());
                }

                inputTables.Add(t);
            }

            while (_isTrainingRunning)
            {
                double learningRate = 0.1;
                double totalError = 0;
                List<double> errors = new();

                for (int i = 0; i < testDataCount; i++)
                {
                    var output = trainingParameters.NeuralNetwork.FeedForward(inputTables[i].ToArray());

                    for (int j = 0; j < outputTables[i].Count; j++)
                    {
                        errors.Add(Math.Pow(output[j] - outputTables[i][j], 2));
                        totalError += errors[j];
                    }

                    trainingParameters.NeuralNetwork.BackPropagate(outputTables[i], learningRate);
                }

                Console.WriteLine("Total error: " + totalError.ToString("F10"));
            }

            TrainingLoopQLearningStopped();*/
        }
        
        private void TrainingLoopQLearningStopped()
        {
            IsEnabled = false;

            if (_isResetAfterTrainingStopped)
            {
                _factory.RemoveNeuralNetwork();
                _factory.RemoveTargetNeuralNetwork();
                _factory.RemoveReplayMemory();
                _factory.RemoveErrorCalculator();
            }

            Console.WriteLine("Training stopped");

            OnTrainingStopped?.Invoke();
        }
    }
}
