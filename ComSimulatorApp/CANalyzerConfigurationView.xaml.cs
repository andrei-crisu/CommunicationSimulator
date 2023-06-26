using Microsoft.Win32;
using System;
using System.Windows;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Data.Common;
using System.Windows.Media;

namespace ComSimulatorApp
{
    
    public partial class CANalyzerConfigurationView : Window
    {
        public  string ConfigurationFilePath { get; set; }
        public string DbcFilePath { get; set; }

        public string CaplFilePath { get; set; }
        //membri utilizati pentru a accesa unealta CANalyzer
        private CANalyzer.Application mCANalyzerApp;
        private CANalyzer.Measurement mCANalyzerMesurement;

        enum SimulationStatus
        {
            SIMULATION_OFF=0,
            SIMULATION_ON=1
        }
        private SimulationStatus simulationStatus;

        public CANalyzerConfigurationView()
        {

            InitializeComponent();
        }

        public CANalyzerConfigurationView(string dbcFilePath=null,string caplFilePath = null)
        {
            DbcFilePath = dbcFilePath;
            CaplFilePath = caplFilePath;
            simulationStatus=SimulationStatus.SIMULATION_OFF;
            InitializeComponent();
        }

        //metoda utilizat pentru incercarea de a porni aplicatia CANalyzer
        private void launchCANalyzer(string configurationFilePath, string dbcFilePath = null, string caplFilePath = null)
        {
           try
            {
                mCANalyzerApp = new CANalyzer.Application();
                Thread.Sleep(4000);
                //
                mCANalyzerApp.Open(configurationFilePath);
                Thread.Sleep(3000);
                mCANalyzerMesurement = (CANalyzer.Measurement)mCANalyzerApp.Measurement;
                CANalyzer.OpenConfigurationResult ocresult = mCANalyzerApp.Configuration.OpenConfigurationResult;

                if (ocresult.result == 0)
                {
                    MessageBox.Show("The configuration file is now open! "+
                        "Please edit it in CANalyzer or press the Run button if you want to run it without to change it!",
                        "Info!", MessageBoxButton.OK, MessageBoxImage.Information);
                    MessageBox.Show("Note that the CAPL file loaded is not the generated one as this process should be done manually!",
                        "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    CANalyzer.CAPL CANalyzerCAPL = (CANalyzer.CAPL)mCANalyzerApp.CAPL;
                    CANalyzerCAPL.Compile(null);
                    launchCANalyzerButton.IsEnabled = false;
                    runSimulation.IsEnabled = true;

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
        }

        //la apasarea butonului selectConfiguration button
        //va aparea fereastra dialog pentru selectia unei configuratii
        private void selectConfigurationButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //deschidere fisier .cfg
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "CANalyzer Configuration (*.cfg)|*.cfg";
                openFileDialog.Title = "Select configuration";
                openFileDialog.FileName = "";
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

        //la apasarea butonului launchCANalyzer 
        //va fi pornita aplicatia CANalyzer daca nu exista erori
        private void launchCANalyzer_Click(object sender, RoutedEventArgs e)
        {
            launchCANalyzer(this.ConfigurationFilePath);
        }

        //butonul startSimulation va incerca pornierea simularii daca 
        // aplicatia CANalzyer este deschisa 
        //daca nu este deschisa va informa utilizatorul
        private void startSimulation_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                if(isToolRunning("CANalyzer")||isToolRunning("CANw64"))
                {
                    //daca simularea este oprita se va porni simularea 
                    if (!mCANalyzerMesurement.Running)
                    {
                        mCANalyzerMesurement.Start();
                        MessageBox.Show("The measurement is running now!Please visit the CANalyzer tool!", "Question",
                           MessageBoxButton.OK, MessageBoxImage.Information);

                        displaySimulationStatus(SimulationStatus.SIMULATION_ON);
                    }
                    else
                    {
                        //altfel este pornita si atunci sa va opri
                        MessageBoxResult result = MessageBox.Show("The measurement is already running! Would you like to stop it?", "Question",
                            MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                        {
                            mCANalyzerMesurement.Stop();
                            displaySimulationStatus(SimulationStatus.SIMULATION_OFF);

                        }
                    }
                }
                else
                {
                    MessageBox.Show("CANalyzer is not running! Lunch it first!", "Warning!", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    runSimulation.IsEnabled = false;
                    displaySimulationStatus(SimulationStatus.SIMULATION_OFF);

                    if (pathIsValid(this.ConfigurationFilePath))
                    {
                        launchCANalyzerButton.IsEnabled = true;
                    }
                    else
                    {
                        MessageBox.Show("The provided path for the .cfg files is not valid anymore."+
                            " Please choose it again and you will be able to launch the CANalyzer tool from here! ",
                            "Warning!", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
            catch(Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
                runSimulation.IsEnabled = false;

            }
        }

        //metoda testaza daca procesul care indica ca aplicatia ruleaza exista 
        //printre procesele sistemului
        private bool isToolRunning(string toolName)
        {
            bool isRunning = false;
            Process[] processes = Process.GetProcesses();

            foreach (Process process in processes)
            {
                if (process.ProcessName.Contains(toolName))
                {
                    isRunning = true; 
                    return isRunning;
                }
            }

            return isRunning;
        }

        //testeaza daca o cale este valida 
        //incercand sa determine daca aceasta exista
        private bool pathIsValid(string path)
        {
            if(File.Exists(path))
                return true;
            else 
                return false;
            
        }

        //metoda actualizeaza pe interfata grafica statusul simularii
        private void displaySimulationStatus(SimulationStatus status)
        {
            string htmlColor;
            Color color;
            simulationStatus = status;

            switch (simulationStatus)
            {
                case SimulationStatus.SIMULATION_OFF:
                    htmlColor = "#AAAA0000";
                    color = (Color)ColorConverter.ConvertFromString(htmlColor);
                    statusRectangle.Fill = new SolidColorBrush(color);
                    simulationStatusBox.Text = "OFF";
                    break;

                case SimulationStatus.SIMULATION_ON:
                    htmlColor = "#AA00AA00";
                    color = (Color)ColorConverter.ConvertFromString(htmlColor);
                    statusRectangle.Fill = new SolidColorBrush(color);
                    simulationStatusBox.Text = "ON";
                    break;

                default: 
                    break;


            }
        }
    }
}
