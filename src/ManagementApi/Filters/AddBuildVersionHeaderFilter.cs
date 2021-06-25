using Application.Common.Config;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ManagementApi.Filters
{
    public class AddBuildVersionHeaderFilter : IResultFilter
    {
        private readonly string _buildVersion;

        public AddBuildVersionHeaderFilter(IAppConfiguration configuration)
        {
            _buildVersion = configuration.BuildVersion;
        }

        public void OnResultExecuted(ResultExecutedContext context)
        {
            //// do nothing
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            context.HttpContext.Response.Headers.Add("X-build-version", _buildVersion);
        }
    }
}
