using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HandwritingRegistration
{
    /// <summary>
    /// Interaction logic for Records.xaml
    /// </summary>
    public partial class Records : Window
    {

        string connString = "";

        public Records()
        {
            InitializeComponent();
        }

        public void deleteRecords(int[] ids)
        {
            if(ids.Length == 0) return;

            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["HandwritingRegistration.Properties.Settings.AccessConnection"];
            
            //Test Connection
            OleDbConnection cnn = new OleDbConnection(connString);

            try
            {
                cnn.Open();

                string sql = "Delete from Registration where ";
                for(int i=0; i<ids.Length; i++){
                    if(i >= 1){
                        sql = sql + " or ";
                    }
                    sql = sql + "id = " + ids[i].ToString();
                }

                OleDbCommand cmd = new OleDbCommand(sql, cnn);
                cmd.ExecuteNonQuery();

                
            }
            catch (Exception) { }
            
            cnn.Close();
        }

        public void showRecords()
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["HandwritingRegistration.Properties.Settings.AccessConnection"];
            if (settings != null)
            {
                connString = settings.ConnectionString;
            }

            //Test Connection
            OleDbConnection cnn = new OleDbConnection(connString);

            OleDbDataAdapter adapter = new OleDbDataAdapter("Select Id, Name, ContactNo, Gender, Email from Registration", cnn);
            DataTable table = new DataTable("Registration");
            adapter.Fill(table);
            dtgRecords.ItemsSource = table.DefaultView;

            cnn.Close();
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            showRecords();
        }

        private void Window_Activated_1(object sender, EventArgs e)
        {
            showRecords();
        }

        private void btnAddRecord_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mw = new MainWindow();
            mw.Show();

            this.Close();
        }

        private void dtgRecords_StylusUp(object sender, StylusEventArgs e)
        {
            try
            {
                DataRowView drv = (DataRowView)dtgRecords.CurrentItem;
                int id = Int32.Parse(drv.Row.ItemArray[0].ToString());
                showRecord(id);
            }
            catch (Exception)
            {

            }
            
        }

        private void showRecord(int id)
        {
            MainWindow main = new MainWindow();
            //main.loadItem(id);
            main.load(id);
            main.Show();
            this.Close();
        }

        private void dtgRecords_MouseUp(object sender, MouseButtonEventArgs e)
        {
            try
            {
                DataRowView drv = (DataRowView)dtgRecords.CurrentItem;
                int id = Int32.Parse(drv.Row.ItemArray[0].ToString());
                showRecord(id);
            }
            catch (Exception) { }
        }

        private void btnDeleteRecord_Click(object sender, RoutedEventArgs e)
        {
            if (dtgRecords.SelectedItems != null && dtgRecords.SelectedItems.Count > 0)
            {
                try
                {
                    int[] recordsToDelete = new int[dtgRecords.SelectedItems.Count];
                    for (int i = 0; i < dtgRecords.SelectedItems.Count; i++)
                    {
                        DataRowView drv = (DataRowView)dtgRecords.SelectedItems[i];
                        int id = Int32.Parse(drv.Row.ItemArray[0].ToString());
                        recordsToDelete[i] = id;
                    }

                    deleteRecords(recordsToDelete);
                    showRecords();
                }
                catch (Exception) { }
            }
        }
    }
}
