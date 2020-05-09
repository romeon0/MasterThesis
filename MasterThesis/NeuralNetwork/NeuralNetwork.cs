/*
 Author: Romeon0, 2019 
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using NeuroevolutionApplication.NN;

namespace NeuroevolutionApplication.NN
{
    [Serializable]
    public class NeuralNetwork
    {
        public int maxEpochs = 1;
        public double learningRate = 0.8;
        public List<LayerBase> layers = new List<LayerBase>();

        private bool initialized = false;
        private Helper helper = new Helper();
        private bool debugActive = false;
        private double maxError = 0.08;
        private Thread lastThread;

        public static class ActivationFunction
        {
            //INFO: Activate neuron using Sigmoid function
            public static double Sigmoid(double value)
            {
                return 1 / (1 + Math.Pow(Math.E, -value));
            }

            //INFO: Activate neuron using Hyperbolic Tangent function
            public static double Tanh(double value)
            {
                return Math.Tanh(value);
            }

            //INFO: Activate neuron using Gaussian function
            public static double Gaussian(double value)
            {
                return Math.Pow(Math.E, -(value * value));
            }

            //INFO: Activate neuron using ElliotSig / Softsign function
            public static double Softsign(double value)
            {
                return value / (1 + Math.Abs(value));
            }
        }

        public static class DeactivationFunction
        {
            //INFO: Deactivate neuron using Sigmoid function
            public static double Sigmoid(double value)
            {
                return value * (1 - value);
            }

            //INFO: Deactivate neuron using Hyperbolic Tangent function
            public static double Tanh(double value)
            {
                return (1 - value * value);
            }

            //INFO: Deactivate neuron using Gaussian function
            public static double Gaussian(double value)
            {
                return -2 * value * Math.Pow(Math.E, -value * value);
            }

            //INFO: Deactivate neuron using ElliotSig / Softsign function
            public static double Softsign(double value)
            {
                return 1 / Math.Pow((1 + Math.Abs(value)), 2);
            }
        }

        private void Debug(string msg, params object[] objs)
        {
            if (debugActive)
                Console.WriteLine(msg, objs);
        }


        //INFO: Forward propagation process. 
        private double[] ForwardPropagate(double[,] image)
        {
            Debug("---Forward Propagate---");

            //input layer
            Helper helper = new Helper();
            double[,] img = image;
            double[] values = null;
            double[] output = null;

            //other layers
            for (int nrLayer = 0; nrLayer < layers.Count; ++nrLayer)
            {
                LayerBase layer = layers.ElementAt(nrLayer);
                LayerBase.LAYER_TYPE layerType = layer.GetType();

                if (layerType == LayerBase.LAYER_TYPE.POOL) //100% WORKING!!!
                {
                    Debug("Forward Propagate: Pool layer.");
                    PoolingLayer realLayer = (PoolingLayer)layer;
                    realLayer.input = img;
                    KeyValuePair<double[,], int[]> result = helper.Pooling_Max(img, 2);
                    img = result.Key;
                    realLayer.sparseMatrix = result.Value;
                }
                else if (layerType == LayerBase.LAYER_TYPE.CONV) //100% WORKING!!!
                {
                    Debug("Forward Propagate: Convolution layer.");
                    ConvolutionLayer realLayer = (ConvolutionLayer)layer;
                    realLayer.input = img;
                    img = helper.CrossCorrelation(img, realLayer.filter);
                }
                else if (layerType == LayerBase.LAYER_TYPE.FLATTENING) //100% WORKING!!!
                {
                    Debug("Forward Propagate: Flattening layer.");
                    FlatteningLayer realLayer = (FlatteningLayer)layer;
                    realLayer.originalHeight = img.GetLength(0);
                    realLayer.originalWidth = img.GetLength(1);
                    values = helper.MatrixToVector(img);
                }
                else if (layerType == LayerBase.LAYER_TYPE.FCL) //100% WORKING!!!
                {
                    for (int nrFCLayer = nrLayer; nrFCLayer < layers.Count; ++nrFCLayer)
                    {
                        Debug("Forward Propagate: Full connected layer " + nrFCLayer);
                        FCLayer realLayer = (FCLayer)layer;
                        List<Neuron> neurons = (realLayer).neurons; 
                        output = new double[neurons.Count];
                        double value;
                        //Process neuron
                        for (int nrNeuron = 0; nrNeuron < neurons.Count; ++nrNeuron)
                        {
                            Neuron neuron = neurons[nrNeuron];
                            neuron.SetInput(values);

                            value = helper.Sum(values, neuron.weights) / neuron.weights.Length; 
                            value = realLayer.Activate(value);
                            
                            output[nrNeuron] = value;
                            neuron.SetOutput(value);
                        }

                        //Move to next layer
                        ++nrLayer;
                        if (nrLayer < layers.Count)
                        {
                            layer = layers.ElementAt(nrLayer);
                        }

                        values = output;
                    }

                    break;//processed all layers, forward propagation finished
                }
            }

            return output;
        }


        private KeyValuePair<double[], double[,]> BackpropagateFCLayer(int nrLayer, double[] prevErrors, double[] correctAnswer)
        {
            FCLayer layer = (FCLayer)layers.ElementAt(nrLayer);
            int nrNeurons = layer.neurons.Count;
            FCLayer prevLayer = null;
            int nrWeights = 0;
            if (nrLayer != layers.Count - 1)
            {
                prevLayer = ((FCLayer)layers.ElementAt(nrLayer + 1));
            }
            nrWeights = layer.neurons[0].weights.Length;
            double[] errors = new double[nrNeurons];
            double[,] weightErrors = new double[nrNeurons, nrWeights];
            double netRatio, outRatio, weightRatio;

            //INFO: calculate weight and neuron errors input<---hidden<---...<--hiddenN
            for (int nrNeuron = 0; nrNeuron < nrNeurons; ++nrNeuron)
            {
                Neuron currNeuron = layer.neurons.ElementAt(nrNeuron);
                double exitValue = currNeuron.output;
                //outRatio: sum all previous errors weighted with necessary weight
                outRatio = 0;
                //INFO: If nrWeights==0 then this layer output layer, if not - hidden or input
                if (prevLayer != null)
                {
                    int nrPrevNeurons = prevLayer.neurons.Count;
                    for (int nrPrevNeuron = 0; nrPrevNeuron < nrPrevNeurons; ++nrPrevNeuron)
                    {
                        double weight = prevLayer.neurons.ElementAt(nrPrevNeuron).weights[nrNeuron];
                        outRatio += prevErrors[nrPrevNeuron] * weight;
                    }
                }
                else
                {
                    double actualOutput = prevErrors[nrNeuron];
                    double desiredOutput = correctAnswer[nrNeuron];
                    outRatio = (actualOutput - desiredOutput);
                }
                //INFO: net ratio
                netRatio = layer.Deactivate(outRatio);

                //INFO: neuron errors
                errors[nrNeuron] = outRatio * netRatio;
                // Debug("Neuron {0}. OutError: {1}; NetError: {2}; NeuronErr: {3}; ExitValue: {4}", nrNeuron, outRatio, netRatio, errors[nrNeuron], exitValue);

                //INFO: weight errors
                if (nrWeights != 0)
                {
                    for (int nrWeight = 0; nrWeight < nrWeights; ++nrWeight)
                    {
                        weightRatio = currNeuron.input[nrWeight];
                        double weightImpact = errors[nrNeuron] * weightRatio;
                        weightErrors[nrNeuron, nrWeight] = weightImpact;
                    }
                }
            }

            return new KeyValuePair<double[], double[,]>(errors, weightErrors);
        }



        private KeyValuePair<double[,], double[,]> BackpropagateConvolutionLayer(int nrLayer, double[,] prevConvErrors)
        {
            Helper h = new Helper();
            ConvolutionLayer layer = (ConvolutionLayer)layers.ElementAt(nrLayer);
            double[,] filter = layer.filter;
            double[,] inputImg = layer.input;
            double[,] outputImg = layer.output;

            int srcImgHeight = filter.GetLength(0);
            int srcImgWidth = filter.GetLength(1);

            //---calculate image errors
            double[,] imgErrors = h.CrossCorrelation(prevConvErrors, filter);



            //---calculate filter weight errors
            int filterSize = filter.GetLength(0);
            double[,] filterErrors = new double[filterSize, filterSize];
            for (int nrWeightY = 0; nrWeightY < filterSize; ++nrWeightY)
            {
                for (int nrWeightX = 0; nrWeightX < filterSize; ++nrWeightX)
                {
                    filterErrors[nrWeightY, nrWeightX] = 0;
                    for (int a = 0; a < prevConvErrors.GetLength(0); ++a)
                    {
                        for (int b = 0; b < prevConvErrors.GetLength(1); ++b)
                        {
                            if ((nrWeightY + a > -1) && (nrWeightX + b > -1) && (nrWeightY + a < srcImgHeight) && (nrWeightX + b < srcImgWidth))
                            {
                                filterErrors[nrWeightY, nrWeightX] += filter[nrWeightY + a, nrWeightX + b] * prevConvErrors[a, b];
                            }
                        }
                    }
                }
            }


            return new KeyValuePair<double[,], double[,]>(imgErrors, filterErrors);
        }

        public double[,] BackpropPoolLayer(int nrLayer, double[,] errors)
        {
            Helper h = new Helper();
            PoolingLayer layer = (PoolingLayer)layers[nrLayer];
            int[] sparseMatrix = layer.sparseMatrix;
            double[,] poolingErrors = h.ReversePooling(errors, sparseMatrix, 2);
            return poolingErrors;
        }

        //INFO: Backpropagation process
        private void BackPropagate(double[] correctAnswer, double[] output)
        {
            Debug("@----Back Propagate----@");

            Helper helper = new Helper();
            LayerBase.LAYER_TYPE prevLayerType = LayerBase.LAYER_TYPE.NONE;
            double[] prevErrors = output; //used in FCL layers
            double[,] prevConvErrors = null;//used in Conv layers
            List<double[,]> weightsErrors = new List<double[,]>();//used in FCL layers
            List<double[,]> filterErrors = new List<double[,]>();//used in Conv layers

            for (int nrLayer = layers.Count - 1; nrLayer >= 0; --nrLayer)
            {
                LayerBase layer = layers[nrLayer];
                LayerBase.LAYER_TYPE layerType = layer.GetType();

                if (layerType == LayerBase.LAYER_TYPE.POOL) //100% WORKING!!!
                {
                    Debug("Back Propagate: Pool layer.");
                    prevConvErrors = BackpropPoolLayer(nrLayer, prevConvErrors);
                }
                else if (layerType == LayerBase.LAYER_TYPE.CONV)
                {
                    Debug("Back Propagate: Convolution layer.");
                    KeyValuePair<double[,], double[,]> result = BackpropagateConvolutionLayer(nrLayer, prevConvErrors);
                    prevConvErrors = result.Key;
                    filterErrors.Add(result.Value);

                    //for (int y = 0; y < 3; ++y)
                    //{
                    //    for (int x = 0; x < 3; ++x)
                    //    {
                    //        Console.WriteLine("FilterErrors[{0},{1}]: {2}", y, x, result.Value[y, x]);
                    //    }
                    //}
                }
                else if (layerType == LayerBase.LAYER_TYPE.FLATTENING)
                {
                    Debug("Back Propagate: Flattening layer.");

                    FCLayer prevLayer = (FCLayer)layers[nrLayer + 1];

                    FlatteningLayer flatLayer = (FlatteningLayer)layer;
                    // prevConvErrors = weightsErrors.ElementAt(weightsErrors.Count - 1);
                    prevConvErrors = helper.VectorToMatrix(prevLayer.neurons[0].input, flatLayer.originalHeight, flatLayer.originalWidth);
                    //Debug("PrevErrors size(flat): {0} x {1}", prevConvErrors.GetLength(0), prevConvErrors.GetLength(1));
                }
                else if (layerType == LayerBase.LAYER_TYPE.FCL && prevLayerType == LayerBase.LAYER_TYPE.NONE)//last FCL layer
                {
                    Debug("Back Propagate: Full connected layer.(last layer)");
                    KeyValuePair<double[], double[,]> result = BackpropagateFCLayer(nrLayer, prevErrors, correctAnswer);
                    prevErrors = result.Key;
                    weightsErrors.Add(result.Value);


                    //string s = "";
                    //foreach(double d in prevErrors)
                    //{
                    //    s += d + ", ";
                    //}
                    //Debug("PrevErrors: " + s);

                    //string s2 = "";
                    //foreach (double d_2 in weightsErrors.ElementAt(weightsErrors.Count-1))
                    //{
                    //    s2 += d_2 + "  ";
                    //    //s2 += "\n";
                    //}
                    //Debug("WeightErrors: " + s2);
                }
                else if (layerType == LayerBase.LAYER_TYPE.FCL)//FCL layer (hidden or input)
                {
                    Debug("Back Propagate: Full connected layer.");
                    KeyValuePair<double[], double[,]> result = BackpropagateFCLayer(nrLayer, prevErrors, correctAnswer);
                    prevErrors = result.Key;
                    weightsErrors.Add(result.Value);
                    //Debug("PrevErrors size: {0}", prevErrors.GetLength(0));
                }
                prevLayerType = layerType;
            }

            Debug("Back Propagate: Updating weights...");
            //---UPDATE WEIGHTS
            int fclIterator = 0, convIterator = 0;
            for (int nrLayer = layers.Count - 1; nrLayer >= 0; --nrLayer)
            {
                LayerBase layer = layers[nrLayer];
                LayerBase.LAYER_TYPE layerType = layer.GetType();

                //Debug("Layer: "+ nrLayer + "; Type:" + layerType);

                if (layerType == LayerBase.LAYER_TYPE.CONV)
                {
                    ConvolutionLayer realLayer = (ConvolutionLayer)layer;
                    double[,] filter = realLayer.filter;
                    double[,] fErrors = filterErrors.ElementAt(convIterator++);
                    for (int y = 0; y < 3; ++y)
                    {
                        for (int x = 0; x < 3; ++x)
                        {
                            //  Console.WriteLine("FilterErrors[{0},{1}]: {2}",y,x, fErrors[y, x]);
                            double currWeight = filter[y, x];
                            currWeight -= learningRate * fErrors[y, x];
                            filter[y, x] = currWeight;
                        }
                    }
                    //  realLayer.filter = filter;
                    //  Console.WriteLine("-----------");
                }
                else if (layerType == LayerBase.LAYER_TYPE.FCL)
                {
                    FCLayer realLayer = (FCLayer)layers.ElementAt(nrLayer);
                    int nrWeights = realLayer.neurons[0].weights.Length;
                    if (nrWeights != 0)//hidden or output layer that has weights
                    {
                        //  FCLayer prevLayer = (FCLayer)layers.get(nrLayer - 1);
                        int nrNeurons = realLayer.neurons.Count;

                        double[,] wErrors = weightsErrors.ElementAt(fclIterator++);
                        // Debug("Dimension: " + wErrors.GetLength(0) + " x " + wErrors.GetLength(1));
                        //Debug("Dimension 2: " + nrNeurons + " x " + nrWeights);
                        int counter = 0;
                        for (int nrNeuron = 0; nrNeuron < nrNeurons; ++nrNeuron)
                        {
                            double[] newWeights = new double[nrWeights];
                            for (int nrWeight = 0; nrWeight < nrWeights; ++nrWeight)
                            {
                                double currWeight = realLayer.neurons[nrNeuron].weights[nrWeight];
                                newWeights[nrWeight] = currWeight - learningRate * wErrors[nrNeuron, nrWeight];

                                if (counter++ < 4)
                                {
                                    //Debug("Weights. Old: {0}; New: {1}", currWeight, newWeights[nrWeight]);
                                }

                                //double momentum = 0.5;
                                //newWeights[nrWeight] = currWeight + learningRate * wErrors[nrNeuron][nrWeight] + momentum * currWeight;

                               ((FCLayer)layers.ElementAt(nrLayer)).neurons[nrNeuron].weights[nrWeight] = newWeights[nrWeight];
                                //realLayer.neurons[nrNeuron].weights[nrWeight] = newWeights[nrWeight];
                            }
                        }
                    }
                    else
                    {
                        // fclIterator++;
                    }//input FCL layer, no weights
                }
            }

            Debug("Back Propagate end.");
        }

        public LayerBase GetLayerByIndex(int layerIndex)
        {
            return layers[layerIndex];
        }


        //INFO: Train the network
        public double Train(List<double[,]> trainData, List<double[]> correctAnswers)
        {
            int startIndex = 0;
            int nrImages = startIndex + 50;// 799;

            if (trainData.Count < nrImages)
                nrImages = trainData.Count;

            double epochError = 0.0;
            double[] output;
            double[] correctAnswer;
            for (int nrEpoch = 0; nrEpoch < maxEpochs; ++nrEpoch)
            {
                //Debug("-----> Epoch " + nrEpoch);
                epochError = 0.0;
                for (int nrStep = 0; nrStep < nrImages; ++nrStep)
                {
                    double[,] image = trainData.ElementAt(nrStep);

                    //Forward propagation
                    output = ForwardPropagate(image);

                    //INFO: Creating correct answer
                    correctAnswer = correctAnswers.ElementAt(nrStep);

                    //Calculate Step error
                    double stepError = 0;
                    for (int b = 0; b < output.Length; ++b)
                    {
                        double desired = correctAnswer[b];
                        double actual = output[b];
                        //Console.WriteLine("Desired: {0}; Actual: {1}", desired, actual);
                        stepError += (0.5 * Math.Pow(desired - actual, 2));
                        //Console.WriteLine("Error: " + (0.5 * Math.Pow(desired - actual, 2)));
                    }
                    epochError += stepError;
                    //Console.WriteLine("Step "+ nrStep + " Error: " + stepError);

                    //Backpropagation
                    BackPropagate(correctAnswer, output);
                }

                //Epoch error
                epochError /= nrImages;
                Console.WriteLine("Epoch " + nrEpoch + " error: " + epochError + "\n----------");// + "\n---------------\n");

                if (epochError < maxError)
                {
                    break;
                }
            }

            //Console.WriteLine("Train finished.");

            //for (int b = 0; b < output.Length; ++b)
            //{
            //    Console.WriteLine(correctAnswer[b] + " --- " + output[b]);
            //}

            return epochError;
        }

        //INFO: Start Train the network
        public void StartTrainAsync(List<double[,]> trainData, List<double[]> correctAnswers)
        {
            if (!initialized)
            {
                MessageBox.Show("The network is not initialized!");
                return;
            }

            Stop();

            lastThread = new Thread(() => Train(trainData, correctAnswers));
            lastThread.IsBackground = true;
            lastThread.Start();
        }

        public double StartTrainSync(List<double[,]> trainData, List<double[]> correctAnswers)
        {
            if (!initialized)
            {
                MessageBox.Show("The network is not initialized!");
                return double.NegativeInfinity;
            }

            return Train(trainData, correctAnswers);
        }

        public List<KeyValuePair<double[], double[]>> Test(List<double[,]> trainData, List<double[]> correctAnswers)
        {
            int startIndex = 0; //800
            int endIndex = startIndex + 20;
            if (endIndex > trainData.Count)
                endIndex = trainData.Count;

            double[] output = null;
            double[] correctAnswer = null;

            List<KeyValuePair<double[], double[]>> list = new List<KeyValuePair<double[], double[]>>();

            for (int nrStep = startIndex; nrStep < endIndex; ++nrStep)
            {
                Debug("-> Step " + nrStep);

                double[,] image = trainData.ElementAt(nrStep);

                //Forward propagation
                output = ForwardPropagate(image);

                correctAnswer = correctAnswers.ElementAt(nrStep);

                for (int i = 0; i < output.Length; ++i)
                {
                    Console.WriteLine("{0:0.000} ::: {1:0.000} ", correctAnswer[i], output[i]);
                }
                Console.WriteLine("---------");

                list.Add(new KeyValuePair<double[], double[]>(correctAnswer, output));
            }

            return list;
        }

        public double Test_2(List<double[,]> trainData, List<double[]> correctAnswers)
        {
            int startIndex = 0;
            int endIndex = startIndex + 100;
            double[] output = null;
            double[] correctAnswer = null;
            double error = 0;

            for (int nrStep = startIndex; nrStep < endIndex; ++nrStep)
            {
                double[,] image = trainData.ElementAt(nrStep);

                //Forward propagation
                output = ForwardPropagate(image);

                //calculate epoch error
                correctAnswer = correctAnswers.ElementAt(nrStep);
                for (int b = 0; b < output.Length; ++b)
                {
                    double desired = correctAnswer[b];
                    double actual = output[b];
                    error += (0.5 * Math.Pow(desired - actual, 2));
                }
            }

          //  Console.WriteLine("Output: " + String.Join("\n", output) + "---");


            return error / (endIndex - startIndex);
        }

        
        public void Stop()
        {
            if (lastThread!=null)
            {
                lastThread.Abort();
                lastThread = null;
            }
        }

        public static List<Neuron> GenerateNeurons(int count, int nrWeights, bool maxWeights = false)
        {
            List<Neuron> neurons = new List<Neuron>();
            Random rand = new Random();
            for (int i = 0; i < count; ++i)
            {
                double[] weights = new double[nrWeights];
                for (int j = 0; j < nrWeights; ++j)
                {
                    weights[j] = maxWeights ? 1 : rand.NextDouble()%0.3;
                }

                neurons.Add(new Neuron(weights));
            }

            return neurons;
        }

        public void Init()
        {
            Random randomizer = new Random();
            //INFO: Filters for convolution layers
            double[,] filter_1 = new double[3, 3] { { 0.8, 0.2, 0.1 }, { 0.2, 0.2, 0.1 }, { 0.6, 0.2, 0.9 } };
            double[,] filter_2 = new double[3, 3] { { 0.2, 0.6, 0.5 }, { 0.1, 0.1, 0.2 }, { 0.2, 0.3, 0.8 } };
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    filter_1[i, j] = randomizer.NextDouble();
                    filter_2[i, j] = randomizer.NextDouble();
                }
            }

            /*
             INFO: Network architecture 
             ConvLayer 1  : matrix 200x200 -> matrix 200x200
             PoolLayer    : matrix 200x200 -> matrix 100x100
             ConvLayer 2  : matrix 100x100 -> matrix 100x100
             PoolLayer    : matrix 100x100 -> matrix 50x50
             Flattening   : matrix 50x50 -> vector 2500
             FCL - input  : vector 2500 -> vector 1000
             FCL - hidden : vector 1000 -> vector 100
             FCL - output : vector 100 -> vector 36
             */
            //INFO: Create layers
            //ConvolutionLayer layer_1 = new ConvolutionLayer(filter_1);
            //PoolingLayer layer_2 = new PoolingLayer(2);
            //ConvolutionLayer layer_3 = new ConvolutionLayer(filter_2);
            //PoolingLayer layer_4 = new PoolingLayer(2);
            //FlatteningLayer layer_5 = new FlatteningLayer(); //flattening
            //FCLayer layer_6 = new FCLayer(GenerateNeurons(1000, 2500, true));
            //FCLayer layer_7 = new FCLayer(GenerateNeurons(100, 1000));
            //FCLayer layer_8 = new FCLayer(GenerateNeurons(26, 100));


            /*
             INFO: Network architecture
             ConvLayer 1  : matrix 20x20 -> matrix 20x20
             PoolLayer    : matrix 20x20 -> matrix 10x10
             ConvLayer 2  : matrix 10x10 -> matrix 10x10
             PoolLayer    : matrix 10x10 -> matrix 5x5
             Flattening   : matrix 5x5 -> vector 25
             FCL - input  : vector 25 -> vector 15
             FCL - hidden : vector 15 -> vector 8
             FCL - output : vector 8 -> vector 3
             */

            ConvolutionLayer layer_1 = new ConvolutionLayer(filter_1);
            PoolingLayer layer_2 = new PoolingLayer(2);
        //    ConvolutionLayer layer_3 = new ConvolutionLayer(filter_2);
        //    PoolingLayer layer_4 = new PoolingLayer(2);
            FlatteningLayer layer_5 = new FlatteningLayer(); //flattening
            FCLayer layer_6 = new FCLayer(GenerateNeurons(100, 196), ActivationFunction.Sigmoid, DeactivationFunction.Sigmoid);
            FCLayer layer_7 = new FCLayer(GenerateNeurons(30, 100), ActivationFunction.Sigmoid, DeactivationFunction.Sigmoid);
            FCLayer layer_8 = new FCLayer(GenerateNeurons(6, 30), ActivationFunction.Tanh, DeactivationFunction.Tanh);

            Console.WriteLine("Made layers: ");
            Console.WriteLine(string.Format("FCL 1. Neurons: {0}; W: {1}", layer_6.neurons.Count, layer_6.neurons[0].weights.Length)); 
            Console.WriteLine(string.Format("FCL 2. Neurons: {0}; W: {1}", layer_7.neurons.Count, layer_7.neurons[0].weights.Length)); 
            Console.WriteLine(string.Format("FCL 3. Neurons: {0}; W: {1}", layer_8.neurons.Count, layer_8.neurons[0].weights.Length));
            Console.WriteLine("------");

            //INFO: Add all layers to list
            layers.Add(layer_1);
            layers.Add(layer_2);
        //    layers.Add(layer_3);
         //   layers.Add(layer_4);
            layers.Add(layer_5);
            layers.Add(layer_6);
            layers.Add(layer_7);
            layers.Add(layer_8);

            initialized = true;
        }

        public void Init_2()
        {
            /*
             INFO: Network architecture 
             ConvLayer 1  : matrix 60x60 -> matrix 60x60
             PoolLayer    : matrix 60x60 -> matrix 30x30
             ConvLayer 2  : matrix 30x30 -> matrix 30x30
             PoolLayer    : matrix 30x30 -> matrix 15x15
             Flattening   : matrix 15x15 -> vector 225
             FCL - input  : vector 225 -> vector 225
             FCL - hidden : vector 160 -> vector 160
             FCL - hidden : vector 100 -> vector 100
             FCL - output : vector 100 -> vector 36
             */

            /*
             INFO: Network architecture 
             ConvLayer 1  : matrix 60x60 -> matrix 60x60
             PoolLayer    : matrix 60x60 -> matrix 30x30
             PoolLayer    : matrix 30x30 -> matrix 15x15
             Flattening   : matrix 15x15 -> vector 225
             FCL - input  : vector 225 -> vector 225
             FCL - hidden : vector 130 -> vector 130
             FCL - hidden : vector 50 -> vector 50
             FCL - output : vector 50 -> vector 36
             */

            Random randomizer = new Random();
            double[,] filter_1 = new double[3, 3] ;
            double[,] filter_2 = new double[3, 3] ;
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    filter_1[i, j] = randomizer.NextDouble();
                    filter_2[i, j] = randomizer.NextDouble();
                }
            }

            //Initializing layers-----------
            List<LayerBase> layers = new List<LayerBase>();
            ConvolutionLayer layer_1 = new ConvolutionLayer(filter_1);
            PoolingLayer layer_2 = new PoolingLayer(2);
       //     ConvolutionLayer layer_3 = new ConvolutionLayer(filter_2);
         PoolingLayer layer_4 = new PoolingLayer(2);
            FlatteningLayer layer_5 = new FlatteningLayer(); //flattening
            FCLayer layer_6 = new FCLayer(NeuralNetwork.GenerateNeurons(225, 225, true), NeuralNetwork.ActivationFunction.Tanh, NeuralNetwork.DeactivationFunction.Tanh);

            learningRate = 0.7;
            int nrNeurons_HiddenLayer1 = 130;
            int nrNeurons_HiddenLayer2 = 50;

            FCLayer layer_7 = new FCLayer(NeuralNetwork.GenerateNeurons(nrNeurons_HiddenLayer1, 225), NeuralNetwork.ActivationFunction.Sigmoid, NeuralNetwork.DeactivationFunction.Sigmoid);
            FCLayer layer_8 = new FCLayer(NeuralNetwork.GenerateNeurons(nrNeurons_HiddenLayer2, nrNeurons_HiddenLayer1), NeuralNetwork.ActivationFunction.Sigmoid, NeuralNetwork.DeactivationFunction.Sigmoid);
            FCLayer layer_9 = new FCLayer(NeuralNetwork.GenerateNeurons(36, nrNeurons_HiddenLayer2), NeuralNetwork.ActivationFunction.Sigmoid, NeuralNetwork.DeactivationFunction.Sigmoid);

            layers.Add(layer_1);
            layers.Add(layer_2);
         //   layers.Add(layer_3);
          layers.Add(layer_4);
            layers.Add(layer_5);
            layers.Add(layer_6);
            layers.Add(layer_7);
            layers.Add(layer_8);
            layers.Add(layer_9);

            Init(layers);

            initialized = true;
        }

        

        public void Init(List<LayerBase> layers)
        {
            this.layers = layers;
            initialized = true;
        }

        public void Reset()
        {
            initialized = false;
        }
    }
}