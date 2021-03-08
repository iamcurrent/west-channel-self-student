using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NationalInstruments.Visa;
namespace Model63200ESeries
{
    public partial class SelectResource : Form
    {
        public SelectResource()
        {
            InitializeComponent();
        }

        private void SelectResource_Load(object sender, EventArgs e)
        {
            using (var rmSession = new ResourceManager())
            {
                var resources = rmSession.Find("(ASRL|GPIB|TCPIP|USB)?*"); //查新当前连接的设备
                foreach (string s in resources)
                {
                    listBox1.Items.Add(s);
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ResourceName = listBox1.SelectedItem.ToString();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBox1.Text = listBox1.SelectedItem.ToString();
        }

        public string ResourceName
        {
            get
            {
                return listBox1.Text;
            }
            set
            {
                listBox1.Text = value;
            }
        }
    }
}
