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
        //members to store informations from the file configuration
        public List<fileUtilities.DbcFile> filesUsedForGeneration;
        private fileUtilities.DbcFile selectedDbcFile;
        public fileUtilities.CaplFile generatedCaplFile;

        //all existing files used for generation
        private ObservableCollection<string> dbcFiles;
        private ObservableCollection<MessageType> messagesItems;
        private ObservableCollection<MsTimerType> msTimerItems;

        //only used to attach send message functionality in the event " on timer "
        private ObservableCollection<MessageType> availableMessages;
        private ObservableCollection<MessageType> attachedMessages;


        //other used variables
        private int countFileSelectionChanged;

        public CodeGeneration(List<fileUtilities.DbcFile> files)
        {
            InitializeComponent();
            countFileSelectionChanged = 0;
            filesUsedForGeneration = files;
            generatedCaplFile = new fileUtilities.CaplFile(null,null);


            selectedDbcFile =new  fileUtilities.DbcFile(null,null);

            //
            dbcFiles = new ObservableCollection<string>();

            foreach (fileUtilities.DbcFile file in filesUsedForGeneration)
            {
                dbcFilesList.Items.Add(file.fileName);
            }

            //
            messagesItems = new ObservableCollection<MessageType>();
            selectedMessagesView.ItemsSource = messagesItems;

            msTimerItems = new ObservableCollection<MsTimerType>();
            createdTimersView.ItemsSource = msTimerItems;
            selectedTimerComboBox.ItemsSource = msTimerItems;


            attachedMessages = new ObservableCollection<MessageType>();
            attachedMessagesListBox.ItemsSource = attachedMessages;
            availableMessages = new ObservableCollection<MessageType>();
            availableMessagesListBox.ItemsSource = availableMessages;
            getAvailableMessages();

        }

        private void getAvailableMessages()
        {
            availableMessages.Clear();
            foreach( MessageType message in messagesItems)
            {
                availableMessages.Add(message);
            }
        }

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

        private void refreshAvailableAndAttachedMessages(List<MessageType> attachedToTimerMessages)
        {
            //get available messages
            getAvailableMessages();

            //get attached messages
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

        private void generateCodeButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            string generatedFileName;
            if (fileNameTextBox.Text.Length > 0)
            {
                generatedFileName = fileNameTextBox.Text;
            }
            else
            {
                generatedFileName = "GEN_FILE_";
                if (selectedDbcFile.fileName != null && selectedDbcFile.fileName.Length > 0)
                {
                    generatedFileName += selectedDbcFile.fileName;
                }
            }

            //save in the generate file the generated file name
            generatedCaplFile.fileName = generatedFileName;

            string initialComment = "";
            //get the text from the  text box
            string textFromUi = initialCommentTextBox.Text;
            if (textFromUi.Length <= 0)
            {
                initialComment += "\n\n THIS IS A SINGLE LINE COMMENT\n\n";
            }
            else
            {
                initialComment += textFromUi;
            }

            if (selectedDbcFile.fileName != null)
            {
                if (messagesItems != null && messagesItems.Count > 0)
                {
                    CaplGenerator generatorInstance = new CaplGenerator(messagesItems.ToList(),msTimerItems.ToList(),initialComment);
                    generatedCaplFile.fileContent = generatorInstance.getResult();
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

            //generatedCaplFile.fileContent = generatedFileContent;
            this.Close();
        }

        private void cancelOperationButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result;
            result = MessageBox.Show("Are you sure you want to close the generation window? All changes will be lost!", 
                "Question", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.OK)
            {
                this.DialogResult = false;
                this.Close();
            }
            else
            {
                //the cancel operation is canceled
            }
        }

        private void onItemSelected(object sender, SelectionChangedEventArgs e)
        {
            //nothing
        }

        //select the dbc file
        private void dbcFileList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult result = MessageBoxResult.OK;        
            if (filesUsedForGeneration != null)
            {
                if (sender is ListViewItem listViewItem)
                {
                    string selectedItemFileName = listViewItem.DataContext.ToString();

                    //this message box will be shown if it is not for the first time a file is selected!
                    if (countFileSelectionChanged != 0)
                    {
                        //Reset to prevent overflow. As the condition is to be different from 0 the value 1 is a valid one.
                        countFileSelectionChanged = 1;
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
                        selectedDbcFile = filesUsedForGeneration.FirstOrDefault(item => item.fileName == selectedItemFileName);
                        if (selectedDbcFile != null)
                        {
                            MessageBox.Show("The file: {" + selectedDbcFile.fileName + "} stores: " + selectedDbcFile.getParsededObjects().messages.Count().ToString() + " messages!"
                                , "About", MessageBoxButton.OK, MessageBoxImage.Information);
                            displaySelectedDbcMessages();
                           
                            //try to refresh the available and attached messages lists
                            //when the dbc is changed
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

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (MessageType messageItem in messagesItems)
            {
                messageItem.OnMessage = true ;
                messageItem.NotifyPropertyChanged(nameof(MessageType.OnMessage));
            }
        }

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

        //permite doar litere si underscore
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

        private void AddNewTimer_Click(object sender, RoutedEventArgs e)
        {
            if (defaultTimerCheckBox.IsChecked==false)
            {

                string timerName = "";
                string timerPeriodString = "";
                UInt32 timerPeriod = MsTimerType.DEFAULT_PERIOD;
                timerName = timerNameBox.Text;
                timerPeriodString = timerPeriodBox.Text;

                bool sameNameExists = msTimerItems.Any(element => element.MsTimerName == timerName);
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

                        if (msTimerItems.Count >= MsTimerType.MAX_TIMER_NR)
                        {
                            MessageBox.Show("The timer has not been added!The maximum nunber of timers {"+
                                MsTimerType.MAX_TIMER_NR.ToString()+"} has been reached!", "Info",MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            //everything is OK => add a new timer
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
            {
                if (msTimerItems.Count >= MsTimerType.MAX_TIMER_NR)
                {
                    MessageBox.Show("The timer has not been added!The maximum nunber of timers {" +
                        MsTimerType.MAX_TIMER_NR.ToString() + "} has been reached!", "Info", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    //everything is OK => add a new timer
                    msTimerItems.Add(new MsTimerType());
                }
            }
        }

        private void UpdateTimer_Click(object sender, RoutedEventArgs e)
        {
            if (createdTimersView.SelectedItem != null)
            {
                MsTimerType selectedTimer = createdTimersView.SelectedItem as MsTimerType;
                if (selectedTimer != null && msTimerItems.Contains(selectedTimer))
                {
                    MessageBoxResult result = MessageBox.Show("Are you sure you want to update the timer?", "Confirmation",
                MessageBoxButton.OKCancel, MessageBoxImage.Question);

                    if (result == MessageBoxResult.OK)
                    {
                        string timerName = timerNameBox.Text;
                        string timerPeriodString = timerPeriodBox.Text;
                        UInt32 timerPeriod;

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
                        //disable the Update attached messages button if the deleted timer is the same
                        // as the selected one in the combo box
                        MsTimerType selectedTimerFromComboBox = (MsTimerType)selectedTimerComboBox.SelectedItem;
                        if (selectedTimerFromComboBox != null)
                        {
                            if(selectedTimerFromComboBox.MsTimerName==selectedTimer.MsTimerName)
                            {
                                updateAttachedMessages.IsEnabled = false;
                                //try to refresh the available and attached messages lists
                                //when the dbc is changed
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

        private void MoveToAttached_Click(object sender, RoutedEventArgs e)
        {
            foreach(var selectedMessage in availableMessagesListBox.SelectedItems.Cast<MessageType>().ToList())
            {
                availableMessages.Remove(selectedMessage);
                attachedMessages.Add(selectedMessage);
                updateAttachedMessages.IsEnabled = true;
            }
        }

        private void MoveToAvailable_Click(object sender, RoutedEventArgs e)
        {

            foreach (var selectedMessage in attachedMessagesListBox.SelectedItems.Cast<MessageType>().ToList())
            {
                attachedMessages.Remove(selectedMessage);
                availableMessages.Add(selectedMessage);
                updateAttachedMessages.IsEnabled = true;
            }
        }

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

        private void ViewMessageDetailsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (selectedMessagesView.SelectedItem != null)
                {
                    MessageType selectedMessage = (MessageType)selectedMessagesView.SelectedItem;
                    if (selectedMessage != null)
                    {
                        ViewMessageDetails messageDetailsWindow = new ViewMessageDetails(selectedMessage);

                        bool? returnStatus = messageDetailsWindow.ShowDialog();
                        if(returnStatus==true)
                        {
                            selectedMessage.OnKey=messageDetailsWindow.currentMessage.OnKey;
                            selectedMessage.OnMessage = messageDetailsWindow.currentMessage.OnMessage;
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
