using MineManager.Models;
using MineManager.ViewModels;
using System;
using System.Windows.Threading;

namespace MineManager.Views
{
    public partial class MiningManagerWindow
    {
        Presenter vm;
        private DispatcherTimer _dispatcherTimer;
        private ClientIdleHandler _clientIdleHandler;
        private DateTime lastActive;
        private int cooldownSeconds = 10;
         
        public MiningManagerWindow()
        {
            InitializeComponent();
            Window_Loaded();
        }

        private void Window_Loaded()
        {
            vm = (Presenter)DataContext;
            //start client idle hook
            _clientIdleHandler = new ClientIdleHandler();
            _clientIdleHandler.Start();

            //start timer
            _dispatcherTimer = new DispatcherTimer();
            _dispatcherTimer.Tick += TimerTick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 1);
            _dispatcherTimer.Start();
        }

        private void TimerTick(object sender, EventArgs e)
        {
            bool handlerActive = _clientIdleHandler.IsActive;
            _clientIdleHandler.IsActive = false;            
            bool fullscreen = FullScreenDetector.IsForegroundFullScreen();
            
            if (handlerActive || fullscreen) lastActive = DateTime.Now; 

            vm.InactiveForTime = lastActive.AddSeconds(cooldownSeconds) < DateTime.Now;
            Console.WriteLine("Inactive for time ? " + vm.InactiveForTime);
        }
    }
}
