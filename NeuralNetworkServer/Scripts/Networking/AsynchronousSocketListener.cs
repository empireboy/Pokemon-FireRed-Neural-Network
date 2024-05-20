using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Globalization;
using System.Collections.Generic;
using System;
using System.Linq;
using NeuralNetworkServer.NeuralNetworks;
using Tensorflow.Keras.Engine;
using Tensorflow;
using Tensorflow.NumPy;

namespace NeuralNetworkServer.Networking
{
    internal class AsynchronousSocketListener
    {
        private readonly IModel _neuralNetwork;
        private readonly ReplayMemory _replayMemory;
        private readonly double _startReplayPriority;

        private Socket? _socket;
        private bool _isListening;

        private readonly StringBuilder _dataBuffer = new();

        internal AsynchronousSocketListener(IModel neuralNetwork, ReplayMemory replayMemory, double startReplayPriority)
        {
            _neuralNetwork = neuralNetwork;
            _replayMemory = replayMemory;
            _startReplayPriority = startReplayPriority;
        }

        public void StartListening()
        {
            byte[] bytes = new byte[1024];
            IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
            IPEndPoint localEndPoint = new(ipAddress, 65432);
            _socket = new(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _isListening = true;

            try
            {
                _socket.Bind(localEndPoint);
                _socket.Listen(10);

                while (_isListening)
                {
                    Socket handler = _socket.Accept();

                    while (_isListening)
                    {
                        int bytesRec = handler.Receive(bytes);

                        _dataBuffer.Append(Encoding.ASCII.GetString(bytes, 0, bytesRec));

                        int eofIndex;

                        while ((eofIndex = _dataBuffer.ToString().IndexOf("<EOF>")) >= 0)
                        {
                            string message = _dataBuffer.ToString(0, eofIndex);
                            _dataBuffer.Remove(0, eofIndex + "<EOF>".Length);

                            string[] dataStrings = message.Split(',');

                            if (dataStrings.Length > 0)
                            {
                                string command = dataStrings[0];
                                string[] arguments = dataStrings.Skip(1).ToArray();

                                ExecuteCommand(handler, command, arguments);
                            }
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                if (!_isListening)
                    return;

                Console.WriteLine("Server catched an error: " + exception);
            }
            finally
            {
                _socket.Close();
                _socket.Dispose();
            }
        }

        public void StopListening()
        {
            if (!_isListening || _socket == null)
            {
                Console.WriteLine("Cannot stop the server, because it is not running");
                return;
            }

            _isListening = false;

            _socket.Close();
            _socket.Dispose();
        }

        private void ExecuteCommand(Socket handler, string command, string[] data)
        {
            switch (command)
            {
                case "Process Inputs":

                    ProcessInputs(handler, data);

                    break;

                case "Add Replay":

                    AddReplay(data);

                    break;
            }
        }

        private void ProcessInputs(Socket handler, string[] data)
        {
            if (data.Length != _neuralNetwork.Layers[0].OutputShape.size)
                throw new ArgumentOutOfRangeException(nameof(data), $"Could not process the inputs: The data received ({data.Length}) is not the same as the Neural Network input layer length ({_neuralNetwork.Layers[0]})");

            float[] dataFloats = Array.ConvertAll(data, dataString => float.Parse(dataString, CultureInfo.InvariantCulture));

            NDArray dataNdArray = new(dataFloats);
            dataNdArray = dataNdArray.reshape(new Shape(1, dataFloats.Length));

            NDArray? output = null;

            lock (_neuralNetwork)
            {
                output = _neuralNetwork.Apply(dataNdArray).numpy();
            }

            string dataToSendBack = string.Join(",", output.ToArray<float>().Select(outputValue => outputValue.ToString(CultureInfo.InvariantCulture))) + "<EOF>\n";

            byte[] msg = Encoding.ASCII.GetBytes(dataToSendBack);

            handler.Send(msg);
        }

        private void AddReplay(string[] data)
        {
            int inputCount = (int)_neuralNetwork.Layers[0].OutputShape.size;
            int outputCount = (int)_neuralNetwork.Layers[^1].OutputShape.size;
            int neededDataLength = inputCount + outputCount + 1 + inputCount;

            if (data.Length != neededDataLength)
                throw new ArgumentOutOfRangeException(nameof(data), $"Could not add the replay: The data received ({data.Length}) is not the same as the data needed to create the replay ({neededDataLength})");

            float[] dataFloats = Array.ConvertAll(data, dataString => float.Parse(dataString, CultureInfo.InvariantCulture));

            List<float> state = dataFloats
                .Take(inputCount)
                .ToList();

            List<float> actions = dataFloats
                .Skip(inputCount)
                .Take(outputCount)
                .ToList();

            float reward = dataFloats[inputCount + outputCount];

            List<float> nextState = dataFloats
                .Skip(inputCount + outputCount + 1)
                .Take(inputCount)
                .ToList();

            _replayMemory.AddReplay(state, actions, reward, nextState, _startReplayPriority);
        }
    }
}
