using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class login : Form
    {

        string user;
        string pwd;

        public login()
        {
            InitializeComponent();
        }

        private void maskedTextBox1_MaskInputRejected(object sender, MaskInputRejectedEventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            user = textBox1.Text.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //if (user != null && pwd != null)
            //{
                this.DialogResult = DialogResult.OK;
            //}
            //else {
            //    MessageBox.Show("用户名或密码未输入");
            //}
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            pwd = textBox2.Text.ToString();
        }
    }
}
