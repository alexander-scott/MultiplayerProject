using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerProject.Source
{
    public interface INetworkedObject
    {
        string NetworkID { get; set; }
    }
}
