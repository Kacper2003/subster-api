using Hangfire.Dashboard;

namespace Subster.API.Filters;

public class AllowAllDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}
