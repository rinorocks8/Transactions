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
        //Load categories: true = yes
        bool categoryload = true;

        private ContextMenuStrip fruitContextMenuStrip;

        string root = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        //Check if code updates datagridview or user does
        bool updatingUI = false;

        DataTable Purchases = new DataTable();

        List<Category> tList = new List<Category>();
        //transactions in chart -- edit anything save this for file
        string starteditcat = "";
        string starteditcomp = "";

        List<string> categories = new List<string>();
        //all categories
        List<List<string>> categoriesComp = new List<List<string>>();
        //all companies per category

        public Color back;
        public Color front;
        public Color front2;
        public Color text;
        public Color glow;

        //ocean
        Color[] chartcolors = new Color[] { ColorTranslator.FromHtml("#89c17c"), ColorTranslator.FromHtml("#75a76d"), ColorTranslator.FromHtml("#5d9466"),
                                            ColorTranslator.FromHtml("#4a7864"), ColorTranslator.FromHtml("#386c6d"), ColorTranslator.FromHtml("#205260"),
                                            ColorTranslator.FromHtml("#318c8c"), ColorTranslator.FromHtml("#628c8c"), ColorTranslator.FromHtml("#62778c"), 
                                            ColorTranslator.FromHtml("#bdd6bd")};
        //beach
        //Color[] chartcolors = new Color[] { ColorTranslator.FromHtml("#E8D0A9"), ColorTranslator.FromHtml("#B7AFA3"), ColorTranslator.FromHtml("#C1DAD6"), ColorTranslator.FromHtml("#F5FAFA"), ColorTranslator.FromHtml("#ACD1E9"), ColorTranslator.FromHtml("#6D929B")};

        //fire
        //Color[] chartcolors = new Color[] { ColorTranslator.FromHtml("#e17a57"), ColorTranslator.FromHtml("#c74b4b"), ColorTranslator.FromHtml("#a13b3b"), ColorTranslator.FromHtml("#852b2b"), ColorTranslator.FromHtml("#6b0808"), ColorTranslator.FromHtml("#e1bf57")};


        public Form1()
        {
            InitializeComponent();
            string path = root + @"\Checking1.csv";
            string catpath = root + @"\categories.save";

            ColorSet(Convert.Properties.Settings.Default.Color);

            chart1.Series[0].Font = new System.Drawing.Font("Calibri", 10f, FontStyle.Bold);
            chart1.Series[0].LabelBackColor = back;
            chart1.Series[0].LabelBorderColor = back;
            chart1.Series[0].LabelBorderWidth = 2;
            chart1.Series[0]["PieLabelStyle"] = "Outside";

            dataGridView3.ColumnCount = 8;

            if (File.Exists(path))
            {

                if (!File.Exists(catpath))
                {
                    var myFile = File.Create(catpath);
                    myFile.Close();
                }

                //reads excel sheet
                string[] rows = File.ReadAllLines(path);
                //reeds logged categories
                string[] cats = File.ReadAllLines(catpath);

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

                    Purchases.Columns.Add("Category");
                    Purchases.Columns.Add("Money");
                    Purchases.Columns.Add("Company");
                    Purchases.Columns.Add("Date");
                    dataGridView2.DataSource = Purchases;

                    for (int r = 0; r < rows.Length; r++)
                    {
                        //purchase only
                        if (transactions[r, 2] == "Purchase")
                        {
                            DataRow row = Purchases.NewRow();
                            row[0] = "";                 //Category
                            row[1] = transactions[r, 1]; //Money
                            row[2] = transactions[r, 3]; //Company
                            row[3] = transactions[r, 0]; //Date
                            Purchases.Rows.Add(row);
                        }
                    }

                    dataGridView2.ReadOnly = false;
                    dataGridView2.Columns[1].ReadOnly = true;
                    dataGridView2.Columns[2].ReadOnly = true;
                    dataGridView2.Columns[3].ReadOnly = true;
                    dataGridView2.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                    dataGridView2.Refresh();

                    dataGridView4.RowCount = 1;
                    dataGridView4.ColumnCount = 4;

                    double sum = 0.0;
                    for (int i = 0; i < Purchases.Rows.Count; i++)
                    {
                        sum += double.Parse(Purchases.Rows[i][1].ToString(), System.Globalization.CultureInfo.InvariantCulture);
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

                //loads categories from file
                if (categoryload == true)
                {
                    for (int d = 0; d < cats.Length; d++)
                    {
                        string[] cat = cats[d].Split('~');
                        for (int e = 0; e < cat.Length; e++)
                        {
                            for (int f = 0; f < Purchases.Rows.Count; f++)
                            {
                                for (int i = 2; i < cat.Length; i++)
                                {
                                    Purchases.Rows[f].BeginEdit();
                                    if (Purchases.Rows[f][2].ToString() == cat[i])
                                    {
                                        Purchases.Rows[f][0] = cat[0];
                                        CategoryRefreshTrue(f, true);
                                    }
                                    Purchases.Rows[f].EndEdit();
                                }
                            }
                        }
                    }
                }
                CategoryRefresh(false, 0);
            }

            dataGridView2.CellValueChanged += DataGridView2_CellValueChanged;
            dataGridView3.CellValueChanged += DataGridView3_CellValueChanged;
            dataGridView2.CellBeginEdit += DataGridView2_CellBeginEdit;

            Colors();

            //context menu strip
            {
                // Create a new ContextMenuStrip control.
                fruitContextMenuStrip = new ContextMenuStrip();

                // Attach an event handler for the 
                // ContextMenuStrip control's Opening event.
                fruitContextMenuStrip.Opening += new System.ComponentModel.CancelEventHandler(cms_Opening);

                // Create a new MenuStrip control and add a ToolStripMenuItem.
                ToolStripMenuItem fruitToolStripMenuItem = new ToolStripMenuItem("Themes", null, null, "Themes");
                ms.Items.Add(fruitToolStripMenuItem);

                // Dock the MenuStrip control to the top of the form.
                ms.Dock = DockStyle.Top;

                // Assign the MenuStrip control as the 
                // ToolStripMenuItem's DropDown menu.
                fruitToolStripMenuItem.DropDown = fruitContextMenuStrip;

                fruitContextMenuStrip.ShowImageMargin = false;

                ms.Renderer = new MyRenderer();
                fruitContextMenuStrip.Renderer = new MyRenderer();

                // Add the MenuStrip control last.
                // This is important for correct placement in the z-order.
                this.Controls.Add(ms);

                fruitContextMenuStrip.ItemClicked += new ToolStripItemClickedEventHandler(menu_Click);
            }
        }

        void menu_Click(object sender, ToolStripItemClickedEventArgs e)
        {
            ColorSet(e.ClickedItem.Text.ToString());
            Properties.Settings.Default.Color = e.ClickedItem.Text.ToString();
            Properties.Settings.Default.Save();
        }

        public void ColorSet(string sent)
        {
            if (sent == "White")
            {
                back = Color.White;
                front = Color.WhiteSmoke;
                front2 = Color.LightGray;
                text = Color.Black;
                glow = Color.DarkGray;

            }
            else if (sent == "Grey")
            {
                back = System.Drawing.ColorTranslator.FromHtml("#32323e");
                front = System.Drawing.ColorTranslator.FromHtml("#393945");
                front2 = System.Drawing.ColorTranslator.FromHtml("#282833");
                text = Color.White;
                glow = Color.Black;
            }
            else if (sent == "Red")
            {
                back = Color.Red;
                front = Color.Red;
                front2 = Color.Red;
                text = Color.Red;
                glow = Color.Red;
            }
            Colors();
        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle = cp.ExStyle | 0x2000000;
                return cp;
            }
        }

        void Colors()
        {
            tableLayoutPanel1.BackColor = back;
            button1.ForeColor = text;
            button2.ForeColor = text;
            label1.ForeColor = text;
            label2.ForeColor = text;
            label3.ForeColor = text;
            label4.ForeColor = text;
            label5.ForeColor = text;
            label6.ForeColor = text;
            label7.ForeColor = text;
            label8.ForeColor = text;
            label9.ForeColor = text;

            label1.BackColor = back;
            label2.BackColor = front;
            label3.BackColor = front;
            label4.BackColor = front;
            label5.BackColor = front;
            label6.BackColor = front;
            label7.BackColor = front;
            label8.BackColor = front;
            label9.BackColor = front;
            label10.BackColor = front;

            dataGridView1.GridColor = front;
            dataGridView2.GridColor = front;
            dataGridView3.GridColor = front;
            dataGridView4.GridColor = front;
            dataGridView5.GridColor = front;
            dataGridView6.GridColor = front;

            dataGridView1.BackgroundColor = front2;
            dataGridView2.BackgroundColor = front2;
            dataGridView3.BackgroundColor = front2;
            dataGridView4.BackgroundColor = front2;
            dataGridView5.BackgroundColor = front2;
            dataGridView6.BackgroundColor = front2;

            dataGridView1.DefaultCellStyle.BackColor = back;
            dataGridView2.DefaultCellStyle.BackColor = back;
            dataGridView3.DefaultCellStyle.BackColor = back;
            dataGridView4.DefaultCellStyle.BackColor = back;
            dataGridView5.DefaultCellStyle.BackColor = back;
            dataGridView6.DefaultCellStyle.BackColor = back;

            dataGridView1.DefaultCellStyle.ForeColor = text;
            dataGridView2.DefaultCellStyle.ForeColor = text;
            dataGridView3.DefaultCellStyle.ForeColor = text;
            dataGridView4.DefaultCellStyle.ForeColor = text;
            dataGridView5.DefaultCellStyle.ForeColor = text;
            dataGridView6.DefaultCellStyle.ForeColor = text;

            dataGridView1.DefaultCellStyle.SelectionBackColor = front;
            dataGridView2.DefaultCellStyle.SelectionBackColor = front;
            dataGridView3.DefaultCellStyle.SelectionBackColor = front;
            dataGridView4.DefaultCellStyle.SelectionBackColor = front;
            dataGridView5.DefaultCellStyle.SelectionBackColor = front;
            dataGridView6.DefaultCellStyle.SelectionBackColor = front;

            dataGridView1.DefaultCellStyle.SelectionForeColor = text;
            dataGridView2.DefaultCellStyle.SelectionForeColor = text;
            dataGridView3.DefaultCellStyle.SelectionForeColor = text;
            dataGridView4.DefaultCellStyle.SelectionForeColor = text;
            dataGridView5.DefaultCellStyle.SelectionForeColor = text;
            dataGridView6.DefaultCellStyle.SelectionForeColor = text;

            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = text;
            dataGridView2.ColumnHeadersDefaultCellStyle.ForeColor = text;
            dataGridView3.ColumnHeadersDefaultCellStyle.ForeColor = text;
            dataGridView4.ColumnHeadersDefaultCellStyle.ForeColor = text;
            dataGridView5.ColumnHeadersDefaultCellStyle.ForeColor = text;
            dataGridView6.ColumnHeadersDefaultCellStyle.ForeColor = text;

            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = back;
            dataGridView2.ColumnHeadersDefaultCellStyle.BackColor = back;
            dataGridView3.ColumnHeadersDefaultCellStyle.BackColor = back;
            dataGridView4.ColumnHeadersDefaultCellStyle.BackColor = back;
            dataGridView5.ColumnHeadersDefaultCellStyle.BackColor = back;
            dataGridView6.ColumnHeadersDefaultCellStyle.BackColor = back;

            dataGridView2.ColumnHeadersDefaultCellStyle.BackColor = back;
            dataGridView3.ColumnHeadersDefaultCellStyle.BackColor = back;
            dataGridView4.ColumnHeadersDefaultCellStyle.BackColor = back;
            dataGridView5.ColumnHeadersDefaultCellStyle.BackColor = back;
            dataGridView6.ColumnHeadersDefaultCellStyle.BackColor = back;

            gGlowBox1.GlowColor = glow;
            gGlowBox2.GlowColor = glow;
            gGlowBox3.GlowColor = glow;
            gGlowBox4.GlowColor = glow;
            gGlowBox5.GlowColor = glow;

            tableLayoutPanel6.BackColor = front;
            tableLayoutPanel7.BackColor = front2;

            ms.BackColor = back;
            ms.ForeColor = text;

            chart1.PaletteCustomColors = chartcolors;
        }

        private class MyRenderer : ToolStripProfessionalRenderer
        {
            public MyRenderer() : base(new MyColors()) { }
        }

        private class MyColors : ProfessionalColorTable
        {
            public override Color MenuItemSelected
            {
                get { return Color.LightGray; }
            }
            public override Color MenuItemSelectedGradientBegin
            {
                get { return Color.DarkGray; }
            }
            public override Color MenuItemSelectedGradientEnd
            {
                get { return Color.DarkGray; }
            }
            public override Color MenuItemBorder
            {
                get { return Color.Transparent; }
            }
            public override Color MenuItemPressedGradientBegin
            {
                get { return Color.DarkGray; }
            }
            public override Color MenuItemPressedGradientEnd
            {
                get { return Color.DarkGray; }
            }
            public override Color MenuBorder
            {
                get { return Color.Transparent; }
            }
        }

        void cms_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Acquire references to the owning control and item.
            Control c = fruitContextMenuStrip.SourceControl as Control;
            ToolStripDropDownItem tsi = fruitContextMenuStrip.OwnerItem as ToolStripDropDownItem;

            // Clear the ContextMenuStrip control's Items collection.
            fruitContextMenuStrip.Items.Clear();

            //Populate the ContextMenuStrip control with its default items.
            fruitContextMenuStrip.Items.Add("White");
            fruitContextMenuStrip.Items.Add("Grey");

            fruitContextMenuStrip.Items.Add("Red");

            fruitContextMenuStrip.BackColor = front;
            fruitContextMenuStrip.ForeColor = text;

            // Set Cancel to false. 
            // It is optimized to true based on empty entry.
            e.Cancel = false;
        }

        public class Category
        {
            public string category { get; set; }
            public double money { get; set; }
            public double budget { get; set; }
        }

        public Tuple<int, int> arrSearch(List<List<string>> mainList, string company)
        {
            int x = -1;
            int y = -1;

            for (int i = 0; i < mainList.Count; i++)
            {
                if (mainList[i].Contains(company))
                {
                    x = i;
                    y = mainList[i].IndexOf(company);
                }
            }
            if (x == -1 || y == -1)
            {
                return null;
            }
            return Tuple.Create(x, y);
        }

        private void CategoryRefreshTrue(int ar, Boolean first)
        {
            string[] cated1 = File.ReadAllLines(root + @"\categories.save");
            List<string> cats1 = new List<string> { };
            for (int g = 0; g < cated1.Length; g++)
            {
                cats1.Add(cated1[g]);
            }
            //removes old catcomp from cat         
            if (categories.Contains(starteditcat))
            {
                int r = categories.IndexOf(starteditcat);
                int r1 = categoriesComp[r].IndexOf(starteditcomp);
                categoriesComp[r].RemoveAt(r1);

                if (categoriesComp[r].Count == 0)
                {
                    categories.RemoveAt(r);
                    tList.RemoveAt(r);
                    categoriesComp.RemoveAt(r);
                    if (first == false && categoryload == true)
                    {
                        for (int d = 0; d < cats1.Count; d++)
                        {
                            string[] cat = cats1[d].Split('~');
                            if (cat[0] == starteditcat)
                            {
                                cats1.RemoveAt(d);
                            }
                        }
                        File.WriteAllLines(root + @"\categories.save", cats1);
                    }
                }
                else
                {
                    if (first == false && categoryload == true)
                    {
                        for (int d = 0; d < cats1.Count; d++)
                        {
                            List<string> cat = new List<string> { };
                            string[] catd = cats1[d].Split('~');
                            for (int h = 0; h < catd.Length; h++)
                            {
                                cat.Add(catd[h]);
                            }
                            if (cat[0] == starteditcat)
                            {
                                if (cat.Contains(Purchases.Rows[ar][2].ToString()))
                                {
                                    int g = cat.IndexOf(Purchases.Rows[ar][2].ToString());
                                    cat.RemoveAt(g);
                                }
                                cats1.RemoveAt(d);
                                cats1.Add(string.Join("~", cat.ToArray()));
                            }
                        }
                        File.WriteAllLines(root + @"\categories.save", cats1);
                    }
                }
            }

            string[] cated = File.ReadAllLines(root + @"\categories.save");
            List<string> cats = new List<string> { };
            for (int g = 0; g < cated.Length; g++)
            {
                cats.Add(cated[g]);
            }

            if (!categories.Contains(Purchases.Rows[ar][0].ToString()) && Purchases.Rows[ar][0].ToString() != "")
            {
                categories.Add(Purchases.Rows[ar][0].ToString());
                tList.Add(new Category
                {
                    category = Purchases.Rows[ar][0].ToString(),
                });
                categoriesComp.Add(new List<string> { Purchases.Rows[ar][2].ToString() });
                if (first == false && categoryload == true)
                {
                    cats.Add(Purchases.Rows[ar][0].ToString() + "~0~" + Purchases.Rows[ar][2].ToString());
                    File.WriteAllLines(root + @"\categories.save", cats);
                }
            }
            else if (categories.Contains(Purchases.Rows[ar][0].ToString()))
            {
                int r = categories.IndexOf(Purchases.Rows[ar][0].ToString());
                //adds new company to catcomp
                if (!categoriesComp[r].Contains(Purchases.Rows[ar][2].ToString()))
                {
                    categoriesComp[r].Add(Purchases.Rows[ar][2].ToString());
                }
                if (starteditcat != "")
                {
                    foreach (DataRow row in Purchases.Rows)
                    {
                        row.BeginEdit();
                        if (row[0].ToString() == starteditcat)
                        {
                            row[0] = Purchases.Rows[ar][0].ToString();
                        }
                        row.EndEdit();
                    }
                }
                if (first == false && categoryload == true)
                {
                    for (int d = 0; d < cats.Count; d++)
                    {
                        string[] catd = cats[d].Split('~');
                        List<string> cat = new List<string>(catd);

                        if (catd[0] == Purchases.Rows[ar][0].ToString())
                        {
                            cat.Add(Purchases.Rows[ar][2].ToString());
                            cats.RemoveAt(d);
                            cats.Add(string.Join("~", cat.ToArray()));
                        }   
                    }
                    File.WriteAllLines(root + @"\categories.save", cats);
                }
            }
            else if (Purchases.Rows[ar][0].ToString() == "")
            {
                foreach (DataRow row in Purchases.Rows)
                {
                    row.BeginEdit();
                    if (row[0].ToString() == starteditcat)
                    {
                        row[0] = Purchases.Rows[ar][0].ToString();
                    }
                    row.EndEdit();
                }
            }

            //looks for used categories and updates rows
            updatingUI = true;
            foreach (DataRow row in Purchases.Rows)
            {
                row.BeginEdit();
                Tuple<int, int> location = arrSearch(categoriesComp, row[2].ToString());
                int x;
                int y;
                if (location != null)
                {
                    x = location.Item1;
                    y = location.Item2;

                    if (row[0].ToString() != categories[x])
                    {
                        row[0] = categories[x];
                    }
                }
                row.EndEdit();
            }
            dataGridView2.Refresh();
            updatingUI = false;
            //end of updated category
        }
        private void CategoryRefresh(Boolean changed, int ar)
        {
            //transactions
            {

                //if a category is changed the lists are updated
                if (changed == true)
                {
                    CategoryRefreshTrue(ar, false);
                }

                //parse tlist for categories, for each category search datagrid2 for category and add sum, set total to tlist[i].money
                //sums individual categories
                for (int r = 0; r < tList.Count; r++)
                {
                    //MessageBox.Show(tList[r].category());
                    tList[r].money = 0;
                    for (int i = 0; i < Purchases.Rows.Count; i++)
                    {
                        if (tList[r].category == Purchases.Rows[i][0].ToString())
                        {
                            tList[r].money += double.Parse(Purchases.Rows[i][1].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                        }
                    }
                }

                if (categoryload == true)
                {
                    string[] cats = File.ReadAllLines(root + @"\categories.save");
                    for (int d = 0; d < cats.Length; d++)
                    {
                        string[] cat = cats[d].Split('~');
                        if (categories.Contains(cat[0]))
                        {
                            int a = categories.IndexOf(cat[0]);
                            tList[a].budget = double.Parse(cat[1], System.Globalization.CultureInfo.InvariantCulture);
                        }
                    }
                }

                //clear categories table and fills it
                dataGridView3.Rows.Clear();
                chart1.Series[0].Points.Clear();
                for (int r = 0; r < tList.Count; r++)
                {
                    if (tList[r].money != 0)
                    {
                        DataGridViewRow row = new DataGridViewRow();
                        row.CreateCells(dataGridView3);

                        row.Cells[0].Value = tList[r].category;     //Category
                        row.Cells[1].Value = -tList[r].money;       //Money
                        row.Cells[6].Value = tList[r].budget;       //Budget
                        double negpos = Math.Round((tList[r].money + tList[r].budget), 2);
                        if (negpos >= 0)
                        {
                            row.Cells[7].Value = "+"+ negpos;
                        }
                        if (negpos < 0)
                        {
                            row.Cells[7].Value = negpos;
                        }
                        dataGridView3.Rows.Add(row);
                        chart1.Series[0].Points.AddXY(tList[r].category, tList[r].money);
                    }
                }

                dataGridView3.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dataGridView3.Columns[1].ReadOnly = true;
                dataGridView3.Columns[2].ReadOnly = true;
                dataGridView3.Columns[3].ReadOnly = true;
                dataGridView3.Columns[4].ReadOnly = true;
                dataGridView3.Columns[5].ReadOnly = true;
                dataGridView3.Columns[6].ReadOnly = false;
                dataGridView3.Columns[7].ReadOnly = true;

                dataGridView3.Columns[0].Name = "Category";
                dataGridView3.Columns[1].Name = "Money";
                dataGridView3.Columns[6].Name = "Budget";
                dataGridView3.Columns[7].Name = "Over/Under";
            }

            //totals categories in bottom
            {
                dataGridView6.RowCount = 1;
                dataGridView6.ColumnCount = 8;

                double sum = 0.0;
                for (int i = 0; i < dataGridView3.RowCount; i++)
                {
                    sum += double.Parse(dataGridView3.Rows[i].Cells[1].Value.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                }

                dataGridView6.Rows[0].Cells[0].Value = "Total";
                dataGridView6.Rows[0].Cells[1].Value = sum;
                double budnegpos = 0.0;
                for (int i = 0; i < dataGridView3.RowCount; i++)
                {
                    budnegpos += double.Parse(dataGridView3.Rows[i].Cells[6].Value.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                }
                dataGridView6.Rows[0].Cells[6].Value = budnegpos;
                double negpos = 0.0;
                for (int i = 0; i < dataGridView3.RowCount; i++)
                {
                    negpos += double.Parse(dataGridView3.Rows[i].Cells[7].Value.ToString(), System.Globalization.CultureInfo.InvariantCulture);
                }
                dataGridView6.Rows[0].Cells[7].Value = negpos;
                if (negpos >= 0)
                {
                    dataGridView6.Rows[0].Cells[7].Value = "+" + negpos;
                    dataGridView6.Rows[0].Cells[7].Style.ForeColor = Color.Green;
                }
                if (negpos < 0)
                {
                    dataGridView6.Rows[0].Cells[7].Value = negpos;
                    dataGridView6.Rows[0].Cells[7].Style.ForeColor = Color.Red;
                }
            }

            if (chart1.Series[0].Points.Count == 0)
            {
                chart1.Series[0].Points.AddXY("Empty", 1);
                chart1.Series[0].Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.Grayscale;
                chart1.Series[0].LabelBackColor = Color.Transparent;
                chart1.Series[0].LabelBorderColor = Color.Transparent;
                chart1.Series[0].LabelForeColor = Color.Transparent;
            }
            else
            {
                chart1.Series[0].Palette = System.Windows.Forms.DataVisualization.Charting.ChartColorPalette.None;
                chart1.Series[0].LabelForeColor = text;
                chart1.Series[0].Color = Color.Transparent;
                chart1.Series[0].LabelBackColor = back;
                chart1.Series[0].LabelBorderColor = back;
            }

            for (int r = 0; r < dataGridView3.Rows.Count; r++)
            {
                if (double.Parse(dataGridView3.Rows[r].Cells[7].Value.ToString()) >= 0)
                {
                    dataGridView3.Rows[r].Cells[7].Style.ForeColor = Color.Green;
                }
                if (double.Parse(dataGridView3.Rows[r].Cells[7].Value.ToString()) < 0)
                {
                    dataGridView3.Rows[r].Cells[7].Style.ForeColor = Color.Red;
                }
            }
            this.Refresh();
        }

        private void DataGridView2_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Update the category table when category changed
            if (updatingUI == true)
            {
                //Code Edit
                CategoryRefresh(false, 0);
            }
            else
            {
                //User Edit
                CategoryRefresh(true, e.RowIndex);
            }
            this.Refresh();
        }

        private void DataGridView3_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            // Update the budget file when budget changed
            string[] cated = File.ReadAllLines(root + @"\categories.save");
            List<string> cats = new List<string> { };
            for (int g = 0; g < cated.Length; g++)
            {
                cats.Add(cated[g]);
            }

            if (categoryload == true)
            {
                for (int d = 0; d < cats.Count; d++)
                {
                    string[] catd = cats[d].Split('~');
                    List<string> cat = new List<string>(catd);

                    if (catd[0] == dataGridView3.Rows[e.RowIndex].Cells[0].Value.ToString())
                    {
                        cat.RemoveAt(1);
                        cat.Insert(1, dataGridView3.Rows[e.RowIndex].Cells[6].Value.ToString());
                        cats.RemoveAt(d);
                        cats.Add(string.Join("~", cat.ToArray()));
                    }
                }
                File.WriteAllLines(root + @"\categories.save", cats);
            }
            CategoryRefresh(false, 0);
            this.Refresh();
        }

        private void DataGridView2_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            starteditcat = dataGridView2[0, e.RowIndex].Value.ToString();
            starteditcomp = dataGridView2[2, e.RowIndex].Value.ToString();
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

        private void tableLayoutPanel6_Paint(object sender, PaintEventArgs e)
        {

        }

        private void chart1_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void chart2_Click(object sender, EventArgs e)
        {

        }

        private void chart3_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click_1(object sender, EventArgs e)
        {

        }
    }
}