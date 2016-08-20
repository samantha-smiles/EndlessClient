﻿// Original Work Copyright (c) Ethan Moffat 2014-2016
// This file is subject to the GPL v2 License
// For additional details, see the LICENSE file

using System;
using System.Collections.Generic;
using System.Linq;

namespace EOLib.Domain.Chat
{
    public interface IChatRepository
    {
        string LocalTypedText { get; set; }

        Dictionary<ChatTab, List<ChatData>> AllChat { get; set; }
    }

    public interface IChatProvider
    {
        string LocalTypedText { get; }

        IReadOnlyDictionary<ChatTab, IReadOnlyList<ChatData>> AllChat { get; }
    }

    public class ChatRepository : IChatRepository, IChatProvider
    {
        public string LocalTypedText { get; set; }

        public Dictionary<ChatTab, List<ChatData>> AllChat { get; set; }

        IReadOnlyDictionary<ChatTab, IReadOnlyList<ChatData>> IChatProvider.AllChat
        {
            get
            {
                return AllChat.ToDictionary(
                    k => k.Key,
                    v => v.Value as IReadOnlyList<ChatData>);
            }
        }

        public ChatRepository()
        {
            LocalTypedText = "";

            AllChat = new Dictionary<ChatTab, List<ChatData>>();
            foreach (var tab in (ChatTab[]) Enum.GetValues(typeof(ChatTab)))
                AllChat.Add(tab, new List<ChatData>());
        }
    }
}