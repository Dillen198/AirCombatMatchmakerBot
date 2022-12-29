﻿using Discord.WebSocket;
using Discord;
using System.Runtime.Serialization;
using System;

[DataContract]
public abstract class BaseChannel : InterfaceChannel
{
    ChannelName InterfaceChannel.ChannelName
    {
        get 
        {
            Log.WriteLine("Getting " + nameof(ChannelName) + ": " + channelName, LogLevel.VERBOSE);
            return channelName;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(channelName) + channelName
                + " to: " + value, LogLevel.VERBOSE);
            channelName = value;
        }
    }

    ulong InterfaceChannel.ChannelId
    {
        get
        {
            Log.WriteLine("Getting " + nameof(channelId) + ": " + channelId, LogLevel.VERBOSE);
            return channelId;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(channelId) + channelId
                + " to: " + value, LogLevel.VERBOSE);
            channelId = value;
        }
    }

    ulong InterfaceChannel.ChannelsCategoryId
    {
        get
        {
            Log.WriteLine("Getting " + nameof(channelsCategoryId) + ": " + channelsCategoryId, LogLevel.VERBOSE);
            return channelsCategoryId;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(channelsCategoryId) + channelsCategoryId
                + " to: " + value, LogLevel.VERBOSE);
            channelsCategoryId = value;
        }
    }

    List<MessageName> InterfaceChannel.ChannelMessages
    {
        get
        {
            Log.WriteLine("Getting " + nameof(channelMessages) + " with count of: " +
                channelMessages.Count, LogLevel.VERBOSE);
            return channelMessages;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(channelMessages)
                + " to: " + value, LogLevel.VERBOSE);
            channelMessages = value;
        }
    }

    Dictionary<string, InterfaceMessage> InterfaceChannel.InterfaceMessagesWithIds
    {
        get
        {
            Log.WriteLine("Getting " + nameof(interfaceMessagesWithIds) + " with count of: " +
                interfaceMessagesWithIds.Count, LogLevel.VERBOSE);
            return interfaceMessagesWithIds;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(interfaceMessagesWithIds)
                + " to: " + value, LogLevel.VERBOSE);
            interfaceMessagesWithIds = value;
        }
    }

    [DataMember] protected ChannelName channelName;
    [DataMember] protected ulong channelId;
    [DataMember] protected ulong channelsCategoryId;
    protected List<MessageName> channelMessages;
    [DataMember] protected Dictionary<string, InterfaceMessage> interfaceMessagesWithIds;


    public BaseChannel()
    {
        channelMessages = new List<MessageName>();
        interfaceMessagesWithIds = new Dictionary<string, InterfaceMessage>();
    }

    public abstract List<Overwrite> GetGuildPermissions(SocketGuild _guild);

    public virtual async Task PrepareChannelMessages()
    {
        Log.WriteLine("Starting to prepare channel messages on: " + channelName, LogLevel.VERBOSE);

        var guild = BotReference.GetGuildRef();

        if (guild == null)
        {
            Exceptions.BotGuildRefNull();
            return;
        }

        // Add to a method later
        var interfaceMessagesWithIdsOnDatabase =
            Database.Instance.Categories.CreatedCategoriesWithChannels.First(
                x => x.Key == channelsCategoryId).Value.InterfaceChannels.First(
                    x => x.ChannelId == channelId).InterfaceMessagesWithIds;

        foreach (var messageName in channelMessages)
        {
            Log.WriteLine("on: " + nameof(messageName) + messageName, LogLevel.VERBOSE);

            InterfaceMessage interfaceMessage = (InterfaceMessage)EnumExtensions.GetInstance(messageName.ToString());

            if (interfaceMessagesWithIdsOnDatabase.ContainsKey(messageName.ToString())) continue;

            Log.WriteLine("Does not contain the key: " +
                messageName + ", continuing", LogLevel.VERBOSE);

            interfaceMessagesWithIdsOnDatabase.Add(messageName.ToString(), interfaceMessage);

            Log.WriteLine("Done with: " + messageName, LogLevel.VERBOSE);
        }
        Log.WriteLine("Done posting channel messages on " +
            channelName + " id: " + channelId, LogLevel.VERBOSE);

        await PostChannelMessages();
    }
    public virtual Task PostChannelMessages()
    {
        Log.WriteLine("Starting to post channel messages on: " + channelName, LogLevel.VERBOSE);

        var guild = BotReference.GetGuildRef();

        if (guild == null)
        {
            Exceptions.BotGuildRefNull();
            return Task.CompletedTask;
        }

        Log.WriteLine("Finding channels: " + channelName + " parent category with id: " +
            channelsCategoryId, LogLevel.VERBOSE);

        /*
        
        if (Database.Instance.Categories.CreatedCategoriesWithChannels.Any(
                x => x.Key == channelsCategoryId))
        {
            Log.WriteLine("Found1: " + channelsCategoryId, LogLevel.VERBOSE);

            var interfaceMessagesWithIdsOnDatabase =
                Database.Instance.Categories.CreatedCategoriesWithChannels.First(
                x => x.Key == channelsCategoryId).Value.InterfaceChannels;

            Log.WriteLine("First1 count: " +
                interfaceMessagesWithIdsOnDatabase.Count, LogLevel.VERBOSE);

            if (interfaceMessagesWithIdsOnDatabase.Any(x => x.ChannelId == channelId))
            {
                Log.WriteLine("Found2: " + channelId, LogLevel.VERBOSE);
                interfaceChannelFromDatabase = interfaceMessagesWithIdsOnDatabase.First(
                    x => x.ChannelId == channelId);

                Log.WriteLine("First2: " + interfaceChannelFromDatabase, LogLevel.VERBOSE);
            }
        }*/

        foreach (var interfaceMessageKvp in interfaceMessagesWithIds) 
        {
            Log.WriteLine("Looping on message: " + interfaceMessageKvp.Value.MessageName + " with id: " +
                interfaceMessageKvp.Key, LogLevel.VERBOSE);

            /*
            if (interfaceChannelFromDatabase != null)
            {
                Log.WriteLine("message found: " + interfaceChannelFromDatabase.InterfaceMessagesWithIds.Any(
                        x => x.Key == interfaceMessageKvp.Key), LogLevel.VERBOSE);
            }*/

            var messageKey = interfaceMessagesWithIds[interfaceMessageKvp.Key];

            Log.WriteLine("Key was 0, message does not exist. Creating it.", LogLevel.VERBOSE);

            ulong id = interfaceMessageKvp.Value.CreateTheMessageAndItsButtonsOnTheBaseClass(
            guild, channelId, interfaceMessageKvp.Key).Result;

            messageKey.MessageId = id;
        }
        return Task.CompletedTask;
    }
}