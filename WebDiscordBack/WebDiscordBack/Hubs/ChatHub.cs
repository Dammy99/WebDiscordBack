using Microsoft.AspNetCore.SignalR;
using System.Linq;

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

                Clients.Group(connection.Room)
                    .SendAsync("ReceiveMessage", $"{connection.Room} room", $"{connection.User} has left");
                if (UserCams.list.ContainsKey(Context.ConnectionId))
                {
                    Clients.Group(connection.Room)
                        .SendAsync("TurnOffCamera", UserCams.list[Context.ConnectionId]);
                }

                _connections.Remove(Context.ConnectionId);
                SendConnectedUsers(connection.Room);

                if(_connections.Values.Count == 0)
                {
                    UserCams.list.Clear();
                }
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(UserConnection connection)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, connection.Room);

            _connections[Context.ConnectionId] = connection;

            await Clients.Group(connection.Room).SendAsync("ReceiveMessage", $"{connection.Room} room",
                $"{connection.User} has joined to {connection.Room}");

            await Clients.Group(connection.Room).SendAsync("UsersInRoom", connection.User);

            await SendConnectedUsers(connection.Room);
        }

        public Task SendConnectedUsers(string room)
        {
            IEnumerable<string> users;

            users = _connections.Values
            .Where(c => c.Room == room)
            .Select(c => c.User);

            return Clients.Group(room).SendAsync("UsersInRoom", users);
        }

        public async Task SendTurnOnCamera(string room, string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, room);
            await Clients.Group(room).SendAsync("TurnOnCamera", userId);
        }

        public async Task TurnOnCamera(UserConnection connection)
        {
            if (!UserCams.list.ContainsKey(Context.ConnectionId))
            {
                UserCams.list.Add(Context.ConnectionId, connection.User);
            }

            await Clients.Group(connection.Room).SendAsync("TurnOnCameraAllCams", connection.User);
        }

        public async Task TurnOffLeavedCamera(UserConnection connection)
        {
            if (UserCams.list.ContainsKey(Context.ConnectionId))
            {
                await Clients.Group(connection.Room)
                    .SendAsync("TurnOffCamera", UserCams.list[Context.ConnectionId]);
            }
            UserCams.list.Remove(Context.ConnectionId);
        }
    }
}
