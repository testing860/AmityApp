using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmityApp.Shared;

public static class AppConstants
{
    public const string ApiBaseUrl = "http://10.0.2.2:5009";

    public const string HubPattern = "/hubs/notifications-hub";

    public const string HubFullUrl = ApiBaseUrl + HubPattern;
}