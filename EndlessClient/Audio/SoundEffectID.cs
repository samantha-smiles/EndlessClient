﻿namespace EndlessClient.Audio
{
    // These are 0 based indexes even though the files start at sfx001
    // sfx001 will be id 0
    // sfx060 will be id 59
    public enum SoundEffectID
    {
        NONE,
        LayeredTechIntro = 1,
        ButtonClick,
        DialogButtonClick,
        TextBoxFocus = 4,
        ChestOpen = TextBoxFocus,
        SpellActivate = TextBoxFocus,
        ServerCommand = TextBoxFocus,
        Login,
        ServerMessage = Login,
        DeleteCharacter,
        MapMutation = DeleteCharacter,
        UnknownStaticSound,
        ScreenCapture = 8,
        PrivateMessageReceived,
        PunchAttack,
        UnknownWarpSound,
        UnknownPingSound = 12,
        HudStatusBarClick,
        AdminAnnounceReceived,
        MeleeWeaponAttack,
        MemberLeftParty = 16,
        TradeAccepted,
        JoinParty = TradeAccepted,
        GroupChatReceived,
        PrivateMessageSent,
        InventoryPickup = 20,
        InventoryPlace,
        ItemUnequip = InventoryPlace,
        Earthquake,
        DoorClose,
        DoorOpen = 24,
        DoorOrChestLocked,
        BuySell,
        Craft,
        UnknownBuzzSound = 28,
        AdminChatReceived,
        UnknownAttackLikeSound,
        PotionOfFlamesEffect,
        AdminWarp = 32,
        NoWallWalk,
        UseScroll = NoWallWalk,
        GhostPlayer = NoWallWalk,
        PotionOfEvilTerrorEffect,
        PotionOfFireworksEffect,
        PotionOfSparklesEffect = 36,
        LearnNewSpell,
        PotionOfLoveEffect = LearnNewSpell,
        AttackBow,
        LevelUp,
        Dead = 40,
        JumpStone,
        Water,
        Heal,
        Harp1 = 44,
        Harp2,
        Harp3,
        Guitar1,
        Guitar2 = 48,
        Guitar3,
        Thunder,
        MapEvacTimer,
        ArenaWin = 52,
        Gun,
        UltimaBlastSpell,
        ShieldSpell,
        RingOfFireSpell = 56,
        IceBlastSpell1,
        EnergyBallSpell,
        WhirlSpell,
        BouldersSpell = 60,
        AuraSpell,
        HeavenSpell,
        IceBlastSpell2,
        MapAmbientNoiseWater = 64,
        MapAmbientNoiseDrone1,
        AdminHide,
        LavaBubbles1,
        UnknownMapAmbientNoise3 = 68,
        FactoryWhirring,
        MapEffectHPDrain,
        MapEffectTPDrain,
        Spikes = 72,
        NoArrows,
        UnknownBoing,
        UnknownMapAmbientNoise5,
        DarkHandSpell = 76,
        TentaclesSpell,
        MagicWhirlSpell,
        PowerWindSpell,
        FireBlastSpell = 80,
        LavaBubbles2,
    }
}
