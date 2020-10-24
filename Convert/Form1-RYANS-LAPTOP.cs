using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Convert
{
    public partial class Form1 : Form
    {
        string root = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        public Form1()
        {
            InitializeComponent();
            string path = root + @"\Checking.csv";
            string input = File.ReadAllText(path);

            if (File.Exists(path))
            {
                string[] rows = File.ReadAllLines(path);
                string[,] transactions = new string[rows.Length, 4];

                for (int i = 0; i < rows.Length; i++)
                {
                    string[] box = rows[i].Split(',');

                    transactions[i, 0] = box[0].Trim('"').TrimEnd('"');
                    transactions[i, 1] = box[1].Trim('"').TrimEnd('"');
                    transactions[i, 2] = box[4].Trim('"').TrimEnd('"');
                    /* Format
                    08/26/2019  -1.99   PURCHASE AUTHORIZED ON 08/24 PAYPAL *ROYAL APPS 402-935-7733 CA S589236614804460 CARD 9233
                    08/19/2019  1.76    MOBILE DEPOSIT : REF NUMBER :214180386190
                    */

                    //Deposit Format
                    if (transactions[i, 2].Substring(0, 14) == "MOBILE DEPOSIT")
                    {
                        //gets ref number
                        transactions[i, 3] = transactions[i, 2].Substring(29);
                        transactions[i, 2] = "Deposit";
                    }

                    //Purchase Format
                    else if (transactions[i, 2].Substring(0, 8) == "PURCHASE")
                    {
                        //gets company name
                        int end;
                        if (transactions[i, 2].Contains('#'))
                        {
                            end = transactions[i, 2].IndexOf('#') - 30;
                        }
                        else
                        {
                            end = 20;
                        }
                        transactions[i, 3] = transactions[i, 2].Substring(29, end);
                        transactions[i, 2] = "Purchase";
                    }

                    //Online Transfer Format
                    else if (transactions[i, 2].Substring(0, 15) == "ONLINE TRANSFER")
                    {
                        //gets sender
                        transactions[i, 3] = transactions[i, 2].Substring(21, transactions[i, 2].IndexOf('#') - 26);
                        transactions[i, 2] = "Deposit";
                    }
                    //Debug.WriteLine(string.Join(":", string.Join("~", Enumerable.Range(0, transactions.GetLength(1)).Select(column => transactions[i, column]))));
                    /*Format    0 Date      1 Money     2 Type      3 Compnay/Number
                                06/28/2019  -47.93      Purchase    NORDSTROM DIRECT
                    */
                }

                //transactions
                dataGridView2.ColumnCount = 5;
                for (int r = 0; r < 9; r++)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    row.CreateCells(dataGridView2);

                    row.Cells[1].Value = transactions[r, 1];
                    row.Cells[2].Value = transactions[r, 2];
                    row.Cells[3].Value = transactions[r, 0];
                    row.Cells[4].Value = transactions[r, 3];
                    dataGridView2.Rows.Add(row);
                }
                dataGridView2.Columns[0].Name = "Category";
                dataGridView2.Columns[1].Name = "Money";
                dataGridView2.Columns[2].Name = "Type";
                dataGridView2.Columns[3].Name = "Date";
                dataGridView2.Columns[4].Name = "Compnay/Number";

                //


            }
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
