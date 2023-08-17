using Microsoft.AspNetCore.SignalR;

namespace WebDiscordBack.Hubs
{
    public class ChatHub : Hub
    {
        private readonly string _botUser;
        private readonly IDictionary<string, UserConnection> _connections;

        public ChatHub(IDictionary<string, UserConnection> connections)
        {
            _botUser = "MyChat Bot";
            _connections = connections;
        }

        public async Task SendMessage(string message)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection connection))
            {
                await Clients.Group(connection.Room)
                    .SendAsync("ReceiveMessage", connection.User, message);
            }
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (_connections.TryGetValue(Context.ConnectionId, out UserConnection connection))
            {
                _connections.Remove(Context.ConnectionId);
                Clients.Group(connection.Room)
                    .SendAsync("ReceiveMessage", $"{connection.Room} room", $"{connection.User} has left");

                SendConnectedUsers(connection.Room);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(UserConnection connection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, connection.Room);

            _connections[Context.ConnectionId] = connection;

            await Clients.Group(connection.Room).SendAsync("ReceiveMessage", $"{connection.Room} room",
                $"{connection.User} has joined to {connection.Room}");

            await SendConnectedUsers(connection.Room);
        }

        public Task SendConnectedUsers(string room)
        {
            var users = _connections.Values
                .Where(c => c.Room == room)
                .Select(c => c.User);
            return Clients.Group(room).SendAsync("UsersInRoom", users);
        }
    }
}
