# User Guide

## Code Base
Xamarin facilitates the development of Android and iOS applications by providing the Xamarin.iOS and Mono.Android libraries. These libraries are built on top of the Mono .NET framework and bridge the gap between the application and the platform specific APIs.

### Android
An Android application is a group of activities, navigable using intents, that provide the code that runs the application. The entry point of the application is the activity whose `MainLauncher` property is set to true, which is the `MainActivity.cs` by default. Activities that provide a view have an associated layout template that is made up of view controls. Activities and view controls reference the following resources:
* layouts – view templates loaded by activities.
* drawables – icons, images etc.
* values – centralised location for string values.
* menus – templates for menu structures.

The `Resource.designer.cs` class provides an index of identifiers for all the resources in the application. This class is referenced by activities and view controls to create an instance for use in the given context.

![Android Structure](https://capgemini.github.io/images/2018-08-03-designing-mobile-cross-platform-applications-with-xamarin/NativeAndroidArchitecture.png)

### iOS
The application is made up of several view controller classes and associated views, collectively known as scenes, that are loaded into the main application window. View controllers are grouped into storyboards with each storyboard having an initial view controller. Views are made up of a view controls used for display or user interaction.

The entry point for the application is the `Main.cs` class that instantiates the specified `AppDelegate.cs` class, which loads the initial view controller of the default storyboard set in the `Info.plist` configuration file. Resources such as images, videos etc are referenced from the `Resources` and `Assets.xcassets` folders by view controllers and view controls. The `AppDelegate.cs` class includes delegates that handle application events and the view controllers handle the lifecycle for a given view.

![iOS Structure](https://capgemini.github.io/images/2018-08-03-designing-mobile-cross-platform-applications-with-xamarin/NativeIOSArchitecture.png)

### Cross Platform Test

## External Services
The application uses multiple 3rd party services that are essential in order for the app to run.

### Git-Secret
Git-Secret (https://git-secret.io/) is used in order to store an encrypted credentials file within the repository. Follow these steps in order to decyrpt the data:
1. Install git-secret by following the installation tutorial (https://git-secret.io/installation)
2. Use `git secret reveal` to decrypt the data - this won't be an issue if you are installing the app from an APK
3. If the previous step fails, then it is most likely that your public key has not been added to the project - please ask a team member to do this for you
4. After the data is decrypted you will be able to see the `credentials.json` file - right click this file and make sure the `Build Action` is set to `EmbeddedResource`

### ResDiary API
The ResDiary consumer API began as an internal API supporting the resdiary.com portal. Everything (and more) that you see on resdiary.com can be replicated via this API. The primary purpose of the consumer API is to supply everything required for a ‘consumer facing’ restaurant discovery search and book user experience. The core features provided are:
* Location Availability Searches
* Restaurant Online Profiles
* Restaurant Availability Searches
* Create-update-cancel Bookings
* Create-update Customer Profiles

The API docs can be found here - https://login.rdbranch.com/Admin/ApiAccount/Documentation

### Custom Vision
Custom vision is an image recognition and training model platform which we are using to store the Restaurant Logos on.

## Running the App
### Android

### iOs

## Testing
### CI Pipeline

### Test Restaurants
Folder of logos in /logos

Google maps