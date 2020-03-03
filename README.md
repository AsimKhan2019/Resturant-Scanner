# Restaurant Logo Scanner 
[![build status](https://stgit.dcs.gla.ac.uk/tp3-2019-se06/se06-main/badges/master/pipeline.svg)](https://stgit.dcs.gla.ac.uk/tp3-2019-se06/se06-main/commits/master)
[![coverage](https://stgit.dcs.gla.ac.uk/tp3-2019-se06/se06-main/badges/master/coverage.svg)](https://stgit.dcs.gla.ac.uk/tp3-2019-se06/se06-main/commits/master)

Logo Scanner is a 3rd year Software Engineering project conducted at the University of Glasgow. We were given a real-life client - in our case ResDiary (https://www.resdiary.com/) - who tasked us with a 9 month long project to develop a mobile application. The premise of the project is to create an application that can recognise a restaurant logo and in near real-time, display pertinent information about the restaurant to the user. The goal of the application is to increase the dicoverability of ResDiary's restaurants.

## Project Details
Our project management processes such as issue tracking and milestones were conducted within the GitLab environment.

### Meet the Team
* Ollie Gardner | 2310049g@student.gla.ac.uk  
* Patrick Devanney | 2329979d@student.gla.ac.uk  
* Andreas Chari | 2293299c@student.gla.ac.uk  
* Pasuta Paopun | 2506138p@student.gla.ac.uk  
* Lucia Cangarova | 2330954c@student.gla.ac.uk  
* Peter Macaldowie (Team Coach) | 2258785m@student.gla.ac.uk  

### Project Specification
* The application must be usable on an iOS/Android mobile phone. There is no requirement to work on both but it would be ideal if it could.
* The application must be able to scan a logo using the phone's camera. 
* The application must present the following information to the user, where available:
    * Cuisine type(s)
    * Overall review score including total number of reviews
    * Price point
    * If there is a table available NOW
    * The next 3 available timeslots
    * A link to the most appropriate PDF menu
* The application must use the ResDiary consumer API

### Technologies
* Xamarin | a Microsoft platform for building Android and iOS apps with .NET and C# | https://dotnet.microsoft.com/apps/xamarin
* Custom Vision | image recognition and training model | https://www.customvision.ai/

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

## App Demo
![App Demo Video](https://www.youtube.com/?gl=GB&hl=en-GB)

## License
See [LICENSE](LICENSE).
