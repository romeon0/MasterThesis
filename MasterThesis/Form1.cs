using ConnectedComponentLabeling;
using ImageProcessor.Imaging.Filters.EdgeDetection;
using Neuroevolution_Application.Extensions;
using Neuroevolution_Application.Helpers;
using NeuroevolutionApplication.NN;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Neuroevolution_Application
{


    public partial class Form1 : Form
    {
        private NeuralNetwork neuralNetwork = null;
        private bool initialized = false;
        private List<double[,]> trainingData = null;// new List<double[,]>();
        private List<double[]> correctAnswers = null;//new List<double[]>();
        private Dictionary<int, string> indexToLabelMap = new Dictionary<int, string>();

        //UpdateGUIThreadSafe<Chart> updateCurrentGeneration = null;
        //UpdateGUIThreadSafe<ListBox> updateListBox = null;
        UpdateGUIThreadSafe<Control> setApplicationState = null;


        private enum ApplicationState
        {
            NONE = 0,
            TRAINING = 1,
            TESTING = 3,
            DETECTING = 3,
            STOPPED = 4
        }


        public Form1()
        {
            InitializeComponent();
            SetApplicationState(ApplicationState.NONE);
        }


        private double[,] SimulateData()
        {
            int imageWidth = 20;
            int imageHeight = 20;

            double[,] data = new double[imageHeight, imageWidth];
            Random rand = new Random();


            for (int i = 0; i < imageHeight; ++i)
            {
                for (int j = 0; j < imageWidth; ++j)
                {
                    //if (i >= 100 && i <= 200 && j >= 100 && j <= 200)
                    //{
                    //    data[i, j] = 1;
                    //}
                    //else
                    //    data[i, j] = 0;

                    data[i, j] = 0.5;// (i + j) / (imageHeight + imageWidth);// rand.NextDouble();
                }
            }

            return data;
        }

        private double[] SimulateCorrectAnswer()
        {
            double[] correctAnswer = new double[26] { 1, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            //Random rand = new Random();
            //for (int i = 0; i < 26; ++i)
            //{
            //    correctAnswer[i] = rand.NextDouble();
            //    Console.WriteLine("correctAnswer[{0}]: {1}", i, correctAnswer[i]);
            //}

            return correctAnswer;
        }

        private void Debug()
        {

            double[,] img = new double[6, 6]
            {
                { 0,1,0,0,3,1},
                { 3,0,0,0,0,9},
                { 0,0,0,0,0,0},
                { 0,0,0,0,0,0},
                { 1,0,0,0,0,0},
                { 0,2,0,0,0,1}
            };
            double[,] filter = new double[2, 2]
            {
                { 1, 1},
                { 2, 1}
            };

            Helper helper = new Helper();


            //img = helper.CrossCorelation(img, filter);
            //double[] values = new double[3] { 0, 2, 2 };
            //double[] weights = new double[3] { 0, 1, 3 };

            //double value = helper.Sum(values, weights);
            //ShowValue(value);
            //value = helper.Activate_Tanh(value);
            //ShowValue(value);


            double[,] errors = new double[3, 3] { { 99, 99, 99 }, { 99, 99, 99 }, { 99, 99, 99 } };
            double[,] output = helper.PoolingAndReplace_Max(img, errors, 2);
            helper.ShowMatrix(output);
        }

        private List<Neuron> GenerateNeurons(int count, int nrWeights, bool maxWeights = false)
        {
            List<Neuron> neurons = new List<Neuron>();
            Random rand = new Random();
            for (int i = 0; i < count; ++i)
            {
                double[] weights = new double[nrWeights];
                for (int j = 0; j < nrWeights; ++j)
                {
                    weights[j] = maxWeights ? 1 : rand.NextDouble();
                }

                neurons.Add(new Neuron(weights));
            }

            return neurons;
        }

        private void TestFullConnectedLayers()
        {
            //double[] input = new double[12] { 0, 0, 0, 1, 1, 0, 1, 1, 0, 1, 0, 0};
            //double[] correctAnswer = new double[3] { 0.3, 0.8, 0.2};

            //Network nn = new Network();
            //Helper helper = new Helper();

            //List<LayerBase> layers = new List<LayerBase>();
            //layers.Add(new FlatteningLayer());
            //layers.Add(new FCLayer(GenerateNeurons(12, 12, true)));
            //layers.Add(new FCLayer(GenerateNeurons(6, 12)));
            //layers.Add(new FCLayer(GenerateNeurons(3, 6)));
            //nn.Init(layers);

            //List<double[,]> trainData = new List<double[,]>();
            //trainData.Add(helper.VectorToMatrix(input, 4, 3));
            //List<double[]> correctAnswers = new List<double[]>();
            //correctAnswers.Add(correctAnswer);
            //nn.Train(trainData, correctAnswers);
        }

        private void CreateLetterFromAttributes()
        {
            /*
                1.	lettr	capital letter	(26 values from A to Z)
                2.	x-box	horizontal position of box	(integer)
                3.	y-box	vertical position of box	(integer)
                4.	width	width of box			(integer)
                5.	high 	height of box			(integer)
                6.	onpix	total # on pixels		(integer)
                7.	x-bar	mean x of on pixels in box	(integer)
                8.	y-bar	mean y of on pixels in box	(integer)
                9.	x2bar	mean x variance			(integer)
               10.	y2bar	mean y variance			(integer)
               11.	xybar	mean x y correlation		(integer)
               12.	x2ybr	mean of x * x * y		(integer)
               13.	xy2br	mean of x * y * y		(integer)
               14.	x-ege	mean edge count left to right	(integer)
               15.	xegvy	correlation of x-ege with y	(integer)
               16.	y-ege	mean edge count bottom to top	(integer)
               17.	yegvx	correlation of y-ege with x	(integer)

            */

            //string line = "T,2,8,3,5,1,8,13,0,6,6,10,8,0,8,0,8";

            //string[] data = line.Split(',');
            //int[] data_numbers = new int[data.Length - 1];

            //for (int i = 1; i < data.Length; ++i)
            //{
            //    data_numbers[i - 1] = int.Parse(data[i]);
            //}

            //int originalRoot = 15;
            //int multiplier = 1;

            //int boxSizeX = data_numbers[2] * multiplier;
            //int boxSizeY = data_numbers[3] * multiplier;

            //string letter = "";
            //for (int y = 0; y < originalRoot * multiplier * 1.5; ++y)
            //{
            //    boxSizeX = data_numbers[2] * multiplier;
            //    for (int x = 0; x < originalRoot * multiplier * 1.5; ++x)
            //    {
            //        if(boxSizeX>0 && boxSizeY > 0)
            //        {
            //            if (x >= data_numbers[0] * (multiplier) && y >= data_numbers[1] * (multiplier))
            //            {
            //                letter += '1';
            //            }
            //            else
            //                letter += '0';
            //        }
            //        else
            //            letter += '0';

            //        --boxSizeX;
            //    }

            //    --boxSizeY;
            //    letter += "\n";
            //}

            //Console.WriteLine("====Letter:=====\n {0}", letter);
        }

        private Bitmap SliceImage(string filePath, int x, int y, int width, int height)
        {
            Image img = Image.FromFile(filePath);
            Bitmap bmp = new Bitmap(width, height);

            Graphics g = Graphics.FromImage(bmp);
            g.DrawImage(img, new Rectangle(0, 0, width, height), new Rectangle(x, y, width, height), GraphicsUnit.Pixel);
            g.Dispose();

            //imgCurrentImage.Image = bmp;

            return bmp;
        }

        private void ShowVector(double[] values)
        {
            for (int i = 0; i < values.Length; ++i)
            {
                Console.Write(values[i] + " ");
            }
            Console.Write("\n");
        }

        void ShowMatrix(double[,] matrix)
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

        void ShowValue(object value)
        {
            Console.WriteLine("Value: " + value);
        }

        private void SaveImage(string filePath, Bitmap image)
        {
            image.Save(filePath, ImageFormat.Png);
        }

        public Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private Image ReadImage(string filePath)
        {
            Image img = Image.FromFile(filePath);
            return img;
        }

        //INFO: Preprocessed data from big images to only numbers. Just to be here, for history :)
        private void OptimizeDatasetImages()
        {
            string rootDirectoty = @".\Data";
            string[] filePaths = Directory.GetFiles(rootDirectoty, "*.jpg");

            string labelsText = "";
            int imageId = 1;
            foreach (string path in filePaths)
            {
                Console.WriteLine("Path: " + path);
                Console.WriteLine("Path_2: " + path.Replace(".jpg", ".txt"));
                string fileText = File.ReadAllText(path.Replace(".jpg", ".txt"));
                string[] dataArray = fileText.Split('\t');

                int x = int.Parse(dataArray[1]);
                int y = int.Parse(dataArray[2]);
                int width = int.Parse(dataArray[3]);
                int height = int.Parse(dataArray[4]);
                string text = dataArray[5].Replace("\r", "");
                text = text.Replace("\n", "");

                Bitmap image = SliceImage(path, x, y, width, height);
                SaveImage(rootDirectoty + @"\Processed\" + imageId + ".png", image);

                dataArray = path.Split('\\');
                string imageName = dataArray[dataArray.Length - 1];

                Console.WriteLine("imageId: " + imageId);
                Console.WriteLine("text: " + text + "||");
                Console.WriteLine("imageName: " + imageName);
                labelsText += imageId++ + " " + text + " " + imageName + "\n";
            }

            File.Create(rootDirectoty + @"\Processed\Labels.txt").Close();
            File.WriteAllText(rootDirectoty + @"\Processed\Labels.txt", labelsText);
        }

        //INFO: Save pixels of all files in one file, instead multiple images
        private void OptimizeImagesToPixels()
        {
            string rootDirectoty = @".\Data";
            string[] filePaths = Directory.GetFiles(rootDirectoty, "*.png");

            string data = "";

            int resizeWidth = 150;
            int resizeHeight = 30;
            string space = " ";
            foreach (string path in filePaths)
            {
                Console.WriteLine("path: " + path);

                Image image = ReadImage(path);

                image = ResizeImage(image, resizeWidth, resizeHeight);

                Bitmap bmp = new Bitmap(image);
                for (int x = 0; x < resizeWidth; ++x)
                {
                    for (int y = 0; y < resizeHeight; ++y)
                    {

                        Color color = bmp.GetPixel(x, y);

                        //   Console.WriteLine("Color: " + color);

                        data += color.R;
                        data += space;
                        data += color.G;
                        data += space;
                        data += color.B;
                        data += space;
                    }
                }

                Graphics g = Graphics.FromImage(bmp);
                g.DrawImage(image, new Rectangle(0, 0, resizeWidth, resizeHeight), new Rectangle(0, 0, resizeWidth, resizeHeight), GraphicsUnit.Pixel);
                g.Dispose();

                data += "\n";

                //Console.WriteLine(data);
                //break;
            }
            Console.WriteLine("OptimizeImagesToPixels: Files reading end.");

            File.Create(rootDirectoty + @"\PixelsData.txt").Close();
            File.WriteAllText(rootDirectoty + @"\PixelsData.txt", data);

            Console.WriteLine("OptimizeImagesToPixels: end.");
        }

        private List<double[,]> ReadTrainData()
        {
            List<double[,]> data = new List<double[,]>();

            string rootDirectoty = @".\Data";
            string allText = File.ReadAllText(rootDirectoty + @"\PixelsData.txt");

            string[] lines = allText.Split('\n');
            double[,] row = new double[150, 30];

            for (int i = 0; i < lines.Length; ++i)
            {
                //Console.WriteLine(lines[i]);

                string[] pixels = lines[i].Split(' ');
                int pixelValue = 0;
                int nrPixelsRead = 0;
                for (int a = 0, x = 0, y = 0; a < pixels.Length; a += 3, ++x)
                {
                    if (x == 150)
                    {
                        x = 0;
                        ++y;
                    }

                    bool parsed = int.TryParse(pixels[a], out pixelValue);
                    if (parsed)
                    {
                        row[x, y] = pixelValue;
                        ++nrPixelsRead;
                    }
                }

                data.Add(row);
                //Console.WriteLine(nrPixelsRead);
            }

           // Console.WriteLine("Data read successful. Nr. Images: " + data.Count);

            return data;
        }

        private void ReadCharsTrainData(out List<double[,]> trainData, out List<double[]> correctAnswers)
        {
            trainData = new List<double[,]>();
            correctAnswers = new List<double[]>();

            Dictionary<string, int> labelToIndexMap = new Dictionary<string, int>();
            labelToIndexMap.Add("A", 27);
            labelToIndexMap.Add("B", 28);
            labelToIndexMap.Add("C", 29);
            labelToIndexMap.Add("D", 30);
            labelToIndexMap.Add("E", 31);
            labelToIndexMap.Add("F", 32);
            labelToIndexMap.Add("G", 33);
            labelToIndexMap.Add("H", 34);
            labelToIndexMap.Add("I", 35);
            labelToIndexMap.Add("J", 36);
            labelToIndexMap.Add("K", 37);
            labelToIndexMap.Add("L", 38);
            labelToIndexMap.Add("M", 39);
            labelToIndexMap.Add("N", 40);
            labelToIndexMap.Add("O", 41);
            labelToIndexMap.Add("P", 42);
            labelToIndexMap.Add("Q", 43);
            labelToIndexMap.Add("R", 44);
            labelToIndexMap.Add("S", 45);
            labelToIndexMap.Add("T", 46);
            labelToIndexMap.Add("U", 47);
            labelToIndexMap.Add("V", 48);
            labelToIndexMap.Add("W", 49);
            labelToIndexMap.Add("X", 50);
            labelToIndexMap.Add("Y", 51);
            labelToIndexMap.Add("Z", 52);

            labelToIndexMap.Add("1", 53);
            labelToIndexMap.Add("2", 54);
            labelToIndexMap.Add("3", 55);
            labelToIndexMap.Add("4", 56);
            labelToIndexMap.Add("5", 57);
            labelToIndexMap.Add("6", 58);
            labelToIndexMap.Add("7", 59);
            labelToIndexMap.Add("8", 60);
            labelToIndexMap.Add("9", 61);
            labelToIndexMap.Add("0", 62);

            string rootDirectoty = @".\Data\Characters\";
            string allText = File.ReadAllText(rootDirectoty + @"\CharsData.txt");

            string[] lines = allText.Split('\n');

            for (int i = 0; i < lines.Length; ++i)
            {
                //Console.WriteLine(lines[i]);
                double[,] row = new double[60, 60];
                string[] pixels = lines[i].Split(' ');

                //label
                string label = pixels[0];
                double[] correctAnswerVector = new double[36];
                int correctLabelIndex = 0;
                bool found = labelToIndexMap.TryGetValue(label, out correctLabelIndex);
                if (found)
                    correctAnswerVector[correctLabelIndex - 27] = 1;
                else
                    continue;

               // Console.WriteLine("CorrectAnswer: " + String.Join(" ", correctAnswerVector));

                //pixels
                double pixelValue = 0;
                int nrPixelsRead = 0;
                for (int a = 1, x = 0, y = 0; a < pixels.Length; a += 1, ++x)
                {
                    if (x == 60)
                    {
                        x = 0;
                        ++y;
                    }

                    double value, r, g, b;
                    bool parsed = double.TryParse(pixels[a], out pixelValue);
                    // bool parsed = double.TryParse(pixels[a], out r);
                    //   parsed = double.TryParse(pixels[a + 1], out g);
                    //   parsed = double.TryParse(pixels[a+2], out b);
                    //pixelValue = (r + g + b) / 3;
                    ///  if (parsed)
                    {
                        row[x, y] = pixelValue;
                        ++nrPixelsRead;
                    }
                }

                trainData.Add(row);
                correctAnswers.Add(correctAnswerVector);
                //Console.WriteLine(nrPixelsRead);

                //for (int x = 0; x < 60; ++x)
                //{
                //    for (int y = 0; y < 60; ++y)
                //    {
                //        Console.Write(pixels[y*60+ x] + " ");
                //    }
                //    Console.WriteLine();
                //}
                //Console.WriteLine("--------------------------------------------------------------");
                //Console.WriteLine("--------------------------------------------------------------");
                //break;
            }

            Console.WriteLine("Data read successful. Nr. Chars: " + trainData.Count+ "; Answers: " + correctAnswers.Count);
        }

        private void OptimizeCharactersDatasetImages()
        {
            Dictionary<int, string> labels = new Dictionary<int, string>();
            labels.Add(27, "A");
            labels.Add(28, "B");
            labels.Add(29, "C");
            labels.Add(30, "D");
            labels.Add(31, "E");
            labels.Add(32, "F");
            labels.Add(33, "G");
            labels.Add(34, "H");
            labels.Add(35, "I");
            labels.Add(36, "J");
            labels.Add(37, "K");
            labels.Add(38, "L");
            labels.Add(39, "M");
            labels.Add(40, "N");

            labels.Add(41, "O");
            labels.Add(42, "P");
            labels.Add(43, "Q");
            labels.Add(44, "R");
            labels.Add(45, "S");
            labels.Add(46, "T");
            labels.Add(47, "U");
            labels.Add(48, "V");
            labels.Add(49, "W");
            labels.Add(50, "X");
            labels.Add(51, "Y");
            labels.Add(52, "Z");

            labels.Add(53, "1");
            labels.Add(54, "2");
            labels.Add(55, "3");
            labels.Add(56, "4");
            labels.Add(57, "5");
            labels.Add(58, "6");
            labels.Add(59, "7");
            labels.Add(60, "8");
            labels.Add(61, "9");
            labels.Add(62, "0");

            string rootDirectoty = @".\Data\Characters";
            string[] directories = Directory.GetDirectories(rootDirectoty);

            int width = 60;
            int height = 60;
            string space = " ";
            string data = "";
            double[] row = new double[width * height];
            foreach (string dirPath in directories)
            {
                string[] filePaths = Directory.GetFiles(dirPath, "*.png");
                foreach (string path in filePaths)
                {
                    Console.WriteLine("Path {0}: start.", path);
                    Image image = ReadImage(path);
                  //  Console.WriteLine("Path 2 {0}: start.", path);
                    Bitmap bmp = new Bitmap(image);
                    for (int y = 0; y < height; ++y)
                    {
                        for (int x = 0; x < width; ++x)
                        {

                            Color color = bmp.GetPixel(x, y);

                         //   Console.WriteLine("Color: " + color);
                            int coord = y * width + x;
                            //row[startY + startX] = color.R/255;
                            //row[startY + startX + 1] = color.G / 255;
                            //row[startY + startX + 2] = color.B / 255;

                            row[coord] = (color.R + color.G + color.B) / 3 / 255;

                            //data += color.R;
                            //data += space;
                            //data += color.G;
                            //data += space;
                            //data += color.B;
                            //data += space; 
                        }
                    }

                    string label = path.Replace(".png", "");
                    string[] parts = label.Split('\\');
                    bool parsed = labels.TryGetValue(int.Parse(parts[parts.Length - 2]), out label);

                    Console.WriteLine("label: {0}", label);
                    data += label + " " + String.Join(" ", row) + "\n";

                    Graphics g = Graphics.FromImage(bmp);
                    g.DrawImage(image, new Rectangle(0, 0, width, height), new Rectangle(0, 0, width, height), GraphicsUnit.Pixel);
                    g.Dispose();

                    Console.WriteLine("Path {0}: end.", path);
                }
                //break;
            }

            Console.WriteLine("OptimizeCharactersDatasetImages: Files reading end.");

            File.Create(rootDirectoty + @"\CharsData.txt").Close();
            File.WriteAllText(rootDirectoty + @"\CharsData.txt", data);

            Console.WriteLine("OptimizeCharactersDatasetImages: end.");
        }

        private void SaveNeuralNetwork(NeuralNetwork network)
        {
            if (null==network) return;

            string text = "";
            int fclLayerCount = 1;
            for (int i = 0; i < network.layers.Count; ++i)
            {
                LayerBase layer = network.layers[i];

                if (layer.GetType() == LayerBase.LAYER_TYPE.FCL)
                {

                    FCLayer realLayer = (FCLayer)layer;
                    int nrWeights = realLayer.neurons[0].weights.Length;
                    double[] weights = new double[nrWeights];
                    for (int nrNeuron = 0; nrNeuron < realLayer.neurons.Count; ++nrNeuron)
                    {
                        Neuron neuron = realLayer.neurons[nrNeuron];
                        for (int nrWeight = 0; nrWeight < nrWeights; ++nrWeight)
                        {
                            weights[nrWeight] = neuron.weights[nrWeight];
                        }
                    }

                    text += "# FCL Layer " + fclLayerCount++ + "; NrWeights: " + nrWeights + "\n";
                    text += String.Join(" ", weights) + "\n";
                }
            }

            DateTime timeStamp = DateTime.Now;
            string timestampString = timeStamp.Year
                + "-" + timeStamp.Month
                + "-" + timeStamp.Day
                + "_" + timeStamp.Hour
                + "-" + timeStamp.Minute
                + "-" + timeStamp.Second;
            string filePath = @".\Data\SavedNetworks\SaveNeuralNetwork_" + timestampString + ".txt";
            File.Create(filePath).Close();
            File.WriteAllText(filePath, text);
        }

        private Image TranslatePixelMatrixToImage(double[,] pixels)
        {
            int height = pixels.GetLength(0);
            int width = pixels.GetLength(1);
            Bitmap bmp = new Bitmap(width, height);

            for (int x = 0; x < width; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    int pixelX = x;
                    int pixelY = y;
                    int r = (int)pixels[pixelX, pixelY] * 255;
                    int g = (int)pixels[pixelX, pixelY] * 255;
                    int b = (int)pixels[pixelX, pixelY] * 255;
                    bmp.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }

            Graphics graphics = Graphics.FromImage(bmp);
            graphics.DrawImage(bmp, bmp.Width, 0, width, height);
            graphics.Dispose();

            return bmp;
            // return Image.FromHbitmap(bmp.GetHbitmap());
        }


  


        private void Init()
        {
            if (trainingData==null)
            {
                ReadCharsTrainData(out trainingData, out correctAnswers);
            }

            if (!initialized)
            {
                neuralNetwork = new NeuralNetwork();
                neuralNetwork.Init_2();

                indexToLabelMap.Add(0, "A");
                indexToLabelMap.Add(1, "B");
                indexToLabelMap.Add(2, "C");
                indexToLabelMap.Add(3, "D");
                indexToLabelMap.Add(4, "E");
                indexToLabelMap.Add(5, "F");
                indexToLabelMap.Add(6, "G");
                indexToLabelMap.Add(7, "H");
                indexToLabelMap.Add(8, "I");
                indexToLabelMap.Add(9, "J");
                indexToLabelMap.Add(10, "K");
                indexToLabelMap.Add(11, "L");
                indexToLabelMap.Add(12, "M");
                indexToLabelMap.Add(13, "N");
                indexToLabelMap.Add(14, "O");
                indexToLabelMap.Add(15, "P");
                indexToLabelMap.Add(16, "Q");
                indexToLabelMap.Add(17, "R");
                indexToLabelMap.Add(18, "S");
                indexToLabelMap.Add(19, "T");
                indexToLabelMap.Add(20, "U");
                indexToLabelMap.Add(21, "V");
                indexToLabelMap.Add(22, "W");
                indexToLabelMap.Add(23, "X");
                indexToLabelMap.Add(24, "Y");
                indexToLabelMap.Add(25, "Z");

                indexToLabelMap.Add(26, "1");
                indexToLabelMap.Add(27, "2");
                indexToLabelMap.Add(28, "3");
                indexToLabelMap.Add(29, "4");
                indexToLabelMap.Add(30, "5");
                indexToLabelMap.Add(31, "6");
                indexToLabelMap.Add(32, "7");
                indexToLabelMap.Add(33, "8");
                indexToLabelMap.Add(34, "9");
                indexToLabelMap.Add(35, "0");

                initialized = true;
            }
        }

        private void Reset(bool isClosing = false)
        {
            if (!isClosing)
                SetApplicationState(ApplicationState.NONE);


            if ((isClosing || initialized) && neuralNetwork != null)
            {
                neuralNetwork.Reset();
            }
        }

        private void SetApplicationState(ApplicationState newState)
        {
            if (setApplicationState == null)
            {
                Action<Form, Control, object> setApplicationStateAction = new Action<Form, Control, object>((form, ctrl, value) => {
                    ApplicationState state = (ApplicationState)value;

                    if (state == ApplicationState.STOPPED || state == ApplicationState.NONE)
                    {
                        //state
                        lblCurrStatus.Text = "None";
                        lblCurrStatus.ForeColor = Color.Red;

                        //buttons
                        foreach (Control c in groupMain.Controls)
                            c.Enabled = true;
                        foreach (Control c in groupTraining.Controls)
                            c.Enabled = true;
                        foreach (Control c in groupTesting.Controls)
                            c.Enabled = true;

                        if (state == ApplicationState.NONE)
                        {
                            btnSaveBestNetwork.Enabled = false;
                            listEvolutionErrors.Items.Clear();
                            listTestingErrors.Items.Clear();
                            chartErrors.Series[0].Points.Clear();
                        }

                        //groups
                        groupMain.Enabled = true;
                        groupTraining.Enabled = true;
                        groupTesting.Enabled = true;
                    }
                    else if (state == ApplicationState.TRAINING)
                    {
                        //state
                        lblCurrStatus.Text = "Training";
                        lblCurrStatus.ForeColor = Color.Green;

                        //buttons
                        foreach (Control c in groupTraining.Controls)
                            c.Enabled = false;
                        btnStopTrain.Enabled = true;

                        //groups
                        groupMain.Enabled = false;
                        groupTesting.Enabled = false;
                        groupTraining.Enabled = true;
                    }
                    else if (state == ApplicationState.TESTING ||
                             state == ApplicationState.DETECTING)
                    {
                        //state
                        lblCurrStatus.Text = 
                            state == ApplicationState.TESTING?"Testing":"Detecting";
                        lblCurrStatus.ForeColor = Color.Green;
    
                        //groups
                        groupMain.Enabled = false;
                        groupTraining.Enabled = false;
                        groupTesting.Enabled = false;
                    }
                });

                setApplicationState = new UpdateGUIThreadSafe<Control>(setApplicationStateAction);
            }

            setApplicationState.UpdateElement(this, chartErrors, newState);//gave random GUI element because we do not care 
        }


        #region Callbacks

        private void BtnSaveNetwork_Click(object sender, EventArgs e)
        {
            //-> test!
            //NeuralNetwork n = new NeuralNetwork();
            //n.Init();
            SaveNeuralNetwork(neuralNetwork);
        }

        private void BtnResetApp_Click(object sender, EventArgs e)
        {
            Reset();
        }

        private void BtnLoadData_Click(object sender, EventArgs e)
        {
            if (trainingData==null)
            {
                ReadCharsTrainData(out trainingData, out correctAnswers);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Reset(true);
        }

        private void btnChangeDataPath_Click(object sender, EventArgs e)
        {

        }

        private void btnStartTrain_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Train clicked!");
            Init();
            SetApplicationState(ApplicationState.TRAINING);

            neuralNetwork.StartTrainAsync(trainingData, correctAnswers);
            //    neuralNetwork.Test(trainingData, correctAnswers);
        }

        private void btnStopTrain_Click(object sender, EventArgs e)
        {
            if (initialized)
            {
                SetApplicationState(ApplicationState.STOPPED);
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            SetApplicationState(ApplicationState.TESTING);
            Init();
        }

        private void btnDetect_Click(object sender, EventArgs e)
        {
            SetApplicationState(ApplicationState.DETECTING);
            Init();
        }

        #endregion

        int currImg = 1;


        private class SobelEdgeFilter2 : IEdgeFilter
        {
            private readonly double[,] filter =
            {
                        { -2, 0, 2},
                        { -4, 0, 4},
                        { -2, 0, 2}
            };

            public double[,] HorizontalGradientOperator
            {
                get
                {
                    return filter;
                }
            }
        }

        private void Process(string fileName)
        {
            Bitmap normal = ImageProcessing.Load(fileName);
            imgInput.Image = normal;
            double trash = (double)tmpValue.Value;
            Console.WriteLine("Trashhold: " + trash);
            Image filtered = ImageProcessing.Grayscale(normal);
       //     filtered = ImageProcessing.FilterBlack((Bitmap)filtered, trash);

            imgOutput.Image = filtered;



            PictureBox[] elems =
            {
                imgOutput2,
                imgOutput3,
                imgOutput4,
                imgOutput5,
            };

            IDictionary<int, Bitmap> images = new CCL().Process((Bitmap)filtered);
            Console.WriteLine("images founds: " + images.Count);

            //Clear
            foreach (PictureBox img in elems)
            {
                img.Image = null;
            }

            //Refresh
            int counter = 0;
            foreach(KeyValuePair<int, Bitmap> img in images)
            {
                if (counter >= elems.Length) break;
                elems[counter].Image = img.Value;
                counter++;
            }


        }

        private void tmpTest_Click(object sender, EventArgs e)
        {
            //Init();
            //trainingData = new List<double[,]>();
            //correctAnswers = new List<double[]>();

            Process(@".\Data\Plates\1_1.jpg");
            //Process(@".\Data\Plates\"+ currImg + ".png");

           // neuralNetwork.StartTrainAsync(trainingData, correctAnswers);
            //    neuralNetwork.Test(trainingData, correctAnswers);
        }

        private void tmpNext_Click(object sender, EventArgs e)
        {
            Process(@".\Data\Plates\" + ++currImg + ".png");
        }

        private void tmpValue_ValueChanged(object sender, EventArgs e)
        {

        }
    }
}
