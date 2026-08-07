using Attendance.Pages;

namespace Attendance;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(nameof(ScheduleEditPage), typeof(ScheduleEditPage));
    }
}
