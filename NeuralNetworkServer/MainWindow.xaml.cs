using NeuralNetworkServer.Debugging;
using NeuralNetworkServer.Factories;
using NeuralNetworkServer.Handlers;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Tensorflow;
using static Tensorflow.Binding;

namespace NeuralNetworkServer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public delegate void ButtonPressedEvent(object sender);

        public event ButtonPressedEvent? OnStartButtonPressed;
        public event ButtonPressedEvent? OnHostButtonPressed;

        private readonly QLearningContext _context;
        private readonly IServerHandler _serverHandler;
        private readonly ITrainingHandler _trainingHandler;
        private readonly InputHandler _inputHandler;
        private readonly SettingsHandler _settingsHandler;
        private readonly SwapButtonAppearanceHandler _swapStartTrainingButtonAppearanceHandler;
        private readonly SwapButtonAppearanceHandler _swapHostServerButtonAppearanceHandler;
        private readonly DebugTextBoxWriter _console;

        public MainWindow()
        {
            InitializeComponent();

            ops.set_default_session(tf.Session());
            ops.set_default_graph(tf.Graph());

            _console = new(tbConsole, svConsole);
            Console.SetOut(_console);

            _context = new();
            _inputHandler = new(this);
            _settingsHandler = new(_inputHandler);

            QLearningFactory factory = new(_context, _inputHandler);

            _serverHandler = new ServerHandler(_context, factory, _inputHandler);
            _trainingHandler = new TrainingHandler(factory, _inputHandler);

            Brush startButtonColor = new BrushConverter().ConvertFrom("#FFDDDDDD") as Brush
                ?? throw new NullReferenceException("Cannot convert hex code to brush: Start button brush is null");

            _swapStartTrainingButtonAppearanceHandler = new(
                _inputHandler.StartTrainingButton,
                startButtonColor,
                Brushes.IndianRed,
                "Start Training",
                "Stop Training"
            );

            _swapHostServerButtonAppearanceHandler = new(
                _inputHandler.HostServerButton,
                startButtonColor,
                Brushes.IndianRed,
                "Host Server",
                "Stop Hosting"
            );

            _settingsHandler.Load();

            InitializeListeners();
        }

        private void InitializeListeners()
        {
            _serverHandler.OnServerStarted += SwapHostServerButtonAppearance;
            _serverHandler.OnServerStopped += SwapHostServerButtonAppearance;

            _trainingHandler.OnTrainingStarted += SwapStartTrainingButtonAppearance;
            _trainingHandler.OnTrainingStopped += SwapStartTrainingButtonAppearance;

            OnHostButtonPressed += StartOrStopServer;
            OnStartButtonPressed += StartOrStopTraining;

            Closing += OnWindowClosing;
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            OnStartButtonPressed?.Invoke(sender);
        }

        private void ButtonHostServer_Click(object sender, RoutedEventArgs e)
        {
            OnHostButtonPressed?.Invoke(sender);
        }

        private void CheckBoxDropout_Checked(object sender, RoutedEventArgs e)
        {
            if (tbDropoutPercentage == null)
                return;

            tbDropoutPercentage.IsEnabled = true;

            /*if (_context.NeuralNetwork != null)
                _context.NeuralNetwork.IsDropoutActive = true;

            if (_context.TargetNeuralNetwork != null)
                _context.TargetNeuralNetwork.IsDropoutActive = true;*/
        }

        private void CheckBoxDropout_Unchecked(object sender, RoutedEventArgs e)
        {
            if (tbDropoutPercentage == null)
                return;

            tbDropoutPercentage.IsEnabled = false;

            /*if (_context.NeuralNetwork != null)
                _context.NeuralNetwork.IsDropoutActive = false;

            if (_context.TargetNeuralNetwork != null)
                _context.TargetNeuralNetwork.IsDropoutActive = false;*/
        }

        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            _settingsHandler.Save();
        }

        private void StartOrStopServer(object sender)
        {
            if (_serverHandler.IsEnabled)
                _serverHandler.StopServer(!_trainingHandler.IsEnabled);
            else
                _serverHandler.StartServer(_trainingHandler.IsEnabled);
        }

        private void StartOrStopTraining(object sender)
        {
            if (_trainingHandler.IsEnabled)
                _trainingHandler.StopTraining(!_serverHandler.IsEnabled);
            else
                _trainingHandler.StartTraining(_serverHandler.IsEnabled);
        }

        private void ButtonClearConsole_Click(object sender, RoutedEventArgs e)
        {
            _console.Clear();
        }

        private void SwapHostServerButtonAppearance()
        {
            _swapHostServerButtonAppearanceHandler.Swap();
        }

        private void SwapStartTrainingButtonAppearance()
        {
            _swapStartTrainingButtonAppearanceHandler.Swap();
        }
    }
}
