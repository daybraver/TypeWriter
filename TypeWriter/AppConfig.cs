using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Windows.Media;
using TypeWriter.PubSubEvents;
using XamlPearls.XamlFont;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace TypeWriter
{
    public enum Accent
    {
        UK = 1,
        US = 2,
    }

    [DisplayName("配置")]
    public class AppConfig : ICloneable
    {
        public AppConfig()
        {
            DefaultMediaFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
            PlayMode = AudioPlayMode.ListLoop;
            ForwardBackTimeMs = 1500;
            SpeedRatioIncrement = 0.1f;
            SpeedRatioDecrement = 0.1f;
        }

        [Category("常规")]
        [DisplayName("自动隐藏")]
        [Browsable(false)]
        public bool AutoHide { get; set; } = true;

        [ExpandableObject]
        [Category("\u0001单词")]
        public LearnWordOption LearnWordOption { get; set; }

        #region 播放器

        [Category("\u0011播放器")]
        [DisplayName("默认音视频目录")]
        [PropertyOrder(1)]
        public string DefaultMediaFolderPath { get; set; }

        [Category("\u0011播放器")]
        [DisplayName("前进后退毫秒")]
        [PropertyOrder(4)]
        public int ForwardBackTimeMs { get; set; }

        [Category("\u0011播放器")]
        [DisplayName("播放模式")]
        [PropertyOrder(5)]
        public AudioPlayMode PlayMode { get; set; }

        [Category("\u0011播放器")]
        [DisplayName("加速倍率")]
        [PropertyOrder(2)]
        [Range(1, 100, ErrorMessage = "值必须在 1 到 100 之间")]
        public float SpeedRatioDecrement { get; set; }

        [Category("\u0011播放器")]
        [DisplayName("减速倍率")]
        [PropertyOrder(3)]
        public float SpeedRatioIncrement { get; set; }

        #endregion 播放器

        #region 句子

        [DisplayName("背景色")]
        [PropertyOrder(3)]
        [Category("\u0012句子")]
        public Color BackColor { get; set; }

        [DisplayName("待输入字体")]
        [PropertyOrder(5)]
        [Category("\u0012句子")]
        [ExpandableObject]
        public FontInfo ToTypeFont { get; set; }

        [Category("\u0012句子")]
        [DisplayName("高度")]
        [PropertyOrder(2)]
        public int TypeBoxHeight { get; set; }

        [Category("\u0012句子")]
        [DisplayName("宽度")]
        [PropertyOrder(1)]
        public int TypeBoxWidth { get; set; }

        [DisplayName("已输入字体")]
        [PropertyOrder(4)]
        [Category("\u0012句子")]
        [ExpandableObject]
        public FontInfo TypedFont { get; set; }

        #endregion 句子

        public object Clone()
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters =
                {
                    new ColorJsonConverter(),
                    new FontInfoJsonConverter()
                }
            };
            return JsonSerializer.Deserialize<AppConfig>(JsonSerializer.Serialize(this, options), options);
        }
    }

    [DisplayName("选项")]
    public class LearnWordOption
    {
        [DisplayName("发音")]
        [PropertyOrder(1)]
        public Accent Accent { get; set; }

        [DisplayName("背景色")]
        [PropertyOrder(4)]
        public Color BackColor { get; set; }

        [DisplayName("高度")]
        [PropertyOrder(3)]
        public int BoxHeight { get; set; }

        [DisplayName("宽度")]
        [PropertyOrder(2)]
        public int BoxWidth { get; set; }

        [ExpandableObject]
        [DisplayName("字体")]
        [PropertyOrder(5)]
        public FontInfo FontInfo { get; set; }
    }
}
