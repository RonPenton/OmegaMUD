using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OmegaMUD.Data;

namespace OmegaMUD
{
    /// <summary>
    /// A class representing a room that the user has seen, either by visiting or by looking
    /// </summary>
    public class SeenRoom : IItemContainer
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> People { get; private set; }
        public List<string> Exits { get; private set; }

        public List<Room> PotentialMatches { get; set; }
        public Room Match
        {
            get
            {
                if (PotentialMatches.Count == 1)
                    return PotentialMatches.First();
                return null;
            }
        }
        public SeenRoom[] AdjacentRooms { get; private set; }

        public Room GetMatch(RoomNumber room)
        {
            return PotentialMatches.SingleOrDefault(x => x.RoomNumber == room);
        }

        public bool HasMatch(RoomNumber room)
        {
            return PotentialMatches.Any(x => x.RoomNumber == room);
        }

        public SeenRoom()
        {
            People = new List<string>();
            Exits = new List<string>();
            AdjacentRooms = new SeenRoom[10];
            Money = new Wallet();
        }

        List<Item> _items = new List<Item>();
        public IEnumerable<Item> Items { get { return _items; } }
        public Wallet Money { get; private set; }
        public void AddItem(Item item) { _items.Add(item); }
        public void RemoveItem(Item item) { _items.Remove(item); }
        public void ClearInventory() { _items.Clear(); Money.Clear(); }

        // cannot equip or unequip items, so throw exceptions.
        public void EquipItem(string type, int? usesLeft, Item item) { throw new InvalidOperationException(); }
        public void UnequipItem(Item item) { throw new InvalidOperationException(); }
    }
}
