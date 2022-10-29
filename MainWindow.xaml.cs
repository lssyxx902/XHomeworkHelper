using FreeSql.DataAnnotations;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;

namespace Dictation
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Global

        public string GetUrlText(string url)
        {
            WebClient Iweb = new WebClient();
            Iweb.Credentials = CredentialCache.DefaultCredentials;
            Byte[] pageBytes = Iweb.DownloadData(url);
            return Encoding.UTF8.GetString(pageBytes);
        }


        #endregion
        #region database
        public user LoginUser = new user();

        public static IFreeSql database = new FreeSql.FreeSqlBuilder()
        .UseConnectionString(FreeSql.DataType.MySql, "Data Source=xing.axing6.cn;Port=3306;User ID=Dictation;Password=1145141919810; Initial Catalog=dictation;Charset=utf8; SslMode=none;Min pool size=1")
        .UseAutoSyncStructure(true)
        .Build();
        public class user
        {
            [Column(IsIdentity = true)]
            public string name { get; set; }
            public int id { get; set; }
            public int subject { get; set; } //Sci=0,Chi=1
            public int type { get; set; } //tch=0,kdb=1
            public string password { get; set; }
        }

        public class word
        {
            public string read { get; set; }
            public string show { get; set; }
            public string answer { get; set; }
        }

        public class main
        {
            public DateTime CreateTime { get; set; }
            public int fromid { get; set; }
            public List<word> input { get; set; }
        }

        public class hwtask
        {
            public int id { get; set; }
            public string name { get; set; }
            public string date { get; set; }
            public int subject { get; set; }
            [Column(StringLength = -1)]
            public string info { get; set; }
            public int classid { get; set; }
        }

        public class homeworklist
        {
            public int id { get; set; }
            public string name { get; set; }
            public int subject { get; set; }
        }

        public class classlist
        {
            public int id { get; set; }
            public string name { get; set; }
        }

        public class studentlist
        {
            public int id { get; set; }
            public string name { get; set; }
            public int classid { get; set; }
        }

        public class maininfo
        {
            public long hwid { get; set; }
            public string name { get; set; }
            public int status { get; set; } //nope=0,have=1
            public int taskid { get; set; }

        }

        public void ChangeUserPassword(string pw)
        {
            if (database.Select<user>().Where(user => user.password == pw).ToOne() == null)
                database.Update<user>()
                    .Where(user => user.id == LoginUser.id)
                    .Set(user => user.password, pw)
                    .ExecuteAffrows();
            else this.Dispatcher.Invoke(new Action(() => MessageBox.Show("ERROR Key was in database.")));
        }

        public user GetUser(int id)
        {
            return database
                .Select<user>()
                .Where(user => user.id == id).ToOne();
        }

        #endregion
        public static string userpath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\XHH";
        public MainWindow()
        {
            InitializeComponent();
        }

        public void UseHomepage()
        {
            New_Task_Grid.Visibility = Visibility.Collapsed;
            Homepage.Visibility = Visibility.Visible;
            Login.Visibility = Visibility.Collapsed;
            Task.Visibility = Visibility.Collapsed;
        }

        public void UseLogin()
        {
            New_Task_Grid.Visibility = Visibility.Collapsed;
            Homepage.Visibility = Visibility.Collapsed;
            Login.Visibility = Visibility.Visible;
            Task.Visibility = Visibility.Collapsed;
        }

        public void UseNewTask()
        {
            New_Task_Grid.Visibility = Visibility.Visible;
            Homepage.Visibility = Visibility.Collapsed;
            Login.Visibility = Visibility.Collapsed;
            Task.Visibility = Visibility.Collapsed;
        }

        public void UseTask()
        {
            New_Task_Grid.Visibility = Visibility.Collapsed;
            Homepage.Visibility = Visibility.Collapsed;
            Login.Visibility = Visibility.Collapsed;
            Task.Visibility = Visibility.Visible;
        }
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { DragMove(); } catch { }
        }
        public Random random = new Random();
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainImage.Source = new BitmapImage(new Uri(GetUrlText("https://xcubestudio.net/img.html") + random.Next(1, 9) + ".png"));

            if (File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/XHH/user.temp"))
            {
                PasswordBox.Password = File.ReadAllText(userpath + "//user.temp");
                Checkbox_Login.IsChecked = true;
            }
            UseLogin();
        }
        private void Button_Close(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
        private void Button_Mini(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
            GC.Collect();
        }

        public bool ChangePW = false;

        public void GetList()
        {
            System.Threading.Tasks.Task.Run(async () =>
            {
                List<hwtask> hwtasks = database.Select<hwtask>()
                    .Where(hwtask => hwtask.subject == LoginUser.subject)
                    .Limit(10)
                    .ToList<hwtask>();
                await this.Dispatcher.InvokeAsync(new Action(() =>
                {
                    for (int i = 0; i < hwtasks.Count; i++)
                    {
                        Label mainContent = new Label()
                        {
                            FontSize = 20,
                            FontWeight = FontWeights.DemiBold,
                            Content = hwtasks[i].name,
                            FontFamily = new FontFamily("Microsoft YaHei UI"),
                        };
                        Label dateContent = new Label()
                        {
                            Content = "Date : " + hwtasks[i].date,
                            FontFamily = new FontFamily("Microsoft YaHei UI"),
                        };
                        Label classContent = new Label()
                        {
                            Content = "Class : " + hwtasks[i].classid,
                            FontFamily = new FontFamily("Microsoft YaHei UI"),
                            FontWeight=FontWeights.DemiBold,
                        };
                        StackPanel mainPanel = new StackPanel
                        {
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                            Orientation = Orientation.Vertical,
                        };
                        StackPanel a = new StackPanel
                        {
                            Orientation = Orientation.Horizontal,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            VerticalAlignment = VerticalAlignment.Top,
                        };
                        Grid maincontentGrid = new Grid
                        {
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                        };
                        Button aButton = new Button
                        {
                            Name = "TID" + hwtasks[i].id.ToString(),
                            Height = 100,
                            Width = 985,
                            Style = (Style)(FindResource("MaterialDesignOutlinedButton")),
                            Content = maincontentGrid
                        };
                        a.Children.Add(mainContent);
                        a.Children.Add(dateContent);
                        mainPanel.Children.Add(a);
                        mainPanel.Children.Add(classContent);
                        maincontentGrid.Children.Add(mainPanel);
                        aButton.Click += new RoutedEventHandler(Button_Main_Click);
                        ButtonAssist.SetCornerRadius(aButton, new CornerRadius(15));
                        ListPanel.Children.Add(aButton);
                    }
                }));
            });
        }
        public string[] status1={"未交","已交","已退回" };

        public string[] status2 =
            { "", "已查", "未写", "未订正", "待订正", "部分未写", "请假", "其他特殊情况","使用通用编号输入" };

        List<maininfo> maininfos=new List<maininfo>();
        private void Button_Main_Click(object sender, RoutedEventArgs e)
        {
            Button aButton = sender as Button;
            int hwid = int.Parse(aButton.Name.Substring(3));
            Main_ProgressBar.Visibility = Visibility.Visible;
            new Task(new Action(async () =>
            {
                hwtask a = database.Select<hwtask>().Where(hwtask=>hwtask.id==hwid).ToOne();
                maininfos = JsonConvert.DeserializeObject<List<maininfo>>(a.info);
                
                await this.Dispatcher.InvokeAsync(new Action(() =>
                {
                    StatusComboBox1.ItemsSource = status1;
                    StatusComboBox2.ItemsSource = status2;
                    HomeWorkIdTextBox.Text = "";
                   
                    TaskName.Content = a.name;
                    TaskDate.Content = "Date : " + a.date+"     Class :"+a.classid.ToString();

                    hwtasklist.ItemsSource = maininfos.Select(x => new
                    {
                        Name = x.name,
                        StatusString = status1[(int)x.status / 10] + " " + status2[(int)x.status % 10]
                    });
                    
                    Main_ProgressBar.Visibility = Visibility.Collapsed;
                    UseTask();
                    HomeWorkIdTextBox.Focus();
                }));
            })).Start();
            
        }

        private void Button_Login_Click(object sender, RoutedEventArgs e)
        {
            var pw = PasswordBox.Password;
            var flag = Checkbox_Login.IsChecked;
            Login_ProgressBar.Visibility = Visibility.Visible;
            new Task(new Action(async () =>
            {
                if (!ChangePW)
                {
                    LoginUser = database.Select<user>()
                        .Where(user => user.password == pw).ToOne();
                    if (LoginUser != null)
                    {

                        if (flag == true)
                        {
                            if (!Directory.Exists(userpath)) Directory.CreateDirectory(userpath);
                            File.WriteAllText(userpath + "\\user.temp", PasswordBox.Password.ToString());
                        }

                        await this.Dispatcher.InvokeAsync(new Action(() =>
                        {


                            if (LoginUser.type == 0)
                            {
                                Label_Welcome.Content = "欢迎您，" + LoginUser.name + "老师";
                                GetList();
                            }
                            else
                            {
                                Label_Welcome.Content = "欢迎您，" + LoginUser.name;
                                GetList();
                            }
                            UseHomepage();
                            Login_ProgressBar.Visibility = Visibility.Collapsed;
                        }));
                    }
                    else this.Dispatcher.Invoke(new Action(() =>
                    {
                        MessageBox.Show("Key was Wrong!");
                        Login_ProgressBar.Visibility = Visibility.Collapsed;
                    }));
                }
                else
                {
                    ChangeUserPassword(pw);
                    await this.Dispatcher.InvokeAsync(new Action(() =>
                    {

                        if (LoginUser.type == 0)
                        {
                            Label_Welcome.Content = "欢迎您，" + LoginUser.name + "老师";
                            Button_NewTask.Visibility = Visibility.Collapsed;
                            GetList();
                        }
                        else
                        {
                            Label_Welcome.Content = "欢迎您，" + LoginUser.name;
                            GetList();
                        }
                        UseHomepage();
                    }));
                }
            })).Start();
        }

        private void Button_ResetPw_Click(object sender, RoutedEventArgs e)
        {
            UseLogin();

            Label_Login_Title.Content = "XHomework Helper Reset Password";
            ChangePW = true;

        }

        private void Button_NT_Click(object sender, RoutedEventArgs e)
        {
            if (HomeworkTextbox.Text != "" && HomeworkComboBox.Text != "" && DatePicker.Text != "" && HomeworkClassComboBox.Text != "")
            {
                NewTask_ProcessBar.Visibility = Visibility.Visible;
                string classid = HomeworkClassComboBox.Text;
                string hwstr = HomeworkComboBox.Text;
                string name = HomeworkTextbox.Text;
                string date = DatePicker.Text;
                new Task(new Action(async () =>
                {
                    List<studentlist> a = database.Select<studentlist>()
                        .Where(studentlist => studentlist.classid == int.Parse(classid))
                        .ToList<studentlist>();
                    List<maininfo> info = new List<maininfo>();
                    int taskid = (int)database.Select<hwtask>().Count() + 10000;
                    homeworklist b = database.Select<homeworklist>().Where(homeworklist => homeworklist.name.Contains(hwstr)).ToOne<homeworklist>();
                    int subject = b.subject;
                    int hwid = b.id;
                    for (int i = 0; i < a.Count; i++)
                    {
                        MainWindow.maininfo temp = new maininfo();
                        temp.name = a[i].name;
                        temp.status = 0;
                        temp.taskid = taskid;
                        temp.hwid = (long)a[i].id * 1000 + (long)hwid;
                        info.Add(temp);
                    }

                    String maininfo = Newtonsoft.Json.JsonConvert.SerializeObject(info);
                    hwtask res = new hwtask();
                    res.classid = int.Parse(classid);
                    res.info = maininfo;
                    res.date = date;
                    res.id = taskid;
                    res.name = name;
                    res.subject = subject;
                    int tmp = database.Insert<hwtask>().AppendData(res).ExecuteAffrows();

                    await this.Dispatcher.InvokeAsync(new Action(() =>
                    {
                        UseHomepage();
                        NewTask_ProcessBar.Visibility = Visibility.Collapsed;
                        GetList();

                    }));
                })).Start();
            }
            else
            {
                MessageBox.Show("You don't have finish the FORM !!");
            }
        }

        private void Button_NewTask_Click(object sender, RoutedEventArgs e)
        {
            Main_ProgressBar.Visibility = Visibility.Visible;
            new Task(new Action(async () =>
            {
                List<homeworklist> a = database.Select<homeworklist>().Where(homeworklist => homeworklist.id > 100).ToList<homeworklist>();
                string[] b = new string[a.Count];
                for (int i = 0; i < a.Count; i++) b[i] = a[i].name;

                List<classlist> c = database.Select<classlist>().ToList<classlist>();
                string[] d = new string[c.Count];
                for (int j = 0; j < c.Count; j++) d[j] = c[j].id.ToString();

                await this.Dispatcher.InvokeAsync(new Action(() =>
                {
                    HomeworkComboBox.ItemsSource = b;
                    HomeworkClassComboBox.ItemsSource = d;
                    DatePicker.Text = System.DateTime.Now.ToString("yyyy/MM/dd");
                    Main_ProgressBar.Visibility = Visibility.Collapsed;
                    UseNewTask();
                }));

            })).Start();


        }

        

        private void HomeworkComboBox_Selected(object sender, RoutedEventArgs e)
        {
            HomeworkTextbox.Text = HomeworkComboBox.SelectedValue.ToString();
        }

        private void GoBackButton_Click(object sender, RoutedEventArgs e)
        {
            UseHomepage();
        }

        

        private void HomeWorkIdTextBox_TextChanged(object sender, KeyEventArgs e)
        {
            if(HomeWorkIdTextBox.Text.Length == 11 )
            {
                    var a = maininfos.Exists(maininfo => maininfo.hwid == long.Parse(HomeWorkIdTextBox.Text));
                    var b = StatusComboBox1.Text != "";
                if ( StatusComboBox1.Text != "" &&
                    maininfos.Exists(maininfo => maininfo.hwid == long.Parse(HomeWorkIdTextBox.Text)))
                {
                    var temp = maininfos.Where(maininfo => maininfo.hwid == long.Parse(HomeWorkIdTextBox.Text))
                        .FirstOrDefault();
                    temp.status = StatusComboBox1.SelectedIndex * 10 + StatusComboBox2.SelectedIndex;
                    maininfos.Where(maininfo => maininfo.hwid == long.Parse(HomeWorkIdTextBox.Text))
                        .FirstOrDefault().status= StatusComboBox1.SelectedIndex * 10 + StatusComboBox2.SelectedIndex;
                    hwtasklist.Items.Clear();
                    for (int i = 0; i < maininfos.Count; i++)
                    {
                        hwtasklist.Items.Add(new
                        {
                            Name = maininfos[i].name,
                            StatusString = status1[(int)maininfos[i].status / 10] + " " +
                                           status2[(int)maininfos[i].status % 10]
                        });
                    }
                }
                else if (HomeWorkIdTextBox.Text.ToString().Substring(8) == "000")
                {

                }
                else
                {
                    
                }
            }
        }

        private void HomeWorkIdTextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (StatusComboBox1 == null 
                || StatusComboBox2 == null 
                || StatusComboBox1.SelectedItem == null 
                || StatusComboBox2.SelectedItem == null)
                return;

            if (string.IsNullOrEmpty(HomeWorkIdTextBox.Text) || HomeWorkIdTextBox.Text.Length != 11)
                return;

            maininfos.FirstOrDefault(x => x.hwid == long.Parse(HomeWorkIdTextBox.Text))
                .status = StatusComboBox1.SelectedIndex * 10 + StatusComboBox2.SelectedIndex;

            hwtasklist.ItemsSource = null;
            hwtasklist.ItemsSource = maininfos.Select(x => new
            {
                Name = x.name,
                StatusString = status1[(int)x.status / 10] + " " + status2[(int)x.status % 10]
            });
        }
    }
}
