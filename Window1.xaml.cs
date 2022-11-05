using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using WPFMediaKit.DirectShow.Controls;
using ZXing;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace XHomeWorkHelper
{
    /// <summary>
    /// Window1.xaml 的交互逻辑
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            
            CamaraComboBox.ItemsSource = MultimediaUtil.VideoInputNames;//获得所有摄像头
            if (MultimediaUtil.VideoInputNames.Length > 0)
            {
                CamaraComboBox.SelectedIndex = 0;//第0个摄像头为默认摄像头
            }
            else
            {
                MessageBox.Show("电脑没有安装任何可用摄像头");
            }
            cameraTimer.Interval = new TimeSpan(200); //执行间隔0.2秒
            cameraTimer.Tick += cameraTimer_Tick;
            cameraTimer.Start();

            codeReader.Options.TryHarder = true;
        }
        BarcodeReader codeReader = new BarcodeReader();
        DispatcherTimer cameraTimer = new DispatcherTimer();

        private void cameraTimer_Tick(object sender, EventArgs e)
        {
            RenderTargetBitmap bmp = new RenderTargetBitmap(1920, 1080, 200, 200, PixelFormats.Default);
            MainVideoCaptureElement.Measure(MainVideoCaptureElement.RenderSize);
            MainVideoCaptureElement.Arrange(new Rect(MainVideoCaptureElement.RenderSize));
            bmp.Render(MainVideoCaptureElement);
            BitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bmp));
            
            using (MemoryStream ms = new MemoryStream())
            {
                encoder.Save(ms);
                Bitmap btiMap = new Bitmap(ms);
                var result = codeReader.Decode(btiMap);//解析条码
                if (result != null&&!datatextbox.Text.Contains(result.ToString()))
                {
                    string temp = result.ToString();
                    datatextbox.Text = datatextbox.Text + temp + ",";
                    SpeechSynthesizer tts=new SpeechSynthesizer();
                    tts.SpeakAsync(temp);
                }
            }
        }
        public string InputValue
        {
            get
            {
                return datatextbox.Text;
            }
            set
            {
                this.datatextbox.Text = value;
            }
        }
        public event EventHandler accept;
        private void CamaraComboBox_Selected(object sender, RoutedEventArgs e)
        {
            MainVideoCaptureElement.VideoCaptureSource = (string)CamaraComboBox.SelectedItem;
        }

        private void CheckButton_Click(object sender, RoutedEventArgs e)
        {
           
                if (accept != null)
                {
                    accept(this, EventArgs.Empty);
                }
                this.Close(); 
            
        }
    }
}
