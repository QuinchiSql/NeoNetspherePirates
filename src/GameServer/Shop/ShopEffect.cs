using System.Collections.Generic;
using System.Linq;
using NeoNetsphere.Database.Game;

namespace NeoNetsphere.Shop
{
    internal class ShopEffectGroup
    {
        public ShopEffectGroup(ShopEffectGroupDto dto)
        {
            Id = dto.Id;
            Name = dto.Name;
            Effects = dto.ShopEffects.Select(e => new ShopEffect(e)).ToList();
            MainEffect = dto.Effect;
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public IList<ShopEffect> Effects { get; set; }
        public uint MainEffect { get; set; }

        public ShopEffect GetEffect(int id)
        {
            return Effects.FirstOrDefault(effect => effect.Id == id);
        }
    }

    internal class ShopEffect
    {
        public ShopEffect(ShopEffectDto dto)
        {
            Id = dto.Id;
            Effect = dto.Effect;
        }

        public int Id { get; set; }
        public uint Effect { get; set; }
    }
}
