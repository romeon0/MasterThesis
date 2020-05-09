using ConnectedComponentLabeling;
using Neuroevolution_Application.Helpers;
using NeuroevolutionApplication.NN;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Neuroevolution_Application
{
    public partial class TestForm : Form
    {
        private List<Bitmap> _lastBitmaps = new List<Bitmap>();
        private int _currIndex = 1;
        private PictureBox[] _imgBoxes = null;


        public TestForm()
        {
            InitializeComponent();

            _imgBoxes = new PictureBox[]
            { 
                pictureBox3,
                pictureBox4,
                pictureBox5,
                pictureBox6,
                pictureBox7,
                pictureBox8,
                pictureBox9,
                pictureBox10,
                pictureBox11,
                pictureBox12
            };
        }



        private void Process(string fileName)
        {
            Console.WriteLine("Processing " + fileName);

            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            //Process
            watch.Start();
            Bitmap normal = ImageProcessing.Load(fileName);
            watch.Stop();
            Console.WriteLine("Test 1: " + watch.ElapsedMilliseconds);
            double trash = (double)tmpValue.Value;
            watch.Start();
            Image filtered = ImageProcessing.Grayscale(normal);
            watch.Stop();
            Console.WriteLine("Test 2: " + watch.ElapsedMilliseconds);
            watch.Reset();
            watch.Start();
            //Image filtered2 = ImageProcessing.FilterBlack((Bitmap)filtered, this.CreateGraphics());
            filtered = ImageProcessing.FilterBlack((Bitmap)filtered, trash);
            watch.Stop();
            Console.WriteLine("Test 3: " + watch.ElapsedMilliseconds);

            //if (true)
            //{
            //    imgOutput.Image = normal;
            //    pictureBox3.Image = filtered;
            //    pictureBox4.Image = filtered2;
            //    return;
            //}

            watch.Reset();
            watch.Start();
            IDictionary<int, Bitmap> images = new CCL().Process((Bitmap)filtered);
            watch.Stop();
            Console.WriteLine("Test 4: " + watch.ElapsedMilliseconds);

            //Debugging
            //Console.WriteLine("Trashhold: " + trash);
            Console.WriteLine("images founds 1: " + images.Count);

            //Clear
            foreach (PictureBox img in _imgBoxes)
            {
                img.Image = null;
            }
            _lastBitmaps.Clear();

            //Caching
            foreach (KeyValuePair<int, Bitmap> img in images)
            {
                Bitmap b = img.Value;
                //Console.WriteLine(string.Format("Caching. Size: {0}x{1}", b.Width, b.Height));
                if (b.Width <= 30 || b.Height <= 30 || b.Width > 200 || b.Height > 200)
                    continue;
                b = ImageProcessing.Resize(b, 28, 28);
                _lastBitmaps.Add(b);
            }
            Console.WriteLine("images remained: " + _lastBitmaps.Count);

            //Refresh
            int counter = 0;
            foreach (Bitmap img in _lastBitmaps)
            {
                if (counter >= _imgBoxes.Length) break;
                _imgBoxes[counter].Image = img;
                counter++;
            }
            imgInput.Image = normal;
            imgOutput.Image = filtered;
        }

        public string GetCurrFilePath()
        {
            return @".\Data\Plates\Testing\" + _currIndex + ".jpg";
        }

        private void tmpTest_Click(object sender, EventArgs e)
        {
            string filePath = GetCurrFilePath();
            if (!File.Exists(filePath))
            {
                MessageBox.Show("File not exists.");
                return;
            }
            Process(filePath);
        }

        private void tmpNext_Click(object sender, EventArgs e)
        {
            ++_currIndex;
            string filePath = GetCurrFilePath();
            if (!File.Exists(filePath))
            {
                MessageBox.Show("File not exists.");
                --_currIndex;
                return;
            }
            Process(filePath);
        }

        private void tmpPrev_Click(object sender, EventArgs e)
        {
            --_currIndex;

            string filePath = GetCurrFilePath();
            if (_currIndex<=0 || !File.Exists(filePath))
            {
                MessageBox.Show("File not exists.");
                ++_currIndex;
                return;
            }
            Process(filePath);
        }

        private void tmpCheck_Click(object sender, EventArgs e)
        {
            int index = (int)tmpValue2.Value;
            string filePath = @".\Data\Plates\Testing\" + index + ".jpg";
            if (!File.Exists(filePath)) return;
            Process(filePath);
        }

        int lastId = 1;
        private byte[] IntToBytes(int value)
        {
            byte[] bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);
            return bytes;
        }

        private int BytesToInt(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
                Array.Reverse(bytes);

            return BitConverter.ToInt32(bytes, 0);
        }


        private void SaveInt(string filePath, int value)
        {
            FileStream stream = File.OpenWrite(filePath);
            lastId = 1;
            byte[] bytes = IntToBytes(value);
            stream.Write(bytes, 0, bytes.Length);
            stream.Close();
        }

        private int ReadInt(string filePath, int defaultValue = 0)
        {
            if (!File.Exists(filePath))
                return defaultValue;

            FileStream stream = File.OpenRead(filePath);
            byte[] bytes = new byte[200];
            int count = stream.Read(bytes, 0, bytes.Length);
            stream.Close();
            Array.Resize(ref bytes, count);
            return BytesToInt(bytes);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (_lastBitmaps.Count == 0)
            {
                MessageBox.Show("No Bitmaps to save!");
                return;
            }
            string lastIdFile = @".\Data\Plates\Testing\FoundNumbers\LastId.txt";

            if (!File.Exists(lastIdFile))
                lastId = 1;
            else
                lastId = ReadInt(lastIdFile, 1);
            Console.WriteLine("lastId: " + lastId);


            string imagePath1 = @".\Data\Plates\Testing\FoundNumbers\";
            string imagePath2 = ".jpg";
            foreach(Bitmap b in _lastBitmaps)
            {
                b.Save(imagePath1 + (++lastId) + imagePath2);
            }
            Console.WriteLine("Saved images: " + _lastBitmaps.Count);

            SaveInt(lastIdFile, lastId);
        }

        NeuralNetwork nn = new NeuralNetwork();
        List<double[,]> trainData = new List<double[,]>();
        List<double[]> correctAnwers = new List<double[]>();
        private void btnTrain_Click(object sender, EventArgs e)
        {
            string imagePath = @".\Data\Plates\Testing\FoundNumbers";

            string[] files = {
                imagePath + @"\2.jpg",
                imagePath + @"\3.jpg",
                imagePath + @"\4.jpg",
                imagePath + @"\5.jpg",
                imagePath + @"\6.jpg"
            };//= Directory.GetFiles(imagePath);
            double[][] answers = new double[5][]{
                new double[]{ 0.9,0,0,0,0,0 },
                new double[]{ 0.9, 0,0,0,0,0 },
                new double[]{ 0,0,0, 0.9, 0,0 },
                new double[]{ 0,0,0,0,0,0.9 },
                new double[]{ 0,0, 0.9, 0,0,0 },
            };

            int counter = 0;
            foreach (string file in files)
            {
                double[,] data = ImageToByteArray(file);
                double[] answer = answers[counter++];
                trainData.Add(data);
                correctAnwers.Add(answer);
            }

            nn.Init();
            nn.Train(trainData, correctAnwers);
        }

        public double[,] ImageToByteArray(string filePath)
        {
            unsafe
            {
                Bitmap input = (Bitmap)Image.FromFile(filePath);
                BitmapData bitmapData = input.LockBits(new Rectangle(0, 0, input.Width, input.Height), ImageLockMode.ReadWrite, input.PixelFormat);
                int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(input.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

                double[,] output = new double[input.Width, input.Height];
                for (int y = 0, a=0; y < heightInPixels; y++, ++a)
                {
                    byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0, b=0; x < widthInBytes; x = x + bytesPerPixel, ++b)
                    {
                        int oldBlue = currentLine[x];
                        int oldGreen = currentLine[x + 1];
                        int oldRed = currentLine[x + 2];

                        double br = 0.2126 * currentLine[x]
                            + 0.7152 * currentLine[x + 1]
                            + 0.0722 * currentLine[x + 2];
                        output[a, b] = br;
                    }
                }
                input.UnlockBits(bitmapData);

                return output;
            }
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            nn.Test(trainData, correctAnwers);
        }

        private void btnTestShit_Click(object sender, EventArgs e)
        {
            Helper h = new Helper();

            double[,] image = new double[,]
            {
                { 3, 0, 1, 1 },
                { 3, 7, 9, 2 },
                { 5, 2, 4, 7 },
                { 3, 2, 1, 0 },
            };
            double[,] image2 = new double[,]
            {
                { 0, 0, 0, 2 },
                { 0, 1, 1, 0 },
                { 0, 0, 0, 0 },
                { 0, 0, 1, 0 },
            };
            double[,] filter = new double[,]
            {
                { 0.5, 0.5, 0.5 },
                { 0.5, 0.5, 0.5 },
                { 0.5, 0.5, 0.5 }
            };
            double[,] filter2 = new double[,]
            {
                { 1, 1, 1 },
                { 1, 1, 1 },
                { 1, 1, 1 },
            };
            double[,] filter3 = new double[,]
            {
                { 1, 1},
                { 1, 1},
            };

            ////Test Max Pooling
            //KeyValuePair<double[,], int[]> result = h.Pooling_Max(image, 2);
            //h.ShowMatrix(result.Key);
            //h.ShowVector(result.Value);
            //Console.WriteLine("--------");

            //Test Cross-correlation
            //double[,] result = h.CrossCorrelation(image2, filter2);
            //h.ShowMatrix(image);
            //Console.WriteLine("--------");
            //h.ShowMatrix(result);
            //Console.WriteLine("--------");

            ////Test flattening
            //double[] result = h.MatrixToVector(image);
            //h.ShowVector(result);

            ////Test activation & input functions
            //double[] v = new double[] { 1, 1, 1, 2, 1 };
            //double[] w = new double[] { 1, 1, 1, 1, 1 };
            //double sum = h.Sum(v,w);
            //FCLayer layer = new FCLayer(null,NeuralNetwork.ActivationFunction.Sigmoid, NeuralNetwork.DeactivationFunction.Sigmoid);
            //double actValue = layer.Activate(sum);
            //Console.WriteLine("Sum: " + sum);
            //Console.WriteLine("Activation: " + actValue);

            ////Test Backpropagation: Pool Layer
            //double[,] result2 = BackpropPoolLayer(result.Key, result.Value);
            //h.ShowMatrix(result2);

            //Test Backpropagation: CrossCorrelation Layer
            double[,] image3 = new double[,]
            {
                { 0, 2, 0},
                { 0, 1, 1},
                { 0, 0, 0},
            };
            double[,] result = h.Convolution(image3, filter3);
            KeyValuePair<double[,], double[,]> result2 = BackpropagateCrossCorrelationLayer(image3, result, filter3);
            h.ShowMatrix(image3);
          //  Console.WriteLine("----$$----");
            h.ShowMatrix(result);
            Console.WriteLine("----$$----");
            h.ShowMatrix(result2.Key);
            Console.WriteLine("--------");
            h.ShowMatrix(result2.Value);
        }

        public double[,] BackpropPoolLayer(double[,] errors, int[] sparseMatrix)
        {
            Helper h = new Helper();
            double[,] poolingErrors = h.ReversePooling(errors, sparseMatrix, 2);
            return poolingErrors;
        }

        private KeyValuePair<double[,], double[,]> BackpropagateCrossCorrelationLayer(double[,] input, double[,] errors, 
            double[,] filter)
        {
            Helper h = new Helper();
            //CrossCorrelationLayer layer = (CrossCorrelationLayer)layers.ElementAt(nrLayer);
            //double[,] filter = layer.filter;
            //double[,] inputImg = layer.input;
            //double[,] outputImg = layer.output;

            //---calculate image errors
            double[,] imgErrors = h.CrossCorrelation(errors, filter);

            //---calculate filter weight errors
            double[,] filterErrors = h.Convolution(input, errors);

            return new KeyValuePair<double[,], double[,]>(imgErrors, filterErrors);
        }
    }
}
