using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OmegaMUD
{
    public abstract class MudPalette
    {
        public abstract string RoomNameColor { get; }
        public abstract string RoomDescriptionColor { get; }
        public abstract string RoomItemsColor { get; }
        public abstract string RoomPeopleColor { get; }
        public abstract string RoomExitsColor { get; }
        public abstract string RoomBankColor { get; }
        public abstract string RoomPeopleClearColor { get; }
    }


    public class MudPalette0 : MudPalette
    {
        public override string RoomNameColor { get { return "\x1B[1;36m"; } }
        public override string RoomDescriptionColor { get { return "\x1B[0;37;40m"; } }
        public override string RoomItemsColor { get { return "\x1B[0;36m"; } }
        public override string RoomPeopleColor { get { return "\x1B[0;35m"; } }
        public override string RoomExitsColor { get { return "\x1B[0;32m"; } }
        public override string RoomBankColor { get { return "\x1B[0;36m"; } }
        public override string RoomPeopleClearColor { get { return "\x1B[0m"; } }
    }
}
