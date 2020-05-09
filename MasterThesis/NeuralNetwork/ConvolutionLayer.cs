using NeuroevolutionApplication.NN;
using System.Collections;
using System.Collections.Generic;

namespace NeuroevolutionApplication.NN
{
    class ConvolutionLayer : LayerBase
    {
        public double[,] filter;
        public double[,] input;
        public double[,] output;

        public ConvolutionLayer(double[,] filter)
        {
            layerType = LAYER_TYPE.CONV;
            this.filter = filter;
        }

        public KeyValuePair<double[,], double[,]> Process(double[,] inputImage, double[,] prevConvErrors, double learningRate)
        {
            Helper helper = new Helper();

            //---calculate image errors
            double[,] imgErrors = helper.Convolution(prevConvErrors, filter);

            //---calculate filter weight errors
            double[ , ] filterErrors = new double[filter.Length, filter.GetLength(1)];
            for (int nrWeightY = 0; nrWeightY < filterErrors.GetLength(1) ; ++nrWeightY)
            {
                for (int nrWeightX = 0; nrWeightX < filterErrors.GetLength(0); ++nrWeightX)
                {
                    filterErrors[nrWeightY, nrWeightX] = 0;
                    for (int a = 0; a < prevConvErrors.GetLength(0); ++a)
                    {
                        for (int b = 0; b < prevConvErrors.GetLength(1); ++b)
                        {
                            filterErrors[nrWeightY, nrWeightX] += inputImage[nrWeightY + a, nrWeightX + b] * prevConvErrors[a, b];
                        }
                    }
                }
            }

            return new KeyValuePair<double[,], double[,]>(imgErrors, filterErrors);
        }


        public void SetInput(double[,] input) { this.input = input; }
        public void SetOutput(double[,] output) { this.output = output; }
    }
}
