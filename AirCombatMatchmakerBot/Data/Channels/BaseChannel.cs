﻿using Discord.WebSocket;
using Discord;
using System.Runtime.Serialization;
using System;
using System.Threading.Channels;

[DataContract]
public abstract class BaseChannel : InterfaceChannel
{
    ChannelType InterfaceChannel.ChannelType
    {
        get 
        {
            Log.WriteLine("Getting " + nameof(ChannelType) + ": " + channelType, LogLevel.VERBOSE);
            return channelType;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(channelType) + channelType
                + " to: " + value, LogLevel.VERBOSE);
            channelType = value;
        }
    }

    string InterfaceChannel.ChannelName
    {
        get
        {
            Log.WriteLine("Getting " + nameof(channelName) + ": " + channelName, LogLevel.VERBOSE);
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

    Dictionary<MessageName, bool> InterfaceChannel.ChannelMessages
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

    Dictionary<ulong, InterfaceMessage> InterfaceChannel.InterfaceMessagesWithIds
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

    [DataMember] protected ChannelType channelType { get; set; }
    [DataMember] protected string channelName { get; set; }
    [DataMember] protected ulong channelId { get; set; }
    [DataMember] protected ulong channelsCategoryId { get; set; }
    [DataMember] protected Dictionary<MessageName, bool> channelMessages { get; set; }
    [DataMember] protected Dictionary<ulong, InterfaceMessage> interfaceMessagesWithIds { get; set; }

    public BaseChannel()
    {
        channelMessages = new Dictionary<MessageName, bool>();
        interfaceMessagesWithIds = new Dictionary<ulong, InterfaceMessage>();
    }

    public abstract List<Overwrite> GetGuildPermissions(
        SocketGuild _guild, params ulong[] _allowedUsersIdsArray);

    public async Task CreateAChannelForTheCategory(SocketGuild _guild,
         params ulong[] _allowedUsersIdsArray)
    {
        Log.WriteLine("Creating a channel named: " + channelType +
            " for category: " + channelsCategoryId, LogLevel.VERBOSE);

        string channelTypeString = EnumExtensions.GetEnumMemberAttrValue(channelType);

        // Temp fix perhaps unnecessary after the name has been set more properly 
        // for non-match channels
        if (channelName.Contains("match-"))
        {
            channelTypeString = channelName;
        }

        var channel = await _guild.CreateTextChannelAsync(channelTypeString, x =>
        {
            x.PermissionOverwrites = GetGuildPermissions(_guild, _allowedUsersIdsArray);
            x.CategoryId = channelsCategoryId;
        });

        channelId = channel.Id;

        Log.WriteLine("Done creating a channel named: " + channelType + " with ID: " + channel.Id +
            " for category: " + channelsCategoryId, LogLevel.DEBUG);
    }

    public async Task<(ulong, string)> CreateAMessageForTheChannelFromMessageName(
        MessageName _MessageName, bool _displayMessage = true)
    {
        Log.WriteLine("Creating a message named: " + _MessageName.ToString(), LogLevel.DEBUG);

        //messageNameString = _MessageName.ToString();

        var guild = BotReference.GetGuildRef();

        if (guild == null)
        {
            return (0, Exceptions.BotGuildRefNull());
        }

        InterfaceMessage interfaceMessage =
            (InterfaceMessage)EnumExtensions.GetInstance(_MessageName.ToString());

        //KeyValuePair<string, InterfaceMessage> interfaceMessageKvp = new(_MessageName.ToString(), interfaceMessage);

        var newMessageTuple = await interfaceMessage.CreateTheMessageAndItsButtonsOnTheBaseClass(
            guild, this, _displayMessage);

        return newMessageTuple;
    }

    /*
    public virtual async Task PrepareChannelMessages()
    {
        Log.WriteLine("Starting to prepare channel messages on: " + channelType, LogLevel.VERBOSE);

        var guild = BotReference.GetGuildRef();

        if (guild == null)
        {
            Exceptions.BotGuildRefNull();
            return;
        }

        // Add to a method later
        var databaseInterfaceChannel =
            Database.Instance.Categories.FindCreatedCategoryWithChannelKvpWithId(channelsCategoryId).
                Value.InterfaceChannels.FirstOrDefault(
                    x => x.Value.ChannelId == channelId);

        foreach (MessageName messageName in channelMessagesFromDb)
        {
            Log.WriteLine("on: " + nameof(messageName) + " " + messageName, LogLevel.VERBOSE);

            InterfaceMessage interfaceMessage =
                (InterfaceMessage)EnumExtensions.GetInstance(messageName.ToString());

            if (databaseInterfaceChannel.Value.InterfaceMessagesWithIds.ContainsKey(
                messageName.ToString())) continue;

            Log.WriteLine("Does not contain the key: " +
                messageName + ", continuing", LogLevel.VERBOSE);

            databaseInterfaceChannel.Value.InterfaceMessagesWithIds.Add(
                messageName.ToString(), interfaceMessage);

            Log.WriteLine("Done with: " + messageName, LogLevel.VERBOSE);
        }
        Log.WriteLine("Done posting channel messages on " +
            channelType + " id: " + channelId, LogLevel.VERBOSE);

        await PostChannelMessages(guild, databaseInterfaceChannel.Value);
    }*/

    public virtual async Task PostChannelMessages(SocketGuild _guild)
    {
        //Log.WriteLine("Starting to post channel messages on: " + channelType, LogLevel.VERBOSE);

        Log.WriteLine("Finding channel: " + channelType + " (" + channelId +
            ") parent category with id: " + channelsCategoryId, LogLevel.VERBOSE);

        // Had to use client here instead of guild for searching the channel, otherwise didn't work (??)
        var client = BotReference.GetClientRef();
        if (client == null)
        {
            Exceptions.BotClientRefNull();
            return;
        }

        // If the message doesn't exist, set it ID to 0 to regenerate it
        var channel = client.GetChannelAsync(channelId).Result as ITextChannel;

        if (channel == null)
        {
            Log.WriteLine(nameof(channel) + " was null!", LogLevel.CRITICAL);
            return;
        }

        var channelMessagesFromDb = await channel.GetMessagesAsync(50, CacheMode.AllowDownload).FirstOrDefaultAsync();
        if (channelMessagesFromDb == null)
        {
            Log.WriteLine(nameof(channelMessagesFromDb) + " was null!", LogLevel.CRITICAL);
            return;
        }

        Log.WriteLine(nameof(interfaceMessagesWithIds) + " count: " +
            interfaceMessagesWithIds.Count + " | " + nameof(channelMessagesFromDb) +
            " count: " + channelMessagesFromDb.Count, LogLevel.VERBOSE);

        if (channelType == ChannelType.LEAGUEREGISTRATION)
        {
            Log.WriteLine("Starting to to prepare channel messages on " + channelType +
                " count: " + Enum.GetValues(typeof(CategoryType)).Length, LogLevel.VERBOSE);

            /*
            // Add to a method later
            var databaseInterfaceChannel =
                Database.Instance.Categories.CreatedCategoriesWithChannels.FirstOrDefault(
                    x => x.Key == channelsCategoryId).Value.InterfaceChannels.FirstOrDefault(
                        x => x.Value.ChannelId == channelId);*/

            //Log.WriteLine("After db find", LogLevel.VERBOSE);

            foreach (CategoryType leagueName in Enum.GetValues(typeof(CategoryType)))
            {
                Log.WriteLine("Looping on to find leagueName: " + leagueName.ToString(), LogLevel.VERBOSE);

                // Skip all the non-leagues
                int enumValue = (int)leagueName;
                if (enumValue > 100) continue;

                string? leagueNameString = EnumExtensions.GetEnumMemberAttrValue(leagueName);
                Log.WriteLine("leagueNameString after enumValueCheck: " + leagueNameString, LogLevel.VERBOSE);
                if (leagueNameString == null)
                {
                    Log.WriteLine(nameof(leagueNameString) + " was null!", LogLevel.CRITICAL);
                    return;
                }

                var leagueInterface = LeagueManager.GetLeagueInstanceWithLeagueCategoryName(leagueName);
                if (leagueInterface == null)
                {
                    Log.WriteLine("leagueInterface was null!", LogLevel.CRITICAL);
                    return;
                }

                var leagueInterfaceFromDatabase =
                    Database.Instance.Leagues.GetInterfaceLeagueCategoryFromTheDatabase(leagueInterface);

                Log.WriteLine("Starting to create a league join button for: " + leagueNameString, LogLevel.VERBOSE);

                if (leagueInterfaceFromDatabase == null)
                {
                    Log.WriteLine("_leagueInterface was null!", LogLevel.CRITICAL);
                    return;
                }

                Log.WriteLine(nameof(leagueInterfaceFromDatabase) + " before creating leagueButtonRegisterationCustomId: "
                    + leagueInterfaceFromDatabase.ToString(), LogLevel.VERBOSE);

                /*
                InterfaceMessage interfaceMessage =
                    
                Log.WriteLine("Created interfaceMessage instance: " +
                    interfaceMessage.MessageName, LogLevel.VERBOSE); */

                if (leagueInterfaceFromDatabase.DiscordLeagueReferences.LeagueRegistrationMessageId != 0) continue;

                InterfaceMessage interfaceMessage =
                    (InterfaceMessage)EnumExtensions.GetInstance(MessageName.LEAGUEREGISTRATIONMESSAGE.ToString());

                var messageTuple = await interfaceMessage.CreateTheMessageAndItsButtonsOnTheBaseClass(
                        _guild, this, true, leagueInterfaceFromDatabase.DiscordLeagueReferences.LeagueCategoryId);

                Log.WriteLine("messagetuple:" + messageTuple.Item1 + " | " + messageTuple.Item2, LogLevel.DEBUG);

                leagueInterfaceFromDatabase.DiscordLeagueReferences.LeagueRegistrationMessageId = messageTuple.Item1;

                //string stringToGetTheInstanceFrom = channelMessagesFromDb.ElementAt(0).ToString();

                interfaceMessagesWithIds.Add(
                    leagueInterfaceFromDatabase.DiscordLeagueReferences.LeagueCategoryId,
                        (InterfaceMessage)EnumExtensions.GetInstance(MessageName.LEAGUEREGISTRATIONMESSAGE.ToString()));

                Log.WriteLine("Added to the dictionary, count is now: " +
                    interfaceMessagesWithIds.Count, LogLevel.VERBOSE);

                //Log.WriteLine("Done looping on: " + leagueNameString, LogLevel.VERBOSE);
            }
        }
        else
        {
            for (int m = 0; m < channelMessages.Count; ++m)
            {
                // Skip the messages that have been generated already
                if (!channelMessages.ElementAt(m).Value)
                {
                    //Log.WriteLine("Looping on message: " + messageName, LogLevel.VERBOSE);

                    //var messageKey = interfaceMessagesWithIds[interfaceMessageKvp.Key];

                    /*
                    Log.WriteLine("messageKey id: " + messageKey.MessageId, LogLevel.VERBOSE);

                    Log.WriteLine("Any: " + channelMessagesFromDb.Any(x => x.Id == messageKey.MessageId), LogLevel.DEBUG);

                    if (!channelMessagesFromDb.Any(x => x.Id == messageKey.MessageId) && messageKey.MessageId != 0)
                    {
                        Log.WriteLine("Message " + messageKey.MessageId +
                            "not found! Setting it to 0 and regenerating", LogLevel.WARNING);
                    1
                        messageKey.ButtonsInTheMessage.Clear();
                        messageKey.MessageId = 0;
                    }

                    if (messageKey.MessageId != 0) continue;

                    Log.WriteLine("Key was 0, message does not exist. Creating it.", LogLevel.VERBOSE);
                    */

                    InterfaceMessage interfaceMessage =
                        (InterfaceMessage)EnumExtensions.GetInstance(channelMessages.ElementAt(m).Key.ToString());

                    await interfaceMessage.CreateTheMessageAndItsButtonsOnTheBaseClass(_guild, this, true);
                    channelMessages[channelMessages.ElementAt(m).Key] = true;
                }
            }

            if (channelType == ChannelType.BOTLOG)
            {
                BotMessageLogging.loggingChannelId = channelId;
            }
        }
    }

    //public bool CheckIf

    // Finds ANY message with that message name (there can be multiple of same messages now)
    public InterfaceMessage? FindInterfaceMessageWithNameInTheChannel(
        MessageName _messageName)
    {
        Log.WriteLine("Getting CategoryKvp with name: " + _messageName, LogLevel.VERBOSE);

        var foundInterfaceMessage = interfaceMessagesWithIds.FirstOrDefault(
            x => x.Value.MessageName == _messageName);
        if (foundInterfaceMessage.Value == null)
        {
            Log.WriteLine(nameof(foundInterfaceMessage) + " was null!", LogLevel.CRITICAL);
            return null;
        }

        Log.WriteLine("Found: " + foundInterfaceMessage.Value.MessageName, LogLevel.VERBOSE);
        return foundInterfaceMessage.Value;
    }

    public async Task<IMessageChannel?> GetMessageChannelById(DiscordSocketClient _client)
    {
        Log.WriteLine("Getting IMessageChannel with id: " + channelId, LogLevel.VERBOSE);

        var channel = await _client.GetChannelAsync(channelId) as IMessageChannel;
        if (channel == null)
        {
            Log.WriteLine(nameof(channel) + " was null!", LogLevel.ERROR);
            return null;
        }

        Log.WriteLine("Found: " + channel.Id, LogLevel.VERBOSE);
        return channel;
    }
}