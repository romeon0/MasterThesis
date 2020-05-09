using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
//using System.ComponentModel.Composition;
using System.Linq;

namespace ConnectedComponentLabeling
{
   // [Export(typeof(IConnectedComponentLabeling))]
   // [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CCL : IConnectedComponentLabeling
    {
        #region Member Variables

        private int[,] _board;
        private Bitmap _input;
        private int _width;
        private int _height;

        #endregion

        #region IConnectedComponentLabeling

        public IDictionary<int, Bitmap> Process(Bitmap input)
        {
            _input = input;
            _width = input.Width;
            _height = input.Height;
            _board = new int[_width, _height];

            Dictionary<int, List<Pixel>> patterns = Find();
            var images = new Dictionary<int, Bitmap>();

            foreach (KeyValuePair<int, List<Pixel>> pattern in patterns)
            {
                Bitmap bmp = CreateBitmap(pattern.Value);
                images.Add(pattern.Key, bmp);
            }

            return images;
        }

        #endregion

        #region Protected Methods

        protected virtual bool CheckIsBackGround(Pixel currentPixel)
        {
            //Console.WriteLine(string.Format("COlor: {0},{1},{2}",
            //    currentPixel.color.R,
            //    currentPixel.color.G,
            //    currentPixel.color.B));
            return currentPixel.color.A == 255 && currentPixel.color.R == 255 && currentPixel.color.G == 255 && currentPixel.color.B == 255;
        }

        #endregion

        #region Private Methods

        private Dictionary<int, List<Pixel>> Find()
        {
            int labelCount = 1;
            var allLabels = new Dictionary<int, Label>();

            for (int i = 0; i < _height; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    Pixel currentPixel = new Pixel(new Point(j, i), _input.GetPixel(j, i));

                    if (CheckIsBackGround(currentPixel))
                    {
                        continue;
                    }

                    IEnumerable<int> neighboringLabels = GetNeighboringLabels(currentPixel);
                    int currentLabel;
                    //Console.WriteLine(string.Format("Pixel[{0},{1}]: {2}",
                    //    j, i, neighboringLabels.Count()));
                    if (!neighboringLabels.Any())
                    {
                        currentLabel = labelCount;
                        allLabels.Add(currentLabel, new Label(currentLabel));
                        labelCount++;
                    }
                    else
                    {
                        currentLabel = neighboringLabels.Min(n => allLabels[n].GetRoot().Name);
                        Label root = allLabels[currentLabel].GetRoot();

                        foreach (var neighbor in neighboringLabels)
                        {
                            if (root.Name != allLabels[neighbor].GetRoot().Name)
                            {
                                allLabels[neighbor].Join(allLabels[currentLabel]);
                            }
                        }
                    }

                    _board[j, i] = currentLabel;
                }
            }

            //string str = "";
            //for(int a=0; a < _height; ++a)
            //{
            //    for (int b = 0; b < _width; ++b)
            //    {
            //        str+=(_board[b, a] + "\t");
            //    }
            //    str += ("\n");
            //}
            //Console.WriteLine(str);

            Dictionary<int, List<Pixel>> patterns = AggregatePatterns(allLabels);

            return patterns;
        }

        private Dictionary<int, List<Pixel>> Find_new()
        {
            int labelCount = 1;
            var allLabels = new Dictionary<int, Label>();

            unsafe
            {
                Bitmap output = _input;
                BitmapData bitmapData = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.ReadWrite, output.PixelFormat);
                int bytesPerPixel = System.Drawing.Bitmap.GetPixelFormatSize(output.PixelFormat) / 8;
                int heightInPixels = bitmapData.Height;
                int widthInBytes = bitmapData.Width * bytesPerPixel;
                byte* ptrFirstPixel = (byte*)bitmapData.Scan0;

                Console.WriteLine("WxH 1: " + _input.Width + "x" + _input.Height);
                Console.WriteLine("WxH 2: " + widthInBytes + "x" + heightInPixels);

                for (int j = 0,x=0; j < heightInPixels; j++, ++x)
                {
                    byte* currentLine = ptrFirstPixel + (j * bitmapData.Stride);
                    for (int i = 0,y=0; i < widthInBytes; i = i + bytesPerPixel, ++y)
                    {
                        int b = currentLine[i];
                        int g = currentLine[i + 1];
                        int r = currentLine[i + 2];
                        
                        Pixel currentPixel = new Pixel(new Point(j, i), 
                            Color.FromArgb(r, g, b));

                        if (CheckIsBackGround(currentPixel))
                        {
                            continue;
                        }

                        IEnumerable<int> neighboringLabels = GetNeighboringLabels(currentPixel);
                        int currentLabel;
             
                        if (!neighboringLabels.Any())
                        {
                            currentLabel = labelCount;
                            allLabels.Add(currentLabel, new Label(currentLabel));
                            labelCount++;
                        }
                        else
                        {
                            currentLabel = neighboringLabels.Min(n => allLabels[n].GetRoot().Name);
                            Label root = allLabels[currentLabel].GetRoot();

                            foreach (var neighbor in neighboringLabels)
                            {
                                if (root.Name != allLabels[neighbor].GetRoot().Name)
                                {
                                    allLabels[neighbor].Join(allLabels[currentLabel]);
                                }
                            }
                        }

                        _board[y, x] = currentLabel;
                    }
                }
                output.UnlockBits(bitmapData);
            }

            for (int i = 0; i < _height; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    
                }
            }

            Dictionary<int, List<Pixel>> patterns = AggregatePatterns(allLabels);

            return patterns;
        }

        private IEnumerable<int> GetNeighboringLabels(Pixel pix)
        {
            var neighboringLabels = new List<int>();


            for (int i = pix.Position.Y - 1; i <= pix.Position.Y + 2 && i < _height - 1; i++)
            {
                for (int j = pix.Position.X - 1; j <= pix.Position.X + 2 && j < _width - 1; j++)
                {
                    if (i > -1 && j > -1 && _board[j, i] != 0)
                    {
                        neighboringLabels.Add(_board[j, i]);
                    }
                }
            }

            return neighboringLabels;
        }

        private Dictionary<int, List<Pixel>> AggregatePatterns(Dictionary<int, Label> allLabels)
        {
            var patterns = new Dictionary<int, List<Pixel>>();

            for (int i = 0; i < _height; i++)
            {
                for (int j = 0; j < _width; j++)
                {
                    int patternNumber = _board[j, i];

                    if (patternNumber != 0)
                    {
                        patternNumber = allLabels[patternNumber].GetRoot().Name;

                        if (!patterns.ContainsKey(patternNumber))
                        {
                            patterns[patternNumber] = new List<Pixel>();
                        }

                        patterns[patternNumber].Add(new Pixel(new Point(j, i), Color.Black));
                    }
                }
            }

            return patterns;
        }

        private Bitmap CreateBitmap(List<Pixel> pattern)
        {
            int minX = pattern.Min(p => p.Position.X);
            int maxX = pattern.Max(p => p.Position.X);

            int minY = pattern.Min(p => p.Position.Y);
            int maxY = pattern.Max(p => p.Position.Y);

            int width = maxX + 1 - minX;
            int height = maxY + 1 - minY;

            var bmp = new Bitmap(width, height);

            foreach (Pixel pix in pattern)
            {
                bmp.SetPixel(pix.Position.X - minX, pix.Position.Y - minY, pix.color);//shift position by minX and minY
            }

            return bmp;
        }

        #endregion
    }
}
