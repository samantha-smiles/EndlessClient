﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.Content;
using EndlessClient.Rendering.Chat;
using EOLib;
using EOLib.Domain.Login;
using EOLib.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace EndlessClient.HUD.Panels
{
    public class HudPanelFactory : IHudPanelFactory
    {
        private const int HUD_CONTROL_LAYER = 130;

        private readonly INativeGraphicsManager _nativeGraphicsManager;
        private readonly IContentManagerProvider _contentManagerProvider;
        private readonly INewsProvider _newsProvider;

        public HudPanelFactory(INativeGraphicsManager nativeGraphicsManager,
                               IContentManagerProvider contentManagerProvider,
                               INewsProvider newsProvider)
        {
            _nativeGraphicsManager = nativeGraphicsManager;
            _contentManagerProvider = contentManagerProvider;
            _newsProvider = newsProvider;
        }

        public NewsPanel CreateNewsPanel()
        {
            var chatFont = _contentManagerProvider.Content.Load<SpriteFont>(Constants.FontSize08);
            
            return new NewsPanel(_nativeGraphicsManager,
                                 new ChatRenderableGenerator(chatFont),
                                 _newsProvider,
                                 chatFont) { DrawOrder = HUD_CONTROL_LAYER };
        }

        public InventoryPanel CreateInventoryPanel()
        {
            return new InventoryPanel();
        }

        public ActiveSpellsPanel CreateActiveSpellsPanel()
        {
            return new ActiveSpellsPanel();
        }

        public PassiveSpellsPanel CreatePassiveSpellsPanel()
        {
            return new PassiveSpellsPanel();
        }

        public ChatPanel CreateChatPanel()
        {
            return new ChatPanel();
        }

        public StatsPanel CreateStatsPanel()
        {
            return new StatsPanel();
        }

        public OnlineListPanel CreateOnlineListPanel()
        {
            return new OnlineListPanel();
        }

        public PartyPanel CreatePartyPanel()
        {
            return new PartyPanel();
        }

        public SettingsPanel CreateSettingsPanel()
        {
            return new SettingsPanel();
        }

        public HelpPanel CreateHelpPanel()
        {
            return new HelpPanel();
        }
    }
}