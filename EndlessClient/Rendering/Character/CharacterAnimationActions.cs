﻿using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.Rendering.Map;
using EOLib;
using EOLib.Domain.Character;
using EOLib.Domain.Extensions;
using EOLib.Domain.Map;
using EOLib.Domain.Notifiers;
using EOLib.IO.Map;
using System.Linq;

namespace EndlessClient.Rendering.Character
{
    [MappedType(BaseType = typeof(ICharacterAnimationActions))]
    [MappedType(BaseType = typeof(IOtherCharacterAnimationNotifier))]
    [MappedType(BaseType = typeof(IEffectNotifier))]
    public class CharacterAnimationActions : ICharacterAnimationActions, IOtherCharacterAnimationNotifier, IEffectNotifier
    {
        private readonly IHudControlProvider _hudControlProvider;
        private readonly ICharacterRepository _characterRepository;
        private readonly ICurrentMapStateProvider _currentMapStateProvider;
        private readonly ICharacterRendererProvider _characterRendererProvider;
        private readonly ICurrentMapProvider _currentMapProvider;
        private readonly ISpikeTrapActions _spikeTrapActions;

        public CharacterAnimationActions(IHudControlProvider hudControlProvider,
                                         ICharacterRepository characterRepository,
                                         ICurrentMapStateProvider currentMapStateProvider,
                                         ICharacterRendererProvider characterRendererProvider,
                                         ICurrentMapProvider currentMapProvider,
                                         ISpikeTrapActions spikeTrapActions)
        {
            _hudControlProvider = hudControlProvider;
            _characterRepository = characterRepository;
            _currentMapStateProvider = currentMapStateProvider;
            _characterRendererProvider = characterRendererProvider;
            _currentMapProvider = currentMapProvider;
            _spikeTrapActions = spikeTrapActions;
        }

        public void Face(EODirection direction)
        {
            var renderProperties = _characterRepository.MainCharacter.RenderProperties;
            renderProperties = renderProperties.WithDirection(direction);

            var newMainCharacter = _characterRepository.MainCharacter.WithRenderProperties(renderProperties);
            _characterRepository.MainCharacter = newMainCharacter;
        }

        public void StartWalking()
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.StartMainCharacterWalkAnimation();
            ShowWaterSplashiesIfNeeded(CharacterActionState.Walking,
                                       _characterRepository.MainCharacter,
                                       _characterRendererProvider.MainCharacterRenderer);
        }

        public void StartAttacking()
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.StartMainCharacterAttackAnimation();
            ShowWaterSplashiesIfNeeded(CharacterActionState.Attacking,
                                       _characterRepository.MainCharacter,
                                       _characterRendererProvider.MainCharacterRenderer);
        }

        public void StartOtherCharacterWalkAnimation(int characterID)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.StartOtherCharacterWalkAnimation(characterID);
            ShowWaterSplashiesIfNeeded(CharacterActionState.Walking,
                                       _currentMapStateProvider.Characters.Single(x => x.ID == characterID),
                                       _characterRendererProvider.CharacterRenderers[characterID]);

            _spikeTrapActions.HideSpikeTrap(characterID);
            _spikeTrapActions.ShowSpikeTrap(characterID);
        }

        public void StartOtherCharacterAttackAnimation(int characterID)
        {
            if (!_hudControlProvider.IsInGame)
                return;

            Animator.StartOtherCharacterAttackAnimation(characterID);
            ShowWaterSplashiesIfNeeded(CharacterActionState.Attacking,
                                       _currentMapStateProvider.Characters.Single(x => x.ID == characterID),
                                       _characterRendererProvider.CharacterRenderers[characterID]);
        }

        public void NotifyWarpLeaveEffect(short characterId, WarpAnimation anim)
        {
            if (anim == WarpAnimation.Admin)
                _characterRendererProvider.CharacterRenderers[characterId].ShowWarpLeave();
        }

        public void NotifyWarpEnterEffect(short characterId, WarpAnimation anim)
        {
            if (anim == WarpAnimation.Admin)
            {
                if (!_characterRendererProvider.CharacterRenderers.ContainsKey(characterId))
                    _characterRendererProvider.NeedsWarpArriveAnimation.Add(characterId);
                else
                    _characterRendererProvider.CharacterRenderers[characterId].ShowWarpArrive();
            }
        }

        private void ShowWaterSplashiesIfNeeded(CharacterActionState action, ICharacter character, ICharacterRenderer characterRenderer)
        {
            var rp = character.RenderProperties;
            if (action == CharacterActionState.Attacking)
            {
                if (_currentMapProvider.CurrentMap.Tiles[rp.MapY, rp.MapX] == TileSpec.Water)
                    characterRenderer.ShowWaterSplashies();
            }
            else if (action == CharacterActionState.Walking)
            {
                if (_currentMapProvider.CurrentMap.Tiles[rp.GetDestinationY(), rp.GetDestinationX()] == TileSpec.Water)
                    characterRenderer.ShowWaterSplashies();
            }
        }

        private ICharacterAnimator Animator => _hudControlProvider.GetComponent<ICharacterAnimator>(HudControlIdentifier.CharacterAnimator);
    }

    public interface ICharacterAnimationActions
    {
        void Face(EODirection direction);

        void StartWalking();

        void StartAttacking();
    }
}
