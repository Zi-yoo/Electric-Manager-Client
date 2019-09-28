using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ZedGraph;
using System.Timers;

namespace WindowsFormsApp1
{

    public partial class Main : Form
    {
        Thread myThread;

        //public string url = @"http://iot.nsfox.com/device/all";
        public string url = @"http://javacloud.bmob.cn/391373278e6ec6a0/dataview";
        public string floor;
        public string room;
        public string inttimer;
        public int inttime;
        public int status;
        public string sendUrl;
        //定义Timer类
        System.Timers.Timer timer;
        //定义委托
        public delegate void SetControlValue(string value);
        public delegate void starturl(object send);

        //WebClient webClient = new WebClient();

        public static bool isUse = true;//是否停止更新

        private int count = 10;
        private GraphPane mPane;
        private PointPairList pointList;

        public DataSet dataSet = new DataSet();
        public DataTable dataTable = new DataTable();
        DataColumn dc1 = new DataColumn();
        DataColumn dc2 = new DataColumn();

        List<string> cb1 = new List<string> { };
        List<string> cb2 = new List<string> { };

        public int frequency = 500;//更新时间频率
        public static string statusInfo = string.Empty;//状态
        private delegate void myDelegate(DataTable dt);//定义委托

        private List<float> powerList = new List<float>();

        public Main()
        {
            InitializeComponent();

            //var json = new WebClient().DownloadString(url);
            //string testjson = @"{
            //    'Table1':[
            //      {
            //        'id':1,
            //        'floor':'F1',
            //        'room':'R1',
            //        'power':'0',
            //        'status':'1',
            //        'mode':'0',
            //        'note':'',
            //        'created_at':'2017 - 03 - 08 08:38:31',
            //        'updated_at':'2017 - 03 - 17 01:59:24'
            //      },
            //      {
            //        'id':2,
            //        'floor':'F2',
            //        'room':'R1',
            //        'power':'3.2',
            //        'status':'1',
            //        'mode':'0',
            //        'note':'',
            //        'created_at':'2017 - 03 - 08 08:38:58',
            //        'updated_at':'2017 - 03 - 08 08:38:58'
            //      }
            //    ]
            //}";



            //Console.WriteLine(dataTable.Rows.Count);
            //MessageBox.Show(" count " +dataTable.Rows.Count);

        }

        private void Main_Load(object sender, EventArgs e)
        {
            mPane = zedGraphControl1.GraphPane;//获取索引到GraphPane面板上
            mPane.XAxis.Title.Text = "时间";//X轴标题
            mPane.YAxis.Title.Text = "功耗（W）";//Y轴标题
            mPane.Title.Text = "功耗数据";//标题
            //mPane.XAxis.Scale.MaxAuto = true;
            mPane.XAxis.Type = ZedGraph.AxisType.LinearAsOrdinal;//出现图表右侧出现空白的情况....
            pointList = new PointPairList();//数据点
            mPane.XAxis.CrossAuto = true;//容许x轴的自动放大或缩小

            myThread = new Thread(startFillDv);//实例化线程
            myThread.Start();

            powerList = new List<float>();
            Random rdn = new Random();

            for (int i = 0; i < 30; i++)
            {
                powerList.Add((float)(rdn.NextDouble()));
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (this.myThread.IsAlive)
            {
                this.myThread.Abort();//结束线程
            }
        }

        private void startFillDv()
        {
            while (true)
            {
                if (isUse)
                {
                    //statusInfo = "正在实时更新数据......";
                    //Debug.WriteLine(statusInfo);
                    string json = HttpGet(url);
                    //string json = webClient.DownloadString(url);
                    json = "{'Table1':" + json + "}";
                    try
                    {
                        dataSet = JsonConvert.DeserializeObject<DataSet>(json);
                        dataTable = dataSet.Tables["Table1"];
                        Combo(dataTable);
                        Grid(dataTable);
                    }
                    catch (Exception)
                    {
                        return;
                    }

                    Thread.Sleep(frequency);
                }
                else
                {
                    statusInfo = "停止更新!";
                }
            }

        }

        private void Grid(DataTable dt)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new myDelegate(Grid), new object[] { dt });
            }
            else
            {
                try
                {
                    this.dataGridView1.DataSource = null;
                    this.dataGridView1.DataSource = dt;
                    dt = null;

                    //statusInfo = "更新完成!";
                    //Debug.WriteLine(statusInfo);
                }
                catch
                {

                }
            }

        }

        private void Combo(DataTable dt)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new myDelegate(Combo), new object[] { dt });
            }
            else
            {
                try
                {
                    if (!dc1.Equals(dt.Columns[1]))
                    {
                        //statusInfo = "更新combo 1 !";
                        //Debug.WriteLine(statusInfo);
                        dc1 = dt.Columns[1];
                        comboBox1.Items.Clear();
                        //dt.Columns.Count - 1
                        for (int i = 0; i < dt.Columns.Count / 2; i++)
                        {
                            comboBox1.Items.Add(dt.Rows[i][1]);
                        }
                    }
                    if (!dc2.Equals(dt.Columns[2]))
                    {
                        //statusInfo = "更新combo 2 !";
                        dc2 = dt.Columns[2];
                        comboBox2.Items.Clear();
                        //dt.Columns.Count - 1
                        for (int i = 0; i < dt.Columns.Count / 2 ; i++)
                        {
                            comboBox2.Items.Add(dt.Rows[i][2]);
                        }
                    }
                }
                catch
            {

            }
        }

        }

        /// 后台发送GET请求
        /// </summary>
        /// <param name="url">服务器地址</param>
        /// <param name="data">发送的数据</param>
        /// <returns></returns>
        public string HttpGet(string url)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                request.ContentType = "text/html;charset=UTF-8";

                //接受返回来的数据
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream stream = response.GetResponseStream();
                StreamReader streamReader = new StreamReader(stream, Encoding.GetEncoding("utf-8"));
                string retString = streamReader.ReadToEnd();

                streamReader.Close();
                stream.Close();
                response.Close();

                return retString;
            }
            catch (Exception)
            {
                return "";
            }
        }
       

        private void domainUpDown1_SelectedItemChanged(object sender, EventArgs e)
        {

        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar >= '0' && e.KeyChar <= '9' || e.KeyChar >= 'a' && e.KeyChar <= 'z' || e.KeyChar == (char)Keys.Back)
            {
                e.Handled = false;
                //已处理
                //允许输入数字，如果输入的在0~9范围内,则返回false,即e.Handle=false;
                //表示对该输入事件进行处理,即接受;
            }
            else
            {
                e.Handled = true;
                //反之,输入在此范围之外,则返回true,不处理,即不接受...
            }

        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comboBox1.Text)) {
                MessageBox.Show("请输入楼层(floor)!");
                return;
            }

            if (string.IsNullOrEmpty(comboBox2.Text))
            {
                MessageBox.Show("请输入房间(room)!");
                return;
            }

            //string floor = comboBox1.Text;
            //string room = comboBox2.Text;
            //string timer = "0";
            floor = comboBox1.Text;
            room = comboBox2.Text;
            inttimer = "0";
            if (!string.IsNullOrEmpty(textBox1.Text))
            {
                inttimer = textBox1.Text;
            }

            status = comboBox3.SelectedIndex;
           sendUrl = "https://javacloud.bmob.cn/391373278e6ec6a0/dataupdate?floor=" + floor + "&room=" + room + "&status=" + status;
            //Console.WriteLine(sendUrl);
            //设置定时间隔(毫秒为单位)
            int.TryParse(inttimer, out inttime);
            int interval = 1000;
            timer = new System.Timers.Timer(interval);
            //设置执行一次（false）还是一直执行(true)
            timer.AutoReset = true;
            //设置是否执行System.Timers.Timer.Elapsed事件
            timer.Enabled = true;
            //绑定Elapsed事件
            timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerDown);


            //System.Timers.Timer t = new System.Timers.Timer();//实例化Timer类
            ////int intTime = 3000;
            //int intTime;
            //int.TryParse(timer, out intTime);

            //t.Interval = intTime;//设置间隔时间，为毫秒；
            //t.Elapsed += new System.Timers.ElapsedEventHandler(setting(status,t));//到达时间的时候执行事件；
            //t.AutoReset = false;//设置是执行一次（false）还是一直执行(true)；
            //t.Enabled = true;//是否执行System.Timers.Timer.Elapsed事件；


            //string sendUrl = "http://iot.nsfox.com/wechat/settimer/floor/" + floor+"/room/"+room+"/timer/"+timer+"/status/"+status;

            //string sendUrl = "https://javacloud.bmob.cn/391373278e6ec6a0/dataupdate?floor=" + floor + "&room=" + room + "&timer=" + timer + "&status=" + status;
            //Debug.WriteLine(sendUrl);

            //string res = HttpGet(sendUrl);

            //MessageBox.Show(res);
        }


        private void TimerDown(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                if (inttime >= 0)
                {
                    inttime -= 1;
                }

                this.Invoke(new SetControlValue(SetTextBoxText), inttime.ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show("执行定时到点事件失败:" + ex.Message);
            }
        }
        private void SetTextBoxText(string strValue)
        {
            if (inttime >= 0)
            {
                this.textBox1.Text = this.inttime.ToString().Trim();
            }
            Console.WriteLine("倒数");
            if (inttime == -1)
            {
                //sendUrl = "https://javacloud.bmob.cn/391373278e6ec6a0/dataupdate?floor=" + floor + "&room=" + room + "&timer=" + timer + "&status=" + status;
                //Debug.WriteLine(sendUrl);
                //Console.WriteLine(sendUrl);
                string res = HttpGet(sendUrl);
            }
        }




        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        public static extern long BitBlt(IntPtr hdcDest, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, int dwRop);
        private Bitmap memoryImage;
        private void CaptureScreen()
        {
            Graphics mygraphics = this.CreateGraphics();
            Size s = this.Size;
            memoryImage = new Bitmap(s.Width, s.Height, mygraphics);
            Graphics memoryGraphics = Graphics.FromImage(memoryImage);
            IntPtr dc1 = mygraphics.GetHdc();
            IntPtr dc2 = memoryGraphics.GetHdc();
            BitBlt(dc2, 10, 10, 1100, 309, dc1, 12, 377, 13369376);
            mygraphics.ReleaseHdc(dc1);
            memoryGraphics.ReleaseHdc(dc2);
        }
        private void printDocument1_PrintPage(System.Object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(memoryImage, 0, 0);
        }
        private void print_Click(object sender, EventArgs e)
        {
            CaptureScreen();
            //打印预览 
            PrintPreviewDialog ppd = new PrintPreviewDialog();
            ppd.ClientSize = new System.Drawing.Size(1200, 500);
            PrintDocument pd = new PrintDocument();
            //设置边距
            Margins margin = new Margins(20, 20, 20, 20);
            pd.DefaultPageSettings.Margins = margin;
            ////纸张设置默认
            PaperSize pageSize = new PaperSize("First custom size", 1150, 400);
            pd.DefaultPageSettings.PaperSize = pageSize;
            //打印事件设置 
            pd.PrintPage += new PrintPageEventHandler(this.printDocument1_PrintPage);
            ppd.Document = pd;
            ppd.ShowDialog();
            /*try
            {
                pd.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "打印出错", MessageBoxButtons.OK, MessageBoxIcon.Error);
                pd.PrintController.OnEndPrint(pd, new PrintEventArgs());
            }*/
        }

        private void getInfo_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(comboBox1.Text))
            {
                MessageBox.Show("请输入楼层(floor)!");
                return;
            }

            if (string.IsNullOrEmpty(comboBox2.Text))
            {
                MessageBox.Show("请输入房间(room)!");
                return;
            }

            string floor = comboBox1.Text;
            string room = comboBox2.Text;

            //string sendUrl = "http://iot.nsfox.com/device/record/floor/" + floor + "/room/" + room + "/count/" + count;
            string sendUrl = "https://javacloud.bmob.cn/391373278e6ec6a0/datadig?floor=" + floor + "&room=" + room + "&count=" + count;

            //Debug.WriteLine(sendUrl);

            string json = HttpGet(sendUrl);

            Console.WriteLine(json);
            List<float> powerList = JsonConvert.DeserializeObject<List<float>>(json);


            Console.WriteLine("");
            
            //Debug.WriteLine("count is " + count + "  , powerList.Count is " + powerList.Count);

            pointList.Clear();

            float sum = 0;
            float average = 0;
            float highest = 0;

            for (int i = 0; i < powerList.Count; i++)
            {
                int x = i;
                float y = powerList[powerList.Count - 1 - i] * 220;
                sum = sum + y;
                if (highest < y) {
                    highest = y;
                }
                pointList.Add(x, y);
            }

            average = sum / powerList.Count;
            highest_label.Text = highest + "W";
            average_label.Text = average + "W";

            LineItem mCure = mPane.AddCurve("", pointList, Color.DarkBlue, SymbolType.None);
            zedGraphControl1.AxisChange();//画到zedGraphControl1控件中，此句必加
            zedGraphControl1.Refresh();//重新刷新
        }
    }
}
