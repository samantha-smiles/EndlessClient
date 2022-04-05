﻿using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.Dialogs.Actions;
using EndlessClient.Dialogs.Factories;
using EndlessClient.HUD;
using EndlessClient.HUD.Controls;
using EndlessClient.HUD.Inventory;
using EndlessClient.HUD.Panels;
using EndlessClient.Input;
using EndlessClient.Rendering;
using EndlessClient.Rendering.Character;
using EndlessClient.Rendering.Factories;
using EOLib.Domain.Character;
using EOLib.Domain.Interact;
using EOLib.Domain.Item;
using EOLib.Domain.Map;
using EOLib.Localization;
using Optional;
using Optional.Collections;
using System;

namespace EndlessClient.Controllers
{
    [AutoMappedType]
    public class MapInteractionController : IMapInteractionController
    {
        private readonly IMapActions _mapActions;
        private readonly ICharacterActions _characterActions;
        private readonly IInGameDialogActions _inGameDialogActions;
        private readonly IPaperdollActions _paperdollActions;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly ICharacterProvider _characterProvider;
        private readonly IStatusLabelSetter _statusLabelSetter;
        private readonly IInventorySpaceValidator _inventorySpaceValidator;
        private readonly IHudControlProvider _hudControlProvider;
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly IContextMenuRepository _contextMenuRepository;
        private readonly IUserInputTimeRepository _userInputTimeRepository;
        private readonly IEOMessageBoxFactory _eoMessageBoxFactory;
        private readonly IContextMenuRendererFactory _contextMenuRendererFactory;

        public MapInteractionController(IMapActions mapActions,
                                        ICharacterActions characterActions,
                                        IInGameDialogActions inGameDialogActions,
                                        IPaperdollActions paperdollActions,
                                        ICurrentMapStateProvider currentMapStateProvider,
                                        ICharacterProvider characterProvider,
                                        IStatusLabelSetter statusLabelSetter,
                                        IInventorySpaceValidator inventorySpaceValidator,
                                        IHudControlProvider hudControlProvider,
                                        ICharacterRendererProvider characterRendererProvider,
                                        IContextMenuRepository contextMenuRepository,
                                        IUserInputTimeRepository userInputTimeRepository,
                                        IEOMessageBoxFactory eoMessageBoxFactory,
                                        IContextMenuRendererFactory contextMenuRendererFactory)
        {
            _mapActions = mapActions;
            _characterActions = characterActions;
            _inGameDialogActions = inGameDialogActions;
            _paperdollActions = paperdollActions;
            _currentMapStateProvider = currentMapStateProvider;
            _characterProvider = characterProvider;
            _statusLabelSetter = statusLabelSetter;
            _inventorySpaceValidator = inventorySpaceValidator;
            _hudControlProvider = hudControlProvider;
            _characterRendererProvider = characterRendererProvider;
            _contextMenuRepository = contextMenuRepository;
            _userInputTimeRepository = userInputTimeRepository;
            _eoMessageBoxFactory = eoMessageBoxFactory;
            _contextMenuRendererFactory = contextMenuRendererFactory;
        }

        public void LeftClick(IMapCellState cellState, IMouseCursorRenderer mouseRenderer)
        {
            if (!InventoryPanel.NoItemsDragging())
            {
                return;
            }

            var optionalItem = cellState.Items.FirstOrNone();
            if (optionalItem.HasValue)
            {
                var item = optionalItem.ValueOr(Item.None);
                if (!_inventorySpaceValidator.ItemFits(item))
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, EOResourceID.STATUS_LABEL_ITEM_PICKUP_NO_SPACE_LEFT);
                else
                    HandlePickupResult(_mapActions.PickUpItem(item), item);
            }
            else if (cellState.Sign.HasValue)
            {
                var sign = cellState.Sign.ValueOr(Sign.None);
                var messageBox = _eoMessageBoxFactory.CreateMessageBox(sign.Message, sign.Title);
                messageBox.ShowDialog();
            }
            else if (cellState.Chest.HasValue) { /* TODO: chest interaction */ }
            else if (_characterProvider.MainCharacter.RenderProperties.SitState != SitState.Standing)
            {
                _characterActions.ToggleSit();
            }
            else if (cellState.InBounds)
            {
                mouseRenderer.AnimateClick();
                _hudControlProvider.GetComponent<ICharacterAnimator>(HudControlIdentifier.CharacterAnimator)
                    .StartMainCharacterWalkAnimation(Option.Some(cellState.Coordinate));
            }
            // todo: board, jukebox

            _userInputTimeRepository.LastInputTime = DateTime.Now;
        }

        // todo: move to new controller for character interaction
        public void RightClick(IMapCellState cellState)
        {
            if (!cellState.Character.HasValue)
                return;

            cellState.Character.MatchSome(c =>
            {
                if (c == _characterProvider.MainCharacter)
                {
                    _paperdollActions.RequestPaperdoll(_characterProvider.MainCharacter.ID);
                    _inGameDialogActions.ShowPaperdollDialog(_characterProvider.MainCharacter, isMainCharacter: true);
                    _userInputTimeRepository.LastInputTime = DateTime.Now;
                }
                else if (_characterRendererProvider.CharacterRenderers.ContainsKey(c.ID))
                {
                    _contextMenuRepository.ContextMenu = _contextMenuRepository.ContextMenu.Match(
                        some: cmr =>
                        {
                            cmr.Dispose();
                            return Option.Some(_contextMenuRendererFactory.CreateContextMenuRenderer(_characterRendererProvider.CharacterRenderers[c.ID]));
                        },
                        none: () => Option.Some(_contextMenuRendererFactory.CreateContextMenuRenderer(_characterRendererProvider.CharacterRenderers[c.ID])));
                    _contextMenuRepository.ContextMenu.MatchSome(r => r.Initialize());
                }
            });
        }

        private void HandlePickupResult(ItemPickupResult pickupResult, IItem item)
        {
            switch (pickupResult)
            {
                case ItemPickupResult.DropProtection:
                    var message = EOResourceID.STATUS_LABEL_ITEM_PICKUP_PROTECTED;
                    var extra = string.Empty;

                    item.OwningPlayerID.MatchSome(playerId =>
                    {
                        message = EOResourceID.STATUS_LABEL_ITEM_PICKUP_PROTECTED_BY;
                        if (_currentMapStateProvider.Characters.ContainsKey(playerId))
                        {
                            extra = _currentMapStateProvider.Characters[playerId].Name;
                        }
                    });

                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_INFORMATION, message, extra);

                    break;
                case ItemPickupResult.TooHeavy:
                    _statusLabelSetter.SetStatusLabel(EOResourceID.STATUS_LABEL_TYPE_WARNING, EOResourceID.DIALOG_ITS_TOO_HEAVY_WEIGHT);
                    break;
                case ItemPickupResult.TooFar:
                case ItemPickupResult.Ok: break;
                default: throw new ArgumentOutOfRangeException(nameof(pickupResult), pickupResult, null);
            }
        }

        private InventoryPanel InventoryPanel => _hudControlProvider.GetComponent<InventoryPanel>(HudControlIdentifier.InventoryPanel);
    }

    public interface IMapInteractionController
    {
        void LeftClick(IMapCellState cellState, IMouseCursorRenderer mouseRenderer);

        void RightClick(IMapCellState cellState);
    }
}