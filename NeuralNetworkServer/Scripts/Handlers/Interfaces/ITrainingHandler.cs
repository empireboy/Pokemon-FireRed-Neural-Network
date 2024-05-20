namespace NeuralNetworkServer.Handlers
{
    public interface ITrainingHandler : IEnableable
    {
        public delegate void TrainingEventHandler();

        event TrainingEventHandler? OnTrainingStarted;
        event TrainingEventHandler? OnTrainingStopped;

        void StartTraining(bool skipFileDeletion = false);
        void StopTraining(bool reset);
    }
}
