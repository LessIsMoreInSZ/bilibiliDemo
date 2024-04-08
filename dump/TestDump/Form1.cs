using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestDump
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // 创建bug让程序闪退
            //double[] doubles = new double[10];
            //for(int i = 0; i < 12; i++) 
            //{
            //    doubles[i] = 5;
            //}

            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Thread thread = new Thread(TestDump);
            //thread.Start();

            while (true)
            {
                Task.Run(() =>
                {
                    if(Thread.CurrentThread.ManagedThreadId.ToString().Equals("1"))
                    {
                        textBox1.Text = "1";
                        TextBox text = new TextBox();
                        this.Controls.Add(text);
                    }
                });
                Thread.Sleep(100);
            }

            Task.Run(() =>
            {
                MessageBox.Show(Thread.CurrentThread.ManagedThreadId.ToString());
                textBox1.Text = "1";
                TextBox text = new TextBox();
                this.Controls.Add(text);
            });
        }

        private void TestDump()
        {
            textBox1.Text = "1";
            TextBox text = new TextBox();
            this.Controls.Add(text);
        }
    }
}
