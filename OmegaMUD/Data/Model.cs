using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegaMUD
{
    public partial class MajorModelEntities
    {
        private Dictionary<RoomNumber, Room> _cachedMap;
        private Dictionary<String, List<Item>> _cachedItemsByName;
        private Dictionary<int, Item> _cachedItemsByID;

        public void PreCache()
        {
            _cachedMap = Rooms.ToDictionary(x => x.RoomNumber);
            _cachedItemsByID = Items.ToDictionary(x => x.Number);

            _cachedItemsByName = (from item in _cachedItemsByID.Values
                                  group item by item.Name into groups
                                  select groups).ToDictionary(x => x.Key, y => y.ToList());
            
        }

        public Room GetRoom(RoomNumber room)
        {
            return _cachedMap[room];
        }

        public Item GetItem(string name)
        {
            List<Item> item = null;
            if (!_cachedItemsByName.TryGetValue(name, out item))
            {
                return null;
                // TODO: create new item here
            }
            return item.First();
        }

        public Item GetItem(int id)
        {
            return _cachedItemsByID[id];
        }
    }
}
