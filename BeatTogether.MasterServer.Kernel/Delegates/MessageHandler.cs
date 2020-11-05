using BeatTogether.MasterServer.Kernel.Models;
using BeatTogether.MasterServer.Messaging.Abstractions.Messages;

namespace BeatTogether.MasterServer.Kernel.Delegates
{
    public delegate void MessageHandler(Session session, IMessage message, ResponseCallback responseCallback);
}
