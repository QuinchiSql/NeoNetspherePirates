using System.IO;
using BlubLib.IO;

namespace NeoNetsphere.Resource.Scene.Chunks
{
    public class BoneSystemChunk : SceneChunk
    {
        public BoneSystemChunk(SceneContainer container)
            : base(container)
        {
        }

        public override ChunkType ChunkType => ChunkType.BoneSystem;

        public int Unk3 { get; set; }

        public override void Serialize(Stream stream)
        {
            base.Serialize(stream);

            using (var w = stream.ToBinaryWriter(true))
            {
                w.Write(Unk3);
            }
        }

        public override void Deserialize(Stream stream)
        {
            base.Deserialize(stream);

            using (var r = stream.ToBinaryReader(true))
            {
                Unk3 = r.ReadInt32();
            }
        }
    }
}
