using DotNetty.Common.Utilities;

namespace NeoNetsphere.API
{
    internal static class ChannelAttributes
    {
        public static readonly AttributeKey<ChannelState> State = AttributeKey<ChannelState>.ValueOf(nameof(State));
    }
}
