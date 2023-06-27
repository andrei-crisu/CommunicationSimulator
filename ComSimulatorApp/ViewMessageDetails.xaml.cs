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

            //se afiseaza informatiile despre mesaj care nu sunt modificabile
            messageNameBox.Text = currentMessage.messageName;
            canIdBox.Text = currentMessage.CanId.ToString("X");
            dlcBox.Text = currentMessage.MessageLength.ToString();
            txNodeBox.Text = currentMessage.SendingNode;

            //se afiseaza informatiile despre mesaj care pot fi modificate
            onMessageCheckbox.IsChecked = currentMessage.OnMessage;
            //se va obtine lista tastelor disponibile
            List<char> comboBoxKeys = generateAvailableKeys();
            selectKeyComboBox.ItemsSource = comboBoxKeys;
            selectKeyComboBox.SelectedItem = currentMessage.OnKey;
            payloadTextBox.Text = cleanPayloadString(currentMessage.MessagePayload.ToString());
            //se va afisa reprezentarea sub forma de text a semnalelor pe care le contine
            //mesajul
            foreach(Signal signal in currentMessage.signals)
            {
                signalsListBox.Items.Add(signal.signalToString());
            }

            //status of data
            messageDataSaved = true;
        }

        //aceasta metoda determina activarea
        //butonului de salvare atunci cand se realizeaza modificari
        //variabila messageDataSaved devine false indicand ca exista date nesalvate
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

        //metoda utilizata pentru a salva modificarile realizate
        //de utilizator

        //modificarile sunt stocate in membrul currentMessage 
        private void saveMessageModifications()
        {
            try
            {
                //variabila care indica statusul datelor ( salvate sau nu ) devine true
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
                    //extragere valoare date din interfata grafica
                    currentMessage.setMessagePayload(cleanPayloadString(payloadTextBox.Text));
                    saveMessageDataButton.IsEnabled = false;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        //testeaza daca un caracter este cifra hexazecimala
        private bool isHexaSymbol(char ch)
        {
            bool result;
            result = (ch >= '0' && ch <= '9') || (ch >= 'A' && ch <= 'F') || (ch >= 'a' && ch <= 'f');

            return result;
        }

        //extrage din sirul de caractere ce reprezinta valoarea datelor unui mesaj
        //doar caracterele ce reprezinta cifre hexazecimale
        //spatiile si alte caractere albe sunt omise
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

            //intre fiecare 2 cifre hexazecimale se va pune un spatiu
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

        //metoda ce se apeleaza atunci cand utilizatorul
        //doreste sa actualizeze informatiile despre mesaj
        private void UpdateMessageButton_Click(object sender, RoutedEventArgs e)
        {
            //se salveaza modificarile
            saveMessageModifications();
            //fereastra va returna un status true
            this.DialogResult = true;
            //se inchide fereastra
            this.Close();
        }

        //metoda ce se executa atunci cand apare evenimentul de
        //inchidere a ferestrei
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                //daca informatia nu este salvata
                if (!messageDataSaved)
                {
                    //se intreaba daca se doreste salvarea modificarilor
                    MessageBoxResult result;
                    result = MessageBox.Show("There is unsaved work!Would you like to save it before closing?",
                        "Question", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                    //daca raspunsul este Yes
                    if (result == MessageBoxResult.Yes)
                    {
                        //se salveaza modificarile si se inchide fereastra
                        saveMessageModifications();
                        this.DialogResult = true;
                        
                    } //altfel ( daca raspunsul este No
                    else if(result==MessageBoxResult.No)
                    {
                        //se inchide fereastra fara a se salva modificarile
                        this.DialogResult = false;
                    }
                    else
                    {
                        //altfel ( se doreste mentinerea ferestrei deschise)
                        //operatia de inchidere a ferestrei este anulata
                        e.Cancel = true;
                    }
                } //daca nu exista modificari facute
                else
                {
                    //fereastra se inchide fara a se mai intreba
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "On Close::Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //metoda genereaza o lista de caractere ce cuprinde
        //literele de la A-Z , de la a-z , cifrele 0-9 si caracterul #
        //si este utilizata ca sursa pentru combo box-ul 
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

        //metoda se se executa la apasarea butonului de 
        //generare a unei valori aleatorii pentru valoarea
        //campului de date al mesajului
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

        //se executa atunci cand apare o modificare in campul 
        // payload al mesajului
        private void payloadTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            MessageDataChanged();
            string payloadText = payloadTextBox.Text;

            payloadTextBox.Text = cleanPayloadString(payloadText);
            payloadTextBox.CaretIndex = payloadTextBox.Text.Length;
        }

        //se executa  se executa inainte sa apara o modidicarea a continutului
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
