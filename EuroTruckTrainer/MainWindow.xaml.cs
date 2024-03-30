using System.Windows;
using System.Globalization;

namespace EuroTruckTrainer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static string version = "v1.0.0, Developed on ETS2 Version \"1.49.2.23s(64-bit)\"";


        public bool IsConnected => trainer != null;

        private EuroTrainer? trainer;

        public MainWindow()
        {
            InitializeComponent();

            txtVersion.Text = version;
            btnNegBalance.IsEnabled = false;
            SetBtnVisibility();
        }

        private uint _number;
        public uint Number
        {
            get { return _number; }
            set
            {
                try
                {
                    trainer?.SetMoney(value);
                    trainer?.GetMoney();

                    _number = trainer!.Money;
                    txtNum.Text = _number.ToString("N", CultureInfo.CurrentCulture) + "€";
                    SetBtnVisibility();
                }
                catch (Exception ex)
                {
                    ThrowError(ex);
                }
            }
        }

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsConnected) return;
                trainer = new EuroTrainer();
                SetBtnVisibility();
            }
            catch
            {
                MessageBox.Show(this, "Game not found. Please start the game and try again.", "Error");
            }
            StartUpdateProcessAsync().ConfigureAwait(false);
        }

        private async Task StartUpdateProcessAsync()
        {
            try
            {
                while (true)
                {
                    if (IsConnected)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            trainer?.GetMoney();
                            _number = trainer!.Money;
                            txtNum.Text = _number.ToString("N", CultureInfo.CurrentCulture) + "€";
                            SetBtnVisibility();
                        });
                    }
                    
                    await Task.Delay(1000);
                }
            }
            catch (Exception ex)
            {
                ThrowError(ex);
            }
        }

        private void ThrowError(Exception ex)
        {
            trainer = null;
            SetBtnVisibility();

            string errormsg = "Error on updating Value. Programm is closing!";
            if (ex != null) errormsg += "\n\n" + ex.Message;

            MessageBox.Show(this, errormsg, "Error");
            Environment.Exit(0);
        }

        private void btnNegBalance_Click(object sender, RoutedEventArgs e)
        {
            var ok = MessageBox.Show(this, "This feature is available only when your balance is negative, setting it to -1€. To use this tool normally again, you can secure an in-game loan to return your balance to positive.\n\nDo you wish to proceed?", "EuroTruck Trainer",MessageBoxButton.OKCancel);
            if (ok == MessageBoxResult.OK)
            {
                try
                {
                    trainer?.SetMinusOneForNegativeBalance();
                }
                catch (Exception ex)
                {
                    ThrowError(ex);
                }
                
            }
        }
        private void SetBtnVisibility()
        {
            long number = _number;
            btnMinus100.IsEnabled = IsConnected && (number - 100) >= 0 ? true : false;
            btnMinus1_000.IsEnabled = IsConnected && (number - 1_000) >= 0 ? true : false;
            btnMinus10_000.IsEnabled = IsConnected && (number - 10_000) >= 0 ? true : false;
            btnMinus100_000.IsEnabled = IsConnected && (number - 100_000) >= 0 ? true : false;

            btnPlus100.IsEnabled = IsConnected ? true : false;
            btnPlus1_000.IsEnabled = IsConnected ? true : false;
            btnPlus10_000.IsEnabled = IsConnected ? true : false;
            btnPlus100_000.IsEnabled = IsConnected ? true : false;

            btnNegBalance.IsEnabled = IsConnected;
            btnConnect.IsEnabled = !IsConnected;
        }

        private void Increase100_Click(object sender, RoutedEventArgs e) => Number += 100;
        private void Increase1000_Click(object sender, RoutedEventArgs e) => Number += 1000;
        private void Increase10000_Click(object sender, RoutedEventArgs e) => Number += 10000;
        private void Increase100000_Click(object sender, RoutedEventArgs e) => Number += 100000;
        private void Decrease100_Click(object sender, RoutedEventArgs e) => Number -= 100;
        private void Decrease1000_Click(object sender, RoutedEventArgs e) => Number -= 1000;
        private void Decrease10000_Click(object sender, RoutedEventArgs e) => Number -= 10000;
        private void Decrease100000_Click(object sender, RoutedEventArgs e) => Number -= 100000;

        private void txtVersion_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
    }
}