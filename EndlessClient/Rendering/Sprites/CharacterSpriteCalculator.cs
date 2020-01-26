﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Linq;
using AutomaticTypeMapper;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Graphics;
using EOLib.IO;
using EOLib.IO.Extensions;
using EOLib.IO.Pub;
using EOLib.IO.Repositories;
using EOLib.Net.API;
using Microsoft.Xna.Framework;

namespace EndlessClient.Rendering.Sprites
{
    [MappedType(BaseType = typeof(ICharacterSpriteCalculator))]
    public class CharacterSpriteCalculator : ICharacterSpriteCalculator
    {
        private readonly INativeGraphicsManager _gfxManager;
        private readonly IEIFFileProvider _eifFileProvider;

        public CharacterSpriteCalculator(INativeGraphicsManager gfxManager,
                                         IEIFFileProvider eifFileProvider)
        {
            _gfxManager = gfxManager;
            _eifFileProvider = eifFileProvider;
        }

        public ISpriteSheet GetBootsTexture(ICharacterRenderProperties characterRenderProperties)
        {
            if (characterRenderProperties.BootsGraphic == 0)
                return new EmptySpriteSheet();

            var type = BootsSpriteType.Standing;
            switch (characterRenderProperties.CurrentAction)
            {
                case CharacterActionState.Walking:
                    switch (characterRenderProperties.WalkFrame)
                    {
                        case 1: type = BootsSpriteType.WalkFrame1; break;
                        case 2: type = BootsSpriteType.WalkFrame2; break;
                        case 3: type = BootsSpriteType.WalkFrame3; break;
                        case 4: type = BootsSpriteType.WalkFrame4; break;
                    }
                    break;
                case CharacterActionState.Attacking:
                    if (!BowIsEquipped(characterRenderProperties) && characterRenderProperties.AttackFrame == 2 ||
                        BowIsEquipped(characterRenderProperties) && characterRenderProperties.AttackFrame == 1)
                        type = BootsSpriteType.Attack;
                    break;
                case CharacterActionState.Sitting:
                    switch (characterRenderProperties.SitState)
                    {
                        case SitState.Chair: type = BootsSpriteType.SitChair; break;
                        case SitState.Floor: type = BootsSpriteType.SitGround; break;
                    }
                    break;
            }

            var gfxFile = characterRenderProperties.Gender == 0 ? GFXTypes.FemaleShoes : GFXTypes.MaleShoes;

            var offset = GetOffsetBasedOnState(type) * GetBaseOffsetFromDirection(characterRenderProperties.Direction);
            var baseBootGraphic = GetBaseBootGraphic(characterRenderProperties.BootsGraphic);
            var gfxNumber = baseBootGraphic + (int)type + offset;

            return new SpriteSheet(_gfxManager.TextureFromResource(gfxFile, gfxNumber, true));
        }

        public ISpriteSheet GetArmorTexture(ICharacterRenderProperties characterRenderProperties)
        {
            if (characterRenderProperties.ArmorGraphic == 0)
                return new EmptySpriteSheet();

            var type = ArmorShieldSpriteType.Standing;
            switch (characterRenderProperties.CurrentAction)
            {
                case CharacterActionState.Walking:
                    switch (characterRenderProperties.WalkFrame)
                    {
                        case 1: type = ArmorShieldSpriteType.WalkFrame1; break;
                        case 2: type = ArmorShieldSpriteType.WalkFrame2; break;
                        case 3: type = ArmorShieldSpriteType.WalkFrame3; break;
                        case 4: type = ArmorShieldSpriteType.WalkFrame4; break;
                    }
                    break;
                case CharacterActionState.Attacking:
                    if (BowIsEquipped(characterRenderProperties))
                    {
                        switch (characterRenderProperties.AttackFrame)
                        {
                            case 1: type = ArmorShieldSpriteType.Bow; break;
                            case 2: type = ArmorShieldSpriteType.Standing; break;
                        }
                    }
                    else
                    {
                        switch (characterRenderProperties.AttackFrame)
                        {
                            case 1: type = ArmorShieldSpriteType.PunchFrame1; break;
                            case 2: type = ArmorShieldSpriteType.PunchFrame2; break;
                        }
                    }
                    break;
                case CharacterActionState.SpellCast:
                    type = ArmorShieldSpriteType.SpellCast;
                    break;
                case CharacterActionState.Sitting:
                    switch (characterRenderProperties.SitState)
                    {
                        case SitState.Chair:
                            type = ArmorShieldSpriteType.SitChair;
                            break;
                        case SitState.Floor:
                            type = ArmorShieldSpriteType.SitGround;
                            break;
                    }
                    break;
            }

            var gfxFile = characterRenderProperties.Gender == 0 ? GFXTypes.FemaleArmor : GFXTypes.MaleArmor;

            var offset = GetOffsetBasedOnState(type) * GetBaseOffsetFromDirection(characterRenderProperties.Direction);
            var baseArmorValue = GetBaseArmorGraphic(characterRenderProperties.ArmorGraphic);
            var gfxNumber = baseArmorValue + (int)type + offset;

            return new SpriteSheet(_gfxManager.TextureFromResource(gfxFile, gfxNumber, true));
        }

        public ISpriteSheet GetHatTexture(ICharacterRenderProperties characterRenderProperties)
        {
            if (characterRenderProperties.HatGraphic == 0)
                return new EmptySpriteSheet();

            var gfxFile = characterRenderProperties.Gender == 0 ? GFXTypes.FemaleHat : GFXTypes.MaleHat;

            var offset = 2 * GetBaseOffsetFromDirection(characterRenderProperties.Direction);
            var baseHatValue = GetBaseHatGraphic(characterRenderProperties.HatGraphic);
            var gfxNumber = baseHatValue + 1 + offset;

            return new SpriteSheet(_gfxManager.TextureFromResource(gfxFile, gfxNumber, true));
        }

        public ISpriteSheet GetShieldTexture(ICharacterRenderProperties characterRenderProperties)
        {
            if (characterRenderProperties.ShieldGraphic == 0)
                return new EmptySpriteSheet();

            var type = ArmorShieldSpriteType.Standing;
            var offset = GetBaseOffsetFromDirection(characterRenderProperties.Direction);

            //front shields have one size gfx, back arrows/wings have another size.
            if (!EIFFile.IsShieldOnBack(characterRenderProperties.ShieldGraphic))
            {
                if (characterRenderProperties.CurrentAction == CharacterActionState.Walking)
                {
                    switch (characterRenderProperties.WalkFrame)
                    {
                        case 1: type = ArmorShieldSpriteType.WalkFrame1; break;
                        case 2: type = ArmorShieldSpriteType.WalkFrame2; break;
                        case 3: type = ArmorShieldSpriteType.WalkFrame3; break;
                        case 4: type = ArmorShieldSpriteType.WalkFrame4; break;
                    }
                }
                else if (characterRenderProperties.CurrentAction == CharacterActionState.Attacking)
                {
                    switch (characterRenderProperties.AttackFrame)
                    {
                        case 1: type = ArmorShieldSpriteType.PunchFrame1; break;
                        case 2: type = ArmorShieldSpriteType.PunchFrame2; break;
                    }
                }
                else if (characterRenderProperties.CurrentAction == CharacterActionState.SpellCast)
                {
                    type = ArmorShieldSpriteType.SpellCast;
                }
                else if(characterRenderProperties.CurrentAction == CharacterActionState.Sitting)
                {
                    return new EmptySpriteSheet();
                }

                offset *= GetOffsetBasedOnState(type);
            }
            else
            {
                //different gfx numbering scheme for shield items worn on the back:
                //    Standing = 1/2
                //    Attacking = 3/4
                //    Extra = 5 (unused?)
                if (characterRenderProperties.CurrentAction == CharacterActionState.Attacking)
                    type = ArmorShieldSpriteType.ShieldItemOnBack_AttackingWithBow;
            }

            var gfxFile = characterRenderProperties.Gender == 0 ? GFXTypes.FemaleBack : GFXTypes.MaleBack;

            var baseShieldValue = GetBaseShieldGraphic(characterRenderProperties.ShieldGraphic);
            var gfxNumber = baseShieldValue + (int)type + offset;
            return new SpriteSheet(_gfxManager.TextureFromResource(gfxFile, gfxNumber, true));
        }

        public ISpriteSheet GetWeaponTexture(ICharacterRenderProperties characterRenderProperties)
        {
            if(characterRenderProperties.WeaponGraphic == 0)
                return new EmptySpriteSheet();

            var type = WeaponSpriteType.Standing;
            switch (characterRenderProperties.CurrentAction)
            {
                case CharacterActionState.Walking:
                    switch (characterRenderProperties.WalkFrame)
                    {
                        case 1: type = WeaponSpriteType.WalkFrame1; break;
                        case 2: type = WeaponSpriteType.WalkFrame2; break;
                        case 3: type = WeaponSpriteType.WalkFrame3; break;
                        case 4: type = WeaponSpriteType.WalkFrame4; break;
                    }
                    break;
                case CharacterActionState.Attacking:
                    if (BowIsEquipped(characterRenderProperties))
                    {
                        switch (characterRenderProperties.AttackFrame)
                        {
                            case 1: type = WeaponSpriteType.Shooting; break;
                            case 2: type = WeaponSpriteType.Standing; break;
                        }
                    }
                    else
                    {
                        switch (characterRenderProperties.AttackFrame)
                        {
                            case 1: type = WeaponSpriteType.SwingFrame1; break;
                            case 2:
                                type = characterRenderProperties.Direction == EODirection.Down
                                    || characterRenderProperties.Direction == EODirection.Right
                                    ? WeaponSpriteType.SwingFrame2Spec : WeaponSpriteType.SwingFrame2;
                                break;
                        }
                    }
                    break;
                case CharacterActionState.SpellCast:
                    type = WeaponSpriteType.SpellCast;
                    break;
                case CharacterActionState.Sitting:
                    return new EmptySpriteSheet(); //no weapon when sitting
            }

            var gfxFile = characterRenderProperties.Gender == 0 ? GFXTypes.FemaleWeapons : GFXTypes.MaleWeapons;

            var offset = GetOffsetBasedOnState(type) * GetBaseOffsetFromDirection(characterRenderProperties.Direction);
            var baseWeaponValue = GetBaseWeaponGraphic(characterRenderProperties.WeaponGraphic);
            var gfxNumber = baseWeaponValue + (int)type + offset;

            return new SpriteSheet(_gfxManager.TextureFromResource(gfxFile, gfxNumber, true));
        }

        public ISpriteSheet GetSkinTexture(ICharacterRenderProperties characterRenderProperties)
        {
            const int SheetRows = 7;
            var sheetColumns = 4;
            var gfxNum = 1;

            if (characterRenderProperties.CurrentAction == CharacterActionState.Walking && characterRenderProperties.WalkFrame > 0)
            {
                gfxNum = 2;
                sheetColumns = 16;
            }
            else if (characterRenderProperties.CurrentAction == CharacterActionState.Attacking && characterRenderProperties.AttackFrame > 0)
            {
                if (!BowIsEquipped(characterRenderProperties))
                {
                    gfxNum = 3;
                    sheetColumns = 8;
                }
                else if (characterRenderProperties.AttackFrame == 1) //only 1 frame of bow/gun animation
                {
                    gfxNum = 7; //4 columns in this one too
                }
            }
            else if (characterRenderProperties.CurrentAction == CharacterActionState.SpellCast)
            {
                gfxNum = 4;
            }
            else if (characterRenderProperties.CurrentAction == CharacterActionState.Sitting)
            {
                if (characterRenderProperties.SitState == SitState.Floor) gfxNum = 6;
                else if (characterRenderProperties.SitState == SitState.Chair) gfxNum = 5;
            }

            var texture = _gfxManager.TextureFromResource(GFXTypes.SkinSprites, gfxNum, true);

            var rotated = characterRenderProperties.Direction == EODirection.Left ||
                          characterRenderProperties.Direction == EODirection.Up;

            var heightDelta  = texture.Height / SheetRows;
            var widthDelta   = texture.Width / sheetColumns;
            var sectionDelta = texture.Width / 4;

            var walkExtra = characterRenderProperties.WalkFrame > 0 ? widthDelta * (characterRenderProperties.WalkFrame - 1) : 0;
            walkExtra = !BowIsEquipped(characterRenderProperties) && characterRenderProperties.AttackFrame > 0 ? widthDelta * (characterRenderProperties.AttackFrame - 1) : walkExtra;

            // Fix offsets for skins - the source rectangles are not at an evenly spaced interval
            if (characterRenderProperties.Gender == 1)
            {
                if (characterRenderProperties.CurrentAction == CharacterActionState.Walking && !rotated)
                {
                    walkExtra += 1;
                }
                else if (characterRenderProperties.CurrentAction == CharacterActionState.Attacking &&
                         characterRenderProperties.AttackFrame == 1)
                {
                    // This condition needs some shifting, but this must be done in SkinRenderLocationCalculator since it is a shift of the loaded sprite
                }
            }
            else if (characterRenderProperties.Gender == 0)
            {
                if (characterRenderProperties.CurrentAction == CharacterActionState.Attacking)
                {
                    walkExtra += 1;
                }
            }

            var sourceArea = new Rectangle(
                characterRenderProperties.Gender * widthDelta * (sheetColumns / 2) + (rotated ? sectionDelta : 0) + walkExtra,
                characterRenderProperties.Race * heightDelta,
                widthDelta,
                heightDelta);

            return new SpriteSheet(texture, sourceArea);
        }

        public ISpriteSheet GetHairTexture(ICharacterRenderProperties characterRenderProperties)
        {
            if(characterRenderProperties.HairStyle == 0)
                return new EmptySpriteSheet();

            var gfxFile = characterRenderProperties.Gender == 0 ? GFXTypes.FemaleHair : GFXTypes.MaleHair;
            var offset = 2 * GetBaseOffsetFromDirection(characterRenderProperties.Direction);
            var gfxNumber = GetBaseHairGraphic(characterRenderProperties.HairStyle, characterRenderProperties.HairColor) + 2 + offset;

            var hairTexture = _gfxManager.TextureFromResource(gfxFile, gfxNumber, true);
            return new SpriteSheet(hairTexture);
        }

        public ISpriteSheet GetFaceTexture(ICharacterRenderProperties characterRenderProperties)
        {
            if (characterRenderProperties.EmoteFrame < 0 ||
                characterRenderProperties.Emote == Emote.Trade ||
                characterRenderProperties.Emote == Emote.LevelUp)
            {
                return new EmptySpriteSheet();
            }

            //14 rows (7 female - 7 male) / 11 columns
            const int ROWS = 14;
            const int COLS = 11;

            var texture = _gfxManager.TextureFromResource(GFXTypes.SkinSprites, 8, true);

            var widthDelta = texture.Width / COLS;
            var heightDelta = texture.Height / ROWS;
            var genderOffset = texture.Height / 2 * characterRenderProperties.Gender;
            //'playful' is the last face in the gfx (ndx 10), even though it has enum value of 14 (ndx 13)
            var emote = characterRenderProperties.Emote == Emote.Playful ||
                        characterRenderProperties.Emote == Emote.Drunk
                        ? 10 : (int)characterRenderProperties.Emote - 1;

            var sourceRectangle = new Rectangle(widthDelta * emote, heightDelta * characterRenderProperties.Race + genderOffset, widthDelta, heightDelta);

            return new SpriteSheet(texture, sourceRectangle);
        }

        public ISpriteSheet GetEmoteTexture(ICharacterRenderProperties characterRenderProperties)
        {
            if (characterRenderProperties.Emote == 0 || characterRenderProperties.EmoteFrame < 0)
                return new EmptySpriteSheet();

            const int NUM_EMOTES = 15;
            const int NUM_FRAMES = 4;

            var emoteValue = Enum.GetName(typeof (Emote), characterRenderProperties.Emote) ?? "";
            var convertedValuesDictionary = Enum.GetNames(typeof (EmoteSpriteType))
                .ToDictionary(x => x, x => (EmoteSpriteType) Enum.Parse(typeof (EmoteSpriteType), x));
            var convertedEmote = (int)convertedValuesDictionary[emoteValue];

            var emoteTexture = _gfxManager.TextureFromResource(GFXTypes.PostLoginUI, 38, true);

            var eachSet = emoteTexture.Width / NUM_EMOTES;
            var eachFrame = emoteTexture.Width / (NUM_EMOTES * NUM_FRAMES);
            var startX = convertedEmote*eachSet + characterRenderProperties.EmoteFrame*eachFrame;

            var emoteRect = new Rectangle(startX, 0, eachFrame, emoteTexture.Height);

            return new SpriteSheet(emoteTexture, emoteRect);
        }

        private int GetBaseBootGraphic(int bootsGraphic)
        {
            return (bootsGraphic - 1) * 40;
        }

        private int GetBaseArmorGraphic(int armorGraphic)
        {
            return (armorGraphic - 1) * 50;
        }

        private int GetBaseHatGraphic(int hatGraphic)
        {
            return (hatGraphic - 1) * 10;
        }

        private int GetBaseShieldGraphic(int shieldGraphic)
        {
            return (shieldGraphic - 1) * 50;
        }

        private int GetBaseWeaponGraphic(int weaponGraphic)
        {
            return (weaponGraphic - 1) * 100;
        }

        private int GetBaseHairGraphic(int hairStyle, int hairColor)
        {
            return (hairStyle - 1) * 40 + hairColor * 4;
        }

        private int GetBaseOffsetFromDirection(EODirection direction)
        {
            return direction == EODirection.Down ||
                   direction == EODirection.Right ? 0 : 1;
        }

        private int GetOffsetBasedOnState(BootsSpriteType type)
        {
            switch (type)
            {
                case BootsSpriteType.WalkFrame1:
                case BootsSpriteType.WalkFrame2:
                case BootsSpriteType.WalkFrame3:
                case BootsSpriteType.WalkFrame4:
                    return 4;
            }
            return 1;
        }

        private int GetOffsetBasedOnState(ArmorShieldSpriteType type)
        {
            switch (type)
            {
                case ArmorShieldSpriteType.WalkFrame1:
                case ArmorShieldSpriteType.WalkFrame2:
                case ArmorShieldSpriteType.WalkFrame3:
                case ArmorShieldSpriteType.WalkFrame4:
                    return 4;
                case ArmorShieldSpriteType.PunchFrame1:
                case ArmorShieldSpriteType.PunchFrame2:
                    return 2;
            }
            return 1;
        }

        private int GetOffsetBasedOnState(WeaponSpriteType type)
        {
            switch (type)
            {
                case WeaponSpriteType.WalkFrame1:
                case WeaponSpriteType.WalkFrame2:
                case WeaponSpriteType.WalkFrame3:
                case WeaponSpriteType.WalkFrame4:
                    return 4;
                case WeaponSpriteType.SwingFrame1:
                case WeaponSpriteType.SwingFrame2:
                    return 2;
            }
            return 1;
        }

        private bool BowIsEquipped(ICharacterRenderProperties characterRenderProperties)
        {
            if (EIFFile == null || EIFFile.Data == null)
                return false;

            var itemData = EIFFile.Data;
            var weaponInfo = itemData.SingleOrDefault(x => x.Type == ItemType.Weapon &&
                                                            x.DollGraphic == characterRenderProperties.WeaponGraphic);

            return weaponInfo != null && weaponInfo.SubType == ItemSubType.Ranged;
        }

        private IPubFile<EIFRecord> EIFFile => _eifFileProvider.EIFFile;
    }
}
