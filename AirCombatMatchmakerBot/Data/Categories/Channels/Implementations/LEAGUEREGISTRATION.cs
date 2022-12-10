﻿using Discord;
using System.Data;
using System;
using System.Runtime.Serialization;
using Discord.WebSocket;

[DataContract]
public class LEAGUEREGISTRATION : BaseChannel
{
    public LEAGUEREGISTRATION()
    {
        channelName = ChannelName.LEAGUEREGISTRATION;
    }

    public override List<Overwrite> GetGuildPermissions(SocketGuild _guild)
    {
        return new List<Overwrite>
        {
            new Overwrite(_guild.EveryoneRole.Id, PermissionTarget.Role,
                new OverwritePermissions(viewChannel: PermValue.Deny)),
            new Overwrite(RoleManager.CheckIfRoleExistsByNameAndCreateItIfItDoesntElseReturnIt(
                _guild, "Member").Result.Id, PermissionTarget.Role,
                new OverwritePermissions(viewChannel: PermValue.Allow)),
        };
    }

    public override async Task ActivateChannelFeatures()
    {
        LeagueManager.leagueRegistrationChannelId = channelId;

        var guild = BotReference.GetGuildRef();

        if (guild == null)
        {
            Exceptions.BotGuildRefNull();
            return;
        }

        // Hardcoded channel id for now
        var channel = guild.GetTextChannel(channelId) as ITextChannel;

        if (channel == null)
        {
            Log.WriteLine("Channel was null with id: " + channelId, LogLevel.CRITICAL);
            return;
        }
        Log.WriteLine("Channel found: " + channel.Name +
            "(" + channel.Id + ")", LogLevel.VERBOSE);


        await LeagueManager.CreateLeaguesOnStartup(guild, channel);
        await LeagueRegistrationChannelManager.CreateLeagueMessages(this, channel);
    }
}