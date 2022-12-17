using System;
using System.Collections.Generic;
using System.Linq;
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

namespace XHomeWorkHelper
{
    /// <summary>
    /// Window2.xaml 的交互逻辑
    /// </summary>
    public partial class Window2 : Window
    {
        public Window2()
        {
            InitializeComponent();

            ComboBox1.ItemsSource = status1;
            ComboBox1.ItemsSource = status2;
            ComboBox1.SelectedIndex = 1;
        }
        public string[] status1 = { "未交", "已交", "已退回" };

        public string[] status2 =
            { "", "已查", "未写", "未订正", "待订正", "部分未写", "请假", "其他特殊情况","使用通用编号输入" };
        public int InputValue
        {
            get
            {
                return ComboBox1.SelectedIndex*10+ComboBox2.SelectedIndex;
            }
        }
        public event EventHandler accept;
    }
}
