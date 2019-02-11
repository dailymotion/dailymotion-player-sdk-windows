﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.Web.Http;
using DMVideoPlayer.Annotations;
using DMVideoPlayer.Models.Enums;
using DMVideoPlayer.Exceptions;

namespace DMVideoPlayer
{
    public class DMPlayerController : INotifyPropertyChanged
    {

        private static string defaultUrlStage = "https://stage-01.dailymotion.com";
        private static string defaultUrl = "https://www.dailymotion.com";

        private static bool defaultIsTapEnabled = true;

        private static string HockeyAppId = "6d380067c4d848ce863b232a1c5f10ae";
        private static string version = "2.9.3";
        //private static string bundleIdentifier = "com.dailymotion.dailymotion";
        //private static string bundleIdentifier = "WindowsSDK";
        private static string eventName = "dmevent";
        private static string eventNameV2 = "event=";
        private static string pathPrefix = "/embed/";
        private static string pathWithVideoPrefix = "/embed/video/";
        private static string messageHandlerEvent = "triggerEvent";

        public static string COMMAND_NOTIFY_LIKECHANGED = "notifyLikeChanged";
        public static string COMMAND_NOTIFY_WATCHLATERCHANGED = "notifyWatchLaterChanged";
        public static string COMMAND_NOTIFYFULLSCREENCHANGED = "notifyFullscreenChanged";

        public event Action OnDmWebViewMessageUpdated;

        private string _baseUrl; // URL!
        private string _appName; // URL!
        public bool ApiReady { get; set; }
        public bool HasPlayerError { get; set; }
        public string VideoId { get; set; }
        public string AccessToken { get; set; }
        // public string loadedJsonData { get; set; }
        public IDictionary<string, string> WithParameters { get; set; }
        public bool IsHeroVideo { get; set; }
        public bool PendingPlay { get; set; }
        public bool ShowingAd { get; set; }
        public string BaseUrl
        {
            get { return _baseUrl ?? defaultUrl; }
            set { _baseUrl = value; }
        }
        public string AppName
        {
            get { return _appName; }
            set { _appName = value; }
        }

        private bool? _isTapEnabled;

        public bool IsTapEnabled
        {
            get { return _isTapEnabled ?? defaultIsTapEnabled; }
            set { _isTapEnabled = value; }
        }

        private bool _isXbox = false;

        public bool IsXbox
        {
            get { return _isXbox; }
            set { _isXbox = value; }
        }

        private bool _isLogged = false;

        public bool IsLogged
        {
            get { return _isLogged; }
            set { _isLogged = value; }
        }
        
        private WebViewExecutionMode _webViewExecutionModeThread = WebViewExecutionMode.SameThread;

        public WebViewExecutionMode WebViewExecutionModeThread
        {
            get { return _webViewExecutionModeThread; }
            set
            {

                _webViewExecutionModeThread = value;
                OnPropertyChanged();
            }
        }

        private WebView _dmVideoPlayer;

        public WebView DmVideoPlayer
        {
            get { return _dmVideoPlayer; }
            set
            {

                _dmVideoPlayer = value;
                OnPropertyChanged();
            }
        }

        private KeyValuePair<string, string> _dmWebViewMessage;

        public KeyValuePair<string, string> DmWebViewMessage
        {
            get { return _dmWebViewMessage; }
            set
            {

                _dmWebViewMessage = value;
                OnDmWebViewMessageUpdated?.Invoke();
                OnPropertyChanged();
            }
        }

        /// Initialize a new instance of the player
        /// - Parameters:
        ///   - accessToken: An optional oauth token. If provided it will be passed as Bearer token to the player.
        ///   - withParameters:  The dictionary of configuration parameters that are passed to the player.
        ///   - withCookiesParameters:     An optional array of HTTPCookie values that are passed to the player.
        public void Init(string accessToken = "",
            IDictionary<string, string> withParameters = null,
            IDictionary<string, string> withCookiesParameters = null)
        {
            this.AccessToken = accessToken;
            HasPlayerError = false;
            //Creating a new webview when doing a new call
            if (DmVideoPlayer == null)
            {
                DmVideoPlayer = NewWebView();

                //setting cookies if needed
                if (withCookiesParameters != null)
                {
                    //if in params we have the keys v1st or tg then we need to send it to the player in a cookie
                    foreach (var cookie in withCookiesParameters)
                    {
                        //set cookie
                        SetCookieInWebView(cookie.Key, cookie.Value);
                    }
                }

                //////set access token
                if (IsXbox == false && !string.IsNullOrEmpty(this.AccessToken))
                {
                    //when user is not logged access token must be passed as a client_token and when 
                    //user logged as a access_token
                    SetCookieInWebView(IsLogged ? "access_token" : "client_token", this.AccessToken);
                }

                //Recieving the events the player is sending
                DmVideoPlayer.ScriptNotify += DmWebView_ScriptNotify;

                //creating http request message to send to the webview
                HttpRequestMessage request = NewRequest("", withParameters);

                //doing call
                DmVideoPlayer.NavigateWithHttpRequestMessage(request);

                
            }
        }

       
        /// Reset the player with a new instance of the player
        /// - Parameters:
        ///   - accessToken: An optional oauth token. If provided it will be passed as Bearer token to the player.
        ///   - withParameters:  The dictionary of configuration parameters that are passed to the player.
        ///   - withCookiesParameters:     An optional array of HTTPCookie values that are passed to the player.

        public void Reset(string accessToken = "",
                            IDictionary<string, string> withParameters = null,
                            IDictionary<string, string> withCookiesParameters = null)
        {
            //clear
            DmVideoPlayer = null;

            //init player
            Init(accessToken, withParameters, withCookiesParameters);
        }

        /// Load a video with ID and optional OAuth token
        ///
        /// - Parameter videoId:        The video's XID
        ///   - accessToken: An optional oauth token. If provided it will be passed as Bearer token to the player.
        ///   - withParameters:  The dictionary of configuration parameters that are passed to the player.
        ///   - withCookiesParameters:     An optional array of HTTPCookie values that are passed to the player.
        public void Load(string videoId, string accessToken = "", IDictionary<string, string> withParameters = null, IDictionary<string, string> withCookiesParameters = null)
        {
            HasPlayerError = false;
            this.VideoId = videoId;
            this.WithParameters = withParameters;
            this.AccessToken = accessToken;

            //check base url
            if (BaseUrl != null)
            {
                //Creating a new webview when doing a new call
                //or using the JS to load the video if the player is already loaded
                if (DmVideoPlayer == null)
                {
                    //init webview with cookies
                    Init(accessToken, withParameters, withCookiesParameters);
                }
                else
                {
                    if (ApiReady)
                    {
                        Load();
                        PendingPlay = false;
                    }
                    else
                    {
                        PendingPlay = true;
                    }
                }
            }
        }

        public void Unload()
        {
            DmVideoPlayer.ScriptNotify -= DmWebView_ScriptNotify;
            DmVideoPlayer = null;
        }


        /// <summary>
        /// Handles the events fired from the Webview and passes the information on
        /// </summary>
        private void DmWebView_ScriptNotify(object sender, NotifyEventArgs e)
        {
            Debug.WriteLine(e?.Value);
            var eventNames = getEventNames(e?.Value);

            if (eventNames != null)
            {
                foreach (var name in eventNames)
                {
                    //Debug.WriteLine(name.ToString());

                    switch (name.Value)
                    {
                        case "apiready":
                            {
                                ApiReady = true;
                                if (PendingPlay)
                                {
                                    PendingPlay = false;
                                    Load();
                                }

                                //Tracking.setPV5Info(map);
                                break;
                            }
                        case "ad_start":
                            {
                                ShowingAd = true;
                                break;
                            }
                        case "ad_end":
                            {
                                ShowingAd = false;
                                break;
                            }
                    }

                    DmWebViewMessage = name;
                }
            }
        }

        /// <summary>
        /// Transforms the merge information given from the player into a key value dictonary based information
        /// </summary>
        /// <param name="mergedEventNames"></param>
        /// <returns></returns>
        private Dictionary<string, string> getEventNames(string mergedEventNames)
        {
            var queryParameters = new Dictionary<string, string>();
            string[] querySegments = mergedEventNames.Split('&');
            foreach (string segment in querySegments)
            {
                string[] parts = segment.Split('=');
                if (parts.Length > 0)
                {
                    string key = WebUtility.UrlDecode(parts[0].Trim(new char[] { '?', ' ' }));
                    string val = WebUtility.UrlDecode(parts[1].Trim());

                    //making sure the key isnt present  
                    if (!queryParameters.ContainsKey(key))
                    {
                        queryParameters.Add(key, val);
                    }
                }
            }
            return queryParameters;
        }

        /// <summary>
        /// When the player is already loaded this allows to us to call on the JS methods of the player and thus allows to interact with it
        /// Load allows us to start a video without having to requery a URL
        /// </summary>
        private void Load()
        {
            if (WithParameters == null)
                return;

            if (WithParameters.ContainsKey("jsonEnvironmentInfo"))
            {
                InitEnvironmentInfoVariables(WithParameters["jsonEnvironmentInfo"]);
            }
            else
            {
                //set param for partners
            }


            if (WithParameters.ContainsKey("loadedJsonData"))
            {
                CallPlayerMethod("load", VideoId, WithParameters["loadedJsonData"]);
            }
            else
            {
                CallPlayerMethod("load", VideoId);
            }

            //check to see if we wish to mute or not the video
            if (WithParameters.ContainsKey("mute"))
            {
                if (WithParameters["mute"] == "true")
                {
                    Mute();
                }
                else
                {
                    Unmute();
                }
            }
        }

        /// Set a player property, for Dailymotion use only
        /// - Parameter jsonData: The data value to set
        public void InitEnvironmentInfoVariables(string jsonData)
        {
            CallPlayerMethod("setProp", "neon", jsonData);
        }

        /// <summary>
        /// Creates the http url
        /// </summary>
        /// <param name="videoId"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private HttpRequestMessage NewRequest(string videoId, IDictionary<string, string> parameters = null)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, Url(videoId, parameters));

            //Headers
            if (IsXbox)
            {
                //message.Headers.Add("User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 11_0 like Mac OS X) AppleWebKit/604.1.38 (KHTML, like Gecko) Version/11.0 Mobile/15A372 Safari/604.1");
                //message.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; Xbox; Xbox One) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36 Edge/15.15063");
                //message.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; Xbox; Xbox One) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36 Edge/16.16299");
                message.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; Xbox; Xbox One) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/17.17134");
            }
            else
            {
                message.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36 Edge/15.15063");
            }

            if (IsXbox && !string.IsNullOrEmpty(this.AccessToken))
            {
                message.Headers.Add("Authorization", "Bearer " + this.AccessToken);
            }

            return message;
        }
       
        //Creating a new webview
        private WebView NewWebView()
        {
            var webView = new WebView(WebViewExecutionModeThread);
            webView.IsTapEnabled = IsTapEnabled;
            //webView.NavigationStarting += Wb_NavigationStarting;
            webView.Opacity = 1;
            return webView;
        }

        //old code
        //private void NavigateWithHeader(Uri uri)
        //{
        //    var requestMsg = new Windows.Web.Http.HttpRequestMessage(HttpMethod.Get, uri);
        //    requestMsg.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Safari/537.36 Edge/15.15063");
        //    DmVideoPlayer.NavigateWithHttpRequestMessage(requestMsg);

        //    DmVideoPlayer.NavigationStarting += Wb_NavigationStarting;
        //}

        //private void Wb_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        //{
        //    DmVideoPlayer.NavigationStarting -= Wb_NavigationStarting;
        //    args.Cancel = true;
        //    NavigateWithHeader(args.Uri);
        //}


        private void SetCookieInWebView(string key, string value)
        {
            Windows.Web.Http.Filters.HttpBaseProtocolFilter filter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
            Windows.Web.Http.HttpCookie cookie = new Windows.Web.Http.HttpCookie(key, ".dailymotion.com", "/");
            cookie.Value = value;

            filter.CookieManager.SetCookie(cookie, false);
        }

        /// <summary>
        /// Creates a Url with a video id or not depending if we are loading the JS player first or just doing a http call with the video ids
        /// </summary>
        /// <param name="videoId"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private Uri Url(string videoId, IDictionary<string, string> parameters = null)
        {
            var components = videoId != ""
                ? String.Concat(BaseUrl, pathWithVideoPrefix, videoId)
                : String.Concat(BaseUrl, pathPrefix, videoId);

            if (parameters == null)
            {
                parameters = new Dictionary<string, string>();
            }

            parameters["api"] = "nativeBridge";
            //// parameters["objc_sdk_version"] = version;
            parameters["app"] = AppName;
            ////parameters["GK_PV5_ANTI_ADBLOCK"] = "0";
            parameters["GK_PV5_NEON"] = "1";

            var builder = new StringBuilder(components);
            if (parameters.Any())
                builder.Append("?");
            builder.Append(String.Join("&", from p in parameters select String.Format("{0}={1}", p.Key, p.Value)));

            return new Uri(builder.ToString());
        }

        /// <summary>
        /// show the player controls depending on the bool
        /// </summary>
        /// <param name="show"></param>
        public void ToggleControls(bool show)
        {
            var hasControls = show ? "1" : "0";
            //NotifyPlayerApi(method: "controls", argument: hasControls);

            CallEvalWebviewMethod(string.Format("player.controls = {0}", hasControls));

        }

        /// <summary>
        /// Sends a command to the player, for example seek/pause/play
        /// </summary>
        /// <param name="method"></param>
        /// <param name="argument"></param>
        private async void NotifyPlayerApi(string method, string argument = null)
        {
            string callingMethod = string.Format("player.api('{0}')", method);

            List<string> callingJsMethod = new List<string>();
            callingJsMethod.Add(callingMethod);

            try
            {
                await DmVideoPlayer?.InvokeScriptAsync("eval", callingJsMethod);
            }
            catch (Exception e)
            {
                HasPlayerError = true;

                string title = $"Error : {method}";
                Debug.WriteLine(title);
                //throw new PlayerException(tite, e);
            }
        }


        private async void CallEvalWebviewMethod(string callMethod)
        {
            if (!callMethod.Contains("mute"))
            {
                Debug.WriteLine(callMethod);
            }

            List<string> callingJsMethod = new List<string>();
            callingJsMethod.Add(callMethod);

            try
            {
                await DmVideoPlayer?.InvokeScriptAsync("eval", callingJsMethod);
            }
            catch (Exception e)
            {
                HasPlayerError = true;

                string title = $"Error : {callMethod}";
                Debug.WriteLine(title);
                //throw new PlayerException(tite , e);
            }
        }

        /// <summary>
        /// Sends params to the player
        /// </summary>
        /// <param name="method"></param>
        /// <param name="param"></param>
        /// <param name="dataJson"></param>
        public async void CallPlayerMethod(string method, string param, string dataJson = null)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("player.");
            builder.Append(method);
            builder.Append('(');
            builder.Append("'" + param + "'");

            if (dataJson != null)
            {
                builder.Append(",JSON.parse('" + dataJson + "')");
                // builder.Append("," + dataJson + "");
            }

            builder.Append(')');
            String js = builder.ToString();

            CallEvalWebviewMethod(js);
        }

        public void setHeroVideo(bool isHeroVideo)
        {
            IsHeroVideo = isHeroVideo;
        }

        public bool isHeroVideo()
        {
            return IsHeroVideo;
        }

        public void ToggleFullscreen()
        {
            NotifyPlayerApi("notifyFullscreenChanged");
        }

        public void Play()
        {
            Debug.Write("PLAYER", "play");
            NotifyPlayerApi("play");

            if (IsHeroVideo)
            {
                Mute();
            }
            else
            {
                Unmute();
            }
        }

        public void Pause()
        {
            //Debug.Write("PLAYER", "pause");
            NotifyPlayerApi("pause");
        }

        public void Mute()
        {
            //Debug.Write("PLAYER", "MUTE");
            NotifyPlayerApi("mute");
        }

        public void Unmute()
        {
            //Debug.Write("PLAYER", "unmute");
            NotifyPlayerApi("unmute");
        }


        public void Volume(double value)
        {
            if (value >= 0.0 && value <= 1.0)
            {
                //NotifyPlayerApi("setVolume", value.ToString());
                NotifyPlayerApi(string.Format("setVolume({0})", value.ToString()));
            }
        }

        public void Seek(int seconds)
        {
            //player.seek(30);
            CallEvalWebviewMethod(string.Format("player.seek({0})", seconds));
            //NotifyPlayerApi(method: "seek", argument: "\(to)");
        }
        public void setQuality(Qualities videoQuality)
        {
            //player.setQuality('720');
            CallEvalWebviewMethod(string.Format("player.setQuality('{0}')", (int)videoQuality));
        }

        public void setFulScreen(bool isFullScreen)
        {
            //player.setQuality('720');
            CallEvalWebviewMethod(string.Format("player.setFullscreen({0})", isFullScreen));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
