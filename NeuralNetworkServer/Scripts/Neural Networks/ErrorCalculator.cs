using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Media.Animation;

namespace NeuralNetworkServer.NeuralNetworks
{
    public class ErrorCalculator
    {
        private readonly int _maxErrors;
        private readonly double[] _totalErrors;

        private int _totalErrorsIndex;

        public ErrorCalculator(int maxErrors)
        {
            _maxErrors = maxErrors;

            _totalErrors = new double[maxErrors];
            _totalErrorsIndex = 0;
        }

        public void AddError(double error)
        {
            if (_totalErrorsIndex < _maxErrors)
            {
                _totalErrors[_totalErrorsIndex] = error;
            }
            else
            {
                _totalErrorsIndex = 0;
                _totalErrors[_totalErrorsIndex] = error;
            }

            if (_totalErrorsIndex < _maxErrors - 1)
            {
                _totalErrorsIndex++;
            }
        }

        public double? PrintAverageError()
        {
            double? averageError = GetAverageError();

            Console.WriteLine("Total Error: " + averageError);

            return averageError;
        }

        public void SaveAverageError(string filePath)
        {
            lock (this)
            {
                try
                {
                    SaveInternal(filePath);
                }
                catch (Exception exception)
                {
                    throw new Exception($"Cannot save the average error: {exception}");
                }
            }
        }

        private void SaveInternal(string filePath)
        {
            lock (this)
            {
                double? averageError = GetAverageError();

                if (averageError == null)
                    return;

                if (File.Exists(filePath))
                {
                    string[] lines = File.ReadAllLines(filePath);

                    File.WriteAllLines(filePath, lines);
                }

                File.AppendAllText(filePath, averageError.Value.ToString(CultureInfo.InvariantCulture));
            }
        }

        private double? GetAverageError()
        {
            if (_totalErrors.Length <= 0)
                return null;

            double totalError = 0;
            double totalErrorAverage = 0;
            int totalErrorSize = 0;

            for (int i = 0; i < _maxErrors; i++)
            {
                if (_totalErrors[i] != 0)
                {
                    totalError += _totalErrors[i];
                    totalErrorSize++;
                }
            }

            if (totalErrorSize > 0)
            {
                totalErrorAverage = totalError / totalErrorSize;
            }

            // Only keep two decimal places
            totalErrorAverage = Math.Floor(totalErrorAverage * 100) / 100;

            return totalErrorAverage;
        }
    }
}
