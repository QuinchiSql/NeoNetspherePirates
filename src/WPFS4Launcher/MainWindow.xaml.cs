using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using TestServer_Launcher.LoginAPI;

namespace WPFS4Launcher
{
    /// <summary>
    ///     Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        internal string AuthCode = "";
        public byte ButtonState;

        internal bool IsKoreClient;

        public MainWindow()
        {
            InitializeComponent();
            ResetNotice();
            Reset();
            ButtonTx.Text = "CHECKING";
            CheckUpdates();
        }

        public void CheckUpdates()
        {
            ButtonTx.Text = "PATCHING";
            //code
            UpdateFinished();
        }

        public void UpdateFinished()
        {
            ButtonState = 1;
            UpdateButton();
            dbgtx.Content = "Ready for Authentication.";
            ButtonTx.Text = "    LOGIN";
        }

        public void Ready(string code, bool isKoreaClient)
        {
            Dispatcher.Invoke(() =>
            {
                AuthCode = code;
                IsKoreClient = isKoreaClient;
                ButtonTx.Text = "    START";
                ButtonState = 2;
                UpdateButton();
            });
        }

        public void UpdateLabel(string message)
        {
            Dispatcher.Invoke(() => { dbgtx.Content = message; });
        }

        public void UpdateErrorLabel(string message)
        {
            Dispatcher.Invoke(() => { errtx_label.Content = message; });
        }

        public string GetUsername()
        {
            return Dispatcher.Invoke(() => { return login_username.Text; });
        }

        public string GetPassword()
        {
            return Dispatcher.Invoke(() => { return login_passwd.Password; });
        }

        public Label AddNotice(string message)
        {
            var noticemessage = new Label();
            noticemessage.Foreground = Brushes.White;
            noticemessage.Content = "Addlog | News | Future";
            var subheader = new Separator();
            NoticeBox.Items.Add(noticemessage);
            NoticeBox.Items.Add(subheader);

            return noticemessage; //for special editing
        }

        public void ResetNotice()
        {
            NoticeBox.Items.Clear();
            AddNotice("Addlog | News | Future").FontWeight = FontWeights.Bold;
        }

        public void Reset()
        {
            Constants.LoginWindow = this;
            Dispatcher.Invoke(() =>
            {
                ButtonTx.Text = "    LOGIN";
                errtx_label.Content = "";
                dbgtx.Content = "Ready for Authentication.";
                login_passwd.IsEnabled = true;
                login_username.IsEnabled = true;
            });
        }

        public void UpdateButton()
        {
            if (ButtonState == 0)
                Button.Source = new BitmapImage(new Uri("Res/btn_login_patch.png", UriKind.Relative));
            else if (ButtonState == 1)
                Button.Source = new BitmapImage(new Uri("Res/btn_login.png", UriKind.Relative));
            else if (ButtonState == 2)
                Button.Source = new BitmapImage(new Uri("Res/btn_login_start.png", UriKind.Relative));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void Frame_Navigated(object sender, NavigationEventArgs e)
        {
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ButtonState == 1)
            {
                errtx_label.Content = "";

                if (login_passwd.Password.Length < 6 || login_username.Text.Length < 6)
                {
                    errtx_label.Content = "Account information must have atleast 6 chars!";
                }
                else
                {
                    ButtonTx.Text = " LOGGING";
                    login_passwd.IsEnabled = false;
                    login_username.IsEnabled = false;
                    Task.Run(() => LoginClient.Connect(Constants.ConnectEndPoint));
                }
            }
            else if (ButtonState == 2)
            {
                try
                {
                    Process.Start("s4client.exe",
                        $"-rc:eu -lac:eng -auth_server_ip:{Constants.ConnectEndPoint.Address} -aeria_acc_code:{AuthCode}");
                    Environment.Exit(-1);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to execute S4Client.exe", "Error");
                }
            }

            UpdateButton();
        }
    }
}
