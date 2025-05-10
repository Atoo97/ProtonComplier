namespace ProtonComplierUI.Hubs
{
    using Microsoft.AspNetCore.SignalR;
    using System.Threading.Tasks;

    public class CompilerHub : Hub
    {
        public Task<string> GetConnectionId()
        {
            return Task.FromResult(Context.ConnectionId);
        }

        public async Task SendOutput(string message)
        {
            await Clients.All.SendAsync("ConsoleOutput", message);
            await Clients.All.SendAsync("RightEditorOutput", message);
            await Clients.All.SendAsync("RightEditorCodeOutput", message);
            await Clients.All.SendAsync("ErrorsAndWarningsOutput", message);
            await Clients.All.SendAsync("ResetEditor", message);
        }
    }
}