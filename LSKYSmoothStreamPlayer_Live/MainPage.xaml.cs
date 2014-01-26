using Microsoft.Web.Media.SmoothStreaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Browser;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace LSKYSmoothStreamPlayer_Live
{
    public partial class MainPage : UserControl
    {
        private const double DefaultPlayerWidth = 640;
        private const double DefaultPlayerHeight = 480;
                
        //  These get set by the App, before this page is started
        public static double? PlayerWidthFromParameters;
        public static double? PlayerHeightFromParameters;
        public static String configStreamURI = null;

        private static double PlayerWidth;
        private static double PlayerHeight;

        // Stuff for detecting mouse movement (so we can hide controls)
        private TimeSpan ControlTimeout;
        private DateTime LastMouseMovement;
        private static System.Windows.Threading.DispatcherTimer ControlHideTimer;
        private bool areControlsHidden;
        private bool areControlsHideable;


        public MainPage()
        {
            InitializeComponent();


            // Set up event handlers for full screen
            Application.Current.Host.Content.FullScreenChanged += Content_FullScreenChanged;

            // Set up event handlers for the smooth streaming element
            SmoothStreamElement.SmoothStreamingErrorOccurred += new EventHandler<SmoothStreamingErrorEventArgs>(SmoothPlayer_SmoothStreamingErrorOccurred);
            SmoothStreamElement.MediaOpened += new RoutedEventHandler(SmoothPlayer_MediaOpened);
            SmoothStreamElement.LiveEventCompleted += new EventHandler(SmoothPlayer_LiveEventCompleted);
            SmoothStreamElement.MediaEnded += new RoutedEventHandler(SmoothPlayer_MediaEnded);

            // Set up event handlers for the control hide timer
            areControlsHidden = false;
            areControlsHideable = false;
            ControlTimeout = TimeSpan.FromSeconds(2);
            this.MouseMove += new MouseEventHandler(MouseMoveHandler);
            ControlHideTimer = new System.Windows.Threading.DispatcherTimer();
            ControlHideTimer.Interval = TimeSpan.FromSeconds(1);
            ControlHideTimer.Tick += new EventHandler(ControlHideTimer_Tick);
            ControlHideTimer.Start();


            // See if the width and height were set by the user when the player was loaded
            if ((PlayerWidthFromParameters != null) && (PlayerWidthFromParameters > DefaultPlayerWidth))
            {
                PlayerWidth = (double)PlayerWidthFromParameters;
            }
            else
            {
                PlayerWidth = DefaultPlayerWidth;
            }

            if ((PlayerHeightFromParameters != null) && (PlayerHeightFromParameters > DefaultPlayerHeight))
            {
                PlayerHeight = (double)PlayerHeightFromParameters;
            }
            else
            {
                PlayerHeight = DefaultPlayerHeight;
            }

            // Set the player size to whatever we determined from above
            SetPlayerDimensions(PlayerWidth, PlayerHeight);

        }      

        #region General stuff

        /// <summary>
        /// A quick and easy way to change the size of the player
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void SetPlayerDimensions(double width, double height)
        {
            LayoutRoot.Height = height;
            LayoutRoot.Width = width;
        }

        /// <summary>
        /// Sets the status bar text to something
        /// </summary>
        /// <param name="thisStatus"></param>
        public void setStatus(String thisStatus)
        {
            statusText.Text = thisStatus;
        }

        /// <summary>
        /// Clears the status bar
        /// </summary>
        public void clearStatus()
        {
            statusText.Text = "";
        }

        /// <summary>
        /// Deal with formatting the source file URI
        /// </summary>
        /// <param name="thisURI"></param>
        /// <returns></returns>
        private String translateRelativeURI(String thisURI)
        {
            String returnme = "";
            String serverURL = Application.Current.Host.Source.Host;


            if (thisURI.StartsWith("http://"))
            {
                returnme = thisURI;
            }
            else if (thisURI.StartsWith("/"))
            {
                // Assume it's relative to the server root
                returnme = "http://" + serverURL + thisURI;
            }
            else
            {
                // Assume it's relative to the script's root
                String[] splitRoot = Application.Current.Host.Source.AbsoluteUri.ToString().Split('/');

                returnme = "http://";
                for (int x = 2; x < (splitRoot.Length - 1); x++)
                {
                    returnme += splitRoot[x] + "/";
                }

                returnme += thisURI;

            }

            if (thisURI.ToLower().EndsWith("/manifest"))
            {
                return returnme;
            }
            else
            {
                return returnme + "/Manifest";
            }

        }

        /// <summary>
        /// Clear the "cover" logo and expose the media
        /// </summary>
        private void hideLogo()
        {
            cnvBlank.Visibility = Visibility.Collapsed;
            vidVolumeSlider.IsEnabled = true;
        }

        /// <summary>
        /// Hide the media with a logo and white box
        /// </summary>
        private void ShowLogo()
        {
            cnvBlank.Visibility = Visibility.Visible;
            vidVolumeSlider.IsEnabled = false;
        }

        #endregion

        #region Event handlers

        void SmoothPlayer_SmoothStreamingErrorOccurred(object sender,
                               SmoothStreamingErrorEventArgs e)
        {
            setStatus("ERROR: " + e.ErrorMessage);

            if (e.ErrorMessage.StartsWith("Failed to download manifest"))
            {
                setStatus("Event has not started yet, or is no longer in the archive");
                ShowLogo();
            }
            else if (e.ErrorMessage.StartsWith("Reached end of known chunk list"))
            {
                setStatus("Stream ended prematurely - try refreshing the page");
                ShowLogo();
            }
        }

        void SmoothPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            SmoothStreamElement.StartSeekToLive();
            SmoothStreamElement.Play();

            if (SmoothStreamElement.IsLive)
            {
                setStatus("Live");
                hideLogo();
            }
            else
            {
                setStatus("Live event has ended");
                ShowLogo();
                SmoothStreamElement.Stop();
            }
        }

        void SmoothPlayer_LiveEventCompleted(object sender, EventArgs e)
        {
            setStatus("Live event has ended");
            ShowLogo();
        }

        void SmoothPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            setStatus("Live event has ended");
            ShowLogo();
        }

        private void SmoothStreamElement_Loaded(object sender, RoutedEventArgs e)
        {
            vidVolumeSlider.Value = 1;
            loadStreams();
        }

        #endregion

        #region UI Handlers

        private void btnFullScreen_Click(object sender, RoutedEventArgs e)
        {
            if (Application.Current.Host.Content.IsFullScreen)
            {
                Application.Current.Host.Content.IsFullScreen = false;
            }
            else
            {
                Application.Current.Host.Content.IsFullScreen = true;
            }

        }

        private void vidVolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                SmoothStreamElement.Volume = vidVolumeSlider.Value;

                int Percent = (int)(SmoothStreamElement.Volume * 100);
            }
            catch (Exception ex)
            {
                setStatus("ERROR SETTING VOLUME: " + ex.Message);
            } 

        }

        #endregion

        #region Full screen handler
        
        private void Content_FullScreenChanged(object sender, EventArgs e)
        {
            if (Application.Current.Host.Content.IsFullScreen)
            {
                SetPlayerDimensions(Application.Current.Host.Content.ActualWidth, Application.Current.Host.Content.ActualHeight);
            }
            else
            {
                SetPlayerDimensions(PlayerWidth, PlayerHeight);
            }
        }

        #endregion

        #region Stream loading stuff

        private void loadStreams()
        {
            SmoothStreamElement.Visibility = Visibility.Collapsed;
            ShowLogo();
            if (configStreamURI.Length > 1)
            {
                String useThisForURI = translateRelativeURI(configStreamURI);

                setStatus("Loading stream: \"" + useThisForURI + "\"");
                try
                {
                    SmoothStreamElement.SmoothStreamingSource = new Uri(useThisForURI);
                }
                catch (Exception ex)
                {
                    setStatus("Error loading stream: " + ex.Message);
                }
                setStatus("Stream loaded");
                SmoothStreamElement.Visibility = Visibility.Visible;
                SmoothStreamElement.StartSeekToLive();
                hideLogo();
            }
            else
            {
                setStatus("ERROR: Invalid or blank stream URI");
            }
        }

        #endregion

        #region Hide controls and make them reappear when the mouse moves

        /// <summary>
        /// This is called whenever the mouse moves anywhere on the player. It records the last time it saw movement, so that the timer can use that 
        /// to determine if the controls should be hidden.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            LastMouseMovement = DateTime.Now;
            if (areControlsHideable)
            {
                areControlsHidden = false;
                ControlBar.Visibility = Visibility.Visible;
            }
        }

        private void ControlHideTimer_Tick(object sender, EventArgs e)
        {
            // Check if we should hide controls
            if (areControlsHideable)
            {
                if (!areControlsHidden)
                {
                    TimeSpan TimeSinceLastMouseMovement = DateTime.Now - LastMouseMovement;
                    if (TimeSinceLastMouseMovement >= ControlTimeout)
                    {
                        ControlBar.Visibility = Visibility.Collapsed;
                        areControlsHidden = true;
                    }
                }
            }

            // Check if the controls should be hideable at all
            if (SmoothStreamElement.CurrentState == SmoothStreamingMediaElementState.Playing)
            {
                areControlsHideable = true;
            }
            else
            {
                areControlsHideable = false;
            }
        }

        #endregion

    }
}
