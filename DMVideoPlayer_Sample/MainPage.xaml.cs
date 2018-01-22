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
using DMVideoPlayer;
using Newtonsoft.Json;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace DMVideoPlayer_Sample
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        #region prop

        private DMPlayerController dmPlayerController;

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

            //Loaded += MainPage_Loaded;

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


        //private void MainPage_Loaded(object sender, RoutedEventArgs e)
        //{

        //    //With Params and will allow you to recieve dmevents
        //    //rtl live
        //    loadHtmlVideo("xl1km0");
        //}

        private void Init()
        {
            var parameters = new Dictionary<string, string>();

            parameters["autoplay"] = "true";
            parameters["ui-logo"] = "false";
            parameters["endscreen-enable"] = "false";
            // parameters["chromeless"] = "true";
            parameters["auto"] = "true";


            parameters["controls"] = "1";

            parameters["sharing-enable"] = "fullscreen_only";
            parameters["fullscreen-enable"] = "fullscreen_only";
            parameters["collections-enable"] = "fullscreen_only";
            parameters["watchlater-enable"] = "fullscreen_only";
            parameters["like-enable"] = "fullscreen_only";

            parameters["GK_PV5_GLOBAL_TIMEOUT_EXTENDED"] = "true";

            //init
            dmPlayerController = new DMPlayerController();

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


            parameters["controls"] = "1";

            parameters["sharing-enable"] = "fullscreen_only";
            parameters["fullscreen-enable"] = "fullscreen_only";
            parameters["collections-enable"] = "fullscreen_only";
            parameters["watchlater-enable"] = "fullscreen_only";
            parameters["like-enable"] = "fullscreen_only";

            parameters["GK_PV5_GLOBAL_TIMEOUT_EXTENDED"] = "true";

            //init
            dmPlayerController = new DMPlayerController();

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


            parameters["controls"] = "1";

            parameters["sharing-enable"] = "fullscreen_only";
            parameters["fullscreen-enable"] = "fullscreen_only";
            parameters["collections-enable"] = "fullscreen_only";
            parameters["watchlater-enable"] = "fullscreen_only";
            parameters["like-enable"] = "fullscreen_only";

            parameters["GK_PV5_GLOBAL_TIMEOUT_EXTENDED"] = "true";

            //init
            dmPlayerController = new DMPlayerController();

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
			parameters["queue-enable"] = "1";


            var accessToken = "";// "myAccessToken";

            //init the DMVideoPlayer
            dmPlayerController.Load(videoId, accessToken, parameters);

        }
        private void DmPlayerController_OnDmWebViewMessageUpdated()
        {
            Debug.WriteLine(dmPlayerController.DmWebViewMessage);

            //if (dmPlayerController.DmWebViewMessage.Contains("share_requested"))
            //{
            //    Windows.ApplicationModel.DataTransfer.DataTransferManager.ShowShareUI();
            //}
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
            // dmPlayerController.CallMethodeOnPlayer("player.setMuted(true)");

            dmPlayerController.Mute();
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
    }
}
