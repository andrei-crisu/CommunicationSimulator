
using System;
using System.Windows;


namespace ComSimulatorApp
{
    public partial class AboutAppWindow : Window
    {
        public AboutAppWindow()
        {
            InitializeComponent();

            try
            {
                string attributionText =
"Error icons created by juicy_fish - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/error](https://www.flaticon.com/free-icons/error)\n\n" +

"Info icons created by Chanut - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/info](https://www.flaticon.com/free-icons/info)\n\n" +

"Email icons created by Freepik - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/email](https://www.flaticon.com/free-icons/email)\n\n" +

"Question icons created by Freepik - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/question](https://www.flaticon.com/free-icons/question)\n\n" +

"Close icons created by Mayor Icons - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/close](https://www.flaticon.com/free-icons/close)\n\n" +

"Code icons created by Freepik - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/code](https://www.flaticon.com/free-icons/code)\n\n" +

"Save icons created by Freepik - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/save](https://www.flaticon.com/free-icons/save)\n\n" +

"Warning icons created by Good Ware - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/warning](https://www.flaticon.com/free-icons/warning)\n\n" +

"Info icons created by Plastic Donut - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/info](https://www.flaticon.com/free-icons/info)\n\n" +

"Machine learning icons created by Becris - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/machine-learning](https://www.flaticon.com/free-icons/machine-learning)\n\n" +

"Google-plus icons created by Pixel perfect - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/google-plus](https://www.flaticon.com/free-icons/google-plus)\n\n" +

"Email icons created by Freepik - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/email](https://www.flaticon.com/free-icons/email)\n\n" +

"Delete icons created by IYAHICON - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/delete](https://www.flaticon.com/free-icons/delete)\n\n" +

"Timer icons created by fjstudio - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/timer](https://www.flaticon.com/free-icons/timer)\n\n" +

"Heart icons created by Freepik - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/heart](https://www.flaticon.com/free-icons/heart)\n\n" +

"Heart icons created by Freepik - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/heart](https://www.flaticon.com/free-icons/heart)\n\n" +

"Select icons created by Icon Hubs - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/select](https://www.flaticon.com/free-icons/select)\n\n" +

"Unchecked icons created by Chanut-is-Industries - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/unchecked](https://www.flaticon.com/free-icons/unchecked)\n\n" +

"Create icons created by Tempo_doloe - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/create](https://www.flaticon.com/free-icons/create)\n\n" +

"Clear icons created by Pixel perfect - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/clear](https://www.flaticon.com/free-icons/clear)\n\n" +

"Close icons created by Pixel perfect - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/close](https://www.flaticon.com/free-icons/close)\n\n" +

"Comment icons created by Freepik - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/comment](https://www.flaticon.com/free-icons/comment)\n\n" +

"Copy icons created by Pixel perfect - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/copy](https://www.flaticon.com/free-icons/copy)\n\n" +

"Arrow icons created by Freepik - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/arrow](https://www.flaticon.com/free-icons/arrow)\n\n" +

"Efficiency icons created by Uniconlabs - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/efficiency](https://www.flaticon.com/free-icons/efficiency)\n\n" +

"Timer icons created by Gregor Cresnar Premium - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/timer](https://www.flaticon.com/free-icons/timer)\n\n" +

"Dice icons created by bearicons - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/dice](https://www.flaticon.com/free-icons/dice)\n\n" +

"Dice icons created by Stockio - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/dice](https://www.flaticon.com/free-icons/dice)\n\n" +

"Dice icons created by Tanah Basah - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/dice](https://www.flaticon.com/free-icons/dice)\n\n" +

"Settings icons created by Freepik - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/settings](https://www.flaticon.com/free-icons/settings)\n\n" +

"Sine icons created by Freepik - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/sine](https://www.flaticon.com/free-icons/sine)\n\n" +

"Heart icons created by SeyfDesigner - Flaticon\n" +
"Link: [https://www.flaticon.com/free-icons/heart](https://www.flaticon.com/free-icons/heart)";

                attributionsTextBox.Text = attributionText;
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }
            
        }

    }
}
