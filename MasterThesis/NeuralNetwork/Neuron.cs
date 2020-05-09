using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroevolutionApplication.NN
{
    public class Neuron
    {
        public double[] input;
        public double output;
        public double[] weights;

        public Neuron(double[] weights)
        {
            this.weights = weights;
        }

        public Neuron(int nrWeights)
        {
            if (nrWeights < 0) new Exception("NrWeights < 0!");

            weights = new double[nrWeights];
            Random rand = new Random();
            for (int i = 0; i < nrWeights; ++i)
            {
                weights[i] = rand.NextDouble();
            }
        }

        public void SetOutput(double output)
        {
            this.output = output;
        }

        public void SetInput(double[] input)
        {
            this.input = input;
        }

    }
}
