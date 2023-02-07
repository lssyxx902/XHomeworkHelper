using FreeSql.DataAnnotations;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Button = System.Windows.Controls.Button;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using MessageBox = System.Windows.MessageBox;
using Label = System.Windows.Controls.Label;
using Orientation = System.Windows.Controls.Orientation;
using System.Reflection;
using MySqlX.XDevAPI.Common;
using XHomeWorkHelper;
using Clipboard = System.Windows.Clipboard;
using System.Speech.Synthesis;

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
            public int type { get; set; } //tch=0,kdb=
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
            public bool isactive { get; set; }
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

            List<string> temp =
                JsonConvert.DeserializeObject<List<string>>(GetUrlText("https://www.309133584.com/sth.html"));
            Random rand = new Random();
            Text_Title.Text = Text_Title.Text + " ———— " + temp[rand.Next(0, temp.Count() - 1)];
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
                    .Where(hwtask =>
                        (hwtask.subject == LoginUser.subject || LoginUser.type == 1) && hwtask.isactive == true)
                    .ToList<hwtask>();
                hwtasks.Sort((x,y)=> -x.id.CompareTo(y.id));
                
                await this.Dispatcher.InvokeAsync(new Action(() =>
                {
                    ListPanel.Children.Clear();
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
        private hwtask a = new hwtask();
        private void Button_Main_Click(object sender, RoutedEventArgs e)
        {
            Button aButton = sender as Button;
            int hwid = int.Parse(aButton.Name.Substring(3));
            Main_ProgressBar.Visibility = Visibility.Visible;
            new Task(new Action(async () =>
            {
                a = database.Select<hwtask>().Where(hwtask=>hwtask.id==hwid).ToOne();
                maininfos = JsonConvert.DeserializeObject<List<maininfo>>(a.info);
                
                await this.Dispatcher.InvokeAsync(new Action(() =>
                {
                    StatusComboBox1.ItemsSource = status1;
                    StatusComboBox2.ItemsSource = status2;
                    StatusComboBox1.SelectedIndex = 1;
                    StatusComboBox2.SelectedIndex = 0;
                    HomeWorkIdTextBox.Text = "";
                   
                    TaskName.Content = a.name;
                    TaskDate.Content = "Date : " + a.date+"     Class :"+a.classid.ToString();

                    hwtasklist.ItemsSource = maininfos.Select(x => new
                    {
                        Name = x.name,
                        StatusString = status1[(int)x.status / 10] + " " + status2[(int)x.status % 10],
                        StudentID=x.hwid.ToString().Substring(0,8)
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
                    res.isactive = true;
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
            TaskProcessBar.Visibility = Visibility.Visible;
            System.Threading.Tasks.Task.Run(async () =>
            {
                a.info = JsonConvert.SerializeObject(maininfos);
                var Temp = database.Update<hwtask>().Set(x => x.info, a.info).Where(x => x.id == a.id).ExecuteAffrows();

                await this.Dispatcher.InvokeAsync(new Action(() =>
                {

                    SnackbarTwo.Message.Content = "已保存更改";
                    SnackbarTwo.IsActive = true;
                    System.Timers.Timer snackbarTimer = new System.Timers.Timer(4000);
                    snackbarTimer.Elapsed += new System.Timers.ElapsedEventHandler(Snackbarclose);
                    snackbarTimer.Start();
                    UseHomepage();
                    UseTaskPanel();
                    TaskProcessBar.Visibility = Visibility.Collapsed;
                }));
            });
            
        }

        

       /* private void HomeWorkIdTextBox_TextChanged(object sender, KeyEventArgs e)
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
        }*/

       private void HomeWorkIdTextBox_TextChanged_1(object sender, TextChangedEventArgs e)
       {
            if (StatusComboBox1 == null || StatusComboBox2 == null)
               return;

            if ((StatusComboBox1.SelectedItem == null && HomeWorkIdTextBox.Text.Length == 11) || (
                    StatusComboBox2.SelectedItem == null && HomeWorkIdTextBox.Text.Length == 11))
            {
                MessageBox.Show("You don't choose Status !!");
                return;
            }
            
            if (HomeWorkIdTextBox.Text.Length == 11 &&
               !maininfos.Exists(x => x.hwid == long.Parse(HomeWorkIdTextBox.Text)))
            {
               MessageBox.Show("The HWID is WRONG !!");
               HomeWorkIdTextBox.Clear();
               HomeWorkIdTextBox.Focus();
                return;
            }
            if (HomeWorkIdTextBox.Text.Length == 3 && HomeWorkIdTextBox.Text[0] == '*')
            {
                HomeWorkIdTextBox.Text = maininfos.FirstOrDefault(x => x.hwid.ToString().Contains("202002" + HomeWorkIdTextBox.Text.Substring(1))).hwid.ToString();
                return;
            }

            if (string.IsNullOrEmpty(HomeWorkIdTextBox.Text) || HomeWorkIdTextBox.Text.Length != 11 )
                return;

            

            var amaininfo=maininfos.FirstOrDefault(x => x.hwid == long.Parse(HomeWorkIdTextBox.Text));
            maininfos.FirstOrDefault(x => x.hwid == long.Parse(HomeWorkIdTextBox.Text))
                .status = StatusComboBox1.SelectedIndex * 10 + StatusComboBox2.SelectedIndex;

            SnackbarTwo.Message.Content= amaininfo.hwid.ToString().Substring(0, 8) + " " + amaininfo.name + " " + status1[(int)amaininfo.status / 10] + " " + status2[(int)amaininfo.status % 10] ;
            SnackbarTwo.IsActive = true;
            System.Timers.Timer snackbarTimer = new System.Timers.Timer(2000);
            snackbarTimer.Elapsed += new System.Timers.ElapsedEventHandler(Snackbarclose);
            snackbarTimer.Start();
            SpeechSynthesizer tts = new SpeechSynthesizer();
            tts.SpeakAsync(amaininfo.name);

            hwtasklist.ItemsSource = null;
            hwtasklist.ItemsSource = maininfos.Select(x => new
            {
                Name = x.name,
                StatusString = status1[(int)x.status / 10] + " " + status2[(int)x.status % 10],
                StudentID=x.hwid.ToString().Substring(0,8)
            });

            HomeWorkIdTextBox.Clear();
            HomeWorkIdTextBox.Focus();
       }

        public void Snackbarclose(object a, System.Timers.ElapsedEventArgs e)
        {
            this.Dispatcher.InvokeAsync(new Action(() => { SnackbarTwo.IsActive = false; }));
        }

        private void SaveChangesButton_Click(object sender, RoutedEventArgs e)
        {
            TaskProcessBar.Visibility = Visibility.Visible;
            System.Threading.Tasks.Task.Run(async () =>
            {
                a.info = JsonConvert.SerializeObject(maininfos);
                var Temp=database.Update<hwtask>().Set(x=>x.info,a.info).Where(x => x.id == a.id).ExecuteAffrows();

                await this.Dispatcher.InvokeAsync(new Action(() =>
                {

                    SnackbarTwo.Message.Content = "已保存更改";
                    SnackbarTwo.IsActive = true;
                    System.Timers.Timer snackbarTimer = new System.Timers.Timer(4000);
                    snackbarTimer.Elapsed += new System.Timers.ElapsedEventHandler(Snackbarclose);
                    snackbarTimer.Start();

                    TaskProcessBar.Visibility=Visibility.Collapsed;
                }));
            });
        }

        public void UseTools()
        {
            TaskAddButtons.Visibility = Visibility.Collapsed;
            TaskAddPanel.Visibility = Visibility.Collapsed;
            Tools.Visibility=Visibility.Visible;

            string[] a = { "ALL", "异常情况", "正常情况" };
            SearchStatusComboBox.ItemsSource = a;
            SearchStatusComboBox.SelectedIndex = -1;
        }

        public void UseTaskPanel()
        {
            TaskAddPanel.Visibility = Visibility.Visible;
            TaskAddButtons.Visibility = Visibility.Visible;
            Tools.Visibility=Visibility.Collapsed;
            
            
        }

        private void GoToolsButton_Click(object sender, RoutedEventArgs e)
        {
            UseTools();
        }

        private void SearchStatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SearchStatusComboBox.SelectedItem == null) return;
            if (SearchStatusComboBox.SelectedIndex == 0)
            {
                hwtasklist.ItemsSource = maininfos.Select(x => new
                {
                    Name = x.name,
                    StatusString = status1[(int)x.status / 10] + " " + status2[(int)x.status % 10],
                    StudentID = x.hwid.ToString().Substring(0, 8)
                });
            }

            if (SearchStatusComboBox.SelectedIndex == 1)
            {
                hwtasklist.ItemsSource = maininfos.Where(x=>x.status!=10&&x.status!=11)
                    .Select(x => new
                {
                    Name = x.name,
                    StatusString = status1[(int)x.status / 10] + " " + status2[(int)x.status % 10],
                    StudentID = x.hwid.ToString().Substring(0, 8)
                });
            }

            if (SearchStatusComboBox.SelectedIndex == 2)
            {
                hwtasklist.ItemsSource = maininfos.Where(x => x.status == 10 || x.status == 11)
                    .Select(x => new
                    {
                        Name = x.name,
                        StatusString = status1[(int)x.status / 10] + " " + status2[(int)x.status % 10],
                        StudentID = x.hwid.ToString().Substring(0, 8)
                    });
            }
        }

        public class CSVinfo
        {
            public string id;
            public string name;
            public string status;
            public string hwname;
        }

        private void CSVSendButton_Click(object sender, RoutedEventArgs e)
        {
            List<CSVinfo> list = new List<CSVinfo>();
            for (int i = 0; i < maininfos.Count(); i++)
            {
                var tempCSVinfo = new CSVinfo();
                tempCSVinfo.id = maininfos[i].hwid.ToString().Substring(0, 8);
                tempCSVinfo.name = maininfos[i].name;
                tempCSVinfo.status = status1[(int)maininfos[i].status / 10] + " " + status2[(int)maininfos[i].status % 10];
                tempCSVinfo.hwname = a.name;
                list.Add(tempCSVinfo);
            }
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "XHomeWorkHelper >>> 请选择保存文件路径";
            dialog.ShowDialog();
            string Path = "";
            if (dialog.SelectedPath!="")
            {
                Path = dialog.SelectedPath + @"\";
            }
            else
            {
                MessageBox.Show("You Don't Choose Your Path!!");
                return;
            }

            string filename =  a.name + "_" + a.classid + "_统计名单.csv";
            bool temp=SaveDataToCSVFile(list,CreateFile(Path, filename)) ;
            if (!temp)
            {
                MessageBox.Show("May Have Sth. WRONG! It not succeed");
            }

        }
        private string CreateFile(string folder, string fileName)
        {
            FileStream fs = null;
            string filePath = folder + fileName ;
            try
            {
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                fs = File.Create(filePath);
            }
            catch (Exception ex)
            { }
            finally
            {
                if (fs != null)
                {
                    fs.Dispose();
                }
            }
            return filePath;
        }
        
        private bool SaveDataToCSVFile(List<CSVinfo> list, string filePath)
        {
            bool successFlag = true;

            StringBuilder strColumn = new StringBuilder();
            StringBuilder strValue = new StringBuilder();
            StreamWriter sw = null;
            
           // try
           //{
                sw = new StreamWriter(filePath,true,Encoding.GetEncoding("GB2312"));
                strColumn.Append("StudentID,Name,Status,Homework");
                
                sw.WriteLine(strColumn);    //write the column name

                for (int i = 0; i < list.Count; i++)
                {
                    strValue.Remove(0, strValue.Length); //clear the temp row value
                    strValue.Append(list[i].id);
                    strValue.Append(",");
                    strValue.Append(list[i].name);
                    strValue.Append(",");
                    strValue.Append(list[i].status);
                    strValue.Append(",");
                    strValue.Append(list[i].hwname);
                    sw.WriteLine(strValue); //write the row value
                }
                sw.Close();
           // }
           // catch (Exception ex)
            //{
            //    throw (ex);
             //   successFlag = false;
           // }
           // finally
          //  {
            //    if (sw != null)
               // {
                //    sw.Dispose();
               // }
           // }

            return successFlag;
        }

        public string ExcuteString(List<maininfo> list)
        {
            string result ="";
            result += a.name + " " + a.date + " 作业上交情况 \n";

            if (list.Exists(x => x.status / 10 == 0))
            {
                result += "未交： ";
                var templist = list.Where(x => x.status / 10 == 0 && x.status%10 == 0).ToList();
                for (int i = 0; i < templist.Count(); i++)
                {
                    result += templist[i].name + "    ";
                }

                result += "\n";
            }
            if (list.Exists(x => x.status / 10 == 1))
            {
                result += "已交： ";
                var templist = list.Where(x => x.status / 10 == 1).ToList();
                for (int i = 0; i < templist.Count(); i++)
                {
                    result += templist[i].name + "    ";
                }
                result += "\n";
            }
            if (list.Exists(x => x.status % 10 == 2))
            {
                result += "未写： ";
                var templist = list.Where(x => x.status % 10 == 2).ToList();
                for (int i = 0; i < templist.Count(); i++)
                {
                    result += templist[i].name + "    ";
                }
                result += "\n";
            }
            if (list.Exists(x => x.status % 10 == 3))
            {
                result += "未订正： ";
                var templist = list.Where(x => x.status % 10 == 3).ToList();
                for (int i = 0; i < templist.Count(); i++)
                {
                    result += templist[i].name + "    ";
                }
                result += "\n";
            }
            if (list.Exists(x => x.status % 10 == 4))
            {
                result += "待订正： ";
                var templist = list.Where(x => x.status % 10 == 4).ToList();
                for (int i = 0; i < templist.Count(); i++)
                {
                    result += templist[i].name+"    ";
                }
                result += "\n";
            }
            if (list.Exists(x => x.status % 10 == 5))
            {
                result += "部分未写： ";
                var templist = list.Where(x => x.status % 10 == 5).ToList();
                for (int i = 0; i < templist.Count(); i++)
                {
                    result += templist[i].name + "    ";
                }
                result += "\n";
            }
            if (list.Exists(x => x.status % 10 == 6))
            {
                result += "请假： ";
                var templist = list.Where(x => x.status % 10 == 6).ToList();
                for (int i = 0; i < templist.Count(); i++)
                {
                    result += templist[i].name + "    ";
                }
                result += "\n";
            }

            return result;
        }
        private void QQSendButton_Click(object sender, RoutedEventArgs e)
        {
            string res = "";
            if (SearchStatusComboBox.SelectedItem == null) return;
            if (SearchStatusComboBox.SelectedIndex == 0)
            {
               res=ExcuteString(maininfos);
            }

            if (SearchStatusComboBox.SelectedIndex == 1)
            {
                res=ExcuteString(maininfos.Where(x => x.status != 10 && x.status != 11).ToList());
            }

            if (SearchStatusComboBox.SelectedIndex == 2)
            {
                res=ExcuteString(maininfos.Where(x => x.status == 10 || x.status == 11).ToList());
            }

            MessageBox.Show(res + "\n 由于机器人接口并未写好，所以以上文本已复制，可以手动发送");
            SetText(res);
        }

        private void DingTalkSendButton_Click(object sender, RoutedEventArgs e)
        {
            string res = "";
            if (SearchStatusComboBox.SelectedItem == null) return;
            if (SearchStatusComboBox.SelectedIndex == 0)
            {
                res = ExcuteString(maininfos);
            }

            if (SearchStatusComboBox.SelectedIndex == 1)
            {
                res = ExcuteString(maininfos.Where(x => x.status != 10 && x.status != 11).ToList());
            }

            if (SearchStatusComboBox.SelectedIndex == 2)
            {
                res = ExcuteString(maininfos.Where(x => x.status == 10 || x.status == 11).ToList());
            }

            MessageBox.Show(res + "\n 由于机器人接口并未写好，所以以上文本已复制，可以手动发送");
            SetText(res);
        }

        [DllImport("User32")]
        public static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("User32")]
        public static extern bool CloseClipboard();

        [DllImport("User32")]
        public static extern bool EmptyClipboard();

        [DllImport("User32")]
        public static extern bool IsClipboardFormatAvailable(int format);

        [DllImport("User32")]
        public static extern IntPtr GetClipboardData(int uFormat);

        [DllImport("User32", CharSet = CharSet.Unicode)]
        public static extern IntPtr SetClipboardData(int uFormat, IntPtr hMem);

        public static void SetText(string text)
        {
            if (!OpenClipboard(IntPtr.Zero))
            {
                SetText(text);
                return;
            }
            EmptyClipboard();
            SetClipboardData(13, Marshal.StringToHGlobalUni(text));
            CloseClipboard();
        }

        public int deletetemp = new int();
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            deletetemp++;
            if (deletetemp < 3) return;
            deletetemp = 0;
            var Temp = database.Update<hwtask>().Set(x => x.isactive, false).Where(x => x.id == a.id)
                .ExecuteAffrows(); 
            MessageBox.Show("Finish! But We Don't know the status");
            UseHomepage();
            UseTaskPanel();
            GetList();
        }

        private void GoCaramaButton_Click(object sender, RoutedEventArgs e)
        {
            Window1 camaraScan = new Window1();
            camaraScan.accept+= new EventHandler(returnres);
            camaraScan.ShowDialog();
        }
         
        private void returnres(object sender, EventArgs e)
        {
            Window1 temp = (Window1)sender;
            string[] res = temp.InputValue.Split(',');
            if (res == null || res.Count() < 1) return;
            
            for (int i = 0; i < res.Count()-1; i++)
            {
                HomeWorkIdTextBox.Text = res[i];
            }
        }

        private void hwtasklist_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (hwtasklist.SelectedItem != null)
            {
                object selectedItem = hwtasklist.SelectedItem;

                
            }
        }
    }
}
