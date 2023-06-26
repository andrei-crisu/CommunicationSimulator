using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using ComSimulatorApp.caplGenEngine;
using ComSimulatorApp.caplGenEngine.caplTypes;
using System;

namespace ComSimulatorApp
{
    public partial class CodeGeneration:Window
    {
        //lista fisierelor dbc disponibile pentru generare
        public List<fileUtilities.DbcFile> filesUsedForGeneration;
        //fisierul DBC selectat pentru generare
        private fileUtilities.DbcFile selectedDbcFile;
        //fisierul dbc generat
        public fileUtilities.CaplFile generatedCaplFile;

        //colectii utilizate pentru actualizarea interfetei grafice
        private ObservableCollection<string> dbcFiles;
        private ObservableCollection<MessageType> messagesItems;
        private ObservableCollection<MsTimerType> msTimerItems;

        //colectii utilizate pentru a afisa in interfata grafica
        //mesajele ce pot fi atasate la un timer si pe cele care sunt deja atasate
        private ObservableCollection<MessageType> availableMessages;
        private ObservableCollection<MessageType> attachedMessages;


        //utilizat pentru a numara de cate ori se face o selectie a unui fisier dbc
        //acest contor este util pentru a afisa anumite notificari utilizatorului
        private int countFileSelectionChanged;

        public CodeGeneration(List<fileUtilities.DbcFile> files)
        {
            InitializeComponent();
            countFileSelectionChanged = 0;
            filesUsedForGeneration = files;
            //initial se instantiza un fisier CAPL selectat gol
            generatedCaplFile = new fileUtilities.CaplFile(null,null);

            //initial se instantiza un fisier DBC selectat gol
            selectedDbcFile =new  fileUtilities.DbcFile(null,null);

            //se vor adauga in intefata grafica denumirile fisierelor dbc disponibile
            //sub forma unei liste
            dbcFiles = new ObservableCollection<string>();

            foreach (fileUtilities.DbcFile file in filesUsedForGeneration)
            {
                dbcFilesList.Items.Add(file.fileName);
            }

            //realizarea unei legaturi intre colectii si intefata grafica
            messagesItems = new ObservableCollection<MessageType>();
            selectedMessagesView.ItemsSource = messagesItems;

            msTimerItems = new ObservableCollection<MsTimerType>();
            createdTimersView.ItemsSource = msTimerItems;
            selectedTimerComboBox.ItemsSource = msTimerItems;


            attachedMessages = new ObservableCollection<MessageType>();
            attachedMessagesListBox.ItemsSource = attachedMessages;
            availableMessages = new ObservableCollection<MessageType>();
            availableMessagesListBox.ItemsSource = availableMessages;
            //se afiseaza mesajele disponibile pentru a fi atasate
            //initial toate mesajele sunt disponibile
            getAvailableMessages();

        }

        //se reimprospateaza lista mesajelor disponibile pe baza mesajelo din lista messagesItems
        private void getAvailableMessages()
        {
            availableMessages.Clear();
            foreach( MessageType message in messagesItems)
            {
                availableMessages.Add(message);
            }
        }
        //se seteaza lista de mesaje atasate cu elementele listei messages
        private void getAttachedMessages(List<MessageType> messages)
        {
            attachedMessages.Clear();
            if (messages != null)
            {
                foreach (MessageType message in messages)
                {
                    attachedMessages.Add(message);
                }
            }
            else
            {
                MessageBox.Show("FATAL ERR: NULL attached messages List found!", "Error",MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        //reinprospatarea mesajelor atasate si a mesajelor disponibile
        private void refreshAvailableAndAttachedMessages(List<MessageType> attachedToTimerMessages)
        {
            //se obtine mesajele disponibile
            getAvailableMessages();

            //se obtine lsita mesajelor atasate
            getAttachedMessages(attachedToTimerMessages);

            //removes each message from the availableMessage collection
            //if exists in the attachedMessages collection
            //it also removes it from  the attachedMessages collection if
            // it doesn't exist in the messagesItems collection
            try
            {
                List<MessageType> copyListattachedMessages = attachedMessages.ToList();
                foreach (MessageType message in copyListattachedMessages)
                {
                    if (availableMessages.Contains(message))
                    {
                        availableMessages.Remove(message);
                    }

                    if (!messagesItems.Contains(message))
                    {
                        attachedMessages.Remove(message);
                    }
                }
            }
            catch(InvalidOperationException exceptiion1)
            {
                MessageBox.Show("FATAL ERROR => Exception catched: " + exceptiion1.Message, "Exception caught",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //metoda care se apeleaza la apasare butonului de generare cod
        private void generateCodeButton_Click(object sender, RoutedEventArgs e)
        {
            //initial statusul generari este adevarat
            this.DialogResult = true;
            //variabila sir de caractere ce contine denumirea fisierului ce 
            //urmeaza a fi generat
            string generatedFileName;
            //daca denumirea fisierului este valida ( contine cel putin un caracter)
            //se seteaza denumirea specificata in interfata grafica
            if (fileNameTextBox.Text.Length > 0)
            {
                generatedFileName = fileNameTextBox.Text;
            }
            else
            {
                //altfel se va utiliza o denumire default,
                //iar daca denumierea fisierului dbc selectat este una valida
                //se va folosi denumirea acestuia
                generatedFileName = "GEN_FILE_";
                if (selectedDbcFile.fileName != null && selectedDbcFile.fileName.Length > 0)
                {
                    generatedFileName += selectedDbcFile.fileName;
                }
            }

            //se seteaza denumirea fisierului ce urmeaza a fi generat
            generatedCaplFile.fileName = generatedFileName;

            string initialComment = "";
            //se seteaza comenatariul initial
            // daca in intefata grafica s-a specificat un comentariu ... se va seta acela
            //altfel se va folosi unul default
            string textFromUi = initialCommentTextBox.Text;
            if (textFromUi.Length <= 0)
            {
                initialComment += "\n\n THIS IS A SINGLE LINE COMMENT\n\n";
            }
            else
            {
                initialComment += textFromUi;
            }
            //daca fisierul DBC selectat nu este nul
            if (selectedDbcFile.fileName != null)
            {
                //daca lista mesajelor pe care le contine nu este nula sau vida
                if (messagesItems != null && messagesItems.Count > 0)
                {
                    // se genereaza codul capl pe baza listei mesajelor si a listei timerelor
                    CaplGenerator generatorInstance = new CaplGenerator(messagesItems.ToList(),msTimerItems.ToList(),initialComment);
                    //fisierul CAPL generat va avea drept continut rezultatul generarii
                    generatedCaplFile.fileContent = generatorInstance.getResult();
                    // de asemenea va stoca si elementele generate ( variabilele globale)
                    generatedCaplFile.globalVariables = generatorInstance.globalVariables;
                }
                else
                {
                    MessageBox.Show("No message selected!", "Generation", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("No selected file identified!", "Generation", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            //se inchide fereastra pentru configurarea generarii
            this.Close();
        }

        // metoda ce se executa atunci
        //cand se apasa butonul de abandonare a generarii
        private void cancelOperationButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result;
            result = MessageBox.Show("Are you sure you want to close the generation window? All changes will be lost!", 
                "Question", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
            {
                //operatia de anulare a generarii este efectuata
                this.DialogResult = false;
                this.Close();
            }
            else
            {
                //operatia de anulare a generarii este anulata
            }
        }

        private void onItemSelected(object sender, SelectionChangedEventArgs e)
        {
            //nothing
        }

        //selectarea fsierului DBC utilizat pentru generare
        private void dbcFileList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult result = MessageBoxResult.OK;  
            //daca fisierul utilizat pentru generare nu este nul
            if (filesUsedForGeneration != null)
            {
                if (sender is ListViewItem listViewItem)
                {
                    string selectedItemFileName = listViewItem.DataContext.ToString();

                    //daca nu se selecteaza pentru prima data un fisier DBC, altul fiind selectat,
                    //utilizatorul va fi intrebat daca  sigur doreste sa schimbe DBC-ul
                    //deorece acest lucru implica pierderea tuturor configurarilor realizate
                    if (countFileSelectionChanged != 0)
                    {
                        //counterul va fi setat cu valoarea 1 ceea ce reprezinta o valoare diferita de 0 pentru a 
                        //indica ca nu este pentru prima data cand se selecteaza un mesaj
                        countFileSelectionChanged = 1;
                        //dacafisierul DBC pe care s-a dat dublu click  nu este acelasi se face selectia 
                        if (selectedItemFileName != selectedDbcFile.fileName)
                        {
                            result = MessageBox.Show("Are you sure you want to change the selected dbc file?" +
                                "All modifications made until now that target another dbc will be lost (message configurations and other message related events and functions)!", "Question",
                                    MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                        }
                        else
                        {
                            result = MessageBoxResult.Cancel;
                        }
                    }
                   

                    if (result == MessageBoxResult.OK)
                    {
                        //la schimbarea fisierului DBC
                        selectedDbcFile = filesUsedForGeneration.FirstOrDefault(item => item.fileName == selectedItemFileName);
                        if (selectedDbcFile != null)
                        {
                            MessageBox.Show("The file: {" + selectedDbcFile.fileName + "} stores: " + selectedDbcFile.getParsededObjects().messages.Count().ToString() + " messages!"
                                , "About", MessageBoxButton.OK, MessageBoxImage.Information);
                            displaySelectedDbcMessages();
                           
                            //se vor actualiza mesajele disponibile si atasate pentru fiecare timer
                            //cu alte cuvinte orice informatie configurat anterior referitor la fisierul DBC 
                            //se va pierde fiind vorba de un alt dbc
                            try
                            {
                                foreach (MsTimerType timer in msTimerItems)
                                {
                                    timer.clearAttachedMessagesList();
                                }
                                attachedMessages.Clear();
                                availableMessages.Clear();
                                MsTimerType selectedTimer = selectedTimerComboBox.SelectedItem as MsTimerType;

                                if (selectedTimer != null)
                                {
                                    refreshAvailableAndAttachedMessages(selectedTimer.getAttachedMessagesList());
                                }
                            }
                            catch (InvalidOperationException exceptiion1)
                            {
                                MessageBox.Show("FATAL ERROR => Exception catched: " + exceptiion1.Message, "Exception caught",
                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                            }
                           

                            fileNameTextBox.Text = selectedDbcFile.fileName;

                        }
                        else
                        {
                            MessageBox.Show("{" + selectedDbcFile.fileName + " not found!", "About",
                                MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    else
                    {

                    }

                }
                else
                {

                }
            }
            else
            {
                MessageBox.Show("Fatal error!No provided dbc found!The list is NULL. Try to close and reopent the dbc files!", "ERR",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                this.DialogResult = false;
                this.Close();
            }

            countFileSelectionChanged++;
        }

        //metoda utilizata pentru a seta  faptul ca
        // se vor genera evenimente on message pentru toate mesajele
        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (MessageType messageItem in messagesItems)
            {
                messageItem.OnMessage = true ;
                messageItem.NotifyPropertyChanged(nameof(MessageType.OnMessage));
            }
        }

        //metoda utilizata pentru a deselecta, pentru toate mesajele, optiunea
        // de generare a evenimentului on message
        private void DeselectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (MessageType messageItem in messagesItems)
            {
                messageItem.OnMessage = false;
                messageItem.NotifyPropertyChanged(nameof(MessageType.OnMessage));
            }
        }

        //metoda utilizata pentru a adauga mesajele in tabel ("selectedMessagesView")
        private void displaySelectedDbcMessages()
        {
            if(selectedDbcFile.fileName!=null)
            {
                messagesItems.Clear();
                List<char> keys = new List<char>();
                for(char ch='A';ch<='Z';ch++)
                {
                    keys.Add(ch);
                }
                for (char ch = 'a'; ch <= 'z'; ch++)
                {
                    keys.Add(ch);
                }
                for (char ch = '0'; ch <= '9'; ch++)
                {
                    keys.Add(ch);
                }

                int index = 0;
                foreach (dbcParserCore.Message message in selectedDbcFile.getParsededObjects().messages)
                {

                    MessageType messageItem = new MessageType(message, true, keys.ElementAt(index));
                    index++;
                    if(index<keys.Count)
                    {
                    }
                    else
                    {
                        index = 0;
                    }

                    messagesItems.Add(messageItem);
                }
            }
            else
            {
                MessageBox.Show("Selected: {" + selectedDbcFile.fileName + "} is EMPTY:: NULL ERR",
                    "About", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //permite doar litere si underscore in campul ce specifica denumrea fisierului generat
        private void fileNameTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("^[a-zA-Z0-9_]+$");
            if(!regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
            else
            {

            }
        }

        //metoda ce se executa atunci cand a aparut o modificare in textul ce specifica numele
        //fisierului generat
        private void fileNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            int maxFileNameLength = 40;
            if(textBox.Text.Length>maxFileNameLength)
            {
                textBox.Text = textBox.Text.Substring(0, maxFileNameLength);
                textBox.CaretIndex = maxFileNameLength;
            }

            string fileNameString = textBox.Text;
            string cleanString = Regex.Replace(fileNameString, "^[0-9]+", "");
            textBox.Text = cleanString;
        }

        //metoda ce se executa atunci cand a aparut o modificare in textul ce specifica
        //comentatiul initial din fisierul generat
        private void initialCommentTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            List<string> restrictedSubstrings = new List<string> { "//", "/*", "*/" };
            TextBox textBox = (TextBox)sender;
            foreach( string restrictedWord in restrictedSubstrings)
            {
                if(textBox.Text.Contains(restrictedWord))
                {
                    textBox.Text = textBox.Text.Replace(restrictedWord, string.Empty);
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }
        }

        //campul care specifica tasta care 
        //va determina trimitera unui mesaj limiteaza 
        //introducerea unui singur caracter
        //care poate fi litera sau cifra sau caracterul '#'
        private void OnKeyEventBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            var regex = new Regex("^[a-zA-Z0-9#]+$");
            if (!regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
            else
            {

            }

            if(textBox.Text.Length>=1)
            {
                e.Handled = true;
                return;
            }
        }

        private void SelectedTimerView_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {

        }

        //atunci cand se selecteaza un alt timer 
        //din lista de timere existente
        private void SelectedTimerView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(sender is DataGrid dataGridItem)
            {
                MsTimerType timer = dataGridItem.SelectedItem as MsTimerType;
                if(timer!=null)
                {
                    timerNameBox.Text = timer.MsTimerName;
                    timerPeriodBox.Text = timer.MsPeriod.ToString();
                }
            }
        }

    
        //metoda care sterge toate timerele existente
        private void ClearAllTimers_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete all created timers?","Confirmation",
                MessageBoxButton.OKCancel,MessageBoxImage.Question);

            if (result == MessageBoxResult.OK)
            {
                msTimerItems.Clear();
                updateAttachedMessages.IsEnabled = true;
                try
                {

                    attachedMessages.Clear();
                    availableMessages.Clear();
                }
                catch (InvalidOperationException exceptiion1)
                {
                    MessageBox.Show("FATAL ERROR => Exception catched: " + exceptiion1.Message, "Exception caught",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            else
            {

            }
        }

        //adaugaarea unui nu timer in lista
        private void AddNewTimer_Click(object sender, RoutedEventArgs e)
        {
            // daca casuta defaultTimerCheckBox nu este bifata 
            // se va adauga un timer cu denumire si perioada specifica
            if (defaultTimerCheckBox.IsChecked==false)
            {

                string timerName = "";
                string timerPeriodString = "";
                UInt32 timerPeriod = MsTimerType.DEFAULT_PERIOD;
                //se obtin denumirea si perioada timer-ului din interfata grafica
                timerName = timerNameBox.Text;
                timerPeriodString = timerPeriodBox.Text;
                //daca denumirea exista se va indica faptul ca trebuie ales un alt timer
                bool sameNameExists = msTimerItems.Any(element => element.MsTimerName == timerName);
                if (sameNameExists)
                {
                    MessageBox.Show("A timer with the same name already exists!\nChose another name!", "Info",
                          MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    bool parseSucces = UInt32.TryParse(timerPeriodString, out UInt32 periodValue);
                    //daca perioada a fost extrasa cu succes
                    if (parseSucces)
                    {
                        timerPeriod = periodValue;
                       //daca s-a atins numarul maxim de timere 
                       //nu se poate adauga un alt timer
                        if (msTimerItems.Count >= MsTimerType.MAX_TIMER_NR)
                        {
                            MessageBox.Show("The timer has not been added!The maximum nunber of timers {"+
                                MsTimerType.MAX_TIMER_NR.ToString()+"} has been reached!", "Info",MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {   // nu s-a atins numarul maxim de timere
                            //se va adauga un nou timer
                            msTimerItems.Add(new MsTimerType(timerName, timerPeriod));
                        }

                    }

                    else
                    {
                        MessageBox.Show("Timer period is not a number!", "Info", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            else
            {  //se va incerca adaugarea unui timer default
                // daca s-a atins numarul maxim acest nu se va putea adauga timer-ul
                if (msTimerItems.Count >= MsTimerType.MAX_TIMER_NR)
                {
                    MessageBox.Show("The timer has not been added!The maximum nunber of timers {" +
                        MsTimerType.MAX_TIMER_NR.ToString() + "} has been reached!", "Info", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    //altfel se va adauga un timer cu o perioada si denumire predefinita 
                    //in cadrul clasei MsTimerType
                    msTimerItems.Add(new MsTimerType());
                }
            }
        }

        //metoda care se executa atunci cand se incearca actualizarea informatiilor despre un
        //timer
        private void UpdateTimer_Click(object sender, RoutedEventArgs e)
        {
            if (createdTimersView.SelectedItem != null)
            {
                MsTimerType selectedTimer = createdTimersView.SelectedItem as MsTimerType;
                if (selectedTimer != null && msTimerItems.Contains(selectedTimer))
                {
                    //se va intreba daca se doreste actualizarea timer-ului
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to update the timer?", "Confirmation",
                MessageBoxButton.OKCancel, MessageBoxImage.Question);

                    if (result == MessageBoxResult.OK)
                    {
                        string timerName = timerNameBox.Text;
                        string timerPeriodString = timerPeriodBox.Text;
                        UInt32 timerPeriod;
                        //actualizarea se va realiza daca noul nume nu exista deja si daca perioada este valida
                        bool sameNameExists = msTimerItems.Any(timer => timer.MsTimerName == timerName && !timer.Equals(selectedTimer));
                        if (sameNameExists)
                        {
                            MessageBox.Show("A timer with the same name already exists!\nChose another name!", "Info",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            bool parseSucces = UInt32.TryParse(timerPeriodString, out UInt32 periodValue);

                            if (parseSucces)
                            {
                                timerPeriod = periodValue;
                                if (selectedTimer.MsTimerName == timerName && selectedTimer.MsPeriod == timerPeriod)
                                {
                                    MessageBox.Show("Nothing to update!", "Info", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                                }
                                else
                                {

                                    selectedTimer.MsTimerName = timerName;
                                    selectedTimer.MsPeriod = periodValue;
                                    createdTimersView.Items.Refresh();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Timer period is not a number!", "Info", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                    else
                    {

                       
                    }
                }
            }
        }

        //permite stergea timer-ului selectat
        private void DeleteTimer_Click(object sender, RoutedEventArgs e)
        {
            if(createdTimersView.SelectedItem!=null)
            {
                MsTimerType selectedTimer = createdTimersView.SelectedItem as MsTimerType;
                if(selectedTimer!=null && msTimerItems.Contains(selectedTimer))
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to delete the timer?", "Confirmation",
                MessageBoxButton.OKCancel, MessageBoxImage.Question);

                    if (result == MessageBoxResult.OK)
                    {
                        //daca mesajul sters este acelasi precum cel selectat in combo box
                        //operatia de actualizare a mesajelor asociate respectivului timer
                        //se va dezactiva
                        MsTimerType selectedTimerFromComboBox = (MsTimerType)selectedTimerComboBox.SelectedItem;
                        if (selectedTimerFromComboBox != null)
                        {
                            if(selectedTimerFromComboBox.MsTimerName==selectedTimer.MsTimerName)
                            {
                                updateAttachedMessages.IsEnabled = false;
                                //de asemenea se vor sterge mesajeloe disponibile  si atasate timer-ului
                                try
                                {

                                    attachedMessages.Clear();
                                    availableMessages.Clear();
                                }
                                catch (InvalidOperationException exceptiion1)
                                {
                                    MessageBox.Show("FATAL ERROR => Exception catched: " + exceptiion1.Message, "Exception caught",
                                        MessageBoxButton.OK, MessageBoxImage.Warning);
                                }

                            }
                        }

                        

                        msTimerItems.Remove(selectedTimer);
                        timerNameBox.Clear();
                        timerPeriodBox.Clear();
                        defaultTimerCheckBox.IsChecked = false;
                        addTimerButton.IsEnabled = true;
                    }
                    else
                    {

                    }
                }
            }
        }

        //campul in care este setata perioada timer-ului accepta doar cifre
        //nu se accepta ca pe prima pozitie sa se afle cifra 0
        private void timerPeriodBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            var regex = new Regex("^[0-9]+$");
            if (!regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
            else
            {

            }

            string periodString = textBox.Text;
            string cleanString = Regex.Replace(periodString, "^[0]+", "");
         
            textBox.Text = cleanString;
        }

        // numele timer-ului poate contine doar litere si cifre
        private void timerNameBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            var regex = new Regex("^[a-zA-Z0-9_]+$");
            if (!regex.IsMatch(e.Text))
            {
                e.Handled = true;
            }
            else
            {

            }


        }

        private void timerNameBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            string periodString = textBox.Text;
            string cleanString = Regex.Replace(periodString, "^[0-9]+", "");
            textBox.Text = cleanString;

            if (timerPeriodBox.Text.Length > 0 && timerNameBox.Text.Length > 0)
            {
                addTimerButton.IsEnabled = true;
            }
            else
            {
                addTimerButton.IsEnabled = false;
            }
        }

        private void timerPeriodBox_TextChanged(object sender, TextChangedEventArgs e)
        {

            TextBox textBox = (TextBox)sender;
            string textBoxString = textBox.Text;
            char[] removeStartSeq = { '0', ' ', '\t', '\n' };
            textBox.Text = textBoxString.TrimStart(removeStartSeq);
            if (timerPeriodBox.Text.Length > 0 && timerNameBox.Text.Length > 0)
            {
                addTimerButton.IsEnabled = true;
            }
            else
            {
                addTimerButton.IsEnabled = false;
            }
        }

        private void defaultTimerCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            timerNameBox.IsEnabled = false;
            timerNameBox.Clear();
            timerPeriodBox.IsEnabled = false;
            timerPeriodBox.Clear();
            addTimerButton.IsEnabled = true;
        }

        private void defaultTimerCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            timerNameBox.IsEnabled = true;
            timerPeriodBox.IsEnabled = true;
            addTimerButton.IsEnabled = false;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            MsTimerType selectedTimer = selectedTimerComboBox.SelectedItem as MsTimerType;

            if(selectedTimer!=null)
            {
                refreshAvailableAndAttachedMessages(selectedTimer.getAttachedMessagesList());
            }
        }

        //se muta mesajele selectate din lista mesajelor disponibile in lista celor atasate
        private void MoveToAttached_Click(object sender, RoutedEventArgs e)
        {
            foreach(var selectedMessage in availableMessagesListBox.SelectedItems.Cast<MessageType>().ToList())
            {
                availableMessages.Remove(selectedMessage);
                attachedMessages.Add(selectedMessage);
                updateAttachedMessages.IsEnabled = true;
            }
        }

        //se muta mesajele selectate din lista mesajelor atasate in lista celor disponibile
        private void MoveToAvailable_Click(object sender, RoutedEventArgs e)
        {

            foreach (var selectedMessage in attachedMessagesListBox.SelectedItems.Cast<MessageType>().ToList())
            {
                attachedMessages.Remove(selectedMessage);
                availableMessages.Add(selectedMessage);
                updateAttachedMessages.IsEnabled = true;
            }
        }

        //se actualizeaza lista mesajelor atasate pentru timerul selectat
        //atunci cand se apasa butonul de actualizare
        private void updateAttachedMessages_Click(object sender, RoutedEventArgs e)
        {
            MsTimerType selectedTimer = (MsTimerType)selectedTimerComboBox.SelectedItem;

            if (attachedMessages != null && selectedTimer!=null)
            {
                bool result=selectedTimer.SetAttachedMessagesList(attachedMessages.ToList());
                if(result)
                {
                    MessageBox.Show("The attached messages list for timer {"+selectedTimer.MsTimerName +"} has been updated!", "Info", MessageBoxButton.OK);
                    updateAttachedMessages.IsEnabled = false;

                }
            }
        }

        // se seteaza optiunea de generare a blocului on message
        //pe baza unui context menu pentru mesajul selectat
        private void CheckMessageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedMessagesView.SelectedItem != null )
                {
                    MessageType selectedMessage = (MessageType)selectedMessagesView.SelectedItem;
                    if (selectedMessage != null)
                    {
                        
                        selectedMessage.OnMessage = true;
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // se deselecteaza optiunea de generare a blocului on message
        //pe baza unui context menu pentru mesajul selectat
        private void UnckeckMessageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedMessagesView.SelectedItem != null)
                {
                    MessageType selectedMessage = (MessageType)selectedMessagesView.SelectedItem;
                    if (selectedMessage != null)
                    {

                        selectedMessage.OnMessage = false;
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //optiunea contextuala de vizualizare a detaliilor unui mesaj va lansa o noua fereastra in care se 
        //pot vizualiza anumite informatii needitabile
        //dar se si pot seta anumite optiuni
        private void ViewMessageDetailsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedMessagesView.SelectedItem != null)
                {
                    MessageType selectedMessage = (MessageType)selectedMessagesView.SelectedItem;
                    if (selectedMessage != null)
                    {
                        //se construieste o instanta a clasei ViewMessageDetails pentru mesajul selectat
                        ViewMessageDetails messageDetailsWindow = new ViewMessageDetails(selectedMessage);
                        //se deschide noua fereastra
                        bool? returnStatus = messageDetailsWindow.ShowDialog();
                        if(returnStatus==true)
                        {
                            //se seteaza tasta pentru mesajul selectat
                            selectedMessage.OnKey=messageDetailsWindow.currentMessage.OnKey;
                            //se specifica daca se genereaza sau nu handlerul on message
                            selectedMessage.OnMessage = messageDetailsWindow.currentMessage.OnMessage;
                            //se seteaza campul de date
                            selectedMessage.setMessagePayload(messageDetailsWindow.currentMessage.getMessagePayload());
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
