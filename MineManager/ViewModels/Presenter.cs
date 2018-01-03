using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;
using MineManager.Models;
using System.Json;

namespace MineManager.ViewModels
{
    public class Presenter : ObservableObject
    {
        private Timer t_price;
        private bool _mining;
        private string _miningStatus;
        private SolidColorBrush _miningStatusColor;
        private string _miningButtonText;
        private SolidColorBrush _miningButtonTextColor;
        private bool _minimizedMining;
        private bool _manuallyPaused;
        private bool _autoStarted;
        private string _ethUsd;
        private string _batFilePath;
        private readonly ObservableCollection<string> _history = new ObservableCollection<string>();

        private ProcessStartInfo minerStartInfo;
        private Process miner;
        private bool _inactiveForTime;

        public Presenter()
        {
            Mining = false;
            ManuallyPaused = false;
            MinimizedMining = true;
            BatFilePath = Directory.GetCurrentDirectory() + "\\runminer.bat";

            PeriodicTasks();
            AutoMiningToggle();
        }

        private void PeriodicTasks()
        {
            t_price = new Timer((e) => { UpdateEthPrice(); }, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        private void UpdateEthPrice()
        {
            using (var client = new WebClient())
            {
                try
                {
                    var json = client.DownloadString("https://min-api.cryptocompare.com/data/price?fsym=ETH&tsyms=USD");
                    var o = JsonValue.Parse(json);
                    string price = o["USD"].ToString();
                    EthUsd = price;
                }
                catch (Exception)
                {
                    // Hm....
                }

                // TODO: do something with the model
            }
        }

        public bool Mining
        {
            get { return _mining; }
            set { _mining = value;
                Console.WriteLine(Mining);
            }
        }

        public string MiningStatus
        {
            get { return _miningStatus; }
            set
            {
                _miningStatus = value;
                RaisePropertyChangedEvent("MiningStatus");
            }
        }

        public SolidColorBrush MiningStatusColor
        {
            get { return _miningStatusColor; }
            set
            {
                _miningStatusColor = value;
                RaisePropertyChangedEvent("MiningStatusColor");
            }
        }

        public string MiningButtonText
        {
            get { return _miningButtonText; }
            set
            {
                _miningButtonText = value;
                RaisePropertyChangedEvent("MiningButtonText");
            }
        }

        public SolidColorBrush MiningButtonTextColor
        {
            get { return _miningButtonTextColor; }
            set
            {
                _miningButtonTextColor = value;
                RaisePropertyChangedEvent("MiningButtonTextColor");
            }
        }

        public bool MinimizedMining
        {
            get { return _minimizedMining; }
            set
            {
                _minimizedMining = value;
                RaisePropertyChangedEvent("MinimizedMining");
            }
        }

        public string EthUsd
        {
            get { return _ethUsd; }
            set
            {
                _ethUsd = value;
                RaisePropertyChangedEvent("EthUsd");
            }
        }

        public string BatFilePath
        {
            get { return _batFilePath; }
            set
            {
                _batFilePath = value;
                RaisePropertyChangedEvent("BatFilePath");
            }
        }

        public bool ManuallyPaused
        {
            get
            {
                return _manuallyPaused;
            }

            set
            {
                _manuallyPaused = value;
            }
        }

        public bool AutoStarted
        {
            get
            {
                return _autoStarted;
            }

            set
            {
                _autoStarted = value;
            }
        }

        public bool InactiveForTime
        {
            get
            {
                return _inactiveForTime;
            }
            internal set
            {
                if (InactiveForTime != value)
                {
                    _inactiveForTime = value;
                    if (!ManuallyPaused) AutoMiningToggle();
                }
            }
        }

        private void AutoMiningToggle()
        {
            if (!InactiveForTime)
            {
                MiningButtonText = "Manually Stop";
                MiningButtonTextColor = new SolidColorBrush(Colors.Red);
                MiningStatus = "Not mining because:\n1) Game running\nOr 2) Activity Detected (wait 10s)";
                MiningStatusColor = new SolidColorBrush(Colors.Red);
                tryStopMining();
            }
            else
            {
                MiningButtonText = "Stop";
                MiningButtonTextColor = new SolidColorBrush(Colors.Red);
                MiningStatus = "Mining";
                MiningStatusColor = new SolidColorBrush(Colors.Green);
                tryStartMining();
            }
        }

        private void ManualMiningToggle()
        {
            if (ManuallyPaused)
            {
                ManuallyPaused = false;
                InactiveForTime = true;
            }
            else if (Mining || (!Mining && !InactiveForTime))
            {
                Console.WriteLine("2nd branch: Mining: {0}, Manually Paused: {1}", Mining, ManuallyPaused);
                // Do the things to help user start minining again.
                MiningButtonText = "Start";
                MiningButtonTextColor = new SolidColorBrush(Colors.Green);
                MiningStatus = "Not Mining (Paused Manually)\nMining will not resume unless Manually Resumed";
                MiningStatusColor = new SolidColorBrush(Colors.Red);

                // And now stop the mining
                ManuallyPaused = true;
                tryStopMining();
            }
            
            else
            {
                Console.WriteLine("3rd branch: Mining: {0}, Manually Paused: {1}", Mining, ManuallyPaused);
                // Do the things to help user stop mining again.
                MiningButtonText = "Stop";
                MiningButtonTextColor = new SolidColorBrush(Colors.Red);
                MiningStatus = "Mining";
                MiningStatusColor = new SolidColorBrush(Colors.Green);

                // And now start the mining
                ManuallyPaused = false;
                tryStartMining();
            }
        }

        private void tryStopMining()
        {
            try
            {
                miner.CloseMainWindow();
                miner.Close();
            }
            catch
            {

            }
            Mining = false;
        }

        private void tryStartMining()
        {
            try
            {
                minerStartInfo = new ProcessStartInfo(BatFilePath);
                if (MinimizedMining) minerStartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                else minerStartInfo.WindowStyle = ProcessWindowStyle.Normal;
                miner = Process.Start(minerStartInfo);                
            }
            catch
            {
            }
            Mining = true;
        }

        private void FindBatFile()
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".bat";
            dlg.Filter = "BAT Script Files (*.bat)|*.bat";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                BatFilePath = filename;
            }
        }

        public ICommand FindBatFileCommand
        {
            get { return new DelegateCommand(FindBatFile); }
        }

        public ICommand AutomaticMiningToggleCommand
        {
            get { return new DelegateCommand(AutoMiningToggle); }
        }

        public ICommand ManualMiningToggleCommand
        {
            get { return new DelegateCommand(ManualMiningToggle); }
        }

    }
}
