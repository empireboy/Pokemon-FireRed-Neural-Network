using NeuralNetworkServer.Utility;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using Tensorflow;
using Tensorflow.Keras.Engine;
using Tensorflow.NumPy;
using static Tensorflow.Binding;

namespace NeuralNetworkServer.NeuralNetworks
{
    public class BatchTrainer
    {
        private float _dynamicLearningRate;
        private float _learningRateAdjustmentFactor = 0.1f;
        private double _previousError;

        private readonly List<float> _losses = new();

        public BatchTrainer(float learningRate)
        {
            _dynamicLearningRate = learningRate;
        }

        public void Train(TrainingParameters trainingParameters)
        {
            lock (trainingParameters.ReplayMemory)
            {
                List<Replay> replays = trainingParameters.ReplayMemory.GetBatch(trainingParameters.BatchSize, false, 3);

                if (replays.Count > 0)
                {
                    lock (trainingParameters.NeuralNetwork)
                    {
                        lock (trainingParameters.TargetNeuralNetwork)
                        {
                            TrainOnBatch2(trainingParameters, replays);

                            /*Process currentProcess = Process.GetCurrentProcess();
                            long memoryUsageBytes = currentProcess.WorkingSet64;
                            double memoryUsageMB = memoryUsageBytes / (1024.0 * 1024.0);
                            Console.WriteLine($"Memory usage: {memoryUsageMB:F2} MB");*/

                            //var test = ops.get_default_session();
                            //var test2 = keras.backend.get_graph();

                            //GC.Collect();
                            //tf.reset_default_graph();
                            //keras.backend.clear_session();
                        }
                    }
                }
            }
        }

        public void PlotLoss(string filePath)
        {
            Plot myPlot = new();

            float[] dataX = Enumerable.Range(1, _losses.Count).Select(x => (float)x).ToArray();
            float[] dataY = _losses.ToArray();
            myPlot.Add.Scatter(dataX, dataY);

            myPlot.SavePng(filePath, 400, 300);
        }

        public void TrainTest(TrainingParameters trainingParameters)
        {
            lock (trainingParameters.ReplayMemory)
            {
                List<Replay> replays = trainingParameters.ReplayMemory.GetRandomBatch(trainingParameters.BatchSize, trainingParameters.ReplayMemory.Size, false);

                lock (trainingParameters.NeuralNetwork)
                {
                    lock (trainingParameters.TargetNeuralNetwork)
                    {
                        List<float>[] states = replays
                            .Select(replay => replay.State)
                            .ToArray();

                        float[,] statesMultiArray = ConvertToMultiArray(states);

                        Tensors statesTensors = tf.constant(statesMultiArray);

                        List<float>[] nextStates = replays
                            .Select(replay => replay.NextState)
                            .ToArray();

                        float[,] nextStatesMultiArray = ConvertToMultiArray(nextStates);

                        Tensors nextStatesTensors = tf.constant(nextStatesMultiArray);

                        List<float>[] actions = replays
                            .Select(replay => replay.Actions)
                            .ToArray();

                        float[,] actionsMultiArray = ConvertToMultiArray(actions);

                        Tensors actionsTensors = tf.constant(actionsMultiArray);

                        Tensors evaluatedQValues = trainingParameters.NeuralNetwork.Apply(statesTensors);
                        Tensors nextQValues = trainingParameters.TargetNeuralNetwork.Apply(nextStatesTensors);

                        NDArray evaluatedQValuesArray = evaluatedQValues.numpy();

                        for (int i = 0; i < replays.Count; i++)
                        {
                            (var maxAction, var maxActionIndex) = replays[i].Actions.MaxAndIndex();

                            float maxNextQValue = np.amax(nextQValues.numpy()[i]);

                            evaluatedQValuesArray[i, maxActionIndex] = replays[i].Reward + trainingParameters.DiscountFactor * maxNextQValue;
                        }

                        trainingParameters.NeuralNetwork.fit(statesTensors.numpy(), evaluatedQValuesArray, batch_size: replays.Count, epochs: 1, verbose: 2, workers: 500, use_multiprocessing: true);
                    }
                }
            }
        }

        private void TrainOnBatch2(TrainingParameters trainingParameters, List<Replay> replays, bool updatePriorityOnly = false)
        {
            // Convert replay states to Tensors
            List<float>[] states = replays
                .Select(replay => replay.State)
                .ToArray();

            float[,] statesMultiArray = ConvertToMultiArray(states);

            Tensors statesTensors = tf.constant(statesMultiArray);

            List<float>[] nextStates = replays
                .Select(replay => replay.NextState)
                .ToArray();

            float[,] nextStatesMultiArray = ConvertToMultiArray(nextStates);

            Tensors nextStatesTensors = tf.constant(nextStatesMultiArray);

            List<float>[] actions = replays
                .Select(replay => replay.Actions)
                .ToArray();

            float[,] actionsMultiArray = ConvertToMultiArray(actions);

            Tensors actionsTensors = tf.constant(actionsMultiArray);

            Tensors evaluatedQValues = trainingParameters.NeuralNetwork.Apply(statesTensors);
            Tensors nextQValues = trainingParameters.TargetNeuralNetwork.Apply(nextStatesTensors);

            //Tensors evaluatedQValues = trainingParameters.NeuralNetwork.predict(statesTensors, use_multiprocessing: true, workers: 500, verbose: 0);
            //Tensors nextQValues = trainingParameters.TargetNeuralNetwork.predict(nextStatesTensors, use_multiprocessing: true, workers: 500, verbose: 0);

            //Tensors targetQValues = new(evaluatedQValues);

            NDArray evaluatedQValuesArray = evaluatedQValues.numpy();
            //var targetQValuesArray = targetQValues.numpy();

            for (int i = 0; i < replays.Count; i++)
            {
                (var maxAction, var maxActionIndex) = replays[i].Actions.MaxAndIndex();

                float maxNextQValue = np.amax(nextQValues.numpy()[i]);

                evaluatedQValuesArray[i, maxActionIndex] = replays[i].Reward + trainingParameters.DiscountFactor * maxNextQValue;
            }

            ICallback history = trainingParameters.NeuralNetwork.fit(statesTensors.numpy(), evaluatedQValuesArray, batch_size: replays.Count, epochs: 1, verbose: 2, workers: 500, use_multiprocessing: true);
            
            float loss = history.history["loss"][0];
            _losses.Add(loss);
            //trainingParameters.NeuralNetwork.Apply(statesTensors.numpy(), evaluatedQValues.numpy(), true);

            /*statesTensors.Dispose();
            nextStatesTensors.Dispose();
            actionsTensors.Dispose();
            evaluatedQValues.Dispose();
            nextQValues.Dispose();
            actionsTensorsArray.Dispose();
            GC.Collect();*/
        }

        private void TrainOnBatch(TrainingParameters trainingParameters, List<Replay> replays, bool updatePriorityOnly = false)
        {
            // Convert replay states to Tensors
            List<float>[] states = replays
                .Select(replay => replay.State)
                .ToArray();

            float[,] statesMultiArray = ConvertToMultiArray(states);

            NDArray statesArray = new(statesMultiArray);
            Tensors statesTensors = new(statesArray);

            List<float>[] nextStates = replays
                .Select(replay => replay.NextState)
                .ToArray();

            float[,] nextStatesMultiArray = ConvertToMultiArray(nextStates);

            NDArray nextStatesArray = new(nextStatesMultiArray);
            Tensors nextStatesTensors = new(nextStatesArray);

            Tensors targetQValues = trainingParameters.NeuralNetwork.predict(statesTensors, use_multiprocessing: true, workers: 500);
            Tensors nextQValues = trainingParameters.TargetNeuralNetwork.predict(nextStatesTensors, use_multiprocessing: true, workers: 500);

            for (int i = 0; i < trainingParameters.BatchSize; i++)
            {
                if (i >= replays.Count)
                    break;

                float maxQValue = tf.reduce_max(nextQValues.numpy()[i]).numpy();
                //float maxQValue = np.argmax(nextQValues.numpy()[i])[0];

                float target = BellmanEquationHelper.MaxQValue(replays[i].Reward, trainingParameters.DiscountFactor, maxQValue) * 100;

                //float target = (float)(replays[i].Reward + trainingParameters.DiscountFactor * maxQValue);

                (var maxAction, var maxActionIndex) = replays[i].Actions.MaxAndIndex();

                float targetError = BellmanEquationHelper.BellmanEquation(trainingParameters.BellmanLearningRate, maxAction, target);

                targetQValues.numpy()[i, maxActionIndex] = target;
            }

            /*// Define the shape of the tensors
            int batchSize = 1;
            int inputSize = 25;
            int outputSize = 4;

            // Generate random data for statesTensors
            var randomStatesData = np.random.random(new Shape(batchSize, inputSize)).astype(np.float32);
            var statesTensorsTEST = new Tensors(randomStatesData);

            // Generate random data for targetQValues
            var randomTargetQValuesData = np.random.random(new Shape(batchSize, outputSize)).astype(np.float32);
            var targetQValuesTEST = new Tensors(randomTargetQValuesData);*/

            trainingParameters.NeuralNetwork.fit(statesTensors.numpy(), targetQValues.numpy(), batch_size: replays.Count, epochs: 1, verbose: 0, workers: 12, use_multiprocessing: true);

            /*List<double> targetOutputs = new(new double[replays[0].Actions.Count]);
            double totalReplaysError = 0;

            for (int i = 0; i < replays.Count; i++)
            {
                List<double> outputPolicy = new(trainingParameters.NeuralNetwork.FeedForward(replays[i].State.ToArray()));

                List<double> outputTarget = new(trainingParameters.TargetNeuralNetwork.FeedForward(replays[i].NextState.ToArray()));

                List<double> outputErrors = new(new double[outputPolicy.Count]);

                double maxTargetAction = outputTarget.Max();

                double maxQValue = BellmanEquationHelper.MaxQValue(replays[i].Reward, trainingParameters.DiscountFactor, maxTargetAction);

                (double maxReplayValue, int maxReplayIndex) = replays[i].Actions.MaxAndIndex();

                double tdError = Math.Abs(maxQValue - outputErrors[maxReplayIndex]);

                for (int errorIndex = 0; errorIndex < outputPolicy.Count; errorIndex++)
                {
                    // Update only the max Q value
                    if (errorIndex == maxReplayIndex)
                    {
                        outputErrors[errorIndex] = BellmanEquationHelper.BellmanEquation(trainingParameters.BellmanLearningRate, maxReplayValue, maxQValue);

                        totalReplaysError += tdError; //Math.Abs(maxQValue - outputErrors[errorIndex]);

                        targetOutputs[errorIndex] = outputErrors[errorIndex];
                    }
                    else
                    {
                        targetOutputs[errorIndex] = outputPolicy[errorIndex];
                    }
                }

                trainingParameters.ReplayMemory.UpdatePriority(replays[i], tdError);

                if (!updatePriorityOnly)
                    trainingParameters.NeuralNetwork.BackPropagate(targetOutputs, trainingParameters.NeuralNetworkLearningRate);
            }

            // Total errors calculation
            if (!updatePriorityOnly)
            {
                totalReplaysError /= replays.Count;

                // Dynamically adjust learning rate
                if (totalReplaysError < _previousError)
                    _dynamicLearningRate *= 1.0f + _learningRateAdjustmentFactor;
                else
                    _dynamicLearningRate *= 1.0f - _learningRateAdjustmentFactor;

                _dynamicLearningRate = Math.Clamp(_dynamicLearningRate, 0.0001f, 0.1f);

                _previousError = totalReplaysError;

                trainingParameters.ErrorCalculator.AddError(totalReplaysError);
            }*/
        }

        private static float[,] ConvertToMultiArray(List<float>[] multiList)
        {
            int numRows = multiList.Length;
            int numCols = multiList[0].Count;

            float[,] result = new float[numRows, numCols];

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    result[i, j] = multiList[i][j];
                }
            }

            return result;
        }
    }
}
