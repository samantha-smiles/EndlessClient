using System;
using AutomaticTypeMapper;
using EndlessClient.ControlSets;
using EndlessClient.HUD.Controls;
using EndlessClient.UIControls;
using Microsoft.Xna.Framework.Input;

namespace EndlessClient.HUD.Chat
{
    [AutoMappedType]
    public class ChatTextBoxActions : IChatTextBoxActions
    {
        private readonly IHudControlProvider _hudControlProvider;
        private KeyboardState _previousKeyboardState;

        public ChatTextBoxActions(IHudControlProvider hudControlProvider)
        {
            _hudControlProvider = hudControlProvider;
            _previousKeyboardState = Keyboard.GetState();
        }

        public void Update()
        {
            var currentKeyboardState = Keyboard.GetState();
            if (IsKeyPressed(Keys.Enter, currentKeyboardState, _previousKeyboardState))
            {
                FocusChatTextBox();
            }
            _previousKeyboardState = currentKeyboardState;
        }

        public void ClearChatText()
        {
            var chatTextBox = GetChatTextBox();
            chatTextBox.Text = "";
        }

        public void FocusChatTextBox()
        {
            GetChatTextBox().Selected = true;
        }

        private ChatTextBox GetChatTextBox()
        {
            return _hudControlProvider.GetComponent<ChatTextBox>(HudControlIdentifier.ChatTextBox);
        }

        private bool IsKeyPressed(Keys key, KeyboardState currentState, KeyboardState previousState)
        {
            // A key is considered as being pressed if it was down in the current state but not in the previous one
            return currentState.IsKeyDown(key) && previousState.IsKeyUp(key);
        }
    }
}