using System.IO;
using System.Numerics;
using BlubLib.IO;

namespace NeoNetsphere.Resource.Scene.Chunks
{
    public abstract class SceneChunk : IManualSerializer
    {
        protected SceneChunk(SceneContainer container)
        {
            Name = "";
            SubName = "";
            Unk1 = 0.1f;
            Unk2 = 0.0f;
            Matrix = Matrix4x4.Identity;
            Container = container;
        }

        public SceneContainer Container { get; }

        public abstract ChunkType ChunkType { get; }
        public string Name { get; set; }
        public string SubName { get; set; }

        public float Unk1 { get; set; }
        public Matrix4x4 Matrix { get; set; }
        public float Unk2 { get; set; }

        public virtual void Serialize(Stream stream)
        {
            using (var w = stream.ToBinaryWriter(true))
            {
                w.Write(Unk1);
                w.Write(Matrix);
            }
        }

        public virtual void Deserialize(Stream stream)
        {
            using (var r = stream.ToBinaryReader(true))
            {
                Unk1 = r.ReadSingle();
                Matrix = r.ReadMatrix();
            }
        }

        public override string ToString()
        {
            return Name + " - " + SubName;
        }
    }
}
