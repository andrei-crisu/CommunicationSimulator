using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using ComSimulatorApp.caplGenEngine.caplTypes;
using ComSimulatorApp.dbcParserCore;
namespace ComSimulatorApp
{
    /// <summary>
    /// Interaction logic for ViewMessageDetails.xaml
    /// </summary>
    public partial class ViewMessageDetails : Window
    {
        public MessageType currentMessage;
        private bool messageDataSaved;
        public ViewMessageDetails(MessageType message)
        {
            currentMessage = new MessageType(message);
            InitializeComponent();

            //se afiseaza informatiile despre mesaj
            messageNameBox.Text = currentMessage.messageName;
            canIdBox.Text = currentMessage.CanId.ToString("X");
            dlcBox.Text = currentMessage.MessageLength.ToString();
            txNodeBox.Text = currentMessage.SendingNode;

            onMessageCheckbox.IsChecked = currentMessage.OnMessage;
            List<char> comboBoxKeys = generateAvailableKeys();
            selectKeyComboBox.ItemsSource = comboBoxKeys;
            selectKeyComboBox.SelectedItem = currentMessage.OnKey;
            payloadTextBox.Text = cleanPayloadString(currentMessage.MessagePayload.ToString());

            foreach(Signal signal in currentMessage.signals)
            {
                signalsListBox.Items.Add(signal.signalToString());
            }

            //status of data
            messageDataSaved = true;
        }

        private void MessageDataChanged()
        {
            try
            {
                messageDataSaved = false;
                if (saveMessageDataButton!=null)
                {
                    saveMessageDataButton.IsEnabled = true;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void saveMessageModifications()
        {
            try
            {
                //store changes in currentMessage member

                messageDataSaved = true;
                if (saveMessageDataButton != null)
                {
                    if (onMessageCheckbox != null)
                    {
                        if (onMessageCheckbox.IsChecked != null)
                        {
                            if(onMessageCheckbox.IsChecked==true)
                            {
                                currentMessage.OnMessage = true;
                            }
                            else
                            {
                                currentMessage.OnMessage = false;
                            }
                        }
                    }

                    if(selectKeyComboBox.SelectedItem!=null)
                    {
                        char ch = Convert.ToChar(selectKeyComboBox.SelectedItem);
                        if (generateAvailableKeys().Contains(ch))
                        {
                            currentMessage.OnKey = ch;
                        }
                        else
                        {
                            currentMessage.OnKey = '#';
                        }
                    }

                    currentMessage.setMessagePayload(cleanPayloadString(payloadTextBox.Text));
                    saveMessageDataButton.IsEnabled = false;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private bool isHexaSymbol(char ch)
        {
            bool result;
            result = (ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F') || (ch >= 'a' && ch <= 'f');

            return result;
        }

        private string cleanPayloadString(string payloadString)
        {
            string cleanString="";
            foreach(char ch in payloadString)
            {
                if(isHexaSymbol(ch))
                {
                    cleanString += ch;
                }
            }

            int counter = 0;
            string beautifulString = "";
            foreach (char ch in cleanString)
            {
                counter++;
                beautifulString += ch;
                if (counter >= 2)
                {
                    beautifulString += " ";
                    counter = 0;
                }
            }
            beautifulString = beautifulString.ToUpper();
            beautifulString=beautifulString.TrimEnd();

            return beautifulString;
        }

        private void UpdateMessageButton_Click(object sender, RoutedEventArgs e)
        {
            saveMessageModifications();
            this.DialogResult = true;
            this.Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                if (!messageDataSaved)
                {
                    //ask to save modifications
                    MessageBoxResult result;
                    result = MessageBox.Show("There is unsaved work!Would you like to save it before closing?",
                        "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        //save than close
                        saveMessageModifications();
                        this.DialogResult = true;
                        
                    }
                    else if(result==MessageBoxResult.No)
                    {
                        //close without saving
                        this.DialogResult = false;
                    }
                    else
                    {
                        //the cancel operation is canceled
                        e.Cancel = true;
                    }
                }
                else
                {
                    //just close
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "On Close::Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private List<char> generateAvailableKeys()
        {
            List<char> possibleKeys = new List<char>();
            possibleKeys.Add('#');
            for(char ch='A';ch<='Z';ch++)
            {
                possibleKeys.Add(ch);
            }
            for (char ch = 'a'; ch <= 'z'; ch++)
            {
                possibleKeys.Add(ch);
            }
            for (char ch = '0'; ch <= '9'; ch++)
            {
                possibleKeys.Add(ch);
            }
            return possibleKeys;
        }

        private void onMessageCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            MessageDataChanged();
        }

        private void onMessageCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            MessageDataChanged();
        }

        private void selectedKeyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MessageDataChanged();
        }

        private void generateRandomPayload_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageDataChanged();
                Random randomObject = new Random();
                string stringPayload = "";
                byte randomByteValue;
                for(int i=0;i< currentMessage.MessageLength;i++)
                {
                    randomByteValue = (byte)randomObject.Next(256);
                    stringPayload += randomByteValue.ToString("X2") + " ";
                }
                stringPayload.TrimEnd();
                payloadTextBox.Text = stringPayload;

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void payloadTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MessageDataChanged();
            string payloadText = payloadTextBox.Text;

            payloadTextBox.Text = cleanPayloadString(payloadText);
            payloadTextBox.CaretIndex = payloadTextBox.Text.Length;
        }

        private void payloadTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string payloadText = textBox.Text + e.Text;

            if(payloadText.Length>23)
            {
                e.Handled = true;
            }
        }
    }
}
