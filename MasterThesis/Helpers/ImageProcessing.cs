using ImageProcessor;
using ImageProcessor.Imaging;
using ImageProcessor.Imaging.Filters.EdgeDetection;
using ImageProcessor.Imaging.Formats;
using Neuroevolution_Application.Extensions.ArrayExtensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neuroevolution_Application.Helpers
{
    class ImageProcessing
    {
        public static Bitmap Load(string filePath)
        {
            return new Bitmap(filePath);
        }


        //public static Bitmap FilterBlack(Bitmap file, double trashhold = 0.8)
        //{
        //    Bitmap img = new Bitmap(file.Width, file.Height);
        //    for (int i = 0; i < file.Width; i++)
        //    {
        //        for (int j = 0; j < file.Height; j++)
        //        {
        //            Color pixel = file.GetPixel(i, j);
        //            //Console.WriteLine("Br: " + (pixel.GetBrightness()));
        //            //Console.WriteLine(string.Format("Color: {0},{1},{2}",
        //            //    pixel.R, pixel.G, pixel.B));
        //            double br = pixel.GetBrightness();
        //            if (br > trashhold)
        //                img.SetPixel(i, j, Color.White);
        //            else
        //                //img.SetPixel(i, j, Color.FromArgb((int)(255), 0, 0));
        //                img.SetPixel(i, j, Color.Black);//black

        //        }
        //    }


        //    return img;
        //}

        public static byte[] ImageToByte(Image img)
        {
            var stream = new MemoryStream();
            Console.WriteLine("stream: " + stream);
            Console.WriteLine("img.RawFormat: " + img.RawFormat);

            EncoderParameters encParams = new EncoderParameters(1);
            encParams.Param[0] = new
              EncoderParameter(System.Drawing.Imaging.Encoder.Compression,
              (long)EncoderValue.CompressionCCITT4);
            string mimeType = "image/png";
            ImageCodecInfo imageCodec = ImageCodecInfo.GetImageEncoders().
              Where(x => x.MimeType == mimeType).First();

            img.Save(stream, imageCodec, encParams);
            byte[] bytes = stream.ToArray();
            stream.Close();
            return bytes;
        }

       
        public static Bitmap FilterBlack(Image img, double trashhold = 0.4)
        {
            unsafe
            {
                Bitmap output = new Bitmap(img);
                BitmapData bitmapData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.ReadWrite, output.PixelFormat);
                int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(output.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

                for (int y = 0; y < heightInPixels; y++)
                {
                    byte* currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                    for (int x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        int oldBlue = currentLine[x];
                        int oldGreen = currentLine[x + 1];
                        int oldRed = currentLine[x + 2];

                        double br = 0.2126 * currentLine[x]
                            + 0.7152 * currentLine[x + 1]
                            + 0.0722 * currentLine[x + 2];

                        if (br < trashhold)
                        {
                            currentLine[x] = (byte)0;
                            currentLine[x + 1] = (byte)0;
                            currentLine[x + 2] = (byte)0;
                        }
                        else
                        {
                            currentLine[x] = (byte)255;
                            currentLine[x + 1] = (byte)255;
                            currentLine[x + 2] = (byte)255;
                        }
                    }
                }
                output.UnlockBits(bitmapData);

                return output;
            }
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap Resize(Image image, int width, int height)
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

        public static Bitmap FilterBlack(string filePath, double trashhold = 0.8)
        {
            Bitmap img = Load(filePath);
            return null;// return FilterBlack(img, trashhold);
        }

        public static double[,] FilterBlackGetBinary(string filePath, double trashhold=0.8)
        {
            Bitmap img = Load(filePath);
            double[,] filtered = new double[img.Width, img.Height];
            for (int i = 0; i < img.Width; i++)
            {
                for (int j = 0; j < img.Height; j++)
                {
                    Color pixel = img.GetPixel(i, j);

                    if (pixel.GetBrightness() > trashhold)
                        filtered[i, j] = 0;
                    else
                        filtered[i, j] = 1;
                }
            }

            return filtered;
        }


        //INFO: 
        public static Bitmap FilterImage(string filePath, IEdgeFilter filter)
        {
            byte[] photoBytes = File.ReadAllBytes(filePath);
            // Format is automatically detected though can be changed.
            ISupportedImageFormat format = new PngFormat { Quality = 70 };
            using (MemoryStream inStream = new MemoryStream(photoBytes))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    // Initialize the ImageFactory using the overload to preserve EXIF metadata.
                    using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                    {
                        // Load, resize, set the format and quality and save an image.
                        imageFactory.Load(inStream)
                                    //.Resize(size)
                                    .Format(format)
                                  //  .DetectEdges(filter)
                                    .Contrast(100)
                                    .Save(outStream);
                    }
                    // Do something with the stream.

                    return (Bitmap)Bitmap.FromStream(outStream);
                }
            }
        }

        public static Bitmap Grayscale(Bitmap original)
        {
            //create a blank bitmap the same size as original
            Bitmap newBitmap = new Bitmap(original.Width, original.Height);

            //get a graphics object from the new image
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                //create the grayscale ColorMatrix
                ColorMatrix colorMatrix = new ColorMatrix(
                   new float[][]
                   {
                         new float[] {.3f, .3f, .3f, 0, 0},
                         new float[] {.59f, .59f, .59f, 0, 0},
                         new float[] {.11f, .11f, .11f, 0, 0},
                         new float[] {0, 0, 0, 1, 0},
                         new float[] {0, 0, 0, 0, 1}
                   });

                //create some image attributes
                using (ImageAttributes attributes = new ImageAttributes())
                {

                    //set the color matrix attribute
                    attributes.SetColorMatrix(colorMatrix);

                    //draw the original image on the new image
                    //using the grayscale color matrix
                    g.DrawImage(original, new Rectangle(0, 0, original.Width, original.Height),
                                0, 0, original.Width, original.Height, GraphicsUnit.Pixel, attributes);
                }
            }
            return newBitmap;
        }
    }
}
