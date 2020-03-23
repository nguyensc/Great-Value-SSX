using System;
using System.Collections.Generic;
using System.Text;

namespace SurfsUpServer
{
    class gameLogic
    {
        public static void Update()
        {
            foreach (client cli in server.clients.Values)
            {
                if (cli.player != null)
                {
                    cli.player.Update();
                }        
            }
            threadManager.UpdateMain();
        }
    }
}
