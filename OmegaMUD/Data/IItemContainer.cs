using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OmegaMUD.Data;
using System.Text.RegularExpressions;

namespace OmegaMUD
{
    public interface IItemContainer
    {
        IEnumerable<Item> Items { get; }
        void AddItem(Item item);
        void RemoveItem(Item item);
        void ClearInventory();

        void EquipItem(string type, int? usesLeft, Item item);
        void UnequipItem(Item item);
        Wallet Money { get; }
    }

    public static class ItemContainerExtensions
    {
        public static int GetTotalMoney(this IItemContainer container, MajorModelEntities model)
        {
            int sum = 0;
            foreach (var pair in container.Money.Dictionary)
                sum += pair.Value * model.Currencies[pair.Key];
            return sum;
        }

        public static void PopulateInventory(this IItemContainer container, MatchCollection itemMatches, MatchCollection keyMatches, MajorModelEntities model)
        {
            container.ClearInventory();

            if (itemMatches != null)
            {
                foreach (Match match in itemMatches)
                {
                    var name = match.Groups["name"].Value;
                    int quantity = 1;
                    if (match.Groups["quantity"].Success)
                        quantity = Int32.Parse(match.Groups["quantity"].Value);
                    if (model.Currencies.ContainsKey(match.Groups["name"].Value))
                    {
                        container.Money[name] = quantity;
                    }
                    else
                    {
                        var item = model.GetItem(name);
                        for (int i = 0; i < quantity; i++)
                            container.AddItem(item);

                        if (match.Groups["equipped"].Success)
                        {
                            int? readied = null;
                            if (match.Groups["readiedlength"].Success)
                                readied = Int32.Parse(match.Groups["readiedlength"].Value);
                            container.EquipItem(match.Groups["equipped"].Value, readied, item);
                        }
                    }
                }
            }

            if (keyMatches != null)
            {
                foreach (Match match in keyMatches)
                {
                    var name = match.Groups["name"].Value;
                    int quantity = 1;
                    if (match.Groups["quantity"].Success)
                        quantity = Int32.Parse(match.Groups["quantity"].Value);
                    if (quantity > 1)
                        name = name.Substring(0, name.Length - 1);  // chop off the trailing "s" if there's multiple items.
                    var item = model.GetItem(name);
                    for (int i = 0; i < quantity; i++)
                        container.AddItem(item);
                }
            }
        }
    }
}
