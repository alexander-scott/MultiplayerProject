using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source
{
    public interface IMessageable
    {
        MessageableComponent ComponentType { get; set; }

        List<ServerConnection> Clients { get; set; }

        void ProcessClientMessage(ServerConnection client);
    }
}
