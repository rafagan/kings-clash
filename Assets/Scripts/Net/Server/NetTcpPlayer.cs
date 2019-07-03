//---------------------------------------------
//             Network
//---------------------------------------------

using System.Net;
using System.IO;

namespace Net
{
    /// <summary>
    /// Class containing information about connected players.
    /// </summary>

    public class TcpPlayer : TcpProtocol
    {
        /// <summary>
        /// Channel that the player is currently in.
        /// </summary>

        public Channel channel;

        /// <summary>
        /// UDP end point if the player has one open.
        /// </summary>

        public IPEndPoint udpEndPoint;

        /// <summary>
        /// Channel joining process involves multiple steps. It's faster to perform them all at once.
        /// </summary>

        public void FinishJoiningChannel()
        {
            Buffer buffer = Buffer.Create();

            // Step 2: Tell the player your own data and who else is in the channel
            BinaryWriter writer = buffer.BeginPacket(Packet.ResponseJoiningChannel);
            {
                writer.Write(id);
                writer.Write(string.IsNullOrEmpty(name) ? "Guest" : name);
                writer.Write(teamId);
                writer.Write(role);

                writer.Write((short)channel.players.size);

                for (int i = 0; i < channel.players.size; ++i)
                {
                    TcpPlayer tp = channel.players[i];
                    writer.Write(tp.id);
                    writer.Write(string.IsNullOrEmpty(tp.name) ? "Guest" : tp.name);
                    writer.Write(tp.teamId);
                    writer.Write(tp.role);
                    writer.Write(tp.house);
                    writer.Write(tp.isReady); 
                }
            }

            // End the first packet, but remember where it ended
            int offset = buffer.EndPacket();

            // Step 5: Inform the player of what level we're on
            //buffer.BeginPacket(Packet.ResponseLoadLevel, offset);
            //writer.Write(string.IsNullOrEmpty(channel.level) ? "" : channel.level);
            //offset = buffer.EndTcpPacketStartingAt(offset);

            // Step 9: The join process is now complete
            buffer.BeginPacket(Packet.ResponseJoinChannel, offset);
            writer.Write(true);
            offset = buffer.EndTcpPacketStartingAt(offset);

            // Send the entire buffer
            SendTcpPacket(buffer);
            buffer.Recycle();
        }
    }
}
