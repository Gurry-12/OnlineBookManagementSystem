namespace OnlineBookManagementSystem.Core.Application.Interfaces.Analytics;

public interface IChartDataProvider
{
    string ChartType { get; }
    Task<object> GetDataAsync();
}
