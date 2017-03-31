using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Florentis;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace HandwritingRegistration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string appDir;
        CounterView cv;
        SigObj sigObj;
        int currentId;
        string currentName;
        string currentGender;
        string currentEmail;
        string currentContactNo;

        double screenWidth;
        double screenHeight;

        const double originalWidth = 1280;
        const double originalHeight = 800;

        public MainWindow()
        {
            InitializeComponent();
            appDir = AppDomain.CurrentDomain.BaseDirectory;
        }

        public void getScreenResolution()
        {
            //screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            //screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
        }

        public void adjustControls()
        {
            //double B5_ASPECT_RATIO = B5_WIDTH_MM / B5_HEIGHT_MM;
            double widthRatio = screenWidth / originalWidth;
            double heightRatio = screenHeight / originalHeight;

            int ctrlCount = VisualTreeHelper.GetChildrenCount(MainGrid);
            for (int i = 0; i < ctrlCount; i++)
            {
                DependencyObject dep = VisualTreeHelper.GetChild(MainGrid, i);
                if (dep is FrameworkElement)
                {
                    FrameworkElement e = (FrameworkElement)dep;

                    e.Width = e.Width * widthRatio;
                    e.Height = e.Height * heightRatio;

                    if (dep is TextBox)
                    {
                        TextBox txtBox = (TextBox)dep;
                        txtBox.FontSize = txtBox.FontSize * heightRatio;
                    }
                    else if (dep is Label)
                    {
                        Label lbl = (Label)dep;
                        lbl.FontSize = lbl.FontSize * heightRatio;
                    }

                    //if (!(dep is InkCanvas) && !(dep is System.Windows.Controls.Image))
                    if (dep is System.Windows.Controls.Image)
                    {
                        //double left = e.Margin.Left * widthRatio;
                        //double top = e.Margin.Top * heightRatio;
                        //e.Margin = new Thickness(left, top, e.Margin.Right, e.Margin.Bottom);
                    }

                    if (dep is Border)
                    {
                        DependencyObject dep2 = VisualTreeHelper.GetChild(dep, 0);
                        if (dep2 is InkCanvas)
                        {
                            FrameworkElement e2 = (FrameworkElement)dep2;
                            //Debug.Print(e2.Name + " " + e2.GetType().ToString());

                            e2.Width = e2.Width * widthRatio;
                            e2.Height = e2.Height * heightRatio;
                        }
                    }

                }
            }
        }

        public void load(int id)
        {
            currentId = id;

            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["HandwritingRegistration.Properties.Settings.AccessConnection"];
            OleDbConnection cnn = new OleDbConnection(settings.ConnectionString);

            try
            {
                cnn.Open();
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot connect to database");
            }

            if (cnn.State == ConnectionState.Open)
            {
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = cnn;
                cmd.CommandText = "Select `Id`,`NameInk`,`GenderMInk`,`GenderFInk`,`EmailInk`,`ContactNoInk`, `Name`, `Gender`, `Email`, `ContactNo`, `Signature` from Registration where `Id` = @Id";
                cmd.Parameters.AddWithValue("@Id", id);
                OleDbDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    byte[] data = reader.GetValue(1) as byte[];
                    loadInk(data, inkName);

                    data = reader.GetValue(2) as byte[];
                    loadInk(data, inkM);
                   
                    data = reader.GetValue(3) as byte[];
                    loadInk(data, inkF);
                    
                    data = reader.GetValue(4) as byte[];
                    loadInk(data, inkEmail);

                    data = reader.GetValue(5) as byte[];
                    loadInk(data, inkContactNo);

                    currentName = reader.GetString(6);
                    currentGender = reader.GetString(7);
                    currentEmail = reader.GetString(8);
                    currentContactNo = reader.GetString(9);
                    sigObj = new SigObj();

                    string sigString = reader.GetString(10);
                    if (sigString != null && sigString.Length > 0)
                    {
                        //ReadEncodedBitmapResult result = currentSignature.ReadEncodedBitmap(sigString);
                        sigObj.SigText = sigString;
                        string filename = "sig" + Guid.NewGuid() + ".png";

                        try
                        {
                            sigObj.RenderBitmap(filename, 200, 150, "image/png", 0.5f, 0xff0000, 0xffffff, 10.0f, 10.0f, RBFlags.RenderOutputFilename | RBFlags.RenderColor32BPP | RBFlags.RenderEncodeData);

                            imgSignature.Source = new BitmapImage(new Uri(appDir + @"\" + filename));
                            imgSignature.Stretch = Stretch.Uniform;
                        }
                        catch (Exception e)
                        {
                            MessageBox.Show(e.Message);
                        }

                    }
                    
                }
                reader.Close();
                cnn.Close();
            }
        }

        private void loadInk(byte[] data, InkCanvas canvas)
        {
            if (data != null)
            {
                StrokeCollection strokes = UnserializeStrokes(data);
                canvas.Strokes.Add(strokes);
            }
        }

        public byte[] SerializeStrokes(StrokeCollection strokes)
        {
            
            CustomStrokes customStrokes = new CustomStrokes();
            customStrokes.StrokeCollection = new System.Windows.Point[strokes.Count][];

            for (int i = 0; i < strokes.Count; i++)
            {
                customStrokes.StrokeCollection[i] = new System.Windows.Point[strokes[i].StylusPoints.Count];

                for (int j = 0; j < strokes[i].StylusPoints.Count; j++)
                {
                    customStrokes.StrokeCollection[i][j] = new System.Windows.Point();
                    customStrokes.StrokeCollection[i][j].X = strokes[i].StylusPoints[j].X;
                    customStrokes.StrokeCollection[i][j].Y = strokes[i].StylusPoints[j].Y;
                }
            }

            //Serialize our "strokes"
            MemoryStream ms = new MemoryStream();

            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, customStrokes);

            return ms.GetBuffer();
            
        }

        StrokeCollection UnserializeStrokes(byte[] data)
        {

            if (data == null) { return null; }

            try
            {
                
                //deserialize it
                BinaryFormatter bf = new BinaryFormatter();
                MemoryStream ms = new MemoryStream(data);

                CustomStrokes customStrokes = bf.Deserialize(ms) as CustomStrokes;
                StrokeCollection strokes = new StrokeCollection();

                //rebuilt it
                for (int i = 0; i < customStrokes.StrokeCollection.Length; i++)
                {
                    if (customStrokes.StrokeCollection[i] != null)
                    {
                        StylusPointCollection stylusCollection = new StylusPointCollection(customStrokes.StrokeCollection[i]);
                        Stroke stroke = new Stroke(stylusCollection);
                        strokes.Add(stroke);
                    }
                }
                return strokes;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return null;
        }

        private void imgSignature_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SigCtl sigCtl = new SigCtl();
            DynamicCapture dc = new DynamicCaptureClass();
            DynamicCaptureResult res = dc.Capture(sigCtl, "Who", "Why", null, null);
            if (res == DynamicCaptureResult.DynCaptOK)
            {

                sigObj = (SigObj)sigCtl.Signature;
                String filename = "sig" + Guid.NewGuid().ToString() + ".png";
                try
                {
                    sigObj.RenderBitmap(filename, 200, 150, "image/png", 0.5f, 0xff0000, 0xffffff, 10.0f, 10.0f, RBFlags.RenderOutputFilename | RBFlags.RenderColor32BPP | RBFlags.RenderEncodeData);
                    
                    imgSignature.Source = new BitmapImage(new Uri(appDir + @"\" + filename));
                    imgSignature.Stretch = Stretch.Uniform;

                    if (cv != null)
                    {
                        cv.imgSignature.Source = new BitmapImage(new Uri(appDir + @"\" + filename));
                        cv.imgSignature.Stretch = Stretch.Uniform;
                    }
                    
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            else
            {
                switch (res)
                {
                    case DynamicCaptureResult.DynCaptCancel: break;
                    case DynamicCaptureResult.DynCaptError: MessageBox.Show("no capture service available"); break;
                    case DynamicCaptureResult.DynCaptPadError: MessageBox.Show("signing device error"); break;
                    default: MessageBox.Show("Unexpected error code "); break;
                }
            }
        }

        private void switchToPenMode()
        {
            inkName.EditingMode = InkCanvasEditingMode.Ink;
            inkM.EditingMode = InkCanvasEditingMode.Ink;
            inkF.EditingMode = InkCanvasEditingMode.Ink;
            inkContactNo.EditingMode = InkCanvasEditingMode.Ink;
            inkEmail.EditingMode = InkCanvasEditingMode.Ink;

            penMode.Source = new BitmapImage(new Uri("pencil.png", UriKind.Relative));
            eraserMode.Source = new BitmapImage(new Uri("eraser_grey.png", UriKind.Relative));
        }
        


        private void penMode_StylusUp(object sender, StylusEventArgs e)
        {
            switchToPenMode();
        }

        private void eraserMode_StylusUp(object sender, StylusEventArgs e)
        {
            switchToEraserMode();
        }

        private void switchToEraserMode()
        {
            inkName.EditingMode = InkCanvasEditingMode.EraseByPoint;
            inkM.EditingMode = InkCanvasEditingMode.EraseByPoint;
            inkF.EditingMode = InkCanvasEditingMode.EraseByPoint;
            inkContactNo.EditingMode = InkCanvasEditingMode.EraseByPoint;
            inkEmail.EditingMode = InkCanvasEditingMode.EraseByPoint;

            penMode.Source = new BitmapImage(new Uri("pencil_grey.png", UriKind.Relative));
            eraserMode.Source = new BitmapImage(new Uri("eraser.png", UriKind.Relative));
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            clear();   
        }

        private void clear()
        {
            inkName.Strokes.Clear();
            inkM.Strokes.Clear();
            inkF.Strokes.Clear();
            inkEmail.Strokes.Clear();
            inkContactNo.Strokes.Clear();
            imgSignature.Source = new BitmapImage(new Uri("white_space.png", UriKind.Relative));
            imgSignature.Stretch = Stretch.Fill;

            if (cv != null)
            {
                cv.txtName.Text = "";
                cv.txtEmail.Text = "";
                cv.txtContactNo.Text = "";
                cv.imgSignature.Source = new BitmapImage(new Uri("white_space.png", UriKind.Relative));
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Records recordList = new Records();
            recordList.Show();
            this.Close();
            if (cv != null)
            {
                cv.Close();
            }
        }

        private string getOcrText(InkCanvas inkCanvas)
        {
            InkAnalyzer inkAnalyzer = new InkAnalyzer();

            if (inkCanvas.Strokes.Count == 0) return "";

            inkAnalyzer.AddStrokes(inkCanvas.Strokes);
            AnalysisStatus status = inkAnalyzer.Analyze();
            if (status.Successful)
            {
                return inkAnalyzer.GetRecognizedString();
            }
            else
            {
                return "";
            }
        }

        private void getOcrText(InkCanvas inkCanvas, TextBox textBox)
        {
            if (inkCanvas.Strokes == null || inkCanvas.Strokes.Count == 0) return;

            InkAnalyzer inkAnalyzer = new InkAnalyzer();

            inkAnalyzer.AddStrokes(inkCanvas.Strokes);
            AnalysisStatus status = inkAnalyzer.Analyze();
            if (status.Successful)
            {
                textBox.Text = inkAnalyzer.GetRecognizedString();
            }
        }

        private void btnShowCounterView_Click(object sender, RoutedEventArgs e)
        {
            if (cv == null)
            {
                cv = new CounterView();
            }
            cv.Show();
            cv.Activate();

            if(currentName != null && currentName.Length > 0)
            {
                cv.txtName.Text = currentName;
            }
            else if (cv.txtName.Text == "" && inkName.Strokes.Count > 0)
            {
                getOcrText(inkName, cv.txtName);
            }

            if (currentEmail != null && currentEmail.Length > 0)
            {
                cv.txtEmail.Text = currentEmail;
            }
            else if (cv.txtEmail.Text == "" && inkEmail.Strokes.Count > 0)
            {
                getOcrText(inkEmail, cv.txtEmail);
            }

            if (currentContactNo != null && currentContactNo.Length > 0)
            {
                cv.txtContactNo.Text = currentContactNo;
            }
            else if (cv.txtContactNo.Text == "" && inkContactNo.Strokes.Count > 0)
            {
                getOcrText(inkContactNo, cv.txtContactNo);
            }

            if (currentGender != null)
            {
                if (currentGender == "M")
                {
                    cv.radGenderM.IsChecked = true;
                }
                else if (currentGender == "F")
                {
                    cv.radGenderF.IsChecked = true;
                }
            }else if (inkM.Strokes.Count > 0)
            {
                cv.radGenderF.IsChecked = true;
            }else if (inkF.Strokes.Count > 0)
            {
                cv.radGenderM.IsChecked = true;
            }
            
            cv.imgSignature.Source = imgSignature.Source;
            
        }

        private void inkNameChanged()
        {
            if (cv != null)
            {
                getOcrText(inkName, cv.txtName);
            }
        }

        private void inkName_StylusUp(object sender, StylusEventArgs e)
        {
            inkNameChanged();
        }

        private void inkMChanged()
        {
            if (inkM.Strokes.Count > 0 && cv != null)
            {
                inkF.Strokes.Clear();
                cv.radGenderF.IsChecked = true;
            }
        }

        private void inkM_StylusUp(object sender, StylusEventArgs e)
        {
            inkMChanged();
        }

        private void inkFChanged()
        {
            if (inkF.Strokes.Count > 0 && cv != null)
            {
                inkM.Strokes.Clear();
                cv.radGenderM.IsChecked = true;
            }
        }

        private void inkF_StylusUp(object sender, StylusEventArgs e)
        {
            inkFChanged();
        }

        private void inkContactNoChanged()
        {
            if (cv != null)
            {
                getOcrText(inkContactNo, cv.txtContactNo);
            }
        }

        private void inkContactNo_StylusUp(object sender, StylusEventArgs e)
        {
            inkContactNoChanged();
        }

        private void inkEmailChanged()
        {
            if (cv != null)
            {
                getOcrText(inkEmail, cv.txtEmail);
            }
        }

        private void inkEmail_StylusUp(object sender, StylusEventArgs e)
        {
            inkEmailChanged();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            bool result = saveData();
            if (result)
            {
                clear();
            }

        }

        private bool saveData()
        {
            string connString;
            OleDbConnection cnn;
            bool result = false;

            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["HandwritingRegistration.Properties.Settings.AccessConnection"];
            connString = settings.ConnectionString;
            cnn = new OleDbConnection(connString);
            
            try
            {
                cnn.Open();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                MessageBox.Show("Cannot connect to database");
            }

            string name = "", contactNo = "", gender = "", email = "", signature = "";

            if (cv != null)
            {
                name = cv.txtName.Text;
                if (cv.radGenderM.IsChecked == true)
                {
                    gender = "M";
                }
                else
                {
                    gender = "F";
                }
                contactNo = cv.txtContactNo.Text;
                email = cv.txtEmail.Text;
            }
            else
            {
                name = getOcrText(inkName);
                contactNo = getOcrText(inkContactNo);
                email = getOcrText(inkEmail);
                if (inkM.Strokes.Count > 0)
                {
                    gender = "M";
                }
                else
                {
                    gender = "F";
                }
            }

            if (sigObj != null)
            {
                signature = sigObj.SigText;
            }

            try
            {
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = cnn;
                cmd.CommandType = CommandType.Text;

                if (currentId > 0)
                {
                    cmd.CommandText = "Update Registration Set `Name` = @pName, `Email` = @pEmail, `Gender` = @pGender, `ContactNo` = @pContactNo, `Signature` = @pSignature, " +
                        "`NameInk` = @pNameInk, `GenderMInk` = @pGenderMInk, `GenderFInk` = @pGenderFInk, `EmailInk` = @pEmailInk, `ContactNoInk` = @pContactNoInk where `ID` = @pId";
                }
                else
                {
                    cmd.CommandText = "Insert into Registration (`Name`, `Email`, `Gender`, `ContactNo`, `Signature`, `NameInk`, `GenderMInk`, `GenderFInk`, `EmailInk`, `ContactNoInk`) Values " +
                       "(@pName, @pEmail, @pGender, @pContactNo, @pSignature, @pNameInk, @pGenderMInk, @pGenderFInk, @pEmailInk, @pContactNoInk)";
                }

                cmd.Parameters.AddWithValue("@pName", name);
                cmd.Parameters.AddWithValue("@pEmail", email);
                cmd.Parameters.AddWithValue("@pGender", gender);
                cmd.Parameters.AddWithValue("@pContactNo", contactNo);
                cmd.Parameters.AddWithValue("@pSignature", signature);
                cmd.Parameters.AddWithValue("@pNameInk", SerializeStrokes(inkName.Strokes));
                cmd.Parameters.AddWithValue("@pGenderMInk", SerializeStrokes(inkM.Strokes));
                cmd.Parameters.AddWithValue("@pGenderFInk", SerializeStrokes(inkF.Strokes));
                cmd.Parameters.AddWithValue("@pEmailInk", SerializeStrokes(inkEmail.Strokes));
                cmd.Parameters.AddWithValue("@pContactNoInk", SerializeStrokes(inkContactNo.Strokes));

                if (currentId > 0)
                {
                    cmd.Parameters.AddWithValue("@pId", currentId);
                }

                int affected = cmd.ExecuteNonQuery();
               
                if (affected > 0)
                {
                    MessageBox.Show("Save Success");
                    result = true;   
                }

            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }

            cnn.Close();
            return result;
        }

        private void penMode_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switchToPenMode();
        }

        private void eraserMode_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switchToEraserMode();
        }

        private void inkName_MouseUp(object sender, MouseButtonEventArgs e)
        {
            inkNameChanged();
        }

        private void inkM_MouseUp(object sender, MouseButtonEventArgs e)
        {
            inkMChanged();
        }

        private void inkF_MouseUp(object sender, MouseButtonEventArgs e)
        {
            inkFChanged();
        }

        private void inkContactNo_MouseUp(object sender, MouseButtonEventArgs e)
        {
            inkContactNoChanged();
        }

        private void inkEmail_MouseUp(object sender, MouseButtonEventArgs e)
        {
            inkEmailChanged();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            string[] paths = Directory.GetFiles(appDir, "sig*.png");
            foreach (string file in paths)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception)
                {
                    //MessageBox.Show(err.Message);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //getScreenResolution();
            //adjustControls();
            if (System.Windows.Forms.Screen.AllScreens.Length > 1)
            {
                this.Left = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;
            }
            this.WindowState = WindowState.Maximized;
            this.Show();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            
        }
    }
}
