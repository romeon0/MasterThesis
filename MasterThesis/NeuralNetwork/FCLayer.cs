using NeuroevolutionApplication.NN;
using System.Collections;
using System.Collections.Generic;

namespace NeuroevolutionApplication.NN
{
    class FCLayer : LayerBase
    {
        public List<Neuron> neurons;

        public delegate double FuncDelegate(double value);
        private FuncDelegate activateFunction;
        private FuncDelegate deactivateFunction;



        public FCLayer(List<Neuron> neurons, FuncDelegate activationFunction, FuncDelegate deactivationFunction)
        {
            layerType = LAYER_TYPE.FCL;
            this.neurons = neurons;
            this.activateFunction = activationFunction;
            this.deactivateFunction = deactivationFunction;
        }


        public void InitFrom(FCLayer other)
        {
            activateFunction = other.activateFunction;
            deactivateFunction = other.deactivateFunction;
        }

        public double Activate(double value)
        {
            return activateFunction(value);
        }

        public double Deactivate(double value)
        {
            return activateFunction(value);
        }
    }
}
