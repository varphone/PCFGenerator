using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace PCFGenerator
{
    public class AppSettings
    {
        public static List<string> fontFamilies = new List<string>();

        public abstract class ComboBoxItemTypeConvert : TypeConverter
        {
            public Hashtable _hash = null;
            public ComboBoxItemTypeConvert()
            {
                _hash = new Hashtable();
                //GetConvertHash();
            }

            public abstract void GetConvertHash();

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                GetConvertHash();

                int[] ids = new int[_hash.Values.Count];
                int i = 0;
                foreach (DictionaryEntry de in _hash)
                {
                    ids[i++] = (int)(de.Key);
                }
                return new StandardValuesCollection(ids);
            }

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string))
                {
                    return true;
                }
                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object v)
            {
                if (v is string)
                {
                    foreach (DictionaryEntry de in _hash)
                    {
                        if (de.Value.Equals((v.ToString())))
                            return de.Key;
                    }
                }
                return base.ConvertFrom(context, culture, v);
            }

            public override object ConvertTo(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object v, Type destinationType)
            {
                if (destinationType == typeof(string))
                {
                    foreach (DictionaryEntry de in _hash)
                    {
                        if (de.Key.Equals(v))
                            return de.Value.ToString();
                    }
                    return "";
                }
                return base.ConvertTo(context, culture, v, destinationType);
            }

            public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
            {
                return false;
            }
        }

        public class PropertyGridBoolItem : ComboBoxItemTypeConvert
        {
            public override void GetConvertHash()
            {
                _hash.Add(0, "是");
                _hash.Add(1, "否");
            }
        }

        public class FontFamilyPropertyItem : ComboBoxItemTypeConvert
        {
            public override void GetConvertHash()
            {
                _hash.Clear();
                for (var i = 0; i < fontFamilies.Count; i++)
                {
                    _hash.Add(i, fontFamilies[i]);
                }
            }
        }

        private string fontFile = "";
        [Category("字体设置"),
         Description("指定制作点阵字库所用的字体文件"),
         DisplayName("字体文件")
        ]
        [EditorAttribute(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string FontFile
        {
            get { return fontFile; }
            set { fontFile = value; }
        }

        private int fontFamily = 0;
        [Category("字体设置"),
         Description("字体名称"),
         DisplayName("字体名称"),
         TypeConverter(typeof(FontFamilyPropertyItem))]
        public int FontFamily
        {
            get { return fontFamily; }
            set { fontFamily = value; }
        }

        private string patternsFile = "";
        [Category("字体设置"),
         Description("指定制作点阵字库所用的样本文件"),
         DisplayName("样本文件")]
        [Editor(typeof(System.Windows.Forms.Design.FileNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string PatternsFile
        {
            get { return patternsFile; }
            set { patternsFile = value; }
        }

        private Boolean bold = false;
        [Category("字体设置"),
         Description("加粗"),
         DisplayName("加粗")]
        public Boolean Bold
        {
            get { return bold; }
            set { bold = value; }
        }

        private Boolean italic = false;
        [Category("字体设置"),
         Description("偏斜"),
         DisplayName("偏斜")]
        public Boolean Italic
        {
            get { return italic; }
            set { italic = value; }
        }

        private Size charSize = new Size(16, 16);
        [Category("字体设置"),
         Description("指定点阵字符的大小(以像素为单位)"),
         DisplayName("字符大小")]
        public Size CharSize
        {
            get { return charSize; }
            set { charSize = value; }
        }

        private Size dpi = new Size(72, 72);
        [Category("字体设置"),
         Description("指定字库目标设备的显示精度"),
         DisplayName("显示精度")]
        public Size DPI
        {
            get { return dpi; }
            set { dpi = value; }
        }

        private string outputDir = "";
        [Category("输出设置"),
         Description("指定制作点阵字库的输出目录"),
         DisplayName("输出目录")
        ]
        [EditorAttribute(typeof(System.Windows.Forms.Design.FolderNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string OutputDir
        {
            get { return outputDir; }
            set { outputDir = value; }
        }

        private string charEncoding = "GBK";
        [Category("输出设置"),
         Description("指定制作点阵字库的字符编码"),
         DisplayName("字符编码")
        ]
        public string CharEncoding
        {
            get { return charEncoding; }
            set { charEncoding = value; }
        }

        private bool logToFile = false;
        [Category("输出设置"),
         Description("输出日志到文件"),
         DisplayName("记录日志")
        ]
        public bool LogToFile
        {
            get { return logToFile; }
            set { logToFile = value; }
        }

        private bool saveBitmap = false;
        [Category("输出设置"),
         Description("保存字符位图到文件"),
         DisplayName("保存位图")
        ]
        public bool SaveBitmap
        {
            get { return saveBitmap; }
            set { saveBitmap = value; }
        }

        public AppSettings()
        {
        }
    }
}