using System;
using System.Collections.Generic;
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
        private CANalyzer.Measurement mCANalyzerMeasurement;
        public CANalyzerConfigurationView()
        {

            InitializeComponent();
        }

        private void launchCANalyzer(string configurationFilePath, string dbcFilePath = null, string caplFilePath = null)
        {
           try
            {
                mCANalyzerApp = new CANalyzer.Application();
                mCANalyzerMeasurement = (CANalyzer.Measurement)mCANalyzerApp.Measurement;
                //
                mCANalyzerApp.Open(@"C:\Users\crisu\Desktop\PROIECT_LICENTA\WORKSPACE_CANalyzer\simulation_crisuandrei.cfg", true, true);
                CANalyzer.OpenConfigurationResult ocresult = mCANalyzerApp.Configuration.OpenConfigurationResult;

                if (ocresult.result == 0)
                {
                    launchCANalyzerButton.IsEnabled = false;
                    CANalyzer.CAPL CANalyzerCAPL = (CANalyzer.CAPL)mCANalyzerApp.CAPL;
                    CANalyzerCAPL.Compile(null);
                    mCANalyzerMeasurement.Start();
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


        private void selectConfigurationButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void launchCANalyzer_Click(object sender, RoutedEventArgs e)
        {
            launchCANalyzer("");
        }
    }
}
