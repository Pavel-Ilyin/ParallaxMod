using System;
using System.Text;
using Sandbox.ModAPI;
using VRage.Utils;

namespace JumpDriveInhibitor
{
    public class NetworkService
    {
        public static void NetworkInit()
        {
            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(42, HandleIncomingPacket);
            
        }

        private static void HandleIncomingPacket(ushort comId, byte[] msg, ulong id, bool relible)
        {
            try
            {
                var message = Encoding.ASCII.GetString(msg);
                JumpDriveInhibitorBlock.Instance.updateDef(message);
                
                
            }
            catch (Exception error)
            {
                //log area
            }
        }


        public static void SendPacket(string data)
        {
            try
            {
                var bytes = Encoding.ASCII.GetBytes(data);
                MyAPIGateway.Multiplayer.SendMessageToOthers(42, bytes, true);
                
            }
            catch (Exception error)
            {
                MyLog.Default.WriteLine($" error in Jump Inhibitor network{error}");
            }
        }

        public static void NetworkEnd()
        {
            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(42, HandleIncomingPacket);
        }    
    }
}