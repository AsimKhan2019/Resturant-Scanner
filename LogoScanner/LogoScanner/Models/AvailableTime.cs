using System;

//Object to store information about the Available Times
public class AvailableTime
{
    public string Date { get; set; }
    public string Time { get; set; }
    public string RestaurantAreas { get; set; }
    public string Promotions { get; set; }
    public string DateTime { get { return Date + "," + Time; } }
    public string StringDate { get; set; }
}