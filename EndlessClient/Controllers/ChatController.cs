﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using EndlessClient.HUD.Chat;

namespace EndlessClient.Controllers
{
    public class ChatController : IChatController
    {
        private readonly IChatTextBoxActions _chatTextBoxActions;

        public ChatController(IChatTextBoxActions chatTextBoxActions)
        {
            _chatTextBoxActions = chatTextBoxActions;
        }

        public void SendChatAndClearTextBox()
        {
            //todo: send chat string to server (see HUD._doTalk)
            _chatTextBoxActions.ClearChatText();
            _chatTextBoxActions.UpdateChatTextRepository();
        }

        public void SelectChatTextBox()
        {
            _chatTextBoxActions.FocusChatTextBox();
        }

        public void ChatTextChanged()
        {
            _chatTextBoxActions.UpdateChatTextRepository();
        }
    }
}
