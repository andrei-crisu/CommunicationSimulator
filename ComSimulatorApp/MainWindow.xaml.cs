
//
// Author: [Crisu Radu Andrei]
// Data : 27.06.2023
// Aceasta aplicatie a fost realizata pentru proiectul de licenta
// Facultatea: Automatica,Calculatoare si Electronica 
// Universitatea din Craiova
//
// Titlul temei: CREAREA AUTOMATĂ A UNUI SIMULATOR DE COMUNICAȚIE ÎN AUTOMOTIVE 
//
// 
// Aplicatia isi propune sa genereze automat cod CAPL sub forma de fisiere .can 
// pe baza unui fisier DBC si a unor reguli de generare
// 
//
// IMPORTANT: Acest mesaj nu trebui sa fie eliminat din fisier.
//
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
        // membru ce stocheza fisierul dbc deschis
        // adica elementele interpretate
        // intr-un viitor update acesta va fi eliminat
        // initial se presupunea ca aplicatia va gestiona un singur fisier
        // ulterior, ca urmare a complexitatii crescute,
        // a fost necesara implementarea unei liste care sa gestioneze mai multe fisiere
      
        public dbcParserCore.DBCFileObj openedDbcFile;

        public List<fileUtilities.FileTypeInterface> handeledFiles ;
        //colectii pentru gestiunea mesajelor de eroare, de avertizarea si a notificarilor generale
        public ObservableCollection<NotificationMessage> appInternalErrorNotificationHistory;
        public ObservableCollection<NotificationMessage> appInternalWarningNotificationHistory;
        public ObservableCollection<NotificationMessage> appInternalMessageNotificationHistory;

        //constructorul ferestrei principale (clasa MainWindow)
        public MainWindow()
        {
            try
            {
                // instantiere tutoror membrilor 
                this.openedDbcFile = new dbcParserCore.DBCFileObj();
                handeledFiles = new List<fileUtilities.FileTypeInterface>();
                appInternalErrorNotificationHistory = new ObservableCollection<NotificationMessage>();
                appInternalWarningNotificationHistory = new ObservableCollection<NotificationMessage>();
                appInternalMessageNotificationHistory = new ObservableCollection<NotificationMessage>();

                InitializeComponent();
                DataContext = this;
                //legarea colectiilor pentru mesajele de notificare de interfata grafica
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
                //fereastra pentru selectarea si deschiderea fisierului DBC
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



                        //interpretare date din fisierul DBC
                        dbcParserCore.DBCParser parseInstance = new dbcParserCore.DBCParser();
                        parseInstance.parserLog.Add(new dbcParserCore.ParseStatusMessage("The parser started for file: " 
                            + fileName + "!",dbcParserCore.ParserConstants.ParserMsgTypes.INFO));
                        Boolean parseFileStatus= parseInstance.parseFile(fileContent);
                        if (parseFileStatus)
                        {
                            //se obtine rezultatul intepretarii
                            openedDbcFile = parseInstance.getParsedResult();
                            //utilizat doar pentru partea de depanare 
                            //displayParsedData(openedDbcFile);
                            //afisarea obiectelor extrase ( noduri si mesaje) intr-o structura de tip arbore
                            addDbcFileTreeViewStructure(dbcTreeView, fileName, openedDbcFile);

                            string[] components = fileName.Trim().Split(".");
                            string justFileName = fileName;
                            if(components.Length>=2)
                            {
                                justFileName = components[components.Length - 2];
                            }
                           
                            fileUtilities.DbcFile fileToOpen = new fileUtilities.DbcFile(justFileName, fileContent,
                                openFileDialog.FileName);

                            fileToOpen.setParsedObjects(openedDbcFile);
                            fileToOpen.fileLog = parseInstance.parserLog;

                            fileToOpen.fileNotificationHistory = parseInstance.getParserNotificationMessages();
                            //adauga fisierul deschis in lista fisierelor gestionate
                            handeledFiles.Add(fileToOpen);
                        }
                        else
                        {
                            //fisierul are informatia corupta
                            // nu poate fi intrepretat => se semnaleaza eroarea
                            parseInstance.parserLog.Add(new dbcParserCore.ParseStatusMessage("FILE CONTENT HAS SYNTAX ERRORS: \n FILE: " + 
                                fileName + " can't be parsed!", dbcParserCore.ParserConstants.ParserMsgTypes.ERROR));

                            parseInstance.RegisterNotificationMessage(new NotificationMessage(
                                NotificationNames.ERR_PARSE, "FILE CONTENT HAS SYNTAX ERRORS: \n FILE: " +
                                fileName + " can't be parsed!",
                                NotificationTypes.Error));
                            ;

                            //se afiseaza o fereastra de eroare
                            MessageBox.Show("Error parsing the file: { " +fileName+" } \n File contains syntax errors!", 
                                "Parser Error", MessageBoxButton.OK, MessageBoxImage.Error);

                        }
                        //afisarea tuturor mesajelor de notificare
                        displayParserNotificationHistory(parseInstance.getParserNotificationMessages());

                    }

                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Exception caught!", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        //deschiderea unui fisier CAPL
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

        //inchidere aplicatie
        private void exitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Close();

        }
        //Save as - salvearea fisierului selectat (fisierul din tabul curent)
        private void saveAsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
            // se obtine tab-ul selectat
                if (caplViewTab.SelectedItem is TabItem selectedTabItem)
            {
                // se obtine continutul tab-ului selectat
                var content = selectedTabItem.Content;
                string fileNameString = selectedTabItem.Header.ToString();
                string[] fileNameComponents = fileNameString.Split(".");
                string saveDialogFilter = "CANdb Network (*.dbc)|*.dbc";
                string fileNameToSave = "";
                 //se verifica numele tabului in vederea extragerii numelui fisierului
                if (fileNameComponents.Length < 2)
                {
                    MessageBox.Show("Tab name { " + fileNameString + " }  has no extension type associated to a file type!",
                        "Info",MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    //in functie de tipul extensiei se seteaza filtrul pentru fereastra de salvare
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

                // Extragerea continutului  din cadrul tab-ului
                string textToSave = string.Empty;
                if (content is ScrollViewer scrollViewer && scrollViewer.Content is TextBox textBox)
                {
                    //Tab-ul contine un scroll viewer ce contine un obiect caseta text
                    //se extrage textul din acea caseta text
                    textToSave = textBox.Text;
                }

                // Se salveaza textul
                if (!string.IsNullOrEmpty(textToSave))
                {
                    try
                    {
                        //Afisarea ferestrei de salvare pentru ca utilizatorul sa selecteze locatia si denumirea
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.Filter = saveDialogFilter;
                        saveFileDialog.FileName = fileNameToSave;
                        //daca fereastra returneaza true
                        if (saveFileDialog.ShowDialog() == true)
                        {
                            // Se obtine calea fisierului salvat
                            string filePath = saveFileDialog.FileName;

                            //se scrie in fisier continutul
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

        //inchiderea unui tab
        private void closeTabFile()
        {
            try
            {
                if (caplViewTab.SelectedItem is TabItem selectedTabItem)
                {
                    string tabName = selectedTabItem.Header.ToString();
                    tabName.Trim();
                    //pentru fisiere .dbc 
                    if (tabName.EndsWith("dbc"))
                    {
                        closeTreeViewItem(dbcTreeView, tabName);
                    }
                    
                    //pentru fisiere .can 
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

        //inchiderea tuturor taburilor, nu se salveaza automat continutul
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

        //deschiderea fisierului CAPL generat
        private bool openGeneratedCaplFile(fileUtilities.CaplFile file)
        {
            try
            { 
                string fullFileName;
                if(file.fileName!=null && file.fileContent!=null)
                {
                    fullFileName = file.fileName + "." + file.fileExtension;
                    //daca denumirea fisierului exista in lista
                    //se va adauga un fisier cu denumirea respectiva la care se adauga data si ora
                    foreach (fileUtilities.FileTypeInterface filInList in handeledFiles)
                    {
                        if (fullFileName == filInList.getFullName())
                        {
                            string dateTimeString = DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss");
                            file.fileName = file.fileName + dateTimeString;
                            fullFileName = file.fileName + "." + file.fileExtension;

                        }
                    }
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

                    //adaugarea fisierului in lista de fisiere deschise
                    handeledFiles.Add(file);

                    //se adauga fisierul in partea dreapta
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

        //aceasta metoda se executa
        //atunci cand se apasa butonul de generare de cod
        //si are rolul de a lansa fereastra in care se specifica optiunile pentru generarea de cod
        private void CodeGeneration_Click(object sender, RoutedEventArgs e)
        {
            try
            { 
                //Din intreaga lista de fisiere deschise se obtin doar fisierele DBC
                List<fileUtilities.DbcFile> dbcFilesList = handeledFiles.OfType<fileUtilities.DbcFile>().ToList();
                //daca rezultatul este cu succes si exista astfel de fisiere
                if (dbcFilesList != null)
                {
                    if (dbcFilesList.Count != 0)
                    {
                        //se instantiaza un obiect de tipul ferestrei pentru generarea de cod
                        CodeGeneration codeGenerationWindow = new CodeGeneration(dbcFilesList);
                        //se afiseaza fereastra
                        bool? returnStatus = codeGenerationWindow.ShowDialog();
                        //dupa ce fereastra este inchisa
                        //daca statusul este adevarat
                        if (returnStatus == true)
                        {
                            //se incearca deschiderea fisierului CAPL (.can) generat
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

        //afisarea listei de notificari interne
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
                //se limieteaza numarul de notificari care pot fi afisate
                //daca sunt prea multe aplicatia se blocheaza
                const int maxItemsToDisplay = 100;

                if (notificationHistory.Count > 0)
                {
                    //se instantiaza o variabila auxiliara
                    NotificationTypes notificationType=new NotificationTypes();
                    //pentru fiecare notificare din lista
                    foreach (NotificationMessage notification in notificationHistory)
                    {
                        //se obtine tipul notificarii
                        notificationType = notification.Type;
                        //in functie de tipul acesteia 
                        //este adaugata in fereastra corespunzatoare
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
                }
                //se afieaza un mesaj intr-o fereastra care indica cate notificari din fiecare
                //tip au aparut ca urmare a interpretarii fisierului DBC
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

        //metoda poate fi utilizata pentru a afisa informatia extrasa din fisierul DBC
        //in acest moment nu este folosita
        //a fost folosita in procesul de depanare
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

        //metode utilizate pentru stergerea notificarilor interne din ferestre 
        // dar si pentru copierea continutului sub forma de text
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


        //pentru a afisa informatii despre fisierul dbc selectat in partea stanga
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

        //metoda este utilizata pentru a adauga 
        //fisierul dbc in lista de fisier DBC din partea stanga
        //in acea sectiune sunt afisate fisierele dbc parsate corect
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

        //afisarea informatii despre aplicatie
        private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            string aboutString = "";
            aboutString+="Author: Crisu Radu Andrei\n";
            aboutString += "App: Communication Simulator\n";
            aboutString += "Target Framework: .NET 5.0  | C#(WPF)\n";
            aboutString += "Description: Designed to generate CAPL script code based on the messages from a .dbc file!\n";
            MessageBox.Show(aboutString,"About", MessageBoxButton.OK, MessageBoxImage.Information);

        }

        //afiseaza informatii despre aplicatie 
        //precum si atribuie munca autorilor pentru  pictogramele utilizate de GUI
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

        //afiseaza informatii despre fiserul CAPL 
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

        //metoda se exeucta atunci cand se apasa butonul
        // launch tool
        // si este utilizat pentru a deschide o ferestra din care 
        // poate fi pornita aplicatia CANalyzer
        private void launchTool_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TreeViewItem selectedCaplFileItem = caplFilesTreeView.SelectedItem as TreeViewItem;
                fileUtilities.CaplFile usedCaplFile = null;
                if (selectedCaplFileItem != null)
                {
                    string itemName = selectedCaplFileItem.Header.ToString();
                    //se cauta fisierul CAPL selectat
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
                    if (usedCaplFile!= null)
                    {
                        //Se obtine calea fisierului CAPL 
                        //daca daca calea fisierului este in regula se deschide ferestra
                        //altfel se solicata salvarea fisierului
                        if (usedCaplFile.filePath!=null)
                        {
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
                                    //Afisarea ferestrei dialog pentru salvare
                                    if (saveFileDialog.ShowDialog() == true)
                                    {
                                        //Se obtine calea
                                        string filePath = saveFileDialog.FileName;

                                        //Se scrie continutul in fisier
                                        File.WriteAllText(filePath, usedCaplFile.fileContent);

                                        //se obtine denumierea fisierului
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
                                        //Se deschide ferestra pentru pornirea aplicatiei CANalyzer
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
