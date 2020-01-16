#tool nuget:?package=NUnit.ConsoleRunner&version=3.4.0
#addin "Cake.FileHelpers"
#addin "Cake.Xamarin"

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Debug");

var solutionFile = File("../LogoScanner.sln");
var androidProject = File("../LogoScanner/LogoScanner.Android/LogoScanner.Android.csproj");
var androidBin = Directory("../LogoScanner/LogoScanner.Android/bin") + Directory(configuration);

Task("Clean")
.Does(() =>
{
CleanDirectory(androidBin);
CleanDirectory(iOSBin);
});

Task("Restore-NuGet")
.IsDependentOn("Clean")
.Does(() =>
{
NuGetRestore(solutionFile);
});

Task("Build-Android")
.Does(() =>
{
XBuild(androidProject, settings =>
settings.SetConfiguration(configuration)
.WithProperty("AndroidSdkDirectory", "/android/sdk")
.WithTarget("SignAndroidPackage"));
});

Task("Build-tests")
.IsDependentOn("Build-Android")
.Does(() =>
{
var parsedSolution = ParseSolution(solutionFile);

foreach(var project in parsedSolution.Projects)
{
if(project.Name.EndsWith("Tests"))
{
Information("Start Building Test: " + project.Name);
XBuild(project.Path, settings => settings.SetConfiguration(configuration));
}
} 
});

Task("Run-unit-tests")
.Does(() =>
{
NUnit3("../**/bin/" + configuration + "/*.Tests.dll", new NUnit3Settings {
NoResults = true
});
});

Task("Default")
.IsDependentOn("Run-unit-tests");

RunTarget(target);

