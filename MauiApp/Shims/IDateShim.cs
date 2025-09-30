namespace WorkMood.MauiApp.Shims;

//create an inteface for DateShim
public interface IDateShim
{
    DateOnly GetDate(int yearOffset);
    DateOnly GetTodayDate();
    DateTime GetToday();
    DateTime Now();
}
