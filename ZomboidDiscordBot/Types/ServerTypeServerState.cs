using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ZomboidTypesLibrary
{
    public partial class ServerTypeServerState
    {
        //Enums
        public enum ServerState
        {
            [Description("Offline")]
            Offline = 0,
            [Description("Online")]
            Online = 1,
            [Description("Restarting")]
            Restart = 2,
            [Description("Unknown")]
            Unknown = 3
        }
    }
}
