﻿using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace ProceduralBitmap
{
    class Program
    {
        static void Texture(Bitmap bitmap_peaks, Bitmap bitmap_grass)
        {
            var target = new Bitmap(bitmap_peaks.Width, bitmap_peaks.Height, PixelFormat.Format32bppArgb);
            var graphics = Graphics.FromImage(target);
            //graphics.CompositingMode = CompositingMode.SourceOver;

            graphics.DrawImage(bitmap_peaks, 0, 0);
            graphics.DrawImage(bitmap_grass, 0, 0);

            if (File.Exists(@"C:\Users\mitch\source\repos\ProceduralBitmap\Texture.bmp"))
                File.Delete(@"C:\Users\mitch\source\repos\ProceduralBitmap\Texture.bmp"); //Replaces file

            target.Save(@"C:\Users\mitch\source\repos\ProceduralBitmap\Texture.bmp", ImageFormat.Bmp);
        }

        static void GrassGen(Bitmap bitmap_peaks) {
            //Function to generate grass based off a given bitmap
            int width = bitmap_peaks.Width;
            int height = bitmap_peaks.Height;

            Bitmap bitmap_grass = new Bitmap(width, height);

            Color Pixel;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Pixel = bitmap_peaks.GetPixel(x, y);

                    if (Pixel.R < 100)
                    {
                        bitmap_grass.SetPixel(x, y, Color.FromArgb(255, 0, Pixel.R, 0));
                    }
                }
            }

            Texture(bitmap_peaks, bitmap_grass);
        }
        static void BitmapPeaksLong(string seed)
        {   //Function to create a bitmap that would be used for hilly terrain
            //points are the 'peaks' in the bitmap

            //Seed current form is:  width,height.peak_num.peak_w1xpeak_h1 ... peakwnxpeak_hn
            //Since this is the long seed version peaks are already predefined so there is no randomness

            string[] seedarray = seed.Split('.'); //Stop seperates sections e.g. resolution.peak_num.peak_points

            string[] resolution = seedarray[0].Split(','); //Comma seperates values in sections e.g. width,height under resolution
            int width = Convert.ToInt16(resolution[0]); //Res will stay low for now
            int height = Convert.ToInt16(resolution[1]);

            Color[] Colours = new Color[5] { Color.FromArgb(255, 255, 255, 255), Color.FromArgb(255, 170, 170, 170),
                                             Color.FromArgb(255, 110, 110, 110), Color.FromArgb(255, 50, 50, 50),
                                             Color.FromArgb(0,0,0,0) };

            Bitmap bitmap = new Bitmap(width, height);

            int peak_num = Convert.ToInt32(seedarray[1]);
            string[] peaks = seedarray[2].Split(',');

            //Many variables that are in use shortly
            int peak_w; //acts as coordinates but due to split use wh instead of xy
            int peak_h;
            string[] temp_peak;
            int rad_cur;

            int[] xrange = new int[2];
            int[] yrange = new int[2];
            int range_prox;
            int x_prox;
            int y_prox;
            int color_num;
            Color Pixel;
            int Location_cur;

            for (int peak_num_cur = 0; peak_num_cur < peak_num; peak_num_cur++)
            {
                temp_peak = peaks[peak_num_cur].Split('x'); //Gets the coordinate of the current peak
                peak_w = Convert.ToInt32(temp_peak[0]);
                peak_h = Convert.ToInt32(temp_peak[1]);

                rad_cur = Convert.ToInt32((((width + height) / 2) * 0.25) / (peak_num_cur + 1)); //Uses an equation for radius to allow seed to produce the same result

                xrange[0] = peak_w - rad_cur; //Finding Upper and Lower limits for x and y
                if (xrange[0] < 0) //Making sure coordinates outside of the resolution aren't accessed
                { xrange[0] = 0; }

                xrange[1] = peak_w + rad_cur;
                if (xrange[1] > width)
                { xrange[1] = width; }

                yrange[0] = peak_h - rad_cur;
                if (yrange[0] < 0)
                { yrange[0] = 0; }

                yrange[1] = peak_h + rad_cur;
                if (yrange[1] > height)
                { yrange[1] = height; }

                //Console.WriteLine("x: {0}, y: {1}, rad_cur: {2}",peak_w,peak_h,rad_cur);
                //Console.WriteLine("xrangemin: {0}, xrangemax: {1}", xrange[0], xrange[1]);
                //Console.WriteLine("yrangemin: {0}, yrangemax: {1}", yrange[0], yrange[1]);

                for (int x = xrange[0]; x < xrange[1]; x++)
                {
                    for (int y = yrange[0]; y < yrange[1]; y++)
                    {
                        x_prox = Math.Abs(peak_w - x);
                        y_prox = Math.Abs(peak_h - y);

                        x_prox = Convert.ToInt32(Math.Pow(x_prox, 2));
                        y_prox = Convert.ToInt32(Math.Pow(y_prox, 2));

                        range_prox = Convert.ToInt32(Math.Sqrt(x_prox + y_prox) * 10); //Pythagoras to grasp proximity to point

                        range_prox = (range_prox / rad_cur);

                        Pixel = bitmap.GetPixel(x, y);
                        Location_cur = Array.IndexOf(Colours, Pixel);


                        if (range_prox < 1) { color_num = 0; } //Highest proximity is ##FFFFFFFF...(White)
                        else if (range_prox < 2) { color_num = 1; }
                        else if (range_prox < 4) { color_num = 2; }
                        else if (range_prox < 8) { color_num = 3; }
                        else { color_num = 4; } //...Lowest is transparent (##00000000) - So just pretty much none at all

                        if (Location_cur > color_num) { bitmap.SetPixel(x, y, Colours[color_num]); } //Sort of weighted so Higher Colours won't be overwitten

                        //Console.WriteLine("Pixel: {0}, Location_cur: {1}, color_num: {2}",Pixel,Location_cur, color_num);
                    }
                }
            }

            if (System.IO.File.Exists(@"C:\Users\mitch\source\repos\ProceduralBitmap\Values.bmp"))
                System.IO.File.Delete(@"C:\Users\mitch\source\repos\ProceduralBitmap\Values.bmp"); //Replaces file

            bitmap.Save(@"C:\Users\mitch\source\repos\ProceduralBitmap\Values.bmp", ImageFormat.Bmp);
        }

        static void BitmapPeaksShort(string seed)
        { //A procedure that attempts to improve on the long string method, don't need to input exact coordinates
            //seed in form resw,resh.num_peaks.generatorstring(12 chars long)

            string[] seedarray = seed.Split('.'); //Stop seperates sections e.g. resolution.peak_num.peak_points

            string[] resolution = seedarray[0].Split(','); //Comma seperates values in sections e.g. width,height under resolution
            int width = Convert.ToInt16(resolution[0]); //Res will stay low for now
            int height = Convert.ToInt16(resolution[1]);

            Bitmap bitmap = new Bitmap(width, height);

            int peak_num = Convert.ToInt32(seedarray[1]);

            string generator_string = seedarray[2];
            //Console.WriteLine(generator_string);
            long generator_int = Convert.ToInt64(generator_string);

            //created so can use generator in different ways
            int generator_int1 = Convert.ToInt32(generator_string.Substring(0, 3));
            int generator_int2 = Convert.ToInt32(generator_string.Substring(3, 3));
            int generator_int3 = Convert.ToInt32(generator_string.Substring(6, 3));
            int generator_int4 = Convert.ToInt32(generator_string.Substring(9, 3));

            int[] generator_list = new int[4] { generator_int1, generator_int2,
                                                generator_int3, generator_int4
            };

            int radius; //self explanatory
            int peak_calc1; //"random" number between 1 and 4
            int peak_calc2;//same as above
            int cur_genw; //act as x and y values
            int cur_genh;

            int[] xrange = new int[2];
            int[] yrange = new int[2];
            double range_prox;
            int x_prox;
            int y_prox;
            Color Pixel;
            int colour_gen;
            int Location_cur;

            for (int peak_num_cur = 0; peak_num_cur < peak_num; peak_num_cur++)
            {
                radius = Convert.ToInt32(((generator_int1 * generator_int2) + 1) % ((peak_num_cur + 1) * 5));//Trying to get a pseudo random value for the radius
                radius = radius % width; //within range of resolution ish

                if (peak_num_cur % 2 == 0) //Means the sequences aren't exactly the same
                {
                    peak_calc1 = (peak_num_cur + 1) % 4;
                    peak_calc2 = ((peak_num_cur * peak_calc1) + 2) % 4;
                }
                else
                {
                    if (peak_num_cur % 3 == 0)
                    {
                        peak_calc1 = (peak_num_cur + 2) % 4;
                        peak_calc2 = ((peak_num_cur * peak_calc1) + 3) % 4;
                    }

                    else
                    {
                        peak_calc1 = (peak_num_cur + 3) % 4;
                        peak_calc2 = ((peak_num_cur * peak_calc1) + 1) % 4;
                    }
                }

                peak_calc2 = peak_num_cur % 4; //Keeps inside generator substrings

                cur_genw = (generator_list[peak_calc1] * radius * (peak_num_cur + 1)) + 1;
                cur_genh = (generator_list[peak_calc2] * radius * (peak_num_cur + 1)) + 1;

                cur_genw = cur_genw % width; //so values stay inside the resolution
                cur_genh = cur_genh % height;

                //Console.WriteLine("width: {0}, height: {1}, radius: {2}",cur_genw, cur_genh, radius);
                //Now need to generate values into bitmap (can use previous algorithm)

                xrange[0] = cur_genw - radius; //Finding Upper and Lower limits for x and y
                if (xrange[0] < 0) //Making sure coordinates outside of the resolution aren't accessed
                { xrange[0] = 0; }

                xrange[1] = cur_genw + radius; ;
                if (xrange[1] > width)
                { xrange[1] = width; }

                yrange[0] = cur_genh - radius; ;
                if (yrange[0] < 0)
                { yrange[0] = 0; }

                yrange[1] = cur_genh + radius; ;
                if (yrange[1] > height)
                { yrange[1] = height; }

                //Console.WriteLine("x: {0}, y: {1}, rad_cur: {2}",peak_w,peak_h,rad_cur);
                //Console.WriteLine("xrangemin: {0}, xrangemax: {1}", xrange[0], xrange[1]);
                //Console.WriteLine("yrangemin: {0}, yrangemax: {1}", yrange[0], yrange[1]);

                for (int x = xrange[0]; x < xrange[1]; x++)
                {
                    for (int y = yrange[0]; y < yrange[1]; y++)
                    {
                        x_prox = Math.Abs(cur_genw - x);
                        y_prox = Math.Abs(cur_genh - y);

                        x_prox = Convert.ToInt32(Math.Pow(x_prox, 2));
                        y_prox = Convert.ToInt32(Math.Pow(y_prox, 2));

                        range_prox = Math.Sqrt(x_prox + y_prox) * 10; //Pythagoras to grasp proximity to point

                        range_prox = (range_prox / radius);

                        Pixel = bitmap.GetPixel(x, y);
                        Location_cur = Pixel.R; //Gets red value of current pixel

                        if (radius > 255) { radius = 255; }//changing radius just for height generation

                        if (range_prox == 0) {colour_gen = radius; } //max height depends on the max radius

                        else
                        {
                            colour_gen = Convert.ToInt16(radius / (range_prox + 0.9)); //don't want decimals

                            if (colour_gen > radius) { colour_gen = radius; }
                        }

                        Pixel = Color.FromArgb(255, colour_gen, colour_gen, colour_gen); //so the gradient is smooth

                        if (Location_cur < colour_gen) { bitmap.SetPixel(x, y, Pixel); } //Sort of weighted so Higher Colours won't be overwitten

                        //Console.WriteLine("Pixel: {0}, Location_cur: {1}, colour_gen: {2}",Pixel,Location_cur, colour_gen);
                    }
                }
            }

            GrassGen(bitmap);

            if (File.Exists(@"C:\Users\mitch\source\repos\ProceduralBitmap\Values.bmp"))
                File.Delete(@"C:\Users\mitch\source\repos\ProceduralBitmap\Values.bmp"); //Replaces file

            bitmap.Save(@"C:\Users\mitch\source\repos\ProceduralBitmap\Values.bmp", ImageFormat.Bmp);
        }

        static void BitmapType(string seed)
        { //Procedure to check what type of seed is being used
            string[] check = seed.Split(',');

            if (check.Length >= 3)
            {
                BitmapPeaksLong(seed);
            }
            else
            {
                BitmapPeaksShort(seed);
            }
        }

        static string BitmapSeedGen(string res)
        {
            //Procedure to generate a (random) string that can be utilised by the Image generator

            Random random = new Random();

            string seed = "";
            int three_dig;

            for (int i = 0; i < 4; i++)
            { //generates four sets of three digit numbers
                three_dig = random.Next(100,1000);
                seed = seed + Convert.ToString(three_dig);
            }

            //Console.WriteLine(seed);

            seed = res + "." + random.Next(50,101) + "." + seed;

            return seed;
        }
        static void Main(string[] args)
        { //Will be no main function later, will just be a library
            //BitmapType("256,256.50.321456123441"); //Short seed
            //BitmapType("500,500.5.212x2,7x400,400x56,250x25,42x150"); //Long seed --outdated--
            BitmapType(BitmapSeedGen("256,256"));

            //Next Plan is to add some extra noise to have rougher terrain - can just generate a second height map and leyer them in some way 
        }
    }
}
