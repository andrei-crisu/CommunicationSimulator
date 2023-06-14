
//
// Author: [Crisu Radu Andrei]
//
//
// IMPORTANT: This notice must not be removed from the code.
//


using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ComSimulatorApp.notifyUtilities;
using System.Windows.Media;

namespace ComSimulatorApp
{
    public partial class MainWindow : Window
    {
        public dbcParserCore.DBCFileObj openedDbcFile;

        public List<fileUtilities.FileTypeInterface> handeledFiles ;

        public ObservableCollection<NotificationMessage> appInternalErrorNotificationHistory;
        public ObservableCollection<NotificationMessage> appInternalWarningNotificationHistory;
        public ObservableCollection<NotificationMessage> appInternalMessageNotificationHistory;

        public MainWindow()
        {
            try
            {
                this.openedDbcFile = new dbcParserCore.DBCFileObj();
                handeledFiles = new List<fileUtilities.FileTypeInterface>();
                appInternalErrorNotificationHistory = new ObservableCollection<NotificationMessage>();
                appInternalWarningNotificationHistory = new ObservableCollection<NotificationMessage>();
                appInternalMessageNotificationHistory = new ObservableCollection<NotificationMessage>();

                InitializeComponent();
                DataContext = this;
                errorNotificationView.ItemsSource = appInternalErrorNotificationHistory;
                warningNotificationView.ItemsSource = appInternalWarningNotificationHistory;
                messageNotificationView.ItemsSource = appInternalMessageNotificationHistory;

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void opendbcMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                //Open File Dialog for .dbc files
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "CANdb Network (*.dbc)|*.dbc";
                if (openFileDialog.ShowDialog() == true)
                {
                    // citire continut fisier .dbc si stocare in variabila fileContent
                    string fileContent = File.ReadAllText(openFileDialog.FileName);
                    // Obtinere denumire fisier 
                    string fileName = Path.GetFileName(openFileDialog.FileName).ToLower();

                    //verificare daca fisierul este deschis deja
                    TabItem existingTabItem = caplViewTab.Items.OfType<TabItem>().FirstOrDefault(TabItem => TabItem.Header.ToString() == fileName);
                    if (existingTabItem!=null)
                    {
                        //este selectat tabul care corespunde fisierului ce se dorea deschis si este
                        //deja deschis
                        MessageBox.Show("File: \n { " + fileName + " }  is already open!","Info", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        caplViewTab.SelectedItem = existingTabItem;
                    }
                    else
                    {
                        // se creeaza un nou tab
                        TabItem newTabItem = new TabItem();
                        newTabItem.Header = fileName;

                        // se construieste continutul tabului
                        TextBox textBox = new TextBox();
                        //afisam in tab continutul fisierului deschis
                        textBox.Text = fileContent;
                        //setari textbox
                        textBox.AcceptsReturn = true;
                        textBox.TextWrapping = TextWrapping.Wrap;
                        textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                        textBox.IsReadOnly = true;
                        textBox.Background = new SolidColorBrush(Colors.LightYellow);

                        // se creeaza ScrollViewer care va contine textBox
                        ScrollViewer scrollViewer = new ScrollViewer();
                        scrollViewer.Content = textBox;

                        // Se seteza elementul scrollViewer drept continut pentru newTabItem
                        newTabItem.Content = scrollViewer;

                        // Se adauga noul tab la TabControl
                        caplViewTab.Items.Add(newTabItem);

                        // Selectare tab nou
                        caplViewTab.SelectedItem = newTabItem;



                        //parse data from the .dbc file
                        dbcParserCore.DBCParser parseInstance = new dbcParserCore.DBCParser();
                        parseInstance.parserLog.Add(new dbcParserCore.ParseStatusMessage("The parser started for file: " 
                            + fileName + "!",dbcParserCore.ParserConstants.ParserMsgTypes.INFO));
                        Boolean parseFileStatus= parseInstance.parseFile(fileContent);
                        if (parseFileStatus)
                        {
                            //create a new object of type DbcFile and add to the handeled files
                            openedDbcFile = parseInstance.getParsedResult();
                            //this is for debugging only (to ensure that .dbc file is parsed correctly) and should be removed/commented 
                            //displayParsedData(openedDbcFile);
                            //show parsed objects in a treeViewStructure
                            addDbcFileTreeViewStructure(dbcTreeView, fileName, openedDbcFile);

                            string[] components = fileName.Trim().Split(".");
                            string justFileName = fileName;
                            if(components.Length>=2)
                            {
                                justFileName = components[components.Length - 2];
                            }
                           
                            fileUtilities.DbcFile fileToOpen = new fileUtilities.DbcFile(justFileName, fileContent,
                                openFileDialog.FileName);
                            //in the future this two lines will be replaced with a method from the DbcFile class 
                            //that does this functionality

                            fileToOpen.setParsedObjects(openedDbcFile);
                            fileToOpen.fileLog = parseInstance.parserLog;

                            fileToOpen.fileNotificationHistory = parseInstance.getParserNotificationMessages();
                            //add file to the handeled files list
                            handeledFiles.Add(fileToOpen);
                        }
                        else
                        {
                            //file is corruputed and can't be parsed
                            //add a message to the parser log
                            parseInstance.parserLog.Add(new dbcParserCore.ParseStatusMessage("FILE CONTENT HAS SYNTAX ERRORS: \n FILE: " + 
                                fileName + " can't be parsed!", dbcParserCore.ParserConstants.ParserMsgTypes.ERROR));

                            parseInstance.RegisterNotificationMessage(new NotificationMessage(
                                NotificationNames.ERR_PARSE, "FILE CONTENT HAS SYNTAX ERRORS: \n FILE: " +
                                fileName + " can't be parsed!",
                                NotificationTypes.Error));
                            ;

                            //error window
                            MessageBox.Show("Error parsing the file: { " +fileName+" } \n File contains syntax errors!", 
                                "Parser Error", MessageBoxButton.OK, MessageBoxImage.Error);

                        }
                        //display all the parser notifications
                        displayParserNotificationHistory(parseInstance.getParserNotificationMessages());

                    }

                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void openCaplMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                //deschidere fisier .CAPL
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "CAPL Script (*.can)|*.can";
                if (openFileDialog.ShowDialog() == true)
                {
                    // citire continut fisier .capl si stocare in variabila fileContent
                    string fileContent = File.ReadAllText(openFileDialog.FileName);
                    // Obtinere denumire fisier 
                    string fileName = Path.GetFileName(openFileDialog.FileName);

                    //bool fileIsOpened = handeledFiles.Any(file => (file.fileName + "." + file.fileExtension) == fileName);
                
                    //verificare daca fisierul este deschis deja
                    TabItem existingTabItem = caplViewTab.Items.OfType<TabItem>().FirstOrDefault(TabItem => TabItem.Header.ToString().Trim() == fileName);
                    if (existingTabItem != null )
                    {
                        //este selectat tabul care corespunde fisierului ce se dorea deschis si este
                        //deja deschis
                        MessageBox.Show("File: \n { " + fileName + " }  is already open!", "Info",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                        caplViewTab.SelectedItem = existingTabItem;
                    }
                    else
                    {
                        // se creeaza un nou tab
                        TabItem newTabItem = new TabItem();
                        newTabItem.Header = fileName;

                        // se construieste continutul tabului
                        TextBox textBox = new TextBox();
                        //afisam in tab continutul fisierului deschis
                        textBox.Text = fileContent;
                        //setari textbox
                        textBox.AcceptsReturn = true;
                        textBox.TextWrapping = TextWrapping.Wrap;
                        textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                        // se creeaza ScrollViewer care va contine textBox
                        ScrollViewer scrollViewer = new ScrollViewer();
                        scrollViewer.Content = textBox;

                        // Se seteza elementul scrollViewer drept continut pentru newTabItem
                        newTabItem.Content = scrollViewer;

                        // Se adauga noul tab la TabControl
                        caplViewTab.Items.Add(newTabItem);

                        // Selectare tab nou
                        caplViewTab.SelectedItem = newTabItem;

                        // 
                        fileUtilities.CaplFile fileToOpen = new fileUtilities.CaplFile(fileName, fileContent);
                        handeledFiles.Add(fileToOpen);

                        //informare deschidere fisier CAPL
                        MessageBox.Show("FILE: { " + fileName + " } \n has been opened!",
                            "Open Info", MessageBoxButton.OK, MessageBoxImage.Information);

                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();

        }
        //salvare fisier ca  ( fisierul din tabul curent)
        private void saveAsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
            // Select the current tab
                if (caplViewTab.SelectedItem is TabItem selectedTabItem)
            {
                // Get the content of the selected tab
                var content = selectedTabItem.Content;
                string fileNameString = selectedTabItem.Header.ToString();
                string[] fileNameComponents = fileNameString.Split(".");
                string saveDialogFilter = "CANdb Network (*.dbc)|*.dbc";
                string fileNameToSave = "";
                if (fileNameComponents.Length < 2)
                {
                    MessageBox.Show("Tab name { " + fileNameString + " }  has no extension type associated to a file type!",
                        "Info",MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    string extensionType = fileNameComponents[1];
                    fileNameToSave = fileNameComponents[0];
                    switch (extensionType)
                    {
                        case "dbc":
                            saveDialogFilter = "CANdb Network (*.dbc)|*.dbc";
                            break;
                        case "can":
                            saveDialogFilter= "CAPL Script (*.CAN)|*.can";
                            break;
                        default:
                            saveDialogFilter = "CANdb Network (*.dbc)|*.dbc";
                            break;
                    }

                }

                // Determine the type of content and extract the text
                string textToSave = string.Empty;
                if (content is ScrollViewer scrollViewer && scrollViewer.Content is TextBox textBox)
                {
                    // Content is a ScrollViewer containing a TextBox
                    textToSave = textBox.Text;
                }
                // Add more cases for other types of content as needed

                // Proceed with saving the text
                if (!string.IsNullOrEmpty(textToSave))
                {
                    try
                    {
                        // Display a SaveFileDialog to let the user choose the file location
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.Filter = saveDialogFilter;
                        saveFileDialog.FileName = fileNameToSave;
                        if (saveFileDialog.ShowDialog() == true)
                        {
                            // Get the selected file path
                            string filePath = saveFileDialog.FileName;

                            // Write the text to the file
                            File.WriteAllText(filePath, textToSave);


                            //obtinere denumire fisier
                            string fileName = Path.GetFileName(filePath);

                                // informare salvare cu succes
                                MessageBox.Show("File: \n { " + fileName + " }  saved successfully!", "Info",
                                        MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    catch (Exception exception)
                    {
                        //gestionare exceptii la salvare fisier
                        MessageBox.Show($"Error saving file: {exception.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void closeTabFile()
        {
            try
            {
                if (caplViewTab.SelectedItem is TabItem selectedTabItem)
                {
                    string tabName = selectedTabItem.Header.ToString();
                    tabName.Trim();
                    //for .dbc files
                    if (tabName.EndsWith("dbc"))
                    {
                        closeTreeViewItem(dbcTreeView, tabName);
                    }
                    
                    //for .can files
                    if (tabName.EndsWith("can"))
                    {
                        closeTreeViewItem(caplFilesTreeView, tabName);
                    }
                    caplViewTab.Items.Remove(selectedTabItem);

                    handeledFiles.RemoveAll(file => (file.fileName + "." + file.fileExtension) == tabName);
                }
            }
            catch (Exception exception)
            {

                //gestionare exceptii la inchidere fisier
                MessageBox.Show($"Error closing file: {exception.Message}", "Close Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //inchidere fisier din tabul curent
        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //inchidere tab curent atunci cand se selecteaza 
            //actiunea close din meniul tabului
            closeTabFile();
        }

        private void closeAllMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                MessageBoxResult result = MessageBox.Show("Are you sure you want to close all files? Unsaved work will be lost!", "Confirmation",
                    MessageBoxButton.OKCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Cancel)
                {
                    appInternalWarningNotificationHistory.Add(new NotificationMessage(NotificationNames.WARNING_X002,
                     "The [CLOSE ALL FILE] operation was canceled!Please save any unsaved work!", NotificationTypes.Warning));
                }
                else
                {
                    caplViewTab.Items.Clear();
                    dbcTreeView.Items.Clear();
                    caplFilesTreeView.Items.Clear();
                    handeledFiles.Clear();
                    appInternalWarningNotificationHistory.Add(new NotificationMessage(NotificationNames.WARNING_X001,
                     "All tabs and associtated files have been closed!All unsaved work is lost!", NotificationTypes.Warning));

                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //
        private bool openGeneratedCaplFile(fileUtilities.CaplFile file)
        {
            try
            { 
                string fullFileName;
                if(file.fileName!=null && file.fileContent!=null)
                {
                    fullFileName = file.fileName + "." + file.fileExtension;

                    foreach (fileUtilities.FileTypeInterface filInList in handeledFiles)
                    {
                        if (fullFileName == filInList.getFullName())
                        {
                            string dateTimeString = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                            file.fileName = file.fileName + dateTimeString;
                            fullFileName = file.fileName + "." + file.fileExtension;

                        }
                    }



                    //open a new tab with the content
                    // se creeaza un nou tab
                    TabItem newTabItem = new TabItem();
                    newTabItem.Header = fullFileName;

                    // se construieste continutul tabului
                    TextBox textBox = new TextBox();
                    //afisam in tab continutul fisierului deschis
                    textBox.Text = file.fileContent;
                    //setari textbox
                    textBox.AcceptsReturn = true;
                    textBox.TextWrapping = TextWrapping.Wrap;
                    textBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

                    // se creeaza ScrollViewer care va contine textBox
                    ScrollViewer scrollViewer = new ScrollViewer();
                    scrollViewer.Content = textBox;

                    // Se seteza elementul scrollViewer drept continut pentru newTabItem
                    newTabItem.Content = scrollViewer;

                    // Se adauga noul tab la TabControl
                    caplViewTab.Items.Add(newTabItem);

                    // Selectare tab nou
                    caplViewTab.SelectedItem = newTabItem;

                
                    //informare deschidere tab CAPL
                   // AICI AS PUTEA ADAUGA UN MESAJ in log-ul de mesaje

                    //add file to the handeled file list
                    handeledFiles.Add(file);

                    //add file to right side
                    TreeViewItem fileParent = new TreeViewItem();
                    fileParent.Header = file.getFullName();
                    caplFilesTreeView.Items.Add(fileParent);

                }
                else
                {
                    return false;
                }
                return true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

        }

        private void CodeGeneration_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                List<fileUtilities.DbcFile> dbcFilesList = handeledFiles.OfType<fileUtilities.DbcFile>().ToList();
                if (dbcFilesList != null)
                {
                    if (dbcFilesList.Count != 0)
                    {
                        CodeGeneration codeGenerationWindow = new CodeGeneration(dbcFilesList);

                        bool? returnStatus = codeGenerationWindow.ShowDialog();
                        if (returnStatus == true)
                        {

                            bool openStatus = openGeneratedCaplFile(codeGenerationWindow.generatedCaplFile);
                            if (openStatus)
                            {
                                appInternalMessageNotificationHistory.Add(new NotificationMessage(NotificationNames.INFO_0003,
                               "The generated file {" + codeGenerationWindow.generatedCaplFile.getFullName() +
                               "} has been opened!", NotificationTypes.Information));
                            }
                            else
                            {
                                MessageBox.Show("Generation failed!No file has been generated", "CAPL File Errors",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                                appInternalErrorNotificationHistory.Add(new NotificationMessage(NotificationNames.ERR_0001,
                              "Generation failed!No file has been generated", NotificationTypes.Error));
                            }

                        }
                        else
                        {
                            appInternalWarningNotificationHistory.Add(new NotificationMessage(NotificationNames.WARNING_0001,
                                "Code generation operation was canceled! ", NotificationTypes.Warning));
                        }
                        appInternalWarningNotificationHistory.Add(new NotificationMessage(NotificationNames.WARNING_0002,
                            "Window Closed with status: {" + returnStatus.ToString() + "}", NotificationTypes.Warning));
                    }
                    else
                    {
                        MessageBox.Show("No dbc file provided!Please open a dbc file first!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

                    }

                }
                else
                {
                    MessageBox.Show("No dbc file found!Please open a dbc file first!", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //notification messages history
        private bool displayParserNotificationHistory(List<NotificationMessage> notificationHistory)
        {
            try
            { 
                int errCounter = 0;
                int warningCounter = 0;
                int infoCounter = 0;
                int otherCounter = 0;
                appInternalErrorNotificationHistory.Clear();
                appInternalWarningNotificationHistory.Clear();
                appInternalMessageNotificationHistory.Clear();
                //limit the number of displayed notification messages 
                //(if there are to many the app freeze )
                const int maxItemsToDisplay = 100;

                if (notificationHistory.Count > 0)
                {
                    NotificationTypes notificationType=new NotificationTypes();
                
                    foreach (NotificationMessage notification in notificationHistory)
                    {

                        notificationType = notification.Type;
                        switch (notificationType)
                        {

                            case NotificationTypes.Error:
                                errCounter++;
                                if (errCounter <= maxItemsToDisplay)
                                {
                                    appInternalErrorNotificationHistory.Add(notification);
                                }
                                break;

                            case NotificationTypes.Warning:
                                warningCounter++;
                                if (warningCounter <= maxItemsToDisplay)
                                {
                                    appInternalWarningNotificationHistory.Add(notification);
                                }
                                break;

                            case NotificationTypes.Information:
                                if(infoCounter<=2*maxItemsToDisplay)
                                {
                                    appInternalMessageNotificationHistory.Add(notification);
                                }
                                infoCounter++;
                                break;

                            case NotificationTypes.Other:
                                otherCounter++;
                                break;

                            default:
                                break;
                        }


                    }
                    //notificationErrorScrollViewer.ScrollToTop();
                    //notificationWarningScrollViewer.ScrollToTop();

                }
                string notificationString = "";
                notificationString += "Parsing status: ";
                notificationString += "[ERRORS]: " + errCounter.ToString() + "; ";
                notificationString += "[WARNINGS]: " + warningCounter.ToString() + "; ";
                notificationString += "[INFORMATIONS]: " + infoCounter.ToString() + "; ";
                notificationString += "[OTHER NOTIFICATIONS]:" + otherCounter.ToString() + "; ";
                MessageBox.Show(notificationString, "Parser Notification", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }
        }

        //display parsed data
        private void displayParsedData(dbcParserCore.DBCFileObj file)
        {
            try
            {
                int msgNumber, nodedsNumber, signalsNumber;
                msgNumber = nodedsNumber = signalsNumber = 0;
                nodedsNumber = file.nodes.Count();
                msgNumber = file.messages.Count();

                appInternalMessageNotificationHistory.Add(new NotificationMessage(NotificationNames.INFO_0001,
                       "%\t ALL PARSED NODES: ", NotificationTypes.Information));

                foreach (dbcParserCore.Node node in file.nodes)
                {
                    appInternalMessageNotificationHistory.Add(new NotificationMessage(NotificationNames.INFO_0001,
                      node.ToString(), NotificationTypes.Information));
                }

                foreach (dbcParserCore.Message message in file.messages)
                {
                    appInternalMessageNotificationHistory.Add(new NotificationMessage(NotificationNames.INFO_0001,
                      message.messageToString(" | ", " ", "\n", "\t\t"), NotificationTypes.Information));
                    signalsNumber += message.signals.Count();
                }

                appInternalMessageNotificationHistory.Add(new NotificationMessage(NotificationNames.INFO_0001,
                      "\nSTATUS:\n  Parsed nodes: " + nodedsNumber.ToString() + "\n  Parsed messages: " + msgNumber.ToString() +
                    "\n  Parsed signals (in messages): " + signalsNumber.ToString() + "\n", NotificationTypes.Information));
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        private void clearErrorViewButton_Click(object sender, RoutedEventArgs e)
        {
            appInternalErrorNotificationHistory.Clear();
        }

        private void copyErrorViewButton_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var item in appInternalErrorNotificationHistory)
                {
                    stringBuilder.AppendLine(item.MessageNotificationToString());

                }

                string clipboardText = stringBuilder.ToString();

                if (!string.IsNullOrEmpty(clipboardText))
                {
                    Clipboard.SetText(clipboardText);
                    MessageBox.Show("Copied to clipboard!", "Clipboard", MessageBoxButton.OK, MessageBoxImage.Information);

                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void clearWarningViewButton_Click(object sender, RoutedEventArgs e)
        {
            appInternalWarningNotificationHistory.Clear();
        }

        private void copyWarningViewButton_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var item in appInternalWarningNotificationHistory)
                {
                    stringBuilder.AppendLine(item.MessageNotificationToString());

                }

                string clipboardText = stringBuilder.ToString();

                if (!string.IsNullOrEmpty(clipboardText))
                {
                    Clipboard.SetText(clipboardText);
                    MessageBox.Show("Copied to clipboard!", "Clipboard", MessageBoxButton.OK, MessageBoxImage.Information);

                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void clearMessageViewButton_Click(object sender, RoutedEventArgs e)
        {
            appInternalMessageNotificationHistory.Clear();
        }

        private void copyMessageViewButton_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                StringBuilder stringBuilder = new StringBuilder();
                foreach (var item in appInternalMessageNotificationHistory)
                {
                    stringBuilder.AppendLine(item.MessageNotificationToString());


                }

                string clipboardText = stringBuilder.ToString();

                if (!string.IsNullOrEmpty(clipboardText))
                {
                    Clipboard.SetText(clipboardText);
                    MessageBox.Show("Copied to clipboard!", "Clipboard", MessageBoxButton.OK, MessageBoxImage.Information);

                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        private void MenuItem_getCurrentDbcItem_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
                {
                    TreeViewItem treeViewItem = contextMenu.PlacementTarget as TreeViewItem;
                    string itemNames = GetSelectedDescendantsNames(treeViewItem);

                    //otherListView.Items.Add("SelectedItems: " + itemNames);
                    MessageBox.Show(itemNames, "Selected elements", MessageBoxButton.OK, MessageBoxImage.Information);

                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private string GetSelectedDescendantsNames(TreeViewItem treeViewItem, bool isSelectedParent = false)
        {
            StringBuilder descendatNames = new StringBuilder();
            bool isSelected = isSelectedParent || treeViewItem.IsSelected;

            //adauga numele elementului intre acolade daca este selectat
            if(isSelected)
            {
                descendatNames.Append("{").Append(treeViewItem.Header.ToString()).Append("}\n");
            }
            else
            {
                //daca se doreste afisarea denumirilor pentru elementele neselectate
                //descendatNames.Append(treeViewItem.Header.ToString()).Append(" ");
            }

            // parcurgere elemente copil
            foreach(TreeViewItem childItem in treeViewItem.Items)
            {
                //apel recursiv al acestei functii pentru fiecare copil
                descendatNames.Append(GetSelectedDescendantsNames(childItem, isSelected));
            }

            return descendatNames.ToString();
        }

        private void addDbcFileTreeViewStructure(TreeView view,string fileName, dbcParserCore.DBCFileObj file)
        {
            try
            { 
                TreeViewItem fileParent = new TreeViewItem();
                fileParent.Header = fileName;
                fileParent.FontWeight = FontWeights.SemiBold;

                //adauga nodurile care au fost extrase
                TreeViewItem nodesParent = new TreeViewItem();
                nodesParent.Header = "Nodes";
                foreach(dbcParserCore.Node node in file.nodes)
                {
                    TreeViewItem nodeItem = new TreeViewItem();
                    nodeItem.Header =node.getName();
                    nodesParent.Items.Add(nodeItem);
                }
                fileParent.Items.Add(nodesParent);


                //adauga mesajele
                TreeViewItem messagesParent = new TreeViewItem();
                messagesParent.Header = "Messages";
                foreach(dbcParserCore.Message message in file.messages)
                {
                    TreeViewItem messageItem = new TreeViewItem();
                    string hexCanId = " (0x" + message.getCanId().ToString("X")+")";
                    messageItem.Header = message.getMessageName()+hexCanId;
                    messagesParent.Items.Add(messageItem);
                }
                fileParent.Items.Add(messagesParent);

                view.Items.Add(fileParent);
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //inchide tabul din cadrul dbcTreeView care are denumiea item StringName
        private void closeTreeViewItem(TreeView view,string itemStringName)
        {
            try
            { 
                TreeViewItem selectedItem = findTreeViewItem(view.Items, itemStringName);
                if (selectedItem != null)
                {
                    ItemsControl parentControl = ItemsControl.ItemsControlFromItemContainer(selectedItem);
                    if(parentControl!=null)
                    {
                        //elimina elementul care este un copil ( mai are parinti)
                        parentControl.Items.Remove(selectedItem);
                    }
                    else
                    {
                        //elimina elementul daca e de tip top level ( adica nu mai are parinti)
                        view.Items.Remove(selectedItem);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }

        //functia cauta elementul in lista de taburi (items) care are denumirea itemStringName
        //daca il gaseste il returneaza , iar in caza contrar returneaza null
        private TreeViewItem findTreeViewItem(ItemCollection items, string itemStringName)
        {
            foreach (var item in items)
            {
                if(item is TreeViewItem treeViewItem)
                {
                    if(treeViewItem.Header.ToString()==itemStringName)
                    {
                        return treeViewItem;
                    }

                    //se cauta recursiv denumirile pentru elementele copil ale acestui element
                    var result = findTreeViewItem(treeViewItem.Items, itemStringName);
                    if(result!=null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string aboutString = "";
            aboutString+="Author: Crisu Radu Andrei\n";
            aboutString += "App: Communication Simulator\n";
            aboutString += "Target Framework: .NET 5.0  | C#(WPF)\n";
            aboutString += "Description: Designed to generate CAPL script code based on the messages from a .dbc file!\n";
            MessageBox.Show(aboutString,"About", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        private void infoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                AboutAppWindow aboutWindow = new AboutAppWindow();
                bool? returnStatus = aboutWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                MessageBox.Show("This is a final year project!", "About", MessageBoxButton.OK, MessageBoxImage.Information);

            }
        }

        private void MenuItem_caplFileDetails_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender is MenuItem menuItem && menuItem.Parent is ContextMenu contextMenu)
                {
                    TreeViewItem selectedCaplFileItem = caplFilesTreeView.SelectedItem as TreeViewItem;
                    if(selectedCaplFileItem != null)
                    {
                        string itemName = selectedCaplFileItem.Header.ToString();

                        MessageBox.Show("Capl file name:  " + itemName, "Info:", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void launchCANalyzerToolButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void launchTool_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TreeViewItem selectedCaplFileItem = caplFilesTreeView.SelectedItem as TreeViewItem;
                fileUtilities.CaplFile usedCaplFile = null;
                if (selectedCaplFileItem != null)
                {
                    string itemName = selectedCaplFileItem.Header.ToString();

                    //MessageBox.Show("Capl file name:  " + itemName, "Info:", MessageBoxButton.OK, MessageBoxImage.Information);

                    foreach(fileUtilities.FileTypeInterface fileItem in handeledFiles)
                    {
                        fileUtilities.CaplFile caplFileItem = fileItem as fileUtilities.CaplFile;
                        if(caplFileItem!=null)
                        {
                           
                            if(caplFileItem.getFullName().Equals(itemName))
                            {
                                usedCaplFile = caplFileItem;
                                break;
                            }
                        }
                    }
                    if(usedCaplFile!= null)
                    {
                        if(usedCaplFile.filePath!=null)
                        {
                            //This will open the CANalyzer configuration window!
                            CANalyzerConfigurationView CANalyzerLaunchWindow = new CANalyzerConfigurationView(null,usedCaplFile.filePath);
                            bool? returnStatus = CANalyzerLaunchWindow.ShowDialog();
                        }
                        else
                        {
                            SaveFileDialog saveFileDialog = new SaveFileDialog();
                            saveFileDialog.Filter = "CAPL Script (*.can)|*.can";
                            saveFileDialog.Title = "Save CAPL file";
                            saveFileDialog.FileName =usedCaplFile.fileName;

                            if (!string.IsNullOrEmpty(usedCaplFile.fileContent))
                            {
                                try
                                {
                                    // Display a SaveFileDialog to let the user choose the file location

                                    if (saveFileDialog.ShowDialog() == true)
                                    {
                                        // Get the selected file path
                                        string filePath = saveFileDialog.FileName;

                                        // Write the text to the file
                                        File.WriteAllText(filePath, usedCaplFile.fileContent);

                                        //obtinere denumire fisier
                                        string fileName = Path.GetFileName(filePath);

                                        // informare salvare cu succes
                                        MessageBox.Show("File: \n { " + fileName + " }  saved successfully!", "Info",
                                                    MessageBoxButton.OK, MessageBoxImage.Information);

                                        selectedCaplFileItem.Header = fileName;

                                        usedCaplFile.filePath = filePath;
                                        string[] nameComponenents = fileName.Split(".");
                                        if (nameComponenents.Length==2)
                                        {
                                            usedCaplFile.fileName = nameComponenents[0];

                                        }
                                        //This will open the CANalyzer configuration window!
                                        CANalyzerConfigurationView CANalyzerLaunchWindow = new CANalyzerConfigurationView(null, usedCaplFile.filePath);
                                        bool? returnStatus = CANalyzerLaunchWindow.ShowDialog();
                                    }
                                }
                                catch (Exception exception)
                                {
                                    //gestionare exceptii la salvare fisier
                                    MessageBox.Show($"Error saving file: {exception.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }

                        }

                    }
                    else
                    {
                        MessageBox.Show("No stored file found that corresponds to the selected name!  ", "App error!", MessageBoxButton.OK, MessageBoxImage.Error);
                    }


                }
                else
                {
                    MessageBox.Show("Select a CAPL file first!", "Info!", MessageBoxButton.OK, MessageBoxImage.Exclamation);

                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
