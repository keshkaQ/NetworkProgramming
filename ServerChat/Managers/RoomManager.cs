using System.Collections.Concurrent;

namespace ServerChat.Managers
{
    public class RoomManager
    {
        private readonly ConcurrentDictionary<string, Room> _rooms = new();
        private readonly ConcurrentDictionary<string, string> _userRooms = new();

        public class Room
        {
            public string RoomId { get; set; }
            public string RoomName { get; set; }
            public string Creator { get; set; }
            public DateTime CreatedAt { get; set; }
            public HashSet<string> Users { get; set; } = new();
        }

        public Room CreateRoom(string roomName, string creator)
        {
            // Проверяем, существует ли комната с таким именем
            if (_rooms.Values.Any(r => r.RoomName.Equals(roomName, StringComparison.OrdinalIgnoreCase)))
                return null;

            var roomId = Guid.NewGuid().ToString();
            var room = new Room
            {
                RoomId = roomId,
                RoomName = roomName,
                Creator = creator,
                CreatedAt = DateTime.Now
            };

            _rooms[roomId] = room;
            return room;
        }

        public Room GetRoom(string roomId)
        {
            return _rooms.TryGetValue(roomId, out var room) ? room : null;
        }

        public bool AddUserToRoom(string roomId, string username)
        {
            if (_rooms.TryGetValue(roomId, out var room))
            {
                room.Users.Add(username);
                _userRooms[username] = roomId;
                return true;
            }
            return false;
        }

        public bool RemoveUserFromRoom(string roomId, string username)
        {
            if (_rooms.TryGetValue(roomId, out var room))
            {
                room.Users.Remove(username);
                _userRooms.TryRemove(username, out _);
                return true;
            }
            return false;
        }

        public Room GetUserRoom(string username)
        {
            return _userRooms.TryGetValue(username, out var roomId) ? GetRoom(roomId) : null;
        }

        public void RemoveRoom(string roomId)
        {
            _rooms.TryRemove(roomId, out _);
        }

        public List<Room> GetAllRooms()
        {
            return _rooms.Values.ToList();
        }
    }
}
