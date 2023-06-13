using Microsoft.Win32;
using System;
using System.Windows;
using System.IO;
using System.Threading;

namespace ComSimulatorApp
{
    /// <summary>
    /// Interaction logic for CANalyzerConfigurationView.xaml
    /// </summary>
    public partial class CANalyzerConfigurationView : Window
    {
        public  string ConfigurationFilePath { get; set; }
        public string DbcFilePath { get; set; }

        public string CaplFilePath { get; set; }
        // members used to access CANalyzer tool
        private CANalyzer.Application mCANalyzerApp;
        public CANalyzerConfigurationView()
        {

            InitializeComponent();
        }

        public CANalyzerConfigurationView(string dbcFilePath=null,string caplFilePath = null)
        {
            DbcFilePath = dbcFilePath;
            CaplFilePath = caplFilePath;
            InitializeComponent();
        }

        private void launchCANalyzer(string configurationFilePath, string dbcFilePath = null, string caplFilePath = null)
        {
           try
            {
                mCANalyzerApp = new CANalyzer.Application();
                Thread.Sleep(5000);
                //
                mCANalyzerApp.Open(configurationFilePath);
                Thread.Sleep(5000);
                CANalyzer.OpenConfigurationResult ocresult = mCANalyzerApp.Configuration.OpenConfigurationResult;

                if (ocresult.result == 0)
                {
                    MessageBox.Show("The configuration file is now open! "+
                        "Please edit it in CANalyzer or press the Run button if you want to run it without to change it!",
                        "Info!", MessageBoxButton.OK, MessageBoxImage.Information);

                    launchCANalyzerButton.IsEnabled = false;
                    //CANalyzer.Configuration cfgObject = mCANalyzerApp.Configuration;

                   // CANalyzer.CAPL CANalyzerCAPL = (CANalyzer.CAPL)mCANalyzerApp.CAPL;
                   // CANalyzerCAPL.Compile(null);


                    //mCANalyzerApp.Measurement.Start();
                }
                else
                {
                    MessageBox.Show(" Something went wrong!CANalyzer not launched!", "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    launchCANalyzerButton.IsEnabled = true;

                }
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ViewWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if(CaplFilePath != null)
            {
                selectedCaplPathBox.Text = CaplFilePath;
            }

            if(DbcFilePath != null)
            {
                selectedDbcPathBox.Text = DbcFilePath;
            }
        }


        private void selectConfigurationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //deschidere fisier .cfg
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "CANalyzer Configuration (*.cfg)|*.cfg";
                openFileDialog.Title = "Select configuration";
                openFileDialog.FileName = "";
                openFileDialog.InitialDirectory = "C:\\Users\\crisu\\Desktop\\PROIECT_LICENTA\\WORKSPACE_CANalyzer";
                if (openFileDialog.ShowDialog() == true)
                {
                    
                    // Obtinere cale fisier 
                    string filePath = openFileDialog.FileName;
                    if(filePath!=null && File.Exists(filePath))
                    {

                        ConfigurationFilePath = filePath;
                        configurationPathBox.Text = ConfigurationFilePath;
                        configurationPathBox.Focus();
                        configurationPathBox.CaretIndex = filePath.Length;

                        launchCANalyzerButton.IsEnabled = true;

                    }
                    else
                    {
                        MessageBox.Show("Invalid path provided!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }

                }
                else
                {

                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void launchCANalyzer_Click(object sender, RoutedEventArgs e)
        {
            launchCANalyzer(this.ConfigurationFilePath);
        }
    }
}
