using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Task
{
    public partial class DataGrid : Form
    {
        // member variables
        private static List<string> mDataList = new List<string>();
        private static SortedDictionary<double, int> mPriceRowDic = new SortedDictionary<double, int>();
        private static Size size = new Size(0, 0);

        public DataGrid()
        {
            try
            {
                InitializeComponent();
                size = this.Size;
                this.Size = new Size(deleteBtn.Size.Width * 3, this.Height);
                dataGridView.Hide();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                InformUser(exception.Message);
            }
        }

        private void DataGrid_Load(object sender, EventArgs e)
        {
            try
            {
               // Do Nothing
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                InformUser(exception.Message);
            }
            
        }

        private void selectCsv_Click(object sender, EventArgs e)
        {
            try
            {
                SelectFile();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                InformUser(exception.Message);
            }
        }

        private void delete_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show(@"You are deleting all rows that is not in stock'.\nDo you want to continue?",
                        "Question", MessageBoxButtons.YesNo)
                    == DialogResult.Yes)
                    DeleteNotInRows();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                InformUser(exception.Message);
            }
        }


        private void PrepareDataGridView()
        {
            try
            {
                this.Size = size;
                this.MinimumSize = size;
                dataGridView.Show();

                // Data Grid View properties
                dataGridView.Columns.Clear();
                dataGridView.Rows.Clear();
                dataGridView.ReadOnly = false;
                dataGridView.AllowUserToAddRows = false;
                dataGridView.AllowUserToDeleteRows = false;
                dataGridView.AllowUserToResizeRows = false;
                dataGridView.AutoGenerateColumns = false;
                dataGridView.AllowUserToResizeColumns = true;
                dataGridView.RowHeadersWidthSizeMode = DataGridViewRowHeadersWidthSizeMode.DisableResizing;
                dataGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
                dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // add necessary columns
                dataGridView.Columns.Add(mDataList[0], mDataList[0]);
                dataGridView.Columns.Add(mDataList[1], mDataList[1]);
                dataGridView.Columns.Add(mDataList[2], mDataList[2]);
                dataGridView.Columns.Add(mDataList[3], mDataList[3]);

                DataGridViewCheckBoxColumn inStockCheckBoxColumn =
                    new DataGridViewCheckBoxColumn
                    {
                        Name = mDataList[4],
                        HeaderText = mDataList[4]
                    };

                DataGridViewComboBoxColumn bindingBoxColumn = 
                    new DataGridViewComboBoxColumn
                    {
                        Name = mDataList[5], 
                        HeaderText = mDataList[5], 
                    };

                DataGridViewButtonColumn descriptionButtonColumn = 
                    new DataGridViewButtonColumn
                    {
                        Name = mDataList[6],
                        HeaderText = mDataList[6]
                    };

                dataGridView.Columns.AddRange(new DataGridViewColumn[] {inStockCheckBoxColumn, bindingBoxColumn, descriptionButtonColumn});

                // DGV combo box only opens if it is in edit mode.
                // If DGVCB clicked then in edit mode else not in edit mode
                dataGridView.CellMouseClick += (sender, args) =>
                {
                    var senderGrid = (DataGridView) sender;

                    senderGrid.ShowCellToolTips = false;

                    if (senderGrid.Columns[args.ColumnIndex] is DataGridViewComboBoxColumn)
                    {
                        dataGridView.ReadOnly = false;
                        dataGridView.EditMode = DataGridViewEditMode.EditOnEnter;
                    }
                    else
                    {
                        dataGridView.ReadOnly = true;
                        dataGridView.EditMode = DataGridViewEditMode.EditProgrammatically;
                    }

                    if (senderGrid.Columns[args.ColumnIndex] is DataGridViewButtonColumn)
                    {
                        string desc = senderGrid.Rows[args.RowIndex].Cells[args.ColumnIndex].ToolTipText;
                        
                        MessageBox.Show(desc, "Description", MessageBoxButtons.OK);
                    }

                };
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                InformUser(exception.Message);
            }
        }

        private void SelectFile()
        {
            try
            {
                // ask user to select csv files
                OpenFileDialog fileDialog = new OpenFileDialog();

                fileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                fileDialog.Filter = "CSV files (*.csv)|*.csv";
                fileDialog.RestoreDirectory = true;

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    PutData(fileDialog.FileName);

                    NotInStockHighLight();

                    PriceGradient();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                InformUser(exception.Message);
            }
        }

        private void PutData(string fileName)
        {
            try
            {
                // Read Csv and fill data grid table
                using (var reader = new StreamReader(fileName))
                {
                    bool isFirstLine = true;
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        List<string> val = new List<string>(line.Split(';'));

                        if (isFirstLine)
                        {
                            mDataList = val;
                            PrepareDataGridView();
                            isFirstLine = false;
                        }
                        else
                        {
                            dataGridView.Rows.Add(val[0], val[1], val[2], val[3], string.Equals(val[4], "yes"), null);

                            int rowsCount = dataGridView.Rows.Count - 1;

                            ((DataGridViewComboBoxCell)dataGridView.Rows[rowsCount].Cells[mDataList[5]]).Items.Add(val[5]);
                            ((DataGridViewComboBoxCell)dataGridView.Rows[rowsCount].Cells[mDataList[5]]).Value = val[5];

                            ((DataGridViewButtonCell)dataGridView.Rows[rowsCount].Cells[mDataList[6]]).ToolTipText = val[6];
                            ((DataGridViewButtonCell)dataGridView.Rows[rowsCount].Cells[mDataList[6]]).Value = "...";

                            mPriceRowDic.Add(double.Parse(val[3].Replace(",", ".")), rowsCount);
                        }


                    }
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                InformUser(exception.Message);
            }
        }

        private void DeleteNotInRows()
        {
            try
            {
                // delete rows which are not in stock
                int rowCnt = dataGridView.RowCount;
                for (int i = 0; i < rowCnt; i++)
                {
                    if (!(bool)((DataGridViewCheckBoxCell)dataGridView.Rows[i].Cells[mDataList[4]]).Value)
                    {
                        dataGridView.Rows.Remove(dataGridView.Rows[i]);
                    }
                }

                PriceGradient();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                InformUser(exception.Message);
            }
        }

        private void NotInStockHighLight()
        {
            try
            {
                // highlight rows which are not in stock
                foreach (DataGridViewRow row in dataGridView.Rows)
                    if (!(bool)((DataGridViewCheckBoxCell)row.Cells[mDataList[4]]).Value)
                    {
                        row.DefaultCellStyle.BackColor = Color.Aqua;
                    }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                InformUser(exception.Message);
            }

        }

        private void PriceGradient()
        {
            try
            {
                // make price cell gradient colors and as much as readable
                int incrementAmount = 255 / (mPriceRowDic.Count - 1);
                int counter = 0;
                foreach (var it in mPriceRowDic)
                {
                    int clr = incrementAmount * counter++;
                    clr = counter == mPriceRowDic.Count ? 255 : clr;
                    int foreClr = 255 - clr;
                    dataGridView.Rows[it.Value].Cells[mDataList[3]].Style.BackColor = Color.FromArgb(clr, clr, clr);
                    dataGridView.Rows[it.Value].Cells[mDataList[3]].Style.ForeColor = Color.FromArgb(60, foreClr, foreClr);
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                InformUser(exception.Message);
            }
        }

        private void InformUser(string str)
        {
            MessageBox.Show(str, "Error", MessageBoxButtons.OK);
        }
    }
}
