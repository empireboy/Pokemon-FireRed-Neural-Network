using System;
using System.Collections.Generic;
using System.Linq;

namespace NeuralNetworkServer.Utility
{
    public static class ListExtension
    {
        public static Tuple<double, int> MaxAndIndex(this List<double> list)
        {
            double maxValue = list.Max();
            int maxValueIndex = list.IndexOf(maxValue);

            return new Tuple<double, int>(maxValue, maxValueIndex);
        }

        public static Tuple<float, int> MaxAndIndex(this List<float> list)
        {
            float maxValue = list.Max();
            int maxValueIndex = list.IndexOf(maxValue);

            return new Tuple<float, int>(maxValue, maxValueIndex);
        }
    }
}
