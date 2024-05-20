namespace NeuralNetworkServer.Handlers
{
    public interface IServerHandler : IEnableable
    {
        public delegate void ServerEventHandler();

        event ServerEventHandler? OnServerStarted;
        event ServerEventHandler? OnServerStopped;

        void StartServer(bool skipFileDeletion = false);
        void StopServer(bool reset);
    }
}
