using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GetDataAndFormat
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            FatchStockData fatchStockData = new FatchStockData();
            await fatchStockData.GetStockData();            
            MessageBox.Show("Data Downloaded!");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DailyDataFormat.FormaatData();
            MessageBox.Show("Data format done!");
        }
    }
}
