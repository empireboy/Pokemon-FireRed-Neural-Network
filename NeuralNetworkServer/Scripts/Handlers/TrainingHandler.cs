using NeuralNetworkServer.Factories;
using NeuralNetworkServer.NeuralNetworks;
using System;
using System.IO;
using System.Threading.Tasks;

namespace NeuralNetworkServer.Handlers
{
    public class TrainingHandler : ITrainingHandler
    {
        public bool IsEnabled { get; set; }

        public event ITrainingHandler.TrainingEventHandler? OnTrainingStarted;
        public event ITrainingHandler.TrainingEventHandler? OnTrainingStopped;

        private readonly QLearningFactory _factory;
        private readonly InputHandler _inputHandler;

        private bool _isTrainingRunning;
        private bool _isResetAfterTrainingStopped;

        public TrainingHandler(QLearningFactory factory, InputHandler inputHandler)
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
                IsEnabled = true;

                _isTrainingRunning = true;

                StartTrainingInternal(batchTrainer, trainingParameters);

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

            //TrainingLoopQLearning(batchTrainer, trainingParameters);
            Task.Run(() => TrainingLoopQLearning(batchTrainer, trainingParameters));
        }

        private void TrainingLoopQLearning(BatchTrainer batchTrainer, TrainingParameters trainingParameters)
        {
            int iteration = 0;
            string neuralNetworkFilePath = Path.Combine(QLearningContext.RootFolder, QLearningContext.NeuralNetworkFileName);
            string replayMemoryFilePath = Path.Combine(QLearningContext.RootFolder, QLearningContext.ReplayMemoryFileName);
            string totalErrorFilePath = Path.Combine(QLearningContext.RootFolder, QLearningContext.TotalErrorFileName);
            string lossPlotFilePath = Path.Combine(QLearningContext.RootFolder, "Losses Graph.png");

            while (_isTrainingRunning)
            {
                if (trainingParameters.ReplayMemory.Size <= 0)
                {
                    Console.WriteLine("There are no Replays in the ReplayMemory. Training will stop");

                    break;
                }

                if (iteration % 4 == 0)
                {
                    batchTrainer.Train(trainingParameters);
                }

                if (iteration % 2 == 0)
                {
                    //trainingParameters.ErrorCalculator.PrintAverageError();
                    //trainingParameters.ErrorCalculator.SaveAverageError(totalErrorFilePath);
                }

                if (iteration % 20000 == 0)
                {
                    trainingParameters.NeuralNetwork.save_weights(neuralNetworkFilePath);
                    trainingParameters.ReplayMemory.Save(replayMemoryFilePath);
                    Console.WriteLine("Updating target neural network");

                    lock (trainingParameters.NeuralNetwork)
                    {
                        lock (trainingParameters.TargetNeuralNetwork)
                        {
                            trainingParameters.TargetNeuralNetwork.set_weights(trainingParameters.NeuralNetwork.get_weights());
                        }
                    }
                }

                iteration++;
            }

            batchTrainer.PlotLoss(lossPlotFilePath);

            trainingParameters.NeuralNetwork.save_weights(neuralNetworkFilePath);
            trainingParameters.ReplayMemory.Save(replayMemoryFilePath);

            TrainingLoopQLearningStopped();
        }

        private void TrainingLoopTest(BatchTrainer batchTrainer, TrainingParameters trainingParameters)
        {
            while (_isTrainingRunning)
            {
                batchTrainer.TrainTest(trainingParameters);
            }

            TrainingLoopQLearningStopped();
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
