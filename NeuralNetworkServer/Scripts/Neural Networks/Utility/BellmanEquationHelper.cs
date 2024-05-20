namespace NeuralNetworkServer.NeuralNetworks
{
    public static class BellmanEquationHelper
    {
        public static float BellmanEquation(float learningRate, float policyQValue, float maxQValue)
        {
            return policyQValue + learningRate * (maxQValue - policyQValue);
        }

        public static float MaxQValue(float reward, float discountFactor, float targetValue)
        {
            return reward + discountFactor * targetValue;
        }
    }
}
