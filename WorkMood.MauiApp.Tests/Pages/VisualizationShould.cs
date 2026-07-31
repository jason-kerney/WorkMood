using System.Reflection;
using WorkMood.MauiApp.Pages;
using Xunit;

namespace WorkMood.MauiApp.Tests.Pages;

public class VisualizationShould
{
    [Fact]
    public void NotExposeImperativeChartCompositionMethods()
    {
        var type = typeof(Visualization);
        var publicAndNonPublicInstance = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        Assert.Null(type.GetMethod("CreateVisualization", publicAndNonPublicInstance));
        Assert.Null(type.GetMethod("CreateLineGraphView", publicAndNonPublicInstance));
        Assert.Null(type.GetMethod("OnViewModelPropertyChanged", publicAndNonPublicInstance));
    }
}
