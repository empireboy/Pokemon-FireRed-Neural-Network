using NeuralNetworkServer.Factories;
using NeuralNetworkServer.Networking;
using System;
using System.Threading.Tasks;
using static Tensorflow.KerasApi;

namespace NeuralNetworkServer.Handlers
{
    public class ServerHandler : IServerHandler
    {
        public bool IsEnabled { get; set; }

        public event IServerHandler.ServerEventHandler? OnServerStarted;
        public event IServerHandler.ServerEventHandler? OnServerStopped;

        private readonly QLearningContext _context;
        private readonly QLearningFactory _factory;
        private readonly InputHandler _inputHandler;

        private AsynchronousSocketListener? _asynchronousSocketListener;

        public ServerHandler(QLearningContext context, QLearningFactory factory, InputHandler inputHandler)
        {
            _context = context;
            _factory = factory;
            _inputHandler = inputHandler;
        }

        public void StartServer(bool skipFileDeletion = false)
        {
            keras.backend.clear_session();

            if (_inputHandler.ResetOnStart && !skipFileDeletion)
            {
                FileRemover fileRemover = new();

                if (!fileRemover.RemoveAllFiles())
                    return;

                Console.WriteLine("Removed all Neural Network save files");

                _factory.CreateNeuralNetwork();
                _factory.CreateReplayMemory();
            }
            else
            {
                _factory.CreateNeuralNetworkIfNull();
                _factory.CreateReplayMemoryIfNull();
            }

            try
            {
                StartServerInternal(_inputHandler.StartReplayPriority);

                IsEnabled = true;

                Console.WriteLine("Server started");

                OnServerStarted?.Invoke();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Failed to run the server: {exception.Message}");
            }
        }

        public void StopServer(bool reset)
        {
            try
            {
                StopServerInternal();

                IsEnabled = false;

                if (reset)
                {
                    _factory.RemoveNeuralNetwork();
                    _factory.RemoveReplayMemory();
                }

                Console.WriteLine("Server stopped");

                OnServerStopped?.Invoke();
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Failed to stop the server: {exception.Message}");
            }
        }

        private void StartServerInternal(double startReplayPriority)
        {
            if (_context.NeuralNetwork == null)
                throw new NullReferenceException(nameof(_context.NeuralNetwork));

            if (_context.ReplayMemory == null)
                throw new NullReferenceException(nameof(_context.ReplayMemory));

            _asynchronousSocketListener = new(_context.NeuralNetwork, _context.ReplayMemory, startReplayPriority);

            Task.Run(_asynchronousSocketListener.StartListening);
        }

        private void StopServerInternal()
        {
            if (_asynchronousSocketListener == null)
                throw new NullReferenceException(nameof(_asynchronousSocketListener));

            _asynchronousSocketListener.StopListening();
        }
    }
}
