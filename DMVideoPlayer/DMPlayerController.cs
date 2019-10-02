using System;
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
using DmVideoPlayer.Models.Enums;
using DmVideoPlayer.Exceptions;
using Windows.Foundation.Collections;
using DmVideoPlayer.Model;

namespace DmVideoPlayer
{

    public class DmPlayerController : INotifyPropertyChanged
    {
        #region EVENTS

        public const string EVENT_APIREADY = "apiready";
        public const string EVENT_TIMEUPDATE = "timeupdate";
        public const string EVENT_DURATION_CHANGE = "durationchange";
        public const string EVENT_PROGRESS = "progress";
        public const string EVENT_SEEKED = "seeked";
        public const string EVENT_SEEKING = "seeking";
        public const string EVENT_GESTURE_START = "gesture_start";
        public const string EVENT_GESTURE_END = "gesture_end";
        public const string EVENT_MENU_DID_SHOW = "menu_did_show";
        public const string EVENT_MENU_DID_HIDE = "menu_did_hide";
        public const string EVENT_VIDEO_START = "video_start";
        public const string EVENT_VIDEO_END = "video_end";
        public const string EVENT_AD_START = "ad_start";
        public const string EVENT_AD_PLAY = "ad_play";
        public const string EVENT_AD_PAUSE = "ad_pause";
        public const string EVENT_AD_END = "ad_end";
        public const string EVENT_AD_TIME_UPDATE = "ad_timeupdate";
        public const string EVENT_ADD_TO_COLLECTION_REQUESTED = "add_to_collection_requested";
        public const string EVENT_LIKE_REQUESTED = "like_requested";
        public const string EVENT_WATCH_LATER_REQUESTED = "watch_later_requested";
        public const string EVENT_SHARE_REQUESTED = "share_requested";
        public const string EVENT_FULLSCREEN_TOGGLE_REQUESTED = "fullscreen_toggle_requested";
        public const string EVENT_PLAY = "play";
        public const string EVENT_PAUSE = "pause";
        public const string EVENT_LOADEDMETADATA = "loadedmetadata";
        public const string EVENT_PLAYING = "playing";
        public const string EVENT_START = "start";
        public const string EVENT_END = "end";
        public const string EVENT_CONTROLSCHANGE = "controlschange";
        public const string EVENT_VOLUMECHANGE = "volumechange";

        public const string EVENT_QUALITY = "qualitychange";
        public const string EVENT_QUALITY_CHANGE = "qualitychange";
        public const string EVENT_QUALITIES_AVAILABLE = "qualitiesavailable";

        public const string EVENT_PLAYBACK_READY = "playback_ready";
        public const string EVENT_CHROME_CAST_REQUESTED = "chromecast_requested";
        public const string EVENT_VIDEO_CHANGE = "videochange";

        #endregion

        private static string defaultUrlStage = "https://stage-01.dailymotion.com";
        private static string defaultUrl = "https://www.dailymotion.com";

        private static bool defaultIsTapEnabled = true;

        private static string HockeyAppId = "6d380067c4d848ce863b232a1c5f10ae";
        private static string version = "3.0.1";
        //private static string bundleIdentifier = "com.dailymotion.dailymotion";
        //private static string bundleIdentifier = "WindowsSDK";
        private static string eventName = "dmevent";
        private static string eventNameV2 = "event=";
        private static string pathPrefix = "/embed/";
        private static string pathWithVideoPrefix = "/embed/video/";
        private static string messageHandlerEvent = "triggerEvent";

        public const string COMMAND_NOTIFY_LIKECHANGED = "notifyLikeChanged";
        public const string COMMAND_NOTIFY_WATCHLATERCHANGED = "notifyWatchLaterChanged";
        public const string COMMAND_NOTIFYFULLSCREENCHANGED = "notifyFullscreenChanged";
        public const string COMMAND_LOAD = "load";
        public const string COMMAND_LOAD_JSON = "load_json";
        public const string COMMAND_MUTE = "mute";
        public const string COMMAND_CONTROLS = "controls";
        public const string COMMAND_PLAY = "play";
        public const string COMMAND_PAUSE = "pause";
        public const string COMMAND_SEEK = "seek";
        public const string COMMAND_SETPROP = "setProp";
        public const string COMMAND_QUALITY = "quality";
        public const string COMMAND_SUBTITLE = "subtitle";
        public const string COMMAND_TOGGLE_CONTROLS = "toggle-controls";
        public const string COMMAND_TOGGLE_PLAY = "toggle-play";
        public const string COMMAND_VOLUME = "volume";

        private IList<Command> mCommandList = new List<Command>();

        public event Action OnDmWebViewMessageUpdated;

        private string _baseUrl; // URL!
        private string _appName; // URL!
        public bool ApiReady { get; set; }
        public bool HasPlayerError { get; set; }
        public string VideoId { get; set; }
        public string AccessToken { get; set; }
        // public string loadedJsonData { get; set; }
        public IDictionary<string, string> WithParameters { get; set; }
        //public bool IsHeroVideo { get; set; }
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

        private bool _videoPaused = false;

        public bool VideoPaused
        {
            get { return _videoPaused; }
            set { _videoPaused = value; }
        }
        private bool _playWhenReady = false;

        public bool PlayWhenReady
        {
            get { return _playWhenReady; }
            set { _playWhenReady = value; }
        }

        private bool _hasPlaybackReady = false;

        public bool HasPlaybackReady
        {
            get { return _hasPlaybackReady; }
            set { _hasPlaybackReady = value; }
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
            WithParameters = null;

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

                //if empty bypass 
                if (WithParameters == null)
                    return;

                ///Creatings commands from passed Paramaters
                //COMMAND_SETPROP
                if (WithParameters.ContainsKey("jsonEnvironmentInfo"))
                {
                    QueueCommand(COMMAND_SETPROP, WithParameters["jsonEnvironmentInfo"]);
                }

                //COMMAND_LOAD_JSON
                if (WithParameters.ContainsKey("loadedJsonData"))
                {
                    var _params = new string[2];
                    _params[0] = VideoId;
                    _params[1] = WithParameters["loadedJsonData"];

                    QueueCommand(COMMAND_LOAD_JSON, _params);
                }
                else
                {
                    //CallPlayerMethod("load", VideoId);
                    QueueCommand(COMMAND_LOAD, VideoId);
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
        }

        /// <summary>
        /// cleaning up events
        /// </summary>
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
            //Debug.WriteLine(e?.Value);
            var eventNames = getEventNames(e?.Value);

            if (eventNames != null)
            {
                foreach (var name in eventNames)
                {
                    //Debug.WriteLine(name.ToString());

                    switch (name.Value)
                    {
                        case EVENT_APIREADY:
                            {
                                ApiReady = true;
                                break;
                            }
                        case EVENT_AD_START:
                            {
                                ShowingAd = true;
                                break;
                            }
                        case EVENT_AD_END:
                            {
                                ShowingAd = false;
                                break;
                            }
                        case EVENT_PLAY:
                            {
                                VideoPaused = false;
                                PlayWhenReady = true;
                                break;
                            }
                        case EVENT_PAUSE:
                            {
                                VideoPaused = true;
                                PlayWhenReady = false;
                                break;
                            }
                        case EVENT_AD_PLAY:
                            {
                                PlayWhenReady = true;
                                break;
                            }
                        case EVENT_AD_PAUSE:
                            {
                                PlayWhenReady = false;
                                break;
                            }

                        case EVENT_PLAYBACK_READY:
                            {
                                HasPlaybackReady = true;
                                break;
                            }
                    }

                    DmWebViewMessage = name;
                }

                ExecuteQueue();
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
        /// Creates the https url
        /// </summary>
        /// <param name="videoId"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private HttpRequestMessage NewRequest(string videoId, IDictionary<string, string> parameters = null)
        {
            var message = new HttpRequestMessage(HttpMethod.Get, Url(videoId, parameters));

            ////special Headers for xbox and windows
            if (IsXbox)
            {
                message.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; Xbox; Xbox One) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.140 Safari/537.36 Edge/17.17134");
                //message.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; Xbox; Xbox One) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36 Edge/18.18988");
            }
            else
            {
                //other windows devices
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
            webView.IsTapEnabled = true;
            webView.IsDoubleTapEnabled = true;
            webView.IsHoldingEnabled = false;
            webView.Opacity = 1;

            //GetSavedCookiesInWebView();

            return webView;
        }

        /// <summary>
        /// setting a cookie in our webview
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        private void SetCookieInWebView(string key, string value)
        {
            Windows.Web.Http.Filters.HttpBaseProtocolFilter filter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
            Windows.Web.Http.HttpCookie cookie = new Windows.Web.Http.HttpCookie(key, ".dailymotion.com", "/");
            cookie.Value = value;          

            filter.CookieManager.SetCookie(cookie, false);
        }


        /// <summary>
        /// getting cookies in our webview
        /// </summary>
        private HttpCookieCollection GetSavedCookiesInWebView()
        {
            Windows.Web.Http.Filters.HttpBaseProtocolFilter filter = new Windows.Web.Http.Filters.HttpBaseProtocolFilter();
            var cookies = filter.CookieManager.GetCookies(new System.Uri("http://www.dailymotion.com"));

            return cookies;
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
            parameters["app"] = AppName;
            parameters["GK_PV5_NEON"] = "1";

            var builder = new StringBuilder(components);
            if (parameters.Any())
                builder.Append("?");
            builder.Append(String.Join("&", from p in parameters select String.Format("{0}={1}", p.Key, p.Value)));

            return new Uri(builder.ToString());
        }


        /// <summary>
        /// queue the js commands we wish to send to the player
        /// </summary>
        /// <param name="method"></param>
        /// <param name="methodParams"></param>
        public void QueueCommand(string method, object methodArguments = null)
        {
            ///remove duplicate commands             
            //none binding copy
            IEnumerator<Command> iterator = DuplicateCommandListToEnumerator();

            while (iterator.MoveNext())
            {
                //cleanup
                if (iterator.Current.methodName.Equals(method))
                {
                    mCommandList.Remove(iterator.Current);
                }
            }


            /// if we're loading a new video, cancel the stuff from before
            if (method.Equals(COMMAND_LOAD))
            {
                //reset
                //ApiReady = false;
                HasPlaybackReady = false;

                ////update iterator
                iterator = DuplicateCommandListToEnumerator();

                while (iterator.MoveNext())
                {
                    Command item = iterator.Current;

                    //foreach (Command item in mCommandList)
                    //{

                    switch (item.methodName)
                    {
                        case COMMAND_NOTIFY_LIKECHANGED:
                        case COMMAND_NOTIFY_WATCHLATERCHANGED:
                        case COMMAND_SEEK:
                        case COMMAND_PAUSE:
                        case COMMAND_PLAY:
                            mCommandList.Remove(item);
                            break;
                    }
                }
            }

            //init method & argument to command
            Command command = new Command();
            command.methodName = method;
            command.methodArguments = methodArguments;
            mCommandList.Add(command);

            ExecuteQueue();
        }

        /// <summary>
        /// Execute all the commands that are queued
        /// </summary>
        private void ExecuteQueue()
        {
            //if not ready wait
            if (!ApiReady)
            {
                return;
            }

            //if empty bypass
            if (!mCommandList.Any())
            {
                return;
            }

            IEnumerator<Command> iterator = DuplicateCommandListToEnumerator();

            while (iterator.MoveNext())
            {
                Command command = iterator.Current;

                //foreach (Command command in mCommandList)
                //{
                //check play pause, if playback not ready dont execute 
                switch (command.methodName)
                {
                    case COMMAND_PAUSE:
                    case COMMAND_PLAY:
                        if (!HasPlaybackReady)
                        {
                            continue;
                        }
                        break;

                }

                //remove before sending
                mCommandList.Remove(command);

                //send command
                sendJavascriptCommand(command);
            }
        }

        /// <summary>
        /// will converter the command to my js query
        /// </summary>
        /// <param name="command"></param>
        private void sendJavascriptCommand(Command command)
        {
            //C# 8 to update 
            //var (methodName, methodArguments) = command switch
            //    {
            //    COMMAND_CONTROLS => "api", "controls",
            //};
            //Debug.WriteLine(command.methodName);
            
            switch (command.methodName)
            {
                case COMMAND_MUTE:
                    //player.api('mute','0')
                    CallPlayerMethodV2("api", (Boolean)command.methodArguments ? "mute" : "unmute");
                    break;
 
                case COMMAND_CONTROLS:
                    //player.api('controls','0')
                    CallPlayerMethodV2("api", command.methodArguments);
                    break;

                case COMMAND_VOLUME:
                case COMMAND_TOGGLE_CONTROLS:
                case COMMAND_NOTIFYFULLSCREENCHANGED:
                    CallPlayerMethodV2("api", command.methodName);
                    break;

                case COMMAND_QUALITY:
                    CallPlayerMethodV2("setQuality", command.methodArguments);
                    break;
                case COMMAND_SEEK:
                    CallPlayerMethodV2(COMMAND_SEEK, command.methodArguments);
                    break;
 
                case COMMAND_SETPROP:
                    CallPlayerMethodV2(COMMAND_SETPROP, "neon", command.methodArguments);
                    break;

                case COMMAND_LOAD_JSON:
                    string[] arguments = command.methodArguments as string[];
                    CallPlayerMethodV2(COMMAND_LOAD, arguments[0], arguments[1]);
                    break;


                case COMMAND_PLAY:
                case COMMAND_PAUSE:
                    CallPlayerMethodV2(command.methodName, command.methodArguments);
                    break;
                default:
                    //COMMAND_LOAD
                    CallPlayerMethodV2(command.methodName, command.methodArguments);
                    break;
            }
        }

        private IEnumerator<Command> DuplicateCommandListToEnumerator()
        {
            return new List<Command>(mCommandList).GetEnumerator();
        }

        #region InvokeScriptAsync UWP

        /// <summary>
        /// Sends js commands to the webview player
        /// </summary>
        /// <param name="method"></param>
        /// <param name="param"></param>
        /// <param name="dataJson"></param>
        private async void CallPlayerMethodV2(string method, object param, object dataJson = null)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("player.");
            builder.Append(method);
            builder.Append('(');

            //when multiple params
            if (method.Equals("api") && param.GetType().Equals(typeof(string[])))
            {
                int count = 0;
                var convertedParams = param as string[];
                foreach (object o in convertedParams)
                {
                    count++;
                    //if (o.GetType().Equals(typeof(string)))
                    //    builder.Append("'" + param + "'");
                    //else
                        builder.Append("'" + o.ToString() + "'");

                    if (count < convertedParams.Length)
                    {
                        builder.Append(",");
                    }
                }
            }
            else if (param !=null)
            {
                builder.Append("'" + param.ToString() + "'");
            }

            if (dataJson != null)
            {
                builder.Append(",JSON.parse('" + dataJson + "')");
            }

            //end
            builder.Append(')');
            string js = builder.ToString();
            //js = "player.pause()";
            //if (!js.Contains("mute"))
            {
                Debug.WriteLine(js);
            }

            List<string> callingJsMethod = new List<string>();
            callingJsMethod.Add(js);

            try
            {
                await DmVideoPlayer?.InvokeScriptAsync("eval", callingJsMethod);
            }
            catch (Exception e)
            {
                HasPlayerError = true;

                string title = $"Error : {js}";
                Debug.WriteLine(title);
                //throw new PlayerException(tite , e);
            }
        }


        #endregion


        #region Command

        /// <summary>
        /// show the player controls depending on the bool
        /// </summary>
        /// <param name="show"></param>
        public void ToggleControls(bool show)
        {
            //var hasControls = show ? "1" : "0";
            var _params = new string[2];
            _params[0] = COMMAND_CONTROLS;
            //_params[1] = show ? "1" : "0";
            _params[1] = show ? "true" : "false";
            QueueCommand(COMMAND_CONTROLS, _params);
            //QueueCommand(COMMAND_TOGGLE_CONTROLS);
        }

        public void ToggleControls()
        {
            QueueCommand(COMMAND_TOGGLE_CONTROLS);
        }

        /// <summary>
        /// update the player icon to fullscreen
        /// </summary>
        public void ToggleFullscreen()
        {
            QueueCommand(COMMAND_NOTIFYFULLSCREENCHANGED);
        }

        /// <summary>
        /// start playing a video
        /// </summary>
        public void Play()
        {
            Debug.Write("PLAYER", "play");

            //calling play
            QueueCommand(COMMAND_PLAY);

            //using new command queue
            //if (IsHeroVideo)
            //{
            //    Mute();
            //}
            //else
            //{
            //    Unmute();
            //}
        }

        /// <summary>
        /// pause a video
        /// </summary>
        public void Pause()
        {
            Debug.Write("PLAYER", "pause");
            QueueCommand(COMMAND_PAUSE);
        }

        /// <summary>
        /// Set Mute or Unmute of video, internal
        /// </summary>
        /// <param name="mute"></param>
        private void mute(bool mute)
        {
            //var _params = new string[2];
            //_params[0] = COMMAND_MUTE;
            //_params[1] = mute ? "1" : "0";

            QueueCommand(COMMAND_MUTE, mute);
        }

        public void Mute()
        {
            mute(true);
        }

        public void Unmute()
        {
            mute(false);
        }

        /// <summary>
        /// set the volume of the player
        /// </summary>
        /// <param name="value"></param>
        public void Volume(double value)
        {
            if (value >= 0.0 && value <= 1.0)
            {
                string volumeValue = string.Format("setVolume({0})", value.ToString());
                QueueCommand(COMMAND_VOLUME, volumeValue);

            }
        }

        /// <summary>
        /// seek in the video +/-
        /// </summary>
        /// <param name="seconds"></param>
        public void Seek(int seconds)
        {
            //player.seek(30);             
            QueueCommand(COMMAND_SEEK, seconds);

        }

        /// <summary>
        /// set the quality of the video we would like, with HLS this is not really needed
        /// </summary>
        /// <param name="videoQuality"></param>
        public void setQuality(Qualities videoQuality)
        {
            QueueCommand(COMMAND_QUALITY, (int)videoQuality);
        }

        /// <summary>
        /// inform player that it is in fullscreen
        /// </summary>
        /// <param name="isFullScreen"></param>
        public void setFulScreen(bool isFullScreen)
        {
            QueueCommand(COMMAND_NOTIFYFULLSCREENCHANGED, isFullScreen);
        }

        #endregion



        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
