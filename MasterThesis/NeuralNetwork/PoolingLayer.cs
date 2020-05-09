using NeuroevolutionApplication.NN;
using System.Collections;
using System.Collections.Generic;

namespace NeuroevolutionApplication.NN
{
    class PoolingLayer : LayerBase
    {
        public int windowSize;
        internal double[,] input;
        internal int[] sparseMatrix;

        public PoolingLayer(int windowSize)
        {
            layerType = LAYER_TYPE.POOL;
            this.windowSize = windowSize;
        }
    }
}
