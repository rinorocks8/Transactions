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

        //Check if code updates datagridview or user does
        bool updatingUI = false;

        public Form1()
        {
            InitializeComponent();
            string path = root + @"\Checking1.csv";
            string input = File.ReadAllText(path);

            //need to build category save file
            //backwards compatability of rename of category

            if (File.Exists(path))
            {
                string[] rows = File.ReadAllLines(path);
                string[,] transactions = new string[rows.Length, 5];

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
                        transactions[i, 4] = "MOBILE DEPOSIT";
                    }

                    //Online Transfer Format
                    else if (transactions[i, 2].Substring(0, 15) == "ONLINE TRANSFER")
                    {
                        //gets sender
                        transactions[i, 3] = transactions[i, 2].Substring(21, transactions[i, 2].IndexOf('#') - 26);
                        transactions[i, 2] = "Deposit";
                        transactions[i, 4] = "ONLINE TRANSFER";
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

                    //Debug.WriteLine(string.Join(":", string.Join("~", Enumerable.Range(0, transactions.GetLength(1)).Select(column => transactions[i, column]))));
                    /*Format    0 Date      1 Money     2 Type      3 Compnay/Number    5 Deposit Type
                                06/28/2019  -47.93      Purchase    NORDSTROM DIRECT    Mobile Deposit
                    */
                }
                //purchases
                {
                    dataGridView2.ColumnCount = 4;
                    for (int r = 0; r < rows.Length; r++)
                    {
                        //purchase only
                        if (transactions[r, 2] == "Purchase")
                        {
                            DataGridViewRow row = new DataGridViewRow();
                            row.CreateCells(dataGridView2);

                            row.Cells[0].Value = "";                 //Category
                            row.Cells[1].Value = transactions[r, 1]; //Money
                            row.Cells[2].Value = transactions[r, 3]; //Company
                            row.Cells[3].Value = transactions[r, 0]; //Date
                            dataGridView2.Rows.Add(row);
                        }
                    }
                    dataGridView2.ReadOnly = false;
                    dataGridView2.Columns[1].ReadOnly = true;
                    dataGridView2.Columns[2].ReadOnly = true;
                    dataGridView2.Columns[3].ReadOnly = true;
                    dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    dataGridView2.Columns[0].Name = "Category";
                    dataGridView2.Columns[1].Name = "Money";
                    dataGridView2.Columns[2].Name = "Company";
                    dataGridView2.Columns[3].Name = "Date";

                    dataGridView4.RowCount = 1;
                    dataGridView4.ColumnCount = 4;

                    double sum = 0.0;
                    for (int i = 0; i < dataGridView2.RowCount; i++)
                    {
                        sum += double.Parse(dataGridView2.Rows[i].Cells[1].Value.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                    }

                    dataGridView4.Rows[0].Cells[0].Value = "Total";
                    dataGridView4.Rows[0].Cells[1].Value = sum;
                }
                //deposits
                {
                    dataGridView1.ColumnCount = 4;
                    for (int r = 0; r < rows.Length; r++)
                    {
                        //deposit only
                        if (transactions[r, 2] == "Deposit")
                        {
                            DataGridViewRow row = new DataGridViewRow();
                            row.CreateCells(dataGridView1);

                            row.Cells[0].Value = transactions[r, 1]; //Money
                            row.Cells[1].Value = transactions[r, 4]; //Company
                            row.Cells[2].Value = transactions[r, 3]; //Sender/Check #
                            row.Cells[3].Value = transactions[r, 0]; //Date
                            dataGridView1.Rows.Add(row);
                        }
                    }
                    dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    dataGridView1.Columns[0].Name = "Money";
                    dataGridView1.Columns[1].Name = "Type";
                    dataGridView1.Columns[2].Name = "Sender/Check #";
                    dataGridView1.Columns[3].Name = "Date";

                    dataGridView5.RowCount = 1;
                    dataGridView5.ColumnCount = 4;

                    double sum = 0.0;
                    for (int i = 0; i < dataGridView1.RowCount; i++)
                    {
                        sum += double.Parse(dataGridView1.Rows[i].Cells[0].Value.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                    }

                    dataGridView5.Rows[0].Cells[0].Value = "Total";
                    dataGridView5.Rows[0].Cells[1].Value = sum;
                }

            }
            CategoryRefresh(false);
            dataGridView2.CellValueChanged += DataGridView2_CellValueChanged;
        }

        public class Category
        {
            public string category { get; set; }
            public double money { get; set; }
            public double budget { get; set; }
        }

        private void CategoryRefresh(Boolean changed)
        {
            /*
            issues:
            using a used category doesnt update other company transactions
            lags when updating datagridview2
            */

            //transactions
            {
                var tList = new List<Category>();
                //transactions in chart -- edit anything save this for file
                dataGridView3.ColumnCount = 8;

                List<string> categories = new List<string>();
                //all categories
                List<string> categoriesComp = new List<string>();
                //all companies

                //if a category is changed the lists are updated
                if (changed == true)
                {
                    //adds new categories
                    List<string> tempCategories = new List<string>();
                    for (int i = 0; i < dataGridView2.Rows.Count; i++)
                    {
                        if (!categories.Contains(dataGridView2.Rows[i].Cells[0].Value.ToString()) && dataGridView2.Rows[i].Cells[0].Value.ToString() != "")
                        {
                            tList.Add(new Category
                            {
                                category = dataGridView2.Rows[i].Cells[0].Value.ToString(),
                            });
                            categories.Add(dataGridView2.Rows[i].Cells[0].Value.ToString());
                            categoriesComp.Add(dataGridView2.Rows[i].Cells[2].Value.ToString());
                        }
                        if (!tempCategories.Contains(dataGridView2.Rows[i].Cells[0].Value.ToString()) && dataGridView2.Rows[i].Cells[0].Value.ToString() != "")
                        {
                            tempCategories.Add(dataGridView2.Rows[i].Cells[0].Value.ToString());
                        }
                    }

                    //removes unused categories
                    for (int i = 0; i < categories.Count; i++)
                    {
                        if (!tempCategories.Contains(categories[i]))
                        {
                            tList.RemoveAt(i);
                            categories.RemoveAt(i);
                            categoriesComp.RemoveAt(i);
                        }
                    }

                    dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                    dataGridView2.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
                    
                    //looks for used categories and updates rows
                    updatingUI = true;

                    for (int i = 0; i < dataGridView2.Rows.Count; i++)
                    {
                        int a = categoriesComp.IndexOf(dataGridView2.Rows[i].Cells[2].Value.ToString());
                        if (categoriesComp.Contains(dataGridView2.Rows[i].Cells[2].Value.ToString()) && dataGridView2.Rows[i].Cells[0].Value.ToString() != categories[a])
                        {
                            dataGridView2.Rows[i].Cells[0].Value = categories[a];
                        }
                    }    

                    updatingUI = false;
                    //end of updated category
                }

                //parse tlist for categories, for each category search datagrid2 for category and add sum, set total to tlist[i].money
                //sums individual categories
                for (int r = 0; r < tList.Count; r++)
                {
                    for (int i = 0; i < dataGridView2.Rows.Count; i++)
                    {
                        if (tList[r].category == dataGridView2.Rows[i].Cells[0].Value.ToString())
                        {
                            tList[r].money += double.Parse(dataGridView2.Rows[i].Cells[1].Value.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                        }
                    }
                }
                
                //clear categories table and fills it
                dataGridView3.Rows.Clear();
                for (int r = 0; r < tList.Count; r++)
                {
                    DataGridViewRow row = new DataGridViewRow();
                    row.CreateCells(dataGridView3);

                    row.Cells[0].Value = tList[r].category;     //Category
                    row.Cells[1].Value = -tList[r].money;       //Money
                    dataGridView3.Rows.Add(row);
                }

                dataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                dataGridView3.Columns[0].Name = "Category";
                dataGridView3.Columns[1].Name = "Money";
            }

            //totals categories in bottom
            {
                dataGridView6.RowCount = 1;
                dataGridView6.ColumnCount = 4;

                double sum = 0.0;
                for (int i = 0; i < dataGridView3.RowCount; i++)
                {
                    sum += double.Parse(dataGridView3.Rows[i].Cells[1].Value.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                }

                dataGridView6.Rows[0].Cells[0].Value = "Total";
                dataGridView6.Rows[0].Cells[1].Value = sum;
            }
            this.Refresh();
        }

        private void DataGridView2_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Update the category table when category changed
            if (updatingUI == true)
            {
                //Code Edit
                CategoryRefresh(false);
            }
            else
            {
                //User Edit
                CategoryRefresh(true);
            }
            
        }

        //not needed
        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private void groupBox3_Enter(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView4_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView5_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView6_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tableLayoutPanel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void groupBox4_Enter_1(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView4_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void tableLayoutPanel7_Paint(object sender, PaintEventArgs e)
        {

        }

        private void dataGridView2_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}