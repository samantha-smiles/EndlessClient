﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System.Threading.Tasks;
using EndlessClient.Dialogs.Actions;
using EndlessClient.GameExecution;
using EndlessClient.HUD.Chat;
using EOLib.Domain.Chat;
using EOLib.Net;
using EOLib.Net.Communication;

namespace EndlessClient.Controllers
{
    public class ChatController : IChatController
    {
        private readonly IChatTextBoxActions _chatTextBoxActions;
        private readonly IChatActions _chatActions;
        private readonly IPrivateMessageActions _privateMessageActions;
        private readonly IGameStateActions _gameStateActions;
        private readonly IErrorDialogDisplayAction _errorDisplayAction;
        private readonly IChatSpeechBubbleActions _chatSpeechBubbleActions;
        private readonly ISafeNetworkOperationFactory _safeNetworkOperationFactory;

        public ChatController(IChatTextBoxActions chatTextBoxActions,
                              IChatActions chatActions,
                              IPrivateMessageActions privateMessageActions,
                              IGameStateActions gameStateActions,
                              IErrorDialogDisplayAction errorDisplayAction,
                              IChatSpeechBubbleActions chatSpeechBubbleActions,
                              ISafeNetworkOperationFactory safeNetworkOperationFactory)
        {
            _chatTextBoxActions = chatTextBoxActions;
            _chatActions = chatActions;
            _privateMessageActions = privateMessageActions;
            _gameStateActions = gameStateActions;
            _errorDisplayAction = errorDisplayAction;
            _chatSpeechBubbleActions = chatSpeechBubbleActions;
            _safeNetworkOperationFactory = safeNetworkOperationFactory;
        }

        public async Task SendChatAndClearTextBox()
        {
            var targetCharacter = _privateMessageActions.GetTargetCharacter();
            var sendChatOperation = _safeNetworkOperationFactory.CreateSafeAsyncOperation(
                async () => await _chatActions.SendChatToServer(targetCharacter),
                SetInitialStateAndShowError);

            if (!await sendChatOperation.Invoke())
                return;

            _chatTextBoxActions.ClearChatText();
            _chatTextBoxActions.UpdateChatTextRepository();

            _chatSpeechBubbleActions.ShowSpeechBubbleForMainCharacter();
        }

        public void SelectChatTextBox()
        {
            _chatTextBoxActions.FocusChatTextBox();
        }

        public void ChatTextChanged()
        {
            _chatTextBoxActions.UpdateChatTextRepository();
        }

        private void SetInitialStateAndShowError(NoDataSentException ex)
        {
            _gameStateActions.ChangeToState(GameStates.Initial);
            _errorDisplayAction.ShowException(ex);
        }
    }

    public interface IChatController
    {
        Task SendChatAndClearTextBox();

        void SelectChatTextBox();

        void ChatTextChanged();
    }
}
