using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gcpe.Hub.API.Helpers
{
    [Flags]
    public enum PublishOptions : int
    {
        PublishNewsArchives = 1,
        PublishNewsOnDemand = 2,
        PublishMediaContacts = 4
    }
}
