using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PCFGenerator
{
    public partial class MainForm : Form
    {
        private AppSettings appSettings;
        private SharpFont.Library ftLib;
        private SharpFont.Face ftFace;
        private FileStream outputStream;
        private StreamWriter outputWriter;
        private TextWriter oldConsoleOut;

        public MainForm()
        {
            InitializeComponent();

            ftLib = new SharpFont.Library();

            appSettings = new AppSettings();
            propertyGrid1.SelectedObject = appSettings;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (appSettings.PatternsFile != "")
            {
                outputStream = File.Open(Path.Combine(appSettings.OutputDir, "pcf.c"), FileMode.Create);
                outputWriter = new StreamWriter(outputStream);
                outputWriter.WriteLine(@"// Font File: {0}", new FileInfo(appSettings.FontFile).Name);
                outputWriter.WriteLine(@"// FamilyName: {0}, Pixels: {1}x{2}", ftFace.FamilyName, appSettings.CharSize.Width, appSettings.CharSize.Height);
                outputWriter.WriteLine(@"// Created: {0}", DateTime.Now.ToUniversalTime().ToString());
                outputWriter.WriteLine(@"unsigned char pcf_{0}x{1}[] = {{", appSettings.CharSize.Width, appSettings.CharSize.Height);
                var patterns = File.ReadAllText(appSettings.PatternsFile);
                var count = 0;
                foreach (var c in patterns)
                {
                    if (DrawChar(c))
                        count++;
                }
                outputWriter.WriteLine("};");
                outputWriter.Close();
                //outputStream.Flush();
                //outputStream.Close();
                outputWriter = null;
                outputStream = null;
                MessageBox.Show(String.Format(@"成功生成 {0} 个字符", count), "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(@"未加载字符【样本文件】", @"错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private Size GetCharSize(Char c, Graphics g)
        {
            var flags = TextFormatFlags.NoPadding | TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter;
            return TextRenderer.MeasureText(g, c.ToString(), fontDialog1.Font, new Size(0,0), flags);
        }

        /*
        private void DrawChar(Char c)
        {
            var g = this.CreateGraphics();
            g.ResetTransform();
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
            //g.
            var charWidth = Int32.Parse(textBoxCharWidth.Text);
            var charHeight = Int32.Parse(textBoxCharHeight.Text);
            //Console.WriteLine(textBoxFontPreview.Font);
            var size = GetCharSize(c, g);
            Bitmap bm = new Bitmap(size.Width, size.Height);
            Brush br = new SolidBrush(Color.White);
            //Graphics g = Graphics.FromImage(bm);
            //String s = "";
            //s += c;
            StringFormat sf = new StringFormat();
            //sf.Alignment = StringAlignment.Center;
            //sf.LineAlignment = StringAlignment.Center;
            //g.ResetTransform();
            //g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
            g.Clear(Color.Black);
            //g.DrawString(s, fontDialog1.Font, br, 1, 1, sf);
            TextRenderer.DrawText(g, c.ToString(), fontDialog1.Font, new Point(0, 0), Color.White);
            //
            var bmf = String.Format("{0}\\pcf_{1}.bmp", textBoxOutputDir.Text, (Int32)c);
            bm.Save(bmf);
            //
            g.Dispose();
        }
        */
        private bool DrawChar(Char c)
        {
            try {
                //ftFace.SetCharSize(appSettings.CharSize.Width, appSettings.CharSize.Height, (uint)appSettings.DPI.Width, (uint)appSettings.DPI.Height);
                ftFace.SetPixelSizes((uint)appSettings.CharSize.Width, (uint)appSettings.CharSize.Height);
                var glyphIndex = ftFace.GetCharIndex((uint)c);
                ftFace.LoadGlyph(glyphIndex, SharpFont.LoadFlags.Default | SharpFont.LoadFlags.NoHinting, SharpFont.LoadTarget.Mono);
                ftFace.Glyph.RenderGlyph(SharpFont.RenderMode.Mono);
                var penX = ftFace.Glyph.BitmapLeft;
                var penY = (int)ftFace.Size.Metrics.Ascender - ftFace.Glyph.BitmapTop;
                while (penY > 0 && penY + ftFace.Glyph.Metrics.Height > appSettings.CharSize.Height)
                    penY--;
                Console.WriteLine("X {0} Y {1} L {2} T {3}", ftFace.Glyph.Advance.X, ftFace.Glyph.Advance.Y, ftFace.Glyph.BitmapLeft, ftFace.Glyph.BitmapTop);
                Console.WriteLine("{0} {1} {2}", ftFace.Size.Metrics.Ascender, ftFace.Size.Metrics.Descender, ftFace.Glyph.Metrics.Height);
                Bitmap bm = ftFace.Glyph.Bitmap.ToGdipBitmap(Color.Black);
                var nbm = new Bitmap(appSettings.CharSize.Width, appSettings.CharSize.Height);
                var g = Graphics.FromImage(nbm);
                g.Clear(Color.White);
                g.DrawImageUnscaled(bm, penX, penY);
                //nbm.SetPixel()
                //
                byte[] mbc;
                if (c > 127)
                    mbc = Encoding.GetEncoding(appSettings.CharEncoding).GetBytes(c.ToString().ToCharArray());
                else
                    mbc = new byte[]{ 0, (byte)c};
                outputWriter.WriteLine(@"0x{0:x2},0x{1:x2}, // {2}", mbc[0], mbc[1], c);

                Console.WriteLine("W {0} H {1}", nbm.Width, nbm.Height);

                BitArray ba = new BitArray(nbm.Width * nbm.Height);
                for (var y = 0; y < nbm.Height; y++)
                {
                    for (var x = 0; x < nbm.Width; x++)
                    {
                        var a = nbm.GetPixel(x, y);
                        ba.Set(y * nbm.Width + x, a.B == 0);
                    }

                }
                byte[] bba = new byte[nbm.Width * nbm.Height / 8];
                ba.CopyTo(bba, 0);
                for(var i = 0; i < bba.Length; i++)
                {
                    if (i > 0 && i % 16 == 0)
                        outputWriter.WriteLine();
                    outputWriter.Write(@"0x{0:x2},", bba[i]);
                }
                outputWriter.WriteLine();
                outputWriter.WriteLine();
                Console.WriteLine(bba.Length);

                if (appSettings.SaveBitmap)
                {
                    var nbmf = String.Format("{0}\\pcf_{1:x2}{2:x2}.bmp", appSettings.OutputDir, mbc[0], mbc[1]);
                    nbm.Save(nbmf);
                }

                ShowBitmap(nbm);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
            return true;
        }

        private bool LoadFontFile(string path)
        {
            AppSettings.fontFamilies.Clear();
            SharpFont.Face face = new SharpFont.Face(ftLib, path, -1);
            //Console.WriteLine("Count {0}, Index {1}, Family {2}", face.FaceCount, face.FaceIndex, face.FamilyName);
            var faceCount = face.FaceCount;
            if (faceCount > 1)
            {

                for (var i = 0; i < faceCount; i++) {
                    face = new SharpFont.Face(ftLib, path, i);
                    AppSettings.fontFamilies.Add(face.FamilyName);
                    Console.WriteLine("Index {0}, Family {1}", face.FaceIndex, face.FamilyName);
                }
            }
            else
            {
                AppSettings.fontFamilies.Add(face.FamilyName);
                Console.WriteLine("Index {0}, Family {1}", face.FaceIndex, face.FamilyName);
            }
            return true;
        }

        private bool LoadFontFamily(int index)
        {
            ftFace = new SharpFont.Face(ftLib, appSettings.FontFile, index);
            return true;
        }

        private void ShowBitmap(Bitmap bm)
        {
            using (bm)
            {
                var bmp2 = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                using (var g = Graphics.FromImage(bmp2))
                {
                    g.InterpolationMode = InterpolationMode.NearestNeighbor;
                    g.DrawImage(bm, new Rectangle(Point.Empty, bmp2.Size));
                    pictureBox1.Image = bmp2;
                }
            }
        }

        private void buttonLocation_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() != DialogResult.Cancel)
            {
                //textBoxOutputDir.Text = folderBrowserDialog1.SelectedPath;
            }
        }

        private void propertyGrid1_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            Console.WriteLine(s);
            var propName = e.ChangedItem.PropertyDescriptor.Name;
            if (propName.Equals("FontFile"))
            {
                LoadFontFile((string)e.ChangedItem.Value);
            }
            else if (propName.Equals("FontFamily"))
            {
                LoadFontFamily((int)e.ChangedItem.Value);
                Console.WriteLine("FontIndex {0}", (int)e.ChangedItem.Value);
            }
            else if (propName.Equals("LogToFile"))
            {
                if ((bool)e.ChangedItem.Value)
                {
                    oldConsoleOut = Console.Out;
                    var fs = new FileStream(Path.Combine(appSettings.OutputDir, "console.log"), FileMode.Create);
                    var sw = new StreamWriter(fs);
                    sw.AutoFlush = true;
                    Console.SetOut(sw);
                }
                else
                {
                    if (oldConsoleOut != null)
                        Console.SetOut(oldConsoleOut);
                }
            }

            Console.WriteLine(propName);
        }
    }
}
