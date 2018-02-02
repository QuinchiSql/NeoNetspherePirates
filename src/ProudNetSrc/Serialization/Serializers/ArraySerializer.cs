using System;
using System.IO;
using BlubLib.IO;
using BlubLib.Reflection;
using BlubLib.Serialization;
using Sigil;
using Sigil.NonGeneric;

namespace ProudNetSrc.Serialization.Serializers
{
    public class ArraySerializer : ISerializerCompiler
    {
        public bool CanHandle(Type type)
        {
            throw new NotImplementedException();
        }

        public void EmitDeserialize(Emit emiter, Local value)
        {
            emiter.LoadArgument(1);
            emiter.Call(ReflectionHelper.GetMethod((BinaryReader x) => x.ReadToEnd()));
            emiter.StoreLocal(value);
        }

        public void EmitSerialize(Emit emiter, Local value)
        {
            emiter.LoadArgument(1);
            emiter.LoadLocal(value);
            emiter.CallVirtual(typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write), new[] {typeof(byte[])}));
        }
    }
}
