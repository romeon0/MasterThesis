using NeuroevolutionApplication.NN;
using System;
using System.Collections.Generic;

namespace NeuroevolutionApplication.NN
{
    [Serializable]
    public class Helper
    {
        //INFO: Apply convolution to an image using a filter. 
        //      Convolution: (filter is NOT inversed, starting from [0-filterSize,0-filterSize])
        public double[,] Convolution(double[,] image, double[,] filter)
        {
            int filterSize = filter.GetLength(0);
            int srcImgHeight = image.GetLength(0);
            int srcImgWidth = image.GetLength(1);
            int destImgHeight = srcImgHeight - filterSize + 1;
            int destImgWidth = srcImgWidth - filterSize + 1;

            filter = FlipKernel(filter);

            double[,] output = new double[destImgHeight, destImgWidth];
            for (int y = 0; y < destImgHeight; ++y)
            {
                for (int x = 0; x < destImgWidth; ++x)
                {
                    output[y, x] = 0.0;
                    for (int filterY = 0; filterY < filterSize; ++filterY)
                    {
                        for (int filterX = 0; filterX < filterSize; ++filterX)
                        {
                            output[y, x] += filter[filterY, filterX] * image[y + filterY, x + filterX];
                        }
                    }
                }
            }

            return output;
        }

        //INFO: Apply cross-corelation/full convolution to an image using a filter
        //     CrossCorrelation: (filter is inversed, starting from [0,0])
        //      https://glassboxmedicine.com/2019/07/26/convolution-vs-cross-correlation/
        public double[,] CrossCorrelation(double[,] image, double[,] filter)
        {
            int filterSize = filter.GetLength(0);
            int srcImgHeight = image.GetLength(0);
            int srcImgWidth = image.GetLength(1);
            int destImgHeight = srcImgHeight + filterSize/2;
            int destImgWidth = srcImgWidth + filterSize/2;

            double[,] output = new double[destImgHeight, destImgWidth];
            for (int y = 0; y < destImgHeight; ++y)
            {
                for (int x = 0; x < destImgWidth; ++x)
                {
                    output[y, x] = 0.0;
                    int absFilterY = y - filterSize + 1;
                    int absFilterX = x - filterSize + 1;
                    for (int filterY = 0; filterY < filterSize; ++filterY)
                    {
                        for (int filterX = 0; filterX < filterSize; ++filterX)
                        {
                            int fx = absFilterX + filterX;
                            int fy = absFilterY + filterY;
                            if (Between(fy, -1, srcImgHeight) && Between(fx, -1, srcImgWidth))
                            {
                                output[y, x] += filter[filterY, filterX] * image[fy, fx];
                            }
                        }
                    }
                }
            }
            
            return output;
        }

        public bool Between(int v, int min, int max)
        {
            return v > min && v < max;
        }

        internal double[] MatrixToVector(double[,] img)
        {
            int dimensionY = img.GetLength(0);
            int dimensionX = img.GetLength(1);

            double[] vector = new double[dimensionX * dimensionY];
            int counter = 0;
            for(int i= 0; i < dimensionY; ++i)
            {
                for (int j = 0; j < dimensionX;  ++j)
                {
                    vector[counter] = img[i, j];
                    ++counter;
                }
            }

            return vector;
        }

        internal double[,] VectorToMatrix(double[] vector, int rows, int columns)
        {
            double[,] matrix = new double[rows, columns];
            int counter = 0;
            for (int y = 0; y < rows; ++y)
            {
                for (int x = 0; x < columns; ++x)
                {
                    matrix[y, x] = vector[counter];
                    ++counter;
                }
            }
            return matrix;
        }

        //INFO: Apply Max pooling operation to an image using windowSize (windowSize: 3x3,4x4 etc)
        public KeyValuePair<double[,], int[]> Pooling_Max(double[,] inputImage, int windowSize)
        {
            int inputImgHeight = inputImage.GetLength(0);
            int inputImgWidth = inputImage.GetLength(1);
            int modH = (inputImgHeight % windowSize);
            int modW = (inputImgWidth % windowSize);
            int outputSizeY = inputImgHeight / windowSize + (modH == 0 ? 0 : 1);
            int outputSizeX = inputImgWidth / windowSize + (modW == 0 ? 0 : 1);
            double[,] poolingValues = new double[outputSizeY, outputSizeX];

            int[] sparseMatrix = new int[outputSizeY * outputSizeX];
            int counter = 0;
            for (int y = 0, valueY = 0; y < inputImgHeight; y += windowSize, ++valueY)
            {
                for (int x = 0, valueX = 0; x < inputImgWidth; x += windowSize, ++valueX)
                {
                    int elementIndex = 0;
                    double max = double.MinValue;
                    for (int poolY = 0; poolY < windowSize; ++poolY)
                    {
                        for (int poolX = 0; poolX < windowSize; ++poolX)
                        {
                            int currY = y + poolY;
                            int currX = x + poolX;
                            if (currY < inputImgHeight && currX < inputImgWidth)
                            {
                                if (inputImage[currY, currX] > max)
                                {
                                    max = inputImage[currY, currX];
                                    elementIndex = poolY * windowSize + poolX;
                                }
                            }
                        }
                    }
                    sparseMatrix[counter++] = elementIndex;

                    poolingValues[valueY, valueX] = max;
                }
            }

            return new KeyValuePair<double[,], int[]>(poolingValues, sparseMatrix);
        }

        public double[,] PoolingAndReturn_Max(double[,] inputImage, int windowSize)
        {
            int inputImgHeight = inputImage.GetLength(0);
            int inputImgWidth = inputImage.GetLength(1);
            int outputSizeY = inputImgHeight / windowSize + ((inputImgHeight % windowSize) == 0 ? 0 : 1);
            int outputSizeX = inputImgWidth / windowSize + ((inputImgWidth % windowSize) == 0 ? 0 : 1);

            for (int y = 0; y < inputImgHeight; y += windowSize)
            {
                for (int x = 0; x < inputImgWidth; x += windowSize)
                {
                    double max = double.MinValue;
                    int lastPoolX = -1, lastPoolY = -1;
                    for (int poolY = 0; poolY < windowSize; ++poolY)
                    {
                        for (int poolX = 0; poolX < windowSize; ++poolX)
                        {
                          //  Console.WriteLine("Sizes----> {0}. {1}.", poolX, poolY);

                            int currY = y + poolY;
                            int currX = x + poolX;
                            if (currY < inputImgHeight && currX < inputImgWidth)
                            {
                                if (inputImage[currY, currX] > max)
                                {
                                    if (lastPoolX != -1)
                                    {
                                        inputImage[lastPoolY, lastPoolX] = 0;
                                    }
                                    lastPoolX = currX;
                                    lastPoolY = currY;
                                    max = inputImage[currY, currX];
                                }
                                else
                                    inputImage[currY, currX] = 0;
                            }
                        }
                    }
                }
            }

            return inputImage;
        }

        public double[,] PoolingAndReplace_Max(double[,] input, double[,] replacement, int windowSize)
        {
            int inputImgHeight = input.GetLength(0);
            int inputImgWidth = input.GetLength(1);

            for (int offsetY = 0, a = 0; offsetY < inputImgHeight; offsetY += windowSize, ++a)
            {
                for (int offsetX = 0, b=0; offsetX < inputImgWidth; offsetX += windowSize,++b)
                {
                    double max = double.MinValue;
                    int lastPoolX = -1, lastPoolY = -1;
                    for (int poolY = 0; poolY < windowSize; ++poolY)
                    {
                        for (int poolX = 0; poolX < windowSize; ++poolX)
                        {
                            int currX = offsetX + poolX;
                            int currY = offsetY + poolY;
                            if (currY < inputImgHeight && currX < inputImgWidth)
                            {
                                if (input[currY, currX] > max)
                                {
                                    if (lastPoolX != -1)
                                    {
                                        input[lastPoolY, lastPoolX] = 0;
                                    }
                                    lastPoolX = currX;
                                    lastPoolY = currY;
                                    max = input[currY, currX];
                                    input[currY, currX] = replacement[a, b];
                                }
                                else
                                    input[currY, currX] = 0;
                            }
                        }
                    }
                }
            }

            return input;
        }

        public double[,] ReversePooling(double[,] replacement, int[] sparseMatrix, int windowSize)
        {
            int inputImgHeight = replacement.GetLength(0);
            int inputImgWidth = replacement.GetLength(1);
            int outHeight = inputImgHeight * windowSize;
            int outWidth = inputImgWidth * windowSize;


            double[,] output = new double[outHeight, outWidth];
            int counter = 0;
            for (int y = 0, outY=0; y < inputImgHeight; ++y, outY+=windowSize)
            {
                for (int x = 0, outX=0; x < inputImgHeight; ++x, outX += windowSize)
                {
                    double value = replacement[y, x];
                    int sparseIndex = sparseMatrix[counter++];
                    int counter2 = 0;
                    for (int windowY = 0; windowY < windowSize; ++windowY)
                    {
                        for (int windowX = 0; windowX < windowSize; ++windowX)
                        {
                            if (counter2++ != sparseIndex)
                                output[outY + windowY, outX + windowX] = 0;
                            else
                                output[outY + windowY, outX + windowX] = value;
                        }
                    }
                }
            }

            return output;
        }


        //INFO: Apply Average pooling operation to an image using windowSize (windowSize: 3x3,4x4 etc)
        public double[,] Pooling_Average(double[,] inputImage, int windowSize)
        {
            int inputImgHeight = inputImage.GetLength(0);
            int inputImgWidth = inputImage.GetLength(1);
            int outputSizeY = inputImgHeight / windowSize + (inputImgHeight % windowSize) == 0 ? 0 : 1;
            int outputSizeX = inputImgWidth / windowSize + (inputImgWidth % windowSize) == 0 ? 0 : 1;
            double[,] poolingValues = new double[outputSizeY, outputSizeX];
            int nrElementsInWindow = windowSize * windowSize;

            for (int y = 0, valueY = 0; y < inputImgHeight; y += windowSize, ++valueY)
            {
                for (int x = 0, valueX = 0; x < inputImgWidth; x += windowSize, ++valueX)
                {
                    double sum = double.MinValue;
                    for (int poolY = 0; poolY < windowSize; ++poolY)
                    {
                        for (int poolX = 0; poolX < windowSize; ++poolX)
                        {
                            int currY = y + poolY;
                            int currX = x + poolX;
                            if (currY < inputImgHeight && currX < inputImgWidth)
                            {
                                sum += inputImage[currY, currX];
                            }
                        }
                    }

                    poolingValues[valueY, valueX] = sum / nrElementsInWindow;
                }
            }

            return poolingValues;
        }

        //INFO: 180 grade rotate of matrix
        public double[,] FlipKernel(double[,] filter) 
        {
           
            int filterSizeY = filter.GetLength(0);
            int filterSizeX = filter.GetLength(1);
            double[,] rotatedFilter = new double[filterSizeY, filterSizeX];
            for (int filterY = 0; filterY < filterSizeY; ++filterY)
            {
                for (int filterX = 0; filterX < filterSizeX; ++filterX)
                {
                    rotatedFilter[filterSizeY - 1 - filterY, filterSizeX - 1 - filterX] = filter[filterY, filterX];
                }
            }
            return rotatedFilter;
        }


        //INFO: Calculate input value of neuron using his weights
        public double Sum(double[] input, double[] weights)
        {
            double output = 0.0;
            for (int i = 0; i < input.Length; ++i)
            {
                //Console.WriteLine("Sum[{0}]: {1} ::: {2}", i, input[i], weights[i]);
                output += input[i] * weights[i];
            }
            return output;
        }

        public void ShowVector(double[] values)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                Console.Write(values[i] + " ");
            }
            Console.Write("\n");
        }

        public void ShowVector(int[] values)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                Console.Write(values[i] + " ");
            }
            Console.Write("\n");
        }

        public void ShowMatrix(double[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); ++i)
            {
                for (int j = 0; j < matrix.GetLength(1); ++j)
                {
                    Console.Write(matrix[i, j] + " ");
                }
                Console.Write("\n");
            }
            Console.Write("\n");
        }

        public void ShowMatrix(int[,] matrix)
        {
            for (int i = 0; i < matrix.GetLength(0); ++i)
            {
                for (int j = 0; j < matrix.GetLength(1); ++j)
                {
                    Console.Write(matrix[i, j] + " ");
                }
                Console.Write("\n");
            }
            Console.Write("\n");
        }
    }
}