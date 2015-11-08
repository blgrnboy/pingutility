using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PingTool;

namespace xPing
{
    public partial class MainForm : Form
    {
        private Timer refreshTimer;
        HostValidator validator;

        public MainForm()
        {
            InitializeComponent();
            //refreshTimer = new Timer();
            //refreshTimer.Interval = 2000;
            //refreshTimer.Tick += RefreshTimer_Tick;
            //refreshTimer.Enabled = true;

        }

        private void RefreshTimer_Tick(object sender, EventArgs e)
        {
            dgvMain.Refresh();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            validator = new HostValidator();
            var node = new Node("google.com", validator);
            validator.DataTable.RowChanged += DataTable_RowChanged;
            validator.DataTable.TableNewRow += DataTable_TableNewRow;
            validator.DataTable.TableCleared += DataTable_TableCleared;
            validator.DataTable.RowDeleted += DataTable_RowDeleted;
            new Node("192.168.1.1", validator);
            new Node("yahoo.com", validator);
            new Node("microsoft.com", validator);

            
        }

        private void DataTable_RowDeleted(object sender, DataRowChangeEventArgs e)
        {
            RefreshDataTable();
        }

        private void DataTable_TableCleared(object sender, DataTableClearEventArgs e)
        {
            RefreshDataTable();
        }

        private void DataTable_TableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            RefreshDataTable();
        }

        private void DataTable_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            RefreshDataTable();
        }

        private void RefreshDataTable()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(InnerRefreshDataTable));
            }
            else
            {
                InnerRefreshDataTable();
            }
        }

        private void InnerRefreshDataTable()
        {
            BindingSource bs = new BindingSource();
            bs.DataSource = validator.DataTable;
            dgvMain.DataSource = bs;
        }
    }
}
