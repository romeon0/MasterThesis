//using Emgu.CV;
//using Emgu.CV.CvEnum;
//using Emgu.CV.Structure;
//using Emgu.CV.Util;
//using ImageProcessor;
//using ImageProcessor.Imaging.Filters.EdgeDetection;
//using ImageProcessor.Imaging.Formats;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Drawing;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Windows.Forms;

//namespace Inov8ing_2020
//{
//    class Testing
//    {
//        private bool _dragging;
//        private Point dragging_start_point;

//        private double cannyThreshold1 = 120.0;
//        private double cannyThreshold2 = 180.0;
//        private double circleAccumulatorThreshold = 120;

//        private Capture capture;
//        private Mat OriginalCAM;
//        private Mat GRAYCAM;
//        private Mat pyrDown;
//        private Mat cannyEdges;
//        private int choice;
//        private VectorOfVectorOfPoint contours;
//        private VectorOfPoint contour;
//        private VectorOfPoint approxContour;

//        List<Triangle2DF> triangleList;
//        List<RotatedRect> boxList;
//        CircleF[] circles;

//        Image<Bgr, byte> inputImg_;

//        /// <summary>
//        /// TMP
//        /// </summary>
//        private System.Windows.Forms.PictureBox imgInput;
//        private System.Windows.Forms.PictureBox imgOutput;
//        private System.Windows.Forms.PictureBox imgOutput2;
//        private Emgu.CV.UI.ImageBox CameraBox;


//        private Bitmap ConvolveThis(string filePath)
//        {

//            //Detect white
//            Bitmap img = new Bitmap(filePath);
//            for (int i = 0; i < img.Width; i++)
//            {
//                for (int j = 0; j < img.Height; j++)
//                {
//                    Color pixel = img.GetPixel(i, j);

//                    if (pixel.GetBrightness() > 0.8)
//                        img.SetPixel(i, j, pixel);
//                    else
//                        img.SetPixel(i, j, Color.FromArgb(0, 0, 0));

//                }
//            }

//            // img.Save(@"D:\Projects\C#\Inov8ing_2020\Inov8ing_2020\bin\Debug\Files\number_plate_2.jpg");

//            //imgOutput.Image = Convolution(filePath, new SobelEdgeFilter());
//            //imgOutput2.Image = Convolution(filePath, new SobelEdgeFilter2());

//            return img;
//        }


//        //INFO: Apply convolution to an image using a filter
//        public double[,] Convolution(double[,] image, double[,] filter)
//        {
//            int filterSize = filter.GetLength(0);
//            int srcImgHeight = image.GetLength(0);
//            int srcImgWidth = image.GetLength(1);
//            int destImgHeight = srcImgHeight - filterSize + 1;
//            int destImgWidth = srcImgWidth - filterSize + 1;

//            filter = FlipKernel(filter);

//            double[,] output = new double[destImgHeight, destImgWidth];
//            for (int y = 0; y < destImgHeight; ++y)
//            {
//                for (int x = 0; x < destImgWidth; ++x)
//                {
//                    output[y, x] = 0.0;
//                    for (int filterY = 0; filterY < filterSize; ++filterY)
//                    {
//                        for (int filterX = 0; filterX < filterSize; ++filterX)
//                        {
//                            output[y, x] += filter[filterY, filterX] * image[y + filterY, x + filterX];
//                        }
//                    }

//                }
//            }

//            return output;
//        }


//        private class SobelEdgeFilter : IEdgeFilter
//        {
//            private readonly double[,] filter =
//            {
//              { 1, 2, 1},
//                { 0, 0, 0},
//                { -1, -2, -1}
//            };

//            private readonly double[,] filter2 =
//            {
//                { 2, 4, 5, 4, 2 },
//                { 4, 9, 12, 9, 4 },
//                { 5, 12, 15, 12, 5 },
//                { 4, 9, 12, 9, 4 },
//                { 2, 4, 5, 4, 2 },
//            };
//            public double[,] HorizontalGradientOperator
//            {
//                get
//                {
//                    double[,] f = new double[filter2.GetLength(0), filter2.GetLength(1)];
//                    for (int a = 0; a < filter2.GetLength(0); ++a)
//                        for (int b = 0; b < filter2.GetLength(1); ++b)
//                            f[a, b] = filter2[a, b] * (1f / 159);
//                    return filter;
//                }
//            }
//        }

//        private class SobelEdgeFilter2 : IEdgeFilter
//        {
//            private readonly double[,] filter =
//            {
//                { -1, 0, 1},
//                { -2, 0, 2},
//                { -1, 0, 1}
//            };

//            public double[,] HorizontalGradientOperator
//            {
//                get
//                {
//                    return filter;
//                }
//            }
//        }


//        //INFO: 
//        public Image Convolution(string filePath, IEdgeFilter filter)
//        {



//            byte[] photoBytes = File.ReadAllBytes(filePath);
//            // Format is automatically detected though can be changed.
//            ISupportedImageFormat format = new JpegFormat { Quality = 70 };
//            Size size = new Size(150, 0);
//            using (MemoryStream inStream = new MemoryStream(photoBytes))
//            {
//                using (MemoryStream outStream = new MemoryStream())
//                {
//                    // Initialize the ImageFactory using the overload to preserve EXIF metadata.
//                    using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
//                    {
//                        // Load, resize, set the format and quality and save an image.
//                        imageFactory.Load(inStream)
//                                    //.Resize(size)
//                                    .Format(format)
//                                    .DetectEdges(filter)
//                                    .Save(outStream);

//                    }
//                    // Do something with the stream.

//                    return Image.FromStream(outStream);
//                }

//            }
//        }

//        //INFO: 180 grade rotate of matrix
//        public double[,] FlipKernel(double[,] filter)
//        {

//            int filterSizeY = filter.GetLength(0);
//            int filterSizeX = filter.GetLength(1);
//            double[,] rotatedFilter = new double[filterSizeY, filterSizeX];
//            for (int filterY = 0; filterY < filterSizeY; ++filterY)
//            {
//                for (int filterX = 0; filterX < filterSizeX; ++filterX)
//                {
//                    rotatedFilter[filterSizeY - 1 - filterY, filterSizeX - 1 - filterX] = filter[filterY, filterX];
//                }
//            }
//            return rotatedFilter;
//        }



//        private void OnImageFromCameraGrabbed(object obj, EventArgs e)
//        {
//            try
//            {
//                OriginalCAM = new Mat();
//                GRAYCAM = new Mat();
//                pyrDown = new Mat();
//                cannyEdges = new Mat();

//                //Get the camera capture
//                capture.Retrieve(OriginalCAM, 0);
//                //Image to grayscale
//                CvInvoke.CvtColor(OriginalCAM, GRAYCAM, ColorConversion.Bgr2Gray);
//                //Some contrast tweaks
//                CvInvoke.EqualizeHist(GRAYCAM, GRAYCAM);
//                //Some operations -_-
//                //     CvInvoke.PyrDown(GRAYCAM, pyrDown);
//                //    CvInvoke.PyrUp(pyrDown, GRAYCAM);
//                //Detect edges
//                CvInvoke.Canny(GRAYCAM, cannyEdges, cannyThreshold1, cannyThreshold2);

//                //Show image o Form 
//                CameraBox.Image = cannyEdges;
//            }
//            catch { }



//            triangleList = new List<Triangle2DF>();
//            boxList = new List<RotatedRect>();
//            contours = new VectorOfVectorOfPoint();


//            CvInvoke.FindContours(cannyEdges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);

//            int count = contours.Size;
//            for (int i = 0; i < count; i++)
//            {
//                contour = contours[i];
//                approxContour = new VectorOfPoint();
//                CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);

//                //Console.WriteLine("ContourArea: " + CvInvoke.ContourArea(approxContour, false));
//                if (CvInvoke.ContourArea(approxContour, false) > 250) //only consider contours with area greater than 250
//                {
//                    if (approxContour.Size == 4) //The contour has 4 vertices.
//                    {

//                        bool isRectangle = true;
//                        Point[] pts = approxContour.ToArray();
//                        LineSegment2D[] edges = PointCollection.PolyLine(pts, true);

//                        for (int j = 0; j < edges.Length; j++)
//                        {
//                            double angle = Math.Abs(
//                                edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
//                            if (angle < 80 || angle > 100)
//                            {
//                                isRectangle = false;
//                                break;
//                            }
//                        }

//                        if (isRectangle)
//                        {
//                            Console.WriteLine("ContourArea: rectangle found!");
//                            boxList.Add(CvInvoke.MinAreaRect(approxContour));
//                        }
//                    }
//                }
//            }


//            foreach (RotatedRect box in boxList)
//            {
//                CvInvoke.Polylines(cannyEdges,
//                    Array.ConvertAll(box.GetVertices(), Point.Round),
//                    true,
//                    new Bgr(Color.Red).MCvScalar, 2);
//            }

//            CameraBox.Image = cannyEdges;
//            GC.Collect();
//        }


//        private void HelloTest(string fileName)
//        {
//            StringBuilder msgBuilder = new StringBuilder("Performance: ");

//            //Load the image from file and resize it for display
//            Image<Bgr, Byte> img =
//               new Image<Bgr, byte>(fileName);
//            // .Resize(400, 400, Emgu.CV.CvEnum.Inter.Linear, true);

//            //Convert the image to grayscale and filter out the noise
//            UMat uimage = new UMat();
//            CvInvoke.CvtColor(img, uimage, ColorConversion.Bgr2Gray);

//            //use image pyr to remove noise
//            UMat pyrDown = new UMat();
//            CvInvoke.PyrDown(uimage, pyrDown);
//            CvInvoke.PyrUp(pyrDown, uimage);

//            //Image<Gray, Byte> gray = img.Convert<Gray, Byte>().PyrDown().PyrUp();

//            Stopwatch watch = Stopwatch.StartNew();
//            double cannyThreshold = 180.0;


//            #region Canny and edge detection
//            watch.Reset(); watch.Start();
//            double cannyThresholdLinking = 120.0;
//            UMat cannyEdges = new UMat();
//            CvInvoke.Canny(uimage, cannyEdges, cannyThreshold, cannyThresholdLinking);

//            LineSegment2D[] lines = CvInvoke.HoughLinesP(
//               cannyEdges,
//               1, //Distance resolution in pixel-related units
//               Math.PI / 45.0, //Angle resolution measured in radians.
//               20, //threshold
//               30, //min Line width
//               10); //gap between lines

//            watch.Stop();
//            msgBuilder.Append(String.Format("Canny & Hough lines - {0} ms; ", watch.ElapsedMilliseconds));
//            #endregion

//            #region Find triangles and rectangles
//            watch.Reset(); watch.Start();
//            List<Triangle2DF> triangleList = new List<Triangle2DF>();
//            List<RotatedRect> boxList = new List<RotatedRect>(); //a box is a rotated rectangle

//            using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
//            {
//                CvInvoke.FindContours(cannyEdges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
//                int count = contours.Size;
//                for (int i = 0; i < count; i++)
//                {
//                    using (VectorOfPoint contour = contours[i])
//                    using (VectorOfPoint approxContour = new VectorOfPoint())
//                    {
//                        CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
//                        if (CvInvoke.ContourArea(approxContour, false) > 250) //only consider contours with area greater than 250
//                        {
//                            if (approxContour.Size == 3) //The contour has 3 vertices, it is a triangle
//                            {
//                                Point[] pts = approxContour.ToArray();
//                                triangleList.Add(new Triangle2DF(
//                                   pts[0],
//                                   pts[1],
//                                   pts[2]
//                                   ));
//                            }
//                            else if (approxContour.Size == 4) //The contour has 4 vertices.
//                            {
//                                #region determine if all the angles in the contour are within [80, 100] degree
//                                bool isRectangle = true;
//                                Point[] pts = approxContour.ToArray();
//                                LineSegment2D[] edges = PointCollection.PolyLine(pts, true);

//                                for (int j = 0; j < edges.Length; j++)
//                                {
//                                    double angle = Math.Abs(
//                                       edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
//                                    if (angle < 80 || angle > 100)
//                                    {
//                                        isRectangle = false;
//                                        break;
//                                    }
//                                }
//                                #endregion

//                                if (isRectangle) boxList.Add(CvInvoke.MinAreaRect(approxContour));
//                            }
//                        }
//                    }
//                }
//            }

//            watch.Stop();
//            msgBuilder.Append(String.Format("Triangles & Rectangles - {0} ms; ", watch.ElapsedMilliseconds));
//            #endregion

//            CameraBox.Image = img;


//            msgBuilder.Append("\n Boxes: " + boxList.Count);
//            msgBuilder.Append("\n Triangle: " + triangleList.Count);
//            msgBuilder.Append("\n Lines: " + lines.Length);

//            #region draw triangles and rectangles
//            Image<Bgr, Byte> triangleRectangleImage = img.CopyBlank();
//            foreach (Triangle2DF triangle in triangleList)
//                triangleRectangleImage.Draw(triangle, new Bgr(Color.DarkBlue), 2);
//            foreach (RotatedRect box in boxList)
//                triangleRectangleImage.Draw(box, new Bgr(Color.DarkOrange), 2);
//            // CameraBox.Image = triangleRectangleImage;
//            #endregion

//            #region draw lines
//            Image<Bgr, Byte> lineImage = img.CopyBlank();
//            foreach (LineSegment2D line in lines)
//                lineImage.Draw(line, new Bgr(Color.Green), 2);
//            CameraBox.Image = lineImage;
//            #endregion


//            Console.WriteLine(msgBuilder.ToString());
//        }

//        private void LoadImage_(string fileName)
//        {

//            string filePath = @".\Files\number_plate.jpg";
//            Bitmap img = ConvolveThis(filePath);

//            //imgInput.Image = img;
//            imgInput.Image = Image.FromFile(filePath);
//            //imgInput.Image.Save("./Files/temp.png",ImageFormat.Png);

//            inputImg_ = new Image<Bgr, byte>(filePath);

//        }


//        private void DetectShapes_()
//        {
//            if (imgInput == null)
//            {
//                return;
//            }

//            try
//            {

//                var temp = inputImg_
//                   .SmoothGaussian(5)
//                  .Canny(0, 0)
//                  .Convert<Gray, byte>()
//                  .ThresholdBinaryInv(new Gray(0), new Gray(250));

//                imgOutput.Image = temp.Bitmap;

//                VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
//                Mat m = new Mat();

//                CvInvoke.FindContours(temp, contours, m, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);

//                for (int i = 0; i < contours.Size; i++)
//                {
//                    double perimeter = CvInvoke.ArcLength(contours[i], true);
//                    VectorOfPoint approx = new VectorOfPoint();
//                    CvInvoke.ApproxPolyDP(contours[i], approx, 0.04 * perimeter, true);

//                    CvInvoke.DrawContours(inputImg_, contours, i, new MCvScalar(0, 0, 255), 2);

//                    //moments  center of the shape

//                    var moments = CvInvoke.Moments(contours[i]);
//                    int x = (int)(moments.M10 / moments.M00);
//                    int y = (int)(moments.M01 / moments.M00);

//                    if (approx.Size == 3)
//                    {
//                        CvInvoke.PutText(inputImg_, "Triangle", new Point(x, y),
//                            Emgu.CV.CvEnum.FontFace.HersheySimplex, 0.5, new MCvScalar(0, 0, 255), 2);
//                    }

//                    if (approx.Size == 4)
//                    {
//                        Rectangle rect = CvInvoke.BoundingRectangle(contours[i]);

//                        double ar = (double)rect.Width / rect.Height;

//                        if (ar >= 0.95 && ar <= 1.05)
//                        {
//                            CvInvoke.PutText(inputImg_, "Square", new Point(x, y),
//                            Emgu.CV.CvEnum.FontFace.HersheySimplex, 1, new MCvScalar(0, 0, 255), 2);
//                        }
//                        else
//                        {
//                            CvInvoke.PutText(inputImg_, "Rectangle", new Point(x, y),
//                            Emgu.CV.CvEnum.FontFace.HersheySimplex, 1, new MCvScalar(0, 0, 255), 2);
//                        }

//                    }

//                    if (approx.Size == 6)
//                    {
//                        CvInvoke.PutText(inputImg_, "Hexagon", new Point(x, y),
//                            Emgu.CV.CvEnum.FontFace.HersheySimplex, 1, new MCvScalar(0, 0, 255), 2);
//                    }


//                    if (approx.Size > 6)
//                    {
//                        CvInvoke.PutText(inputImg_, "Circle", new Point(x, y),
//                            Emgu.CV.CvEnum.FontFace.HersheySimplex, 1, new MCvScalar(0, 0, 255), 2);
//                    }


//                }

//                // imgOutput.Image = inputImg_.Bitmap;
//                //   imgOutput.Image = temp.ToBitmap();

//            }
//            catch (Exception ex)
//            {
//                MessageBox.Show(ex.Message);
//            }
//        }

//        private void Test()
//        {
//            //capture = new Capture();
//            //capture.ImageGrabbed += OnImageFromCameraGrabbed;
//            //capture.FlipHorizontal = true;
//            //capture.Start();

//            //  HelloTest(@"D:\Projects\C#\Inov8ing_2020\Inov8ing_2020\bin\Debug\Files\number_plate.jpg");

//            LoadImage_(@"D:\Projects\C#\Inov8ing_2020\Inov8ing_2020\bin\Debug\Files\number_plate.png");
//            DetectShapes_();
//        }
//    }
//}
