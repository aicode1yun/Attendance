using System.Collections.ObjectModel;
using Attendance.Interfaces;
using Attendance.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Attendance.ViewModels;

public partial class LogsViewModel : BaseViewModel
{
    private readonly IExecutionLogRepository _logRepository;
    private List<ExecutionLog> _allLogs = new();

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private string selectedFilter = "All";

    [ObservableProperty]
    private bool isEmpty;

    public ObservableCollection<string> Filters { get; } = new() { "All", "Morning", "Evening", "Failed" };

    public ObservableCollection<ExecutionLog> Logs { get; } = new();

    public LogsViewModel(IExecutionLogRepository logRepository)
    {
        _logRepository = logRepository;
        Title = "Execution Logs";
    }

    [RelayCommand]
    private async Task AppearingAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            ClearError();
            _allLogs = await _logRepository.GetAllAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            SetError($"Unable to load logs: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedFilterChanged(string value) => ApplyFilter();

    private void ApplyFilter()
    {
        IEnumerable<ExecutionLog> filtered = _allLogs;

        filtered = SelectedFilter switch
        {
            "Morning" => filtered.Where(l => l.Session == ExecutionSession.Morning),
            "Evening" => filtered.Where(l => l.Session == ExecutionSession.Evening),
            "Failed" => filtered.Where(l => l.Result == ExecutionResult.Failed),
            _ => filtered
        };

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var term = SearchText.Trim();
            filtered = filtered.Where(l =>
                l.RequestId.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (l.ErrorMessage?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false) ||
                l.Date.ToString("d").Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        Logs.Clear();
        foreach (var log in filtered)
            Logs.Add(log);

        IsEmpty = Logs.Count == 0;
    }
}
