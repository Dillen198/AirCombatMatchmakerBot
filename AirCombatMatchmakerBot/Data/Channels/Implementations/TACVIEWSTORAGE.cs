﻿using Discord;
using System.Data;
using System;
using System.Runtime.Serialization;
using Discord.WebSocket;
using System.Collections.Concurrent;

[DataContract]
public class TACVIEWSTORAGE : BaseChannel
{
    public TACVIEWSTORAGE()
    {
        channelType = ChannelType.TACVIEWSTORAGE;
    }

    public override ConcurrentBag<Overwrite> GetGuildPermissions(
        SocketGuild _guild, params ulong[] _allowedUsersIdsArray)
    {
        return new ConcurrentBag<Overwrite>
        {
        };
    }
}