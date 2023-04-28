﻿using Discord.WebSocket;
using Discord;
using System.Runtime.Serialization;
using Discord.Rest;
using System;
using System.Collections.Concurrent;

[DataContract]
public abstract class BaseCategory : InterfaceCategory
{
    CategoryType InterfaceCategory.CategoryType
    {
        get
        {
            Log.WriteLine("Getting " + nameof(categoryTypes) + ": " + categoryTypes, LogLevel.VERBOSE);
            return categoryTypes;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(categoryTypes) + categoryTypes
                + " to: " + value, LogLevel.VERBOSE);
            categoryTypes = value;
        }
    }

    ConcurrentBag<ChannelType> InterfaceCategory.ChannelTypes
    {
        get
        {
            Log.WriteLine("Getting " + nameof(channelTypes) + " with count of: " +
                channelTypes.Count, LogLevel.VERBOSE);
            return channelTypes;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(channelTypes)
                + " to: " + value, LogLevel.VERBOSE);
            channelTypes = value;
        }
    }

    ConcurrentDictionary<ulong, InterfaceChannel> InterfaceCategory.InterfaceChannels
    {
        get
        {
            Log.WriteLine("Getting " + nameof(interfaceChannels) + " with count of: " +
                interfaceChannels.Count, LogLevel.VERBOSE);
            return interfaceChannels;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(interfaceChannels)
                + " to: " + value, LogLevel.VERBOSE);
            interfaceChannels = value;
        }
    }

    ulong InterfaceCategory.SocketCategoryChannelId
    {
        get
        {
            Log.WriteLine("Getting " + nameof(socketCategoryChannelId) +
                ": " + socketCategoryChannelId, LogLevel.VERBOSE);
            return socketCategoryChannelId;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(socketCategoryChannelId) + socketCategoryChannelId
                + " to: " + value, LogLevel.VERBOSE);
            socketCategoryChannelId = value;
        }
    }

    [DataMember] protected CategoryType categoryTypes;
    protected ConcurrentBag<ChannelType> channelTypes;
    [DataMember] protected ConcurrentDictionary<ulong, InterfaceChannel> interfaceChannels;
    [DataMember] protected ulong socketCategoryChannelId;

    public BaseCategory()
    {
        channelTypes = new ConcurrentBag<ChannelType>();
        interfaceChannels = new ConcurrentDictionary<ulong, InterfaceChannel>();
    }

    public abstract List<Overwrite> GetGuildPermissions(SocketGuild _guild, SocketRole _role);

    public async Task<SocketCategoryChannel?> CreateANewSocketCategoryChannelAndReturnIt(
        SocketGuild _guild, string _categoryName, SocketRole _role)
    {
        Log.WriteLine("Starting to create a new category with name: " +
            _categoryName, LogLevel.VERBOSE);

        RestCategoryChannel newCategory = await _guild.CreateCategoryChannelAsync(
            _categoryName, x => x.PermissionOverwrites = GetGuildPermissions(_guild, _role));
        if (newCategory == null)
        {
            Log.WriteLine(nameof(newCategory) + " was null!", LogLevel.CRITICAL);
            return null;
        }

        Log.WriteLine("Created a new RestCategoryChannel with ID: " +
            newCategory.Id, LogLevel.VERBOSE);

        SocketCategoryChannel socketCategoryChannel =
            _guild.GetCategoryChannel(newCategory.Id);

        if (socketCategoryChannel == null)
        {
            Log.WriteLine(nameof(socketCategoryChannel) + " was null!", LogLevel.CRITICAL);
            return null;
        }

        Log.WriteLine("Created a new socketCategoryChannel :" +
            socketCategoryChannel.Id.ToString() +" named: " +
            socketCategoryChannel.Name, LogLevel.DEBUG);

        return socketCategoryChannel;
    }

    public async Task CreateChannelsForTheCategory(
        ulong _socketCategoryChannelId, DiscordSocketClient _client, SocketRole _role)
    {
        Log.WriteLine("Starting to create channels for: " + _socketCategoryChannelId + ")" + 
            " Channel count: " + channelTypes.Count +
            " and setting the references", LogLevel.DEBUG);

        socketCategoryChannelId = _socketCategoryChannelId;

        foreach (ChannelType channelType in channelTypes)
        {
            // Checks for missing match channels from the league category
            if (channelType == ChannelType.MATCHCHANNEL)
            {
                await CreateTheMissingMatchChannels(_client, socketCategoryChannelId);
                continue;
            }

            InterfaceChannel? interfaceChannel = 
                await CreateSpecificChannelFromChannelType(channelType, _socketCategoryChannelId, _role);

            if (interfaceChannel == null)
            {
                Log.WriteLine(nameof(interfaceChannel) + " was null!", LogLevel.CRITICAL);
                continue;
            }

            await interfaceChannel.PostChannelMessages(_client);
        }
    }

    public async Task<InterfaceChannel?> CreateSpecificChannelFromChannelType(
        ChannelType _channelType, ulong _socketCategoryChannelId, SocketRole _role,
        string _overrideChannelName = "",// Keeps the functionality, but overrides the channel name
                                         // It is used for creating matches with correct name ID right now.
        params ulong[] _allowedUsersIdsArray)
    {
        bool channelExists = false;

        Log.WriteLine("Creating channel name: " + _channelType, LogLevel.DEBUG);

        var guild = BotReference.GetGuildRef();
        if (guild == null)
        {
            Exceptions.BotGuildRefNull();
            return null;
        }

        InterfaceChannel? interfaceChannel = GetChannelInstance(_channelType.ToString());

        Log.WriteLine("interfaceChannel initialsetup: " +
            interfaceChannel.ChannelType.ToString(), LogLevel.DEBUG);

        if (interfaceChannel == null)
        {
            Log.WriteLine(nameof(interfaceChannel) + " was null!", LogLevel.CRITICAL);
            return null;
        }

        interfaceChannel.ChannelName =
            GetChannelNameFromOverridenString(_overrideChannelName, _channelType);

        // Channel found from the basecategory (it exists)
        if (interfaceChannels.Any(
            x => x.Value.ChannelName == interfaceChannel.ChannelName))
        {
            Log.WriteLine(nameof(interfaceChannels) + " already contains channel: " +
                interfaceChannel.ChannelName, LogLevel.VERBOSE);

            // Replace interfaceChannel with a one that is from the database
            interfaceChannel = interfaceChannels.FirstOrDefault(
                x => x.Value.ChannelType == _channelType).Value;

            Log.WriteLine("Replaced with: " +
                interfaceChannel.ChannelType + " from db", LogLevel.DEBUG);

            channelExists = ChannelRestore.CheckIfChannelHasBeenDeletedAndRestoreForCategory(
                _socketCategoryChannelId, interfaceChannel, guild);
        }

        interfaceChannel.ChannelsCategoryId = _socketCategoryChannelId;

        if (!channelExists)
        {
            Log.WriteLine("Creating a channel named: " + interfaceChannel.ChannelType +
                " for category: " + categoryTypes + " (" +
                _socketCategoryChannelId + ")" + " with name: " +
                interfaceChannel.ChannelName, LogLevel.DEBUG);

            ulong categoryId =
                Database.Instance.Categories.FindCreatedCategoryWithChannelKvpByCategoryName(
                    categoryTypes).Key;

            await interfaceChannel.CreateAChannelForTheCategory(guild, _role, _allowedUsersIdsArray);

            interfaceChannel.InterfaceMessagesWithIds.Clear();

            interfaceChannels.TryAdd(interfaceChannel.ChannelId, interfaceChannel);

            Log.WriteLine("Done adding to the db. Count is now: " +
                interfaceChannels.Count +
                " for the ConcurrentBag of category: " + categoryTypes.ToString() +
                " (" + _socketCategoryChannelId + ")", LogLevel.VERBOSE);
        }
        /*
        else
        {
            var channel = guild.GetChannel(interfaceChannel.ChannelId);

            // Get the existing permission overrides for the channel
            OverwritePermissions existingPermissions = channel.GetPermissionOverwrite(guild, _allowedUsersIdsArray);

            // Modify the permission overrides as desired
            OverwritePermissions newPermissions = existingPermissions.Modify(viewChannel: PermValue.Deny);

            // Apply the modified permission overrides to the channel
            await channel.AddPermissionOverwriteAsync(guild.EveryoneRole, newPermissions);


            , x =>
            {
                x.PermissionOverwrites = GetGuildPermissions(_guild, _allowedUsersIdsArray);
                x.CategoryId = channelsCategoryId;
            });
        }*/

        Log.WriteLine("Done creating channel: " + interfaceChannel.ChannelId + " with name: " 
            + interfaceChannel.ChannelName, LogLevel.VERBOSE);

        return interfaceChannel;
    }

    private static InterfaceChannel GetChannelInstance(string _channelType)
    {
        return (InterfaceChannel)EnumExtensions.GetInstance(_channelType);
    }

    private static Task CreateTheMissingMatchChannels(
        DiscordSocketClient _client, ulong _socketCategoryChannelId)
    {
        Log.WriteLine("Checking for missing matches in: " + _socketCategoryChannelId, LogLevel.VERBOSE);

        InterfaceLeague? interfaceLeague =
            Database.Instance.Leagues.GetILeagueByCategoryId(_socketCategoryChannelId);

        if (interfaceLeague == null) 
        {
            Log.WriteLine(nameof(interfaceLeague) + " was null!", LogLevel.CRITICAL);
            return Task.CompletedTask;
        }

        Log.WriteLine("Found InterfaceLeague: " + interfaceLeague.LeagueCategoryName, LogLevel.VERBOSE);

        Matches matches = interfaceLeague.LeagueData.Matches;

        foreach (LeagueMatch match in matches.MatchesConcurrentBag)
        {
            Log.WriteLine("Looping on match id: " + match.MatchId +
                " with channelId: " + match.MatchChannelId, LogLevel.VERBOSE);

            var matchChannel = _client.GetChannelAsync(match.MatchChannelId).Result as ITextChannel;

            if (matchChannel != null)
            {
                Log.WriteLine("Found " + nameof(matchChannel) + matchChannel.Name, LogLevel.VERBOSE);
                continue;
            }

            Log.WriteLine(nameof(matchChannel) + " was not found!" +
                " Expected to find a channel with match id: " + match.MatchId, LogLevel.WARNING);

            matches.CreateAMatchChannel(match, interfaceLeague, _client);
        }

        return Task.CompletedTask;
    }

    private static string GetChannelNameFromOverridenString(
        string _overrideChannelName, ChannelType _channelType)
    {
        if (_overrideChannelName == "")
        {
            Log.WriteLine("Settings regular channel name to: " +
                _channelType.ToString(), LogLevel.DEBUG);
            // Maybe insert the name more properly here if needed later
            return _channelType.ToString();
        }
        // Channels such as the match channel, that have the same type,
        // but different names
        else
        {
            Log.WriteLine("Setting overriden channel name to: " +
                _overrideChannelName, LogLevel.DEBUG);
            return _overrideChannelName;
        }
    }

    public InterfaceChannel FindInterfaceChannelWithIdInTheCategory(
        ulong _idToSearchWith)
    {
        Log.WriteLine("Getting CategoryKvp with id: " + _idToSearchWith, LogLevel.VERBOSE);

        var foundInterfaceChannel = interfaceChannels.FirstOrDefault(x => x.Key == _idToSearchWith);
        Log.WriteLine("Found: " + foundInterfaceChannel.Value.ChannelName, LogLevel.VERBOSE);
        return foundInterfaceChannel.Value;
    }

    public InterfaceChannel FindInterfaceChannelWithNameInTheCategory(
        ChannelType _nameToSearchWith)
    {
        Log.WriteLine("Getting CategoryKvp with name: " + _nameToSearchWith, LogLevel.VERBOSE);

        var foundInterfaceChannel = interfaceChannels.FirstOrDefault(x => x.Value.ChannelType == _nameToSearchWith);
        Log.WriteLine("Found: " + foundInterfaceChannel.Value.ChannelName, LogLevel.VERBOSE);
        return foundInterfaceChannel.Value;
    }
}