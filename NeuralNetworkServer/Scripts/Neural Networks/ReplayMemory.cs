using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace NeuralNetworkServer.NeuralNetworks
{
    public class Replay
    {
        public required List<float> State { get; set; }
        public required List<float> Actions { get; set; }
        public required List<float> NextState { get; set; }
        public float Reward { get; set; }
        public double Priority { get; set; }
        public int Index { get; set; }
    }

    public class ReplayMemory
    {
        public int MaxSize { get; set; }
        public int Size { get; set; }
        public List<Replay> Replays { get; set; }

        private readonly int _neuralNetworkInputCount;
        private readonly int _neuralNetworkOutputCount;

        public ReplayMemory(int maxSize, int neuralNetworkInputCount, int neuralNetworkOutputCount)
        {
            MaxSize = maxSize;
            Size = 0;
            Replays = new List<Replay>(maxSize);

            _neuralNetworkInputCount = neuralNetworkInputCount;
            _neuralNetworkOutputCount = neuralNetworkOutputCount;
        }

        public void AddReplay(List<float> state, List<float> actions, float reward, List<float> nextState, double priority)
        {
            lock (this)
            {
                OrderReplaysByPriority();

                if (Size >= MaxSize - 1)
                {
                    Replays.RemoveAt(Replays.Count - 1);
                }
                else
                {
                    Size++;
                }

                Replay replay = new()
                {
                    State = new List<float>(state),
                    Actions = new List<float>(actions),
                    Reward = reward,
                    NextState = new List<float>(nextState),
                    Priority = priority,
                    Index = Size - 1
                };

                Replays.Add(replay);

                OrderReplaysByPriority();

                UpdateIndexes();
            }
        }

        public List<Replay> GetReplaysWithPriority(double priority)
        {
            lock (this)
            {
                return Replays.FindAll(replay => replay.Priority == priority);
            }
        }

        public List<Replay> GetBatch(int size, bool updatePriority = true, float skewFactor = 1f)
        {
            List<Replay> replays;
            Random random = new();

            lock (this)
            {
                size = Math.Min(Size, size);

                if (updatePriority)
                    OrderReplaysByPriority();

                UpdateIndexes();

                // Make sure random pick size is more skewed towards the batch size
                int randomPickSize = (int)Math.Floor(Math.Pow(random.NextDouble(), skewFactor) * (Size + 1 - size) + size);

                replays = GetRandomBatch(size, randomPickSize, updatePriority);
            }

            return replays;
        }

        public List<Replay> GetRandomBatch(int size, int pickSize, bool updatePriority)
        {
            size = Math.Min(Size, size);

            List<Replay> replays = new();
            Random random = new();

            lock (this)
            {
                for (int i = 0; i < size; i++)
                {
                    int randomIndex = random.Next(0, pickSize);
                    Replay randomReplay = Replays[randomIndex];

                    if (updatePriority)
                    {
                        randomReplay.Priority = Math.Abs(randomReplay.Reward);

                        // Make sure that positive rewards have higher priority over negative rewards
                        if (randomReplay.Reward < 0)
                            randomReplay.Priority -= 0.1f;
                    }

                    replays.Add(randomReplay);
                }
            }

            return replays;
        }

        public void OrderReplaysByPriority()
        {
            lock(this)
            {
                Replays = Replays.OrderByDescending(r => r.Priority).ToList();
            }
        }

        public void UpdatePriorityAt(int index, double priority)
        {
            lock (this)
            {
                Replays[index].Priority = priority;
            }
        }

        public void UpdatePriority(Replay replay, double priority)
        {
            lock (this)
            {
                int index = Replays.IndexOf(replay);

                if (index == -1)
                    throw new NullReferenceException("Cannot update replay priority: The replay does not match with any replay from the Replays list");

                Replays[index].Priority = priority;
            }
        }

        public void UpdateIndexes()
        {
            lock (this)
            {
                for (int i = 0; i < Replays.Count; i++)
                {
                    Replays[i].Index = i;
                }
            }
        }

        public void Save(string filePath)
        {
            lock (this)
            {
                try
                {
                    SaveInternal(filePath);
                }
                catch (Exception exception)
                {
                    Console.WriteLine($"Cannot save the Replay Memory: {exception}");
                }
            }
        }

        public void Load(string filePath)
        {
            lock (this)
            {
                try
                {
                    LoadInternal(filePath);
                }
                catch (Exception exception)
                {
                    throw new Exception($"Cannot load the Replay Memory: {exception}");
                }
            }
        }

        private void SaveInternal(string filePath)
        {
            using StreamWriter sw = new(@filePath);

            lock (this)
            {
                OrderReplaysByPriority();

                UpdateIndexes();

                sw.WriteLine(Size.ToString(CultureInfo.InvariantCulture));

                List<Replay> replaysCopy = new(Replays);

                foreach (Replay replay in replaysCopy)
                {
                    sw.WriteLine(replay.Index.ToString(CultureInfo.InvariantCulture));
                    sw.WriteLine(replay.Priority.ToString(CultureInfo.InvariantCulture));

                    foreach (var state in replay.State)
                    {
                        sw.WriteLine(state.ToString(CultureInfo.InvariantCulture));
                    }

                    foreach (var action in replay.Actions)
                    {
                        sw.WriteLine(action.ToString(CultureInfo.InvariantCulture));
                    }

                    sw.WriteLine(replay.Reward.ToString(CultureInfo.InvariantCulture));

                    foreach (var nextState in replay.NextState)
                    {
                        sw.WriteLine(nextState.ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
        }

        private void LoadInternal(string filePath)
        {
            StreamReader? sr = null;
            bool isFileLoaded = false;

            lock (this)
            {
                Replays.Clear();

                while (!isFileLoaded)
                {
                    try
                    {
                        sr = new(@filePath);

                        isFileLoaded = true;
                    }
                    catch (IOException) { }
                }

                Size = int.Parse(sr.ReadLine(), CultureInfo.InvariantCulture);

                for (int replayIndex = 0; replayIndex < Size; replayIndex++)
                {
                    var replay = new Replay
                    {
                        State = new List<float>(),
                        Actions = new List<float>(),
                        NextState = new List<float>(),
                        Index = int.Parse(sr.ReadLine(), CultureInfo.InvariantCulture),
                        Priority = double.Parse(sr.ReadLine(), CultureInfo.InvariantCulture)
                    };

                    for (int stateIndex = 0; stateIndex < _neuralNetworkInputCount; stateIndex++)
                    {
                        replay.State.Add(float.Parse(sr.ReadLine(), CultureInfo.InvariantCulture));
                    }

                    for (int actionIndex = 0; actionIndex < _neuralNetworkOutputCount; actionIndex++)
                    {
                        replay.Actions.Add(float.Parse(sr.ReadLine(), CultureInfo.InvariantCulture));
                    }

                    replay.Reward = float.Parse(sr.ReadLine(), CultureInfo.InvariantCulture);

                    for (int nextStateIndex = 0; nextStateIndex < _neuralNetworkInputCount; nextStateIndex++)
                    {
                        replay.NextState.Add(float.Parse(sr.ReadLine(), CultureInfo.InvariantCulture));
                    }

                    Replays.Add(replay);
                }

                OrderReplaysByPriority();

                UpdateIndexes();
            }
        }
    }
}
