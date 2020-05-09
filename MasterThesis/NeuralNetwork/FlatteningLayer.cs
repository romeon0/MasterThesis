using NeuroevolutionApplication.NN;
using System.Collections;
using System.Collections.Generic;

namespace NeuroevolutionApplication.NN
{
    class FlatteningLayer : LayerBase
    {
        public int originalWidth;
        public int originalHeight;

        public FlatteningLayer()
        {
            layerType = LAYER_TYPE.FLATTENING;
        }
    }
}
