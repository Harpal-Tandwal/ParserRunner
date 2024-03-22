using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Xml.Linq;
using System.Threading;


namespace ParserRunner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string folderPath = @"C:\ParcerRunner\Config";
        string config_path = @"C:\ParcerRunner\Config\config.xml";

        string parser_path = @"D:\all downloads\AnyDesk.exe";
        string testing_process_name = "Arduino IDE";
        private Thread? ModeCheckerThread;
        private bool ModeCheckerThreadRun;
        private bool check_process = true;




        public MainWindow()
        { 
            InitializeComponent();
            ReadyConfigFile();
            ModeCheckerThread = new Thread(new ThreadStart(ProcessMonitor));
           ModeCheckerThread .Start();
   
        }




  // **************  CHECK THE PRESENCE OF CONFIGURATION FILE *************************
        void ReadyConfigFile()
        {


            // Check if the folder exists, if not, create it
            if (!Directory.Exists(folderPath))
            {
                try
                {
                    Directory.CreateDirectory(folderPath);
                    MessageBox.Show("Config folder successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($" Error creating config folder: {ex.Message}");
                    Application.Current.Shutdown();
                }
            }



            if (!File.Exists(config_path))
            {
                try
                {
                    File.Create(config_path).Close();
                    MessageBox.Show("config file created successfully.");

                    XDocument doc = new XDocument(
                        new XElement("Applications",
                            new XElement("TestingProcessName", ""),
                            new XElement("ParcerPath", ""),
                            new XElement("ParserStatus", "Config")
                        )
                    );

                    // Save XML to a file

                    try
                    {
                        doc.Save(config_path);
                        MessageBox.Show("Configuration Format ready. \n App will run in config mode.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error in Preparing configuration Format: {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating config file: {ex.Message}");
                    Application.Current.Shutdown();
                }
            }

        }


        private void ParserMonitor()
        {

           
           

        }


        //**********************Save data in xml file on button click *****************************
        private void SaveConfig()
        {
            string testing_process = tb_testing_process_name.Text;
            string parser_path = tb_parser_path.Text;

            if (string.IsNullOrWhiteSpace(testing_process) || string.IsNullOrWhiteSpace(parser_path))
            {
                tb_status.Text = "Please enter both application names.";
                return;
            }

            // Create XML document
            XDocument doc = new XDocument(
                new XElement("Applications",
                    new XElement("TestingProcessName", testing_process),
                    new XElement("ParcerPath", parser_path),
                    new XElement("ParserStatus", "Running")
                )
            );

            // Save XML to a file

            try
            {
                doc.Save(config_path);
                ModeCheckerThreadRun = true;
                ModeCheckerThread?.Start();
         
                tb_status.Text = "Configuration saved successfully.";
            }
            catch (Exception ex)
            {
                tb_status.Text = $"Error saving configuration: {ex.Message}";
            }
            ReadConfiguration();
        }
//*************************** READ DATA FROM XML FILE ************************************
        private void ReadConfiguration()
        { 
            //D:\all downloads\AnyDesk.exe


            if (!File.Exists(config_path))
            {
                tb_status.Text = "Configuration file not found.";
                return;
            }

            try
            {
                XDocument doc = XDocument.Load(config_path);
                XElement mainAppElement = doc.Element("Applications")?.Element("TestingProcessName");
                XElement secondaryAppElement = doc.Element("Applications")?.Element("ParcerPath");

                if (mainAppElement != null && secondaryAppElement != null)
                {
                     testing_process_name = mainAppElement.Value;
                   parser_path = secondaryAppElement.Value;

                    tb_status.Text = $"Main Application: {testing_process_name}, Secondary Application: {parser_path}";
                }
                else
                {
                    tb_status.Text = "Error: Missing elements in configuration file.";
                }
            }
            catch (Exception ex)
            {
                tb_status.Text = $"Error reading configuration: {ex.Message}";

               
            }
        }
        //****************************FUNTION TO BE USED IN THREADING ************************
        private async void ProcessMonitor()
        {
            while (ModeCheckerThreadRun)
            {
                Debug.WriteLine("modecheck thread running");

                // Delay for 5 seconds
                Thread.Sleep(5000);
                if (!File.Exists(config_path))
                {
                    tb_status.Text = "Configuration file not found.";
                    return;
                }

                try
                {
                    XDocument doc = XDocument.Load(config_path);
                    XElement mainAppElement = doc.Element("Applications")?.Element("TestingProcessName");
                    XElement secondaryAppElement = doc.Element("Applications")?.Element("ParcerPath");
                    XElement mode = doc.Element("Applications")?.Element("ParserStatus");
                    if (mode?.ToString() == "Running")
                    {
                        check_process = true;
                        this.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        check_process = false;
                        this.Visibility = Visibility.Visible;
                    }
                }
                catch (Exception ex)
                {
                    tb_status.Text = $"Error reading configuration: {ex.Message}";


                }

               // MessageBox.Show("visibility thread running");


            }
//*********************************Process tracking *****************************************************
            while (check_process)
            {
                
                // Check if the process is already running
                Process[] testing = Process.GetProcessesByName(testing_process_name);
                Process[] parser = Process.GetProcessesByName(parser_path);

                if (testing.Length > 0)
                {
                    MessageBox.Show("testing is running.");

                    if (parser.Length > 0)
                    {
                        MessageBox.Show("Parser is running.");

                    }
                    else
                    {
                        // Check if the application exists
                        if (System.IO.File.Exists(parser_path))
                        {
                            // Start the application process
                            Process.Start(parser_path);

                            MessageBox.Show("parser started successfully.");
                        }
                        else
                        {
                            MessageBox.Show("Error: parser path not found.");
                        }

                    }

                }
                else
                {

                    MessageBox.Show("testing is not running.");
                }
                //   MessageBox.Show("parser monitor thread running");
            }



        }







        //**************************** UI CLICK EVENT HANDLING ******************* 
        private void btn_read_Click(object sender, RoutedEventArgs e)
        {
            ReadConfiguration();
        }

        private  async void  btn_check_Click(object sender, RoutedEventArgs e)
        {
                 ParserMonitor();

                 this.Visibility = Visibility.Collapsed;

                 await Task.Delay(5000);
                 this.Visibility = Visibility.Visible;

         

        }
        private void btn_save_config_Click(object sender, RoutedEventArgs e)
        {
            SaveConfig();
        }

//***************************HANDLING PARSER-RUNNER CLOSING.***************************************
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);
            // Set stopChecking flag to stop the checking thread
            MessageBox.Show("Parser Monitor Getting Closed");
            // Wait for the checking thread to terminate
            // checkingThread.Join();
            ModeCheckerThreadRun = false;
           
            ModeCheckerThread.Join();
         


    }




}
}