using System.Threading.Tasks;
using UOToolBox.Models;

namespace UOToolBox.Interface
{
    public interface IChatClient
    {
        Task ReceiveMessage(ChatMessage message);
    }
}