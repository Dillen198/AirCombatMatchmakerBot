﻿using Discord.WebSocket;
using Discord;
using System.Runtime.Serialization;

[DataContract]
public class REGISTRATIONCATEGORY : BaseCategory
{
    public REGISTRATIONCATEGORY()
    {
        categoryName = CategoryName.REGISTRATIONCATEGORY;
        channelNames = new List<ChannelName>()
        {
            ChannelName.REGISTRATIONCHANNEL,
            ChannelName.LEAGUEREGISTRATION
        };
    }

    public override List<Overwrite> GetGuildPermissions(SocketGuild _guild, SocketRole _role)
    {
        Log.WriteLine("executing permissions from REGISTRATIONCATEGORY", LogLevel.VERBOSE);
        return new List<Overwrite>
        {
        };
    }
}