using System;

namespace NeoNetsphere.API
{
    internal class ChannelState
    {
        public byte? ServerId { get; set; }
        public DateTimeOffset ConnectionTime { get; set; } = DateTimeOffset.Now;
        public DateTimeOffset LastActivity { get; set; } = DateTimeOffset.Now;
    }
}
