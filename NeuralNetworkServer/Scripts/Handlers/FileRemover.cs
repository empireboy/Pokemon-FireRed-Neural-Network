using System;
using System.Collections.Generic;
using System.IO;

namespace NeuralNetworkServer.Handlers
{
    internal class FileRemover
    {
        private readonly List<string> _fileNamesToDelete;

        public FileRemover()
        {
            _fileNamesToDelete = new List<string>
            {
                QLearningContext.NeuralNetworkFileName,
                QLearningContext.ReplayMemoryFileName,
                QLearningContext.TotalErrorFileName,
                QLearningContext.TotalCorrectFileName
            };
        }

        public bool RemoveAllFiles()
        {
            bool success = true;

            foreach (string fileName in _fileNamesToDelete)
            {
                string filePath = Path.Combine(QLearningContext.RootFolder, fileName);

                try
                {
                    if (File.Exists(filePath))
                        File.Delete(filePath);
                }
                catch (Exception exception)
                {
                    success = false;

                    Console.WriteLine($"Cannot delete file '{fileName}': {exception.Message}");
                }
            }

            return success;
        }
    }
}
