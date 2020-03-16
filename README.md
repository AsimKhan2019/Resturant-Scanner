# Restaurant Logo Scanner 
[![build status](https://stgit.dcs.gla.ac.uk/tp3-2019-se06/se06-main/badges/master/pipeline.svg)](https://stgit.dcs.gla.ac.uk/tp3-2019-se06/se06-main/commits/master)
[![coverage](https://stgit.dcs.gla.ac.uk/tp3-2019-se06/se06-main/badges/master/coverage.svg)](https://stgit.dcs.gla.ac.uk/tp3-2019-se06/se06-main/commits/master)
[![MIT License](https://img.shields.io/apm/l/atomic-design-ui.svg?)](https://stgit.dcs.gla.ac.uk/tp3-2019-se06/se06-main/-/blob/master/LICENSE)

Restaurant Logo Scanner is a 3rd year Software Engineering project conducted at the University of Glasgow under the Team Project 3 (TP3) course.

> The TP3 and PSD3 courses are an integral and distinctive part of any Glasgow Computing Science degree. They allows students to experience ﬁrst hand the many issues concerning eﬀective teamwork as well as the various aspects of the software lifecycle in some depth. The TP3 course lasts the whole academic year. Each team will work with a real world project customer from the problem domain to negotiate the requirements and agreed schedule for their individual project

Our customer was **ResDiary** (https://www.resdiary.com/) who are one of the world's leading providers of online restaurant reservations, seating over 13.9 million diners per month across more than 8,100 restaurants in 59 countries. ResDiary tasked us with developing a mobile application that can recognise a restaurant logo and in near real-time, display pertinent information about the restaurant to the user. The goal of the application was to increase the dicoverability of ResDiary's restaurants.

## Project Details
Our project management processes such as issue tracking and milestones were conducted within the GitLab environment. Throughout the project, we used Slack as our method of communication within the team and with the customer.

### Contributors
* Ollie Gardner • 2310049g@student.gla.ac.uk  
* Patrick Devanney • 2329979d@student.gla.ac.uk  
* Andreas Chari • 2293299c@student.gla.ac.uk  
* Pasuta Paopun • 2506138p@student.gla.ac.uk  
* Lucia Cangarova • 2330954c@student.gla.ac.uk  
* Peter Macaldowie (Team Coach) • 2258785m@student.gla.ac.uk  

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

### Features
* Functional on both Android and iOS
* Ability to scan a restaurant logo and for this to be recognised by the app
* Maps integration
* Call, email, view website from within the app
* View information about the restaurant
* Ability to choose number of people/slots and see available time slots
* Book a table
* View the restaurant's menu(s)
* View and interact with customer reviews
* Light/dark mode which links in to system settings

### Technologies
The application was developed with:
* Xamarin • a Microsoft platform for building Android and iOS apps with .NET and C# • https://dotnet.microsoft.com/apps/xamarin
* Custom Vision • image recognition and training model • https://www.customvision.ai/
* Various Xamarin NuGet Plugins:
  * NUnit • test suite • https://www.nuget.org/packages/NUnit/
  * Plugin.Permissions • see if users have granted or denied permission • https://www.nuget.org/packages/Plugin.Permissions/6.0.0-beta
  * Rg.Plugins.Popup • allows you to open any page as a popup • https://www.nuget.org/packages/Rg.Plugins.Popup/
  * SuaveControls.FloatingActionButton • create a FloatingActionButton for both Android and iOS • https://www.nuget.org/packages/SuaveControls.FloatingActionButton/2018.3.1-pre1
  * Syncfusion.Xamarin.SfListView • create custom a ListView • https://www.nuget.org/packages/Syncfusion.Xamarin.SfListView/
  * Syncfusion.Xamarin.SfPdfViewer • PDF viewer control • https://www.nuget.org/packages/Syncfusion.Xamarin.SfPdfViewer/
  * Syncfusion.Xamarin.SfRating • star rating bar • https://www.nuget.org/packages/Syncfusion.Xamarin.SfRating/
  * Xam.Plugin.Connectivity • network connectivity information • https://www.nuget.org/packages/Xam.Plugin.Connectivity/4.0.0.190-beta
  * Xam.Plugins.Forms.ImageCircle • turn your images into elegant circle images • https://www.nuget.org/packages/Xam.Plugins.Forms.ImageCircle/3.1.1.1-beta
  * Xamarin.Forms.Maps • maps models and renderers • https://www.nuget.org/packages/Xamarin.Forms.Maps/4.6.0.379-pre1
* ResDiary Consumer API • retrieve restaurant information • https://sales.resdiary.com/consumer-api/
* Google Maps API • retrieve API key for Xamarin.Forms.Maps plugin • https://developers.google.com/maps/documentation

## App Demo
### Video
#### Android
![Android App Demo Video](https://www.youtube.com/?gl=GB&hl=en-GB)
#### iOS
![iOS App Demo Video](https://www.youtube.com/?gl=GB&hl=en-GB)

### Screenshots
#### Android
| ![Screenshot]() | ![Screenshot]() | ![Screenshot]() | ![Screenshot]() | ![Screenshot]() | ![Screenshot]()
|:---:|:---:|:---:|:---:|:---:|:---:|
| Screenshot 1 | Screenshot 2 | Screenshot3 | Screenshot 4 | Screenshot 5 | Screenshot 6|

#### iOS
| ![Screenshot]() | ![Screenshot]() | ![Screenshot]() | ![Screenshot]() | ![Screenshot]() | ![Screenshot]()
|:---:|:---:|:---:|:---:|:---:|:---:|
| Screenshot 1 | Screenshot 2 | Screenshot3 | Screenshot 4 | Screenshot 5 | Screenshot 6|

## User Guide
The user guide has been written with the goal of informing our customer, ResDiary, on how to take over the project if they wish to continue development. See [USERGUIDE.md](USERGUIDE.md) file for details.

## License
This project is licensed under the MIT License - see [LICENSE](LICENSE) file for details.
