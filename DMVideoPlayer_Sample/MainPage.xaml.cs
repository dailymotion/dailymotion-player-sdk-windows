using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Newtonsoft.Json;
using Windows.UI.ViewManagement;
using DMVideoPlayer;
using DmVideoPlayer;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DMVideoPlayer_Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region prop

        private DmPlayerController dmPlayerController;

        #endregion

        /// <summary>
        /// you can find additional information here:https://developer.dailymotion.com/player
        /// </summary>
        /// 
        /// 
        /// SUPER IMPORTANT MAKE SURE YOU WHITE LIST : https://*.dailymotion.com in your appxmanifest
        public MainPage()
        {
            this.InitializeComponent();

            Loaded += MainPage_Loaded;

            //handling share example
            Windows.ApplicationModel.DataTransfer.DataTransferManager.GetForCurrentView().DataRequested += MainPage_DataRequested;
        }

        private void MainPage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            if (dmPlayerController != null)
            {
                //args.Request.Data.SetText(dmPlayerController.DmWebViewMessage);
                args.Request.Data.Properties.Title = Windows.ApplicationModel.Package.Current.DisplayName;
            }
            else
            {
                args.Request.FailWithDisplayText("Nothing to share");
            }
        }


        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
            //With Params and will allow you to recieve dmevents
            //JO Video
            //loadHtmlVideo("x6ez4x0");
            //loadHtmlVideo("x6f0xd3");

            //imagine dragons
            loadHtmlVideo("x5mscur");


            //loadHtmlVideo("x5793u6");

            var size = GetActualSize();

            MyRootGrid.Width = size.Width - 90;
            MyRootGrid.Height = size.Height - 190;
        }

        private void Init()
        {
            var parameters = new Dictionary<string, string>();

            parameters["fullscreen-action"] = "trigger_event";

            parameters["sharing-action"] = "trigger_event";
            parameters["like-action"] = "trigger_event";
            parameters["collections-action"] = "trigger_event";
            parameters["watchlater-action"] = "trigger_event";

            parameters["autoplay"] = "true";
            parameters["ui-logo"] = "0";
            parameters["endscreen-enable"] = "false";
            // parameters["chromeless"] = "true";
            parameters["auto"] = "true";


            parameters["controls"] = "1";

            parameters["sharing-enable"] = "false";
            parameters["fullscreen-enable"] = "fullscreen_only";
            parameters["collections-enable"] = "false";
            parameters["watchlater-enable"] = "false";
            parameters["like-enable"] = "false";


            parameters["quality"] = "2160";
            //parameters["quality"] = "1080";
            //parametersCookies["clsu"] = "1";
            parameters["GK_PV5_GLOBAL_TIMEOUT_EXTENDED"] = "true";

            //init
            dmPlayerController = new DmPlayerController();


            //if needed you can set a specific user agent into the webview
            //dmPlayerController.CustomUserAgent = My UserAgent;

            //you can tell the the SDK is it is running on a Xbox or not
            //dmPlayerController.IsXbox = false;

            //are you using an Oauth Token, if so is it a user token 
            //dmPlayerController.IsLogged = false;


            var accessToken = "";// "myAccessToken";

            //init the DMVideoPlayer
            dmPlayerController.Init(accessToken, parameters);

            if (!MyRootGrid.Children.Contains(dmPlayerController.DmVideoPlayer))
            {
                //adding DmVideoPlayer to the page
                MyRootGrid.Children.Add(dmPlayerController.DmVideoPlayer);
            }

            dmPlayerController.OnDmWebViewMessageUpdated += DmPlayerController_OnDmWebViewMessageUpdated;
        }

        private void InitAndLoad()
        {
            var parameters = new Dictionary<string, string>();

            parameters["autoplay"] = "true";
            parameters["ui-logo"] = "false";
            parameters["endscreen-enable"] = "false";
            // parameters["chromeless"] = "true";
            parameters["auto"] = "true";
            parameters["mse"] = "0";


            parameters["controls"] = "1";

            parameters["sharing-enable"] = "fullscreen_only";
            parameters["fullscreen-enable"] = "fullscreen_only";
            parameters["collections-enable"] = "fullscreen_only";
            parameters["watchlater-enable"] = "fullscreen_only";
            parameters["like-enable"] = "fullscreen_only";

            parameters["GK_PV5_GLOBAL_TIMEOUT_EXTENDED"] = "true";

            //init
            dmPlayerController = new DmPlayerController();

            var accessToken = "";// "myAccessToken";

            //init the DMVideoPlayer
            dmPlayerController.Load("xl1km0", accessToken, parameters);

            if (!MyRootGrid.Children.Contains(dmPlayerController.DmVideoPlayer))
            {
                //adding DmVideoPlayer to the page
                MyRootGrid.Children.Add(dmPlayerController.DmVideoPlayer);
            }

            dmPlayerController.OnDmWebViewMessageUpdated += DmPlayerController_OnDmWebViewMessageUpdated;
        }

        private void Reset()
        {
            var parameters = new Dictionary<string, string>();

            parameters["fullscreen-action"] = "trigger_event";

            parameters["sharing-action"] = "trigger_event";
            parameters["like-action"] = "trigger_event";
            parameters["collections-action"] = "trigger_event";
            parameters["watchlater-action"] = "trigger_event";

            parameters["autoplay"] = "true";
            parameters["ui-logo"] = "false";
            parameters["endscreen-enable"] = "false";
            // parameters["chromeless"] = "true";
            parameters["auto"] = "true";

            parameters["quality"] = "2160";
            parameters["controls"] = "1";

            parameters["sharing-enable"] = "fullscreen_only";
            parameters["fullscreen-enable"] = "fullscreen_only";
            parameters["collections-enable"] = "fullscreen_only";
            parameters["watchlater-enable"] = "fullscreen_only";
            parameters["like-enable"] = "fullscreen_only";

            parameters["GK_PV5_GLOBAL_TIMEOUT_EXTENDED"] = "true";

            //init
            dmPlayerController = new DmPlayerController();

            //if needed you can set a specific user agent into the webview
            //dmPlayerController.CustomUserAgent = My UserAgent;

            //you can tell the the SDK is it is running on a Xbox or not
            //dmPlayerController.IsXbox = false;

            //are you using an Oauth Token, if so is it a user token 
            //dmPlayerController.IsLogged = false;

            var accessToken = "";// "myAccessToken";

            //init the DMVideoPlayer
            dmPlayerController.Reset(accessToken, parameters);

            //removing player instance
            if (MyRootGrid.Children.Contains(dmPlayerController.DmVideoPlayer))
            {
                MyRootGrid.Children.Remove(dmPlayerController.DmVideoPlayer);
            }


            //adding DmVideoPlayer to the page
            MyRootGrid.Children.Add(dmPlayerController.DmVideoPlayer);

            //MyWebview = dmPlayerController.DmVideoPlayer;

            dmPlayerController.OnDmWebViewMessageUpdated += DmPlayerController_OnDmWebViewMessageUpdated;
        }



        private void loadHtmlVideo(string videoId)
        {
            var parameters = new Dictionary<string, string>();

            parameters["fullscreen-action"] = "trigger_event";
            parameters["sharing-action"] = "trigger_event";
            parameters["autoplay"] = "true";

            //this will allow the player to auto next to the next related video
            parameters["queue-enable"] = "false";
            parameters["queue-enable"] = "false";


            var accessToken = "";// "myAccessToken";

            //init the DMVideoPlayer
            dmPlayerController.Load(videoId, accessToken, parameters);

        }
        private void DmPlayerController_OnDmWebViewMessageUpdated()
        {
            if (dmPlayerController.DmWebViewMessage.Key == null)
            {
                return;
            }

            if (!dmPlayerController.DmWebViewMessage.Key.Contains("time")
                && !dmPlayerController.DmWebViewMessage.Value.Contains("time")
                && !dmPlayerController.DmWebViewMessage.Key.Contains("progress")
                && !dmPlayerController.DmWebViewMessage.Value.Contains("progress")

                )
            {
                Debug.WriteLine(dmPlayerController.DmWebViewMessage);
            }

            //if (dmPlayerController.DmWebViewMessage.Contains("share_requested"))
            //{
            //    Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
            //}

            if (dmPlayerController.DmWebViewMessage.Value.Equals("fullscreen_toggle_requested"))
            {
                DmPlayer_FullscreenToggle();
            }
        }


        private void DmPlayer_FullscreenToggle()
        {
            ApplicationView view = ApplicationView.GetForCurrentView();

            if (view.TryEnterFullScreenMode())
            {
                var size = GetActualSize();

                MyRootGrid.Width = size.Width - 90;
                MyRootGrid.Height = size.Height - 190;
            }
        }

        public static Size GetActualSize()
        {
            return new Size(Window.Current.Bounds.Width, Window.Current.Bounds.Height);
        }

        private void PlayButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            dmPlayerController?.Play();
        }

        private void PauseButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            dmPlayerController?.Pause();
        }

        private void Setting1ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var parameters = new Dictionary<string, string>();

            parameters["autoplay"] = "true";
            parameters["ui-logo"] = "false";
            parameters["endscreen-enable"] = "false";
            parameters["mute"] = "true";
            //parameters["quality"] = "2160";
            var accessToken = "";// "myAccessToken";

            //init the DMVideoPlayer
            dmPlayerController.Load("x551owj", accessToken, parameters);
        }

        private void Setting2ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var parameters = new Dictionary<string, string>();


            parameters["autoplay"] = "true";
            parameters["ui-logo"] = "false";
            parameters["endscreen-enable"] = "false";
            parameters["locale"] = "en";
            parameters["controls"] = "false";

            var accessToken = "";// "myAccessToken";

            //init the DMVideoPlayer
            dmPlayerController.Load("x2ycweg", accessToken, parameters);
        }

        private void mute_click(object sender, RoutedEventArgs e)
        {
            //muting 
            dmPlayerController.Mute();
        }
        private void unmute_click(object sender, RoutedEventArgs e)
        {
            //muting 
            dmPlayerController.Unmute();
        }

        private void InitPlayer_click(object sender, RoutedEventArgs e)
        {
            Init();
        }

        private void InitAndPlayPlayer_click(object sender, RoutedEventArgs e)
        {
            InitAndLoad();
        }

        private void ResetPlayer_click(object sender, RoutedEventArgs e)
        {
            Reset();
        }


        private void Load_1_OnClick(object sender, RoutedEventArgs e)
        {
            //music - imagine dragons
            loadHtmlVideo("x5mscur");
        }


        private void Load_2_OnClick(object sender, RoutedEventArgs e)
        {
            //JO Video - can be geoblocked
            loadHtmlVideo("x6ez4x0");
        }


        private void Load_3_OnClick(object sender, RoutedEventArgs e)
        {
            //video trailer
            loadHtmlVideo("x5793u6");
        }
    }
}
