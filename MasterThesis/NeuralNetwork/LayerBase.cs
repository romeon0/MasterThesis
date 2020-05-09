using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroevolutionApplication.NN
{
    public class LayerBase 
    {
        public enum LAYER_TYPE
        {
            FCL,
            CONV,
            POOL,
            INPUT,
            FLATTENING,
            NONE
        }

        protected LAYER_TYPE layerType = LAYER_TYPE.NONE;

        public new LAYER_TYPE GetType()
        {
            return layerType;
        }
    }
}
