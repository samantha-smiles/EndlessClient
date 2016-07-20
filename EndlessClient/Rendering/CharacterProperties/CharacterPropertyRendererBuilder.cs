﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Collections.Generic;
using System.Linq;
using EOLib;
using EOLib.Domain.Character;
using EOLib.IO;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.Rendering.CharacterProperties
{
    public class CharacterPropertyRendererBuilder
    {
        private readonly SpriteBatch _spriteBatch;
        private readonly ICharacterRenderProperties _renderProperties;
        private readonly ICharacterTextures _textures;
        private readonly IEIFFileProvider _eifFileProvider;

        public CharacterPropertyRendererBuilder(SpriteBatch spriteBatch,
                                                ICharacterRenderProperties renderProperties,
                                                ICharacterTextures textures,
                                                IEIFFileProvider eifFileProvider)
        {
            _spriteBatch = spriteBatch;
            _renderProperties = renderProperties;
            _textures = textures;
            _eifFileProvider = eifFileProvider;
        }

        public IEnumerable<ICharacterPropertyRenderer> BuildList(bool isShieldOnBack)
        {
            var rendererList = new List<ICharacterPropertyRenderer>();

            if (ShieldEquipped() && IsShieldBehindCharacter(isShieldOnBack))
                rendererList.Add(new ShieldRenderer(_spriteBatch, _renderProperties, _textures.Shield));

            if (WeaponEquipped() && IsWeaponBehindCharacter())
                rendererList.Add(new WeaponRenderer(_spriteBatch, _renderProperties, _textures.Weapon, ItemFile));

            rendererList.Add(new SkinRenderer(_spriteBatch, _renderProperties, _textures.Skin, ItemFile));
            if (IsCharacterDoingEmote())
            {
                rendererList.Add(new FaceRenderer(_spriteBatch, _renderProperties, _textures.Face, ItemFile));
                rendererList.Add(new EmoteRenderer(_spriteBatch, _renderProperties, _textures.Emote, ItemFile));
            }

            if (BootsEquipped())
                rendererList.Add(new BootsRenderer(_spriteBatch, _renderProperties, _textures.Boots, ItemFile));

            if (ArmorEquipped())
                rendererList.Add(new ArmorRenderer(_spriteBatch, _renderProperties, _textures.Armor, ItemFile));

            if (WeaponEquipped() && !rendererList.OfType<WeaponRenderer>().Any())
                rendererList.Add(new WeaponRenderer(_spriteBatch, _renderProperties, _textures.Weapon, ItemFile));

            var hairOnTopOfHat = new List<ICharacterPropertyRenderer>();
            if (HatEquipped())
                hairOnTopOfHat.Add(new HatRenderer(_spriteBatch, _renderProperties, _textures.Hat, ItemFile));
            if (!IsBald())
                hairOnTopOfHat.Add(new HairRenderer(_spriteBatch, _renderProperties, _textures.Hair, ItemFile));
            if (hairOnTopOfHat.Any())
                rendererList.AddRange(IsHairOnTopOfHat() ? hairOnTopOfHat : hairOnTopOfHat.ToArray().Reverse());

            if (ShieldEquipped() && !rendererList.OfType<ShieldRenderer>().Any())
                rendererList.Add(new ShieldRenderer(_spriteBatch, _renderProperties, _textures.Shield));

            return rendererList;
        }

        private bool IsShieldBehindCharacter(bool isShieldOnBack)
        {
            return _renderProperties.IsFacing(EODirection.Right, EODirection.Down) && isShieldOnBack;
        }

        private bool IsWeaponBehindCharacter()
        {
             var weaponInfo = ItemFile.Data.SingleOrDefault(
                x => x.Type == ItemType.Weapon &&
                     x.DollGraphic == _renderProperties.WeaponGraphic);

            var pass1 = _renderProperties.AttackFrame < 2;
            var pass2 = _renderProperties.IsFacing(EODirection.Up, EODirection.Left);
            var pass3 = weaponInfo == null || weaponInfo.SubType == ItemSubType.Ranged;

            return pass1 || pass2 || pass3;
        }

        private bool IsCharacterDoingEmote()
        {
            return _renderProperties.CurrentAction == CharacterActionState.Emote &&
                   _renderProperties.EmoteFrame > 0;
        }

        private bool IsHairOnTopOfHat()
        {
            //todo: i might have this backwards...

            var hatInfo = ItemFile.Data.SingleOrDefault(
                x => x.Type == ItemType.Hat &&
                     x.DollGraphic == _renderProperties.HatGraphic);

            return hatInfo != null && hatInfo.SubType == ItemSubType.FaceMask;
        }

        private bool BootsEquipped()
        {
            return _renderProperties.BootsGraphic != 0 && _textures.Boots != null;
        }

        private bool ArmorEquipped()
        {
            return _renderProperties.ArmorGraphic != 0 && _textures.Armor != null;
        }

        private bool HatEquipped()
        {
            return _renderProperties.HatGraphic != 0 && _textures.Hat != null;
        }

        private bool ShieldEquipped()
        {

            return _renderProperties.ShieldGraphic != 0 && _textures.Shield != null;
        }

        private bool WeaponEquipped()
        {
            return _renderProperties.WeaponGraphic != 0 && _textures.Weapon != null;
        }

        private bool IsBald()
        {
            return _textures.Hair == null || _renderProperties.HairStyle == 0;
        }

        private IPubFile<EIFRecord> ItemFile
        {
            get { return _eifFileProvider.EIFFile ?? new EIFFile(); }
        }
    }
}
