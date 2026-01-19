namespace Shared.ServerApp.Model
{
    public readonly struct ObtainItemKey
    {
        public ObtainItemKey(byte type, long id,string uniqueKey="")
        {
            Type = type;
            Id = id;
            UniqueKey = uniqueKey;
        }

        public byte Type { get; }
        public long Id { get; }
        public string UniqueKey { get;}
    }
}