# Dailymotion Player SDK for Windows UWP

This repository contains the official open source .NET SDK that allows you to embed Dailymotion Videos in your UWP application.

For a full documentation of the Player API, see [https://developer.dailymotion.com/player](https://developer.dailymotion.com/player#player-parameters)

## Installation

### Manually

Just add the Release Lib `DMVideoPlayer.dll` to your project.

## Usage

Check out the repository and open `Dailymotion_VideoPlayer_Sample.sln` for a working example of how to embed the Dailymotion Player into your app.

Also look at the `Load` methods of `DMPlayerController` for ways to embed the Dailymotion Player.

#### White-listing dailymotion.com in Package.appxmanifest

You will need to add `https://*.dailymotion.com/` to the application contentUri rules so that you can communicate with the player and it will need to have WindowsRuntimeAccess="all".

``` xml
 <Applications>
    <Application Id="App" Executable="$targetnametoken$.exe" EntryPoint="MyApp.App">
      <uap:VisualElements Mode code inhere />
      <uap:ApplicationContentUriRules>
        <uap:Rule Match="https://*.dailymotion.com/" Type="include" WindowsRuntimeAccess="all" />
      </uap:ApplicationContentUriRules>
    </Application>
  </Applications>
```
Screenshot:
![http://i.imgur.com/OgKljKF.png](http://i.imgur.com/OgKljKF.png)

 ### Queue System
 
 The Commands sent to the SDK are sent into a queue, this queue will only be executed once the SDK has recieved and APIReady from the dailymotion endpoint.  If you load another video before this event is raised the SDk will drop all previous commands.
 
 
 ### Custom User Agent 
 
 The SDK allow you to set a custom user agent using the CustomUserAgent property.
 
 ### Demo App

A demo application has been been added that can help you query our public api and then launch the dailymotion player 


 ### Feedback

We are relying on the [GitHub issues tracker](issues) for feedback. File bugs or other issues http://github.com/dailymotion/dailymotion-player-sdk-windows/issues

### TODO List

Here is what is coming in the next months:

- Improve Player SDK for Windows UWP apps

**Warning:** **This library is up-to-date and maintained if you want to use the Dailymotion Player on your native Windows UWP apps**.
