using System;
using System.Windows;
using System.Windows.Threading;
using System.ComponentModel;
using Microsoft.Win32;
using AquaCoaster.Model;
using AquaCoaster.View;
using AquaCoaster.ViewModel;


namespace AquaCoaster
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Fields

        private GameModel _model;
        private GameViewModel _viewModel;
        private MainWindow _view;
        private DispatcherTimer _timer;

        #endregion

        #region Constructors

        /// <summary>
        /// Instantiating the application.
        /// </summary>
        public App()
        {
            Startup += new StartupEventHandler(App_Startup);
        }

        #endregion

        #region Application event handlers

        private void App_Startup(object sender, StartupEventArgs e)
        {

            // create timer
            _timer = new DispatcherTimer();
            _timer.Tick += TimerTick;

            // creating an instance of the modell and attaching the event handlers
            _model = new GameModel("AquaCoaster");
            _model.GameOver += Model_GameOver;
            _model.TimeIntervalChanged += Model_TimeIntervalChanged;

            // Creating an instance of the view-model and attaching the event handlers
            _viewModel = new GameViewModel(_model);
            _viewModel.NewGame += ViewModel_NewGame;
            _viewModel.ExitGame += ViewModel_ExitGame;

            // creating the window
            _view = new MainWindow { DataContext = _viewModel };
            _view.Closing += View_Closing; // window closing event handler
            _view.Show();

            _model.NewGameButDoNotStart("AquaCoaster");
        }

        #endregion

        #region Timer event handler

        private void TimerTick(object sender, EventArgs e)
        {
            _model.AdvanceTime();
        }

        #endregion

        #region View event handlers

        /// <summary>
        /// Window closing event handler.
        /// </summary>
        private void View_Closing(object sender, CancelEventArgs e)
        {
            Boolean restartTimer = _timer.IsEnabled;
            _timer.Stop();

            if (MessageBox.Show("Are you sure, you want to exit?", "AquaCoaster", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                e.Cancel = true; // don't exit

                if (restartTimer)
                    _timer.Start();
            }
        }

        #endregion

        #region ViewModel event handlers

        /// <summary>
        /// New game event handler.
        /// </summary>
        private void ViewModel_NewGame(object sender, EventArgs e)
        {
            // TODO: Implement UI components to specify the name.
            _model.NewGame(_viewModel.Name);
            _timer.Start();
        }

        /// <summary>
        /// Exit game event handler.
        /// </summary>
        private void ViewModel_ExitGame(object sender, System.EventArgs e)
        {
            _view.Close(); // close window
        }

        #endregion

        #region Model event handlers

        private void Model_TimeIntervalChanged(object sender, int newTimeInterval)
        {
            if (newTimeInterval == 0 && _timer.IsEnabled)
            {
                _timer.Stop();
            }
            else
            {
                _timer.Interval = TimeSpan.FromMilliseconds(newTimeInterval);

                if (!_timer.IsEnabled)
                {
                    _timer.Start();
                }
            }
        }

        /// <summary>
        /// Game over event handler.
        /// </summary>
        private void Model_GameOver(object sender, EventArgs args)
        {
            _timer.Stop();

            if(MessageBox.Show("You are out of your budget, you lose!",
                            _viewModel.Name,
                            MessageBoxButton.OK, MessageBoxImage.None) == MessageBoxResult.OK)
            {
                _viewModel.OnNewGame();
            }
            
        }

        #endregion
    }
}
