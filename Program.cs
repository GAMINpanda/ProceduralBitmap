using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

namespace ProceduralBitmap
{
    class Program
    {
        public static string user = Environment.GetEnvironmentVariable("userprofile");
        static Bitmap Texture(Bitmap bitmap_peaks, Bitmap bitmap_grass, bool tf)
        {
            var target = new Bitmap(bitmap_peaks.Width, bitmap_peaks.Height, PixelFormat.Format32bppArgb);
            var graphics = Graphics.FromImage(target);
            //graphics.CompositingMode = CompositingMode.SourceOver;

            graphics.DrawImage(bitmap_peaks, 0, 0);
            graphics.DrawImage(bitmap_grass, 0, 0);

            string path = user + @"\source\repos\ProceduralBitmap\Texture.bmp";

            if (tf)
            {
                if (File.Exists(path))
                    File.Delete(path); //Replaces file

                target.Save(path, ImageFormat.Bmp);
            }
            return target;
        }

        static Bitmap TreesAndFoilageGen(string seed, Bitmap bitmap_grass)
        {
            //Function to add trees and foilage

            string[] seedarray = seed.Split('.'); //Stop seperates sections e.g. resolution.peak_num.peak_points

            string[] resolution = seedarray[0].Split(','); //Comma seperates values in sections e.g. width,height under resolution
            int width = Convert.ToInt16(resolution[0]); //Res will stay low for now
            int height = Convert.ToInt16(resolution[1]);

            Bitmap bitmap_treesfoilage = new Bitmap(width, height);

            int peak_num = Convert.ToInt32(seedarray[1]);

            string generator_string = seedarray[2];
            //Console.WriteLine(generator_string);
            int generator_int = Convert.ToInt32(generator_string.Substring(0, 6));


            int rand;
            string randstring;
            int len;
            int x;
            int y;
            Color Pixel;

            for (int i = 0; i < (peak_num * peak_num * 15); i++)
            {
                rand = (i + 1) * (generator_int % (i + 1));
                rand = Convert.ToInt32(rand / ((i * i) + 1));
                randstring = Convert.ToString(rand);
                len = randstring.Length;

                if (len > 6)
                {
                    randstring = randstring.Substring(len - 4); //generating a random number each time
                }  

                rand = Convert.ToInt32(randstring);

                x = Math.Abs((rand + 1) * (i + 1)) % width;
                y = Math.Abs((rand + 5) * ((i * i) + 1)) % height;

                Pixel = bitmap_grass.GetPixel(x, y);
                //Console.WriteLine("x: {0}, y: {1}",x,y);

                if (Pixel.G < 85 && Pixel.R == 0 && Pixel.G > 60)
                {
                    bitmap_treesfoilage.SetPixel(x, y, Color.FromArgb(255, 108, 59, 30)); //Nice brown
                }
            }

            return bitmap_treesfoilage;
        }

        static Bitmap HeightMapOverlay(Bitmap bitmap_peaks1, Bitmap bitmap_peaks2)
        {
            int width = bitmap_peaks1.Width;
            int height = bitmap_peaks2.Height;
            int avg;

            for (int x = 0; x < width; x++) //Finds average of heights between Bitmaps and sets the pixel to that average
            {
                for (int y = 0; y < height; y++)
                {
                    avg = (bitmap_peaks1.GetPixel(x, y).R) + (bitmap_peaks2.GetPixel(x, y).R);
                    avg = Convert.ToInt32(avg / 2);
                    bitmap_peaks1.SetPixel(x, y, Color.FromArgb(255, avg, avg, avg));
                }
            }

            return bitmap_peaks1;
        }

        static (int[], int[], int[]) CivilisationOverlay(string seed)
        {
            //Function to create a seperate bitmap to outline houses
            //Map contains one large town and two villages

            string[] seedarray = seed.Split('.');
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

            int[] coor1 = { Math.Abs(generator_list[0] - generator_list[1]) % width, Math.Abs(generator_list[1] - generator_list[2]) % height };
            int[] coor2 = { Math.Abs(generator_list[2] - generator_list[3]) % width, Math.Abs(generator_list[3] - generator_list[0]) % height };
            int[] coor3 = { Math.Abs(generator_list[0] - generator_list[2]) % width, Math.Abs(generator_list[1] - generator_list[3]) % height };

            //coordinates on where to place villages

            return (coor1, coor2, coor3);
        }

        static bool IsWater(Color Pixel)
        {
            if (Pixel.R == 35 && Pixel.G == 137 && Pixel.B == 218)
            {
                return true;
            }
            else { return false; }
        }

        static void GrassWaterGen(Bitmap bitmap_peaks, string seed) {
            //Function to generate grass based off a given bitmap
            int width = bitmap_peaks.Width;
            int height = bitmap_peaks.Height;

            Bitmap bitmap_grass = new Bitmap(width, height);

            Color Pixel;
            Color PixelUp;
            Color PixelDown;
            Color PixelLeft;
            Color PixelRight;
            int score = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    score = 0;
                    Pixel = bitmap_peaks.GetPixel(x, y);

                    //Surrounding Pixels, doesn't bother with outside layer of pixels since they won't be visible anyway in unity project
                    if ((x > 1 && x < width - 1) && (y > 1 && y < height - 1))
                    {
                        PixelUp = bitmap_peaks.GetPixel(x, y + 1);
                        PixelDown = bitmap_peaks.GetPixel(x, y - 1);
                        PixelRight = bitmap_peaks.GetPixel(x + 1, y); ;
                        PixelLeft = bitmap_peaks.GetPixel(x - 1, y);

                        if (Pixel.R > 85 && Pixel.G == Pixel.R)
                        {
                            if (PixelUp.R > Pixel.R || IsWater(PixelUp)) {score++;}

                            if (PixelDown.R > Pixel.R || IsWater(PixelDown)) {score++;}

                            if (PixelLeft.R > Pixel.R || IsWater(PixelLeft)) {score++;}

                            if (PixelRight.R > Pixel.R || IsWater(PixelRight)) {score++;}
                            
                            if (score >= 3) //At least three sides need to be higher
                            {bitmap_grass.SetPixel(x, y, Color.FromArgb(255, 35, 137, 218));}
                        }
                    }

                    if (Pixel.R < 58)
                    {
                        bitmap_grass.SetPixel(x, y, Color.FromArgb(255, 35, 137, 218)); //supposed to be a nice shade of blue
                    }

                    else if (Pixel.R < 60)
                    {
                        bitmap_grass.SetPixel(x, y, Color.FromArgb(255, 194, 178, 128)); //supposed to be a nice sand colour
                    }

                    else
                    {
                        if (Pixel.R < 85)
                        {
                            bitmap_grass.SetPixel(x, y, Color.FromArgb(255, 0, Pixel.R, 0)); //Grass
                        }
                    }
                }
            }

            bitmap_grass = Texture(bitmap_grass, TreesAndFoilageGen(seed, bitmap_grass), false);
            Texture(bitmap_peaks, bitmap_grass, true);
        }

        static void BitmapPeaksLong(string seed)
        {   //Function to create a bitmap that would be used for hilly terrain
            //points are the 'peaks' in the bitmap

            //Seed current form is:  width,height.peak_num.peak_w1xpeak_h1 ... peakwnxpeak_hn
            //Since this is the long seed version peaks are already predefined so there is no randomness
            //Outdated method, used for testing and looking back on (test if you want)

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

            string path = user + @"\source\repos\ProceduralBitmap\Values.bmp";

            if (System.IO.File.Exists(path))
                System.IO.File.Delete(path); //Replaces file

            bitmap.Save(path, ImageFormat.Bmp);
        }

        static Bitmap BitmapPeaksShort(string seed)
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
                        range_prox = (range_prox / (radius + 0.1));

                        Pixel = bitmap.GetPixel(x, y);
                        Location_cur = Pixel.R; //Gets red value of current pixel

                        //Console.WriteLine("radius: {0}, range_prox: {1}, rough: {2}",radius, range_prox,rough);

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

            return bitmap;
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
                Bitmap bitmap_peaks = BitmapPeaksShort(seed);
                Bitmap bitmap_peaks2;

                //---
                for (int i = 0; i < 2; i++)
                {
                    bitmap_peaks2 = BitmapPeaksShort(BitmapSeedGen("256,256")); //Generates two different Bitmaps

                    bitmap_peaks = HeightMapOverlay(bitmap_peaks, bitmap_peaks2); //Overlays for further variety
                }
                //--- This small section makes a far more realistic height map
                

                GrassWaterGen(bitmap_peaks, seed); //Generates Grass Texture

                string path = user + @"\source\repos\ProceduralBitmap\Values.bmp";

                if (File.Exists(path))
                    File.Delete(path); //Replaces file

                bitmap_peaks.Save(path, ImageFormat.Bmp);

                (int[] coor1, int[] coor2, int[] coor3) = CivilisationOverlay(seed);

                Console.WriteLine("x: {0}, y: {1}", coor1[0],coor1[1]);
                Console.WriteLine("x: {0}, y: {1}", coor2[0], coor2[1]);
                Console.WriteLine("x: {0}, y: {1}", coor3[0], coor3[1]);
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
           //BitmapPeaksLong("512,512.1.265x256"); //Long seed --outdated--
            BitmapType(BitmapSeedGen("256,256"));

            //Next Plan is to add some extra noise to have rougher terrain - can just generate a second height map and layer them in some way 
        }
    }
}
