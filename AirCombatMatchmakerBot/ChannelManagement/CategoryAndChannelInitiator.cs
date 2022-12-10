﻿using Discord;
using Discord.WebSocket;
using System;

public static class CategoryAndChannelInitiator
{
    public static async Task CreateCategoriesAndChannelsForTheDiscordServer()
    {
        Log.WriteLine("Starting to create categories and channels for" +
            " the discord server", LogLevel.VERBOSE);

        var guild = BotReference.GetGuildRef();
        Log.WriteLine("guild valid", LogLevel.VERBOSE);
        if (guild == null)
        {
            Exceptions.BotGuildRefNull();
            return;
        }

        var categoryEnumValues = Enum.GetValues(typeof(CategoryName));

        Log.WriteLine(nameof(categoryEnumValues) + " length: " + categoryEnumValues.Length, LogLevel.VERBOSE);

        // Loop through every category names creating them and the channels for them
        foreach (CategoryName categoryName in Enum.GetValues(typeof(CategoryName)))
        {
            Log.WriteLine("Looping on category name: " + categoryName.ToString(), LogLevel.VERBOSE);
            // Check here too if a category is missing channels
            bool categoryExists = false;

            CategoryProperties categoryProperties = new();

            if (Database.Instance.CreatedCategoriesWithChannels.Any(x => x.Key.interfaceCategory.CategoryName == categoryName))
            {
                Log.WriteLine(nameof(Database.Instance.CreatedCategoriesWithChannels) + " already contains: " +
                    categoryName.ToString(), LogLevel.VERBOSE);
                categoryExists = true;
            }

            string? categoryNameString = EnumExtensions.GetEnumMemberAttrValue(categoryName);
            if (categoryNameString == null)
            {
                Log.WriteLine(nameof(categoryName).ToString() + " was null!", LogLevel.CRITICAL);
                return;
            }

            Log.WriteLine("Creating a category named: " + categoryNameString, LogLevel.VERBOSE);

            InterfaceCategory interfaceCategory = GetCategoryInstance(categoryName);
            if (interfaceCategory == null)
            {
                Log.WriteLine(nameof(interfaceCategory).ToString() + " was null!", LogLevel.CRITICAL);
                return;
            }

            BaseCategory baseCategory = interfaceCategory as BaseCategory;
            if (baseCategory == null)
            {
                Log.WriteLine(nameof(baseCategory).ToString() + " was null!", LogLevel.CRITICAL);
                return;
            }

            List<Overwrite> permissionsList = baseCategory.GetGuildPermissions(guild);

            SocketCategoryChannel? socketCategoryChannel = null;

            // If the category doesn't exist at all, create it and add it to the database
            if (!categoryExists)
            {
                socketCategoryChannel =
                    await CategoryManager.CreateANewSocketCategoryChannelAndReturnIt(
                        guild, categoryNameString, permissionsList);
                if (socketCategoryChannel == null)
                {
                    Log.WriteLine(nameof(socketCategoryChannel) + " was null!", LogLevel.CRITICAL);
                    return;
                }

                Log.WriteLine("Created a " + nameof(socketCategoryChannel) + " with id: " + socketCategoryChannel.Id +
                    " that's named: " + socketCategoryChannel.Name, LogLevel.VERBOSE);

                // Insert the id to the interface from the socket channel creation
                //categoryId = socketCategoryChannel.Id;

                Log.WriteLine("Adding " + nameof(interfaceCategory) + " to " +
                    nameof(Database.Instance.CreatedCategoriesWithChannels), LogLevel.VERBOSE);

                Database.Instance.CreatedCategoriesWithChannels.Add(categoryProperties, new List<InterfaceChannel>());

                Log.WriteLine("Done adding " + nameof(interfaceCategory) + " to " +
                    nameof(Database.Instance.CreatedCategoriesWithChannels), LogLevel.DEBUG);
            }
            // The category exists, just find it from the database and then get the id of the socketchannel
            else
            {
                var dbCategory = Database.Instance.CreatedCategoriesWithChannels.First(
                    x => x.Key.interfaceCategory.CategoryName == interfaceCategory.CategoryName);

                InterfaceCategory databaseInterfaceCategory = GetCategoryInstance(categoryName);
                if (databaseInterfaceCategory == null)
                {
                    Log.WriteLine(nameof(databaseInterfaceCategory).ToString() + " was null!", LogLevel.CRITICAL);
                    return;
                }

                Log.WriteLine("Found " + nameof(databaseInterfaceCategory) + " with id: " +
                    databaseInterfaceCategory.CategoryId + " named: " +
                    databaseInterfaceCategory.CategoryName, LogLevel.VERBOSE);

                socketCategoryChannel = guild.GetCategoryChannel(databaseInterfaceCategory.CategoryId);

                Log.WriteLine("Found " + nameof(socketCategoryChannel) + " that's named: " +
                    socketCategoryChannel.Name, LogLevel.DEBUG);
            }

            categoryProperties.interfaceCategory = interfaceCategory;
            categoryProperties.categoryId = socketCategoryChannel.Id;
            await CreateChannelsForTheCategory(categoryProperties, socketCategoryChannel, guild);
        }
        await SerializationManager.SerializeDB();
    }

    public static async Task CreateChannelsForTheCategory(
        CategoryProperties _categoryProperties,
        SocketCategoryChannel _socketCategoryChannel,
        SocketGuild _guild)
    {
        Log.WriteLine("Starting to create channels for: " + _socketCategoryChannel.Name +
            " ( " + _socketCategoryChannel.Id + ")" + " Channel count: " +
            _categoryProperties.interfaceCategory.Channels.Count, LogLevel.VERBOSE) ;

        List<InterfaceChannel> channelListForCategory = Database.Instance.CreatedCategoriesWithChannels.First(
            x => x.Key.interfaceCategory.CategoryName == _categoryProperties.interfaceCategory.CategoryName).Value;
        if (channelListForCategory == null)
        {
            Log.WriteLine(nameof(channelListForCategory) + " was null!", LogLevel.CRITICAL);
            return;
        }

        Log.WriteLine("Found " + nameof(channelListForCategory) 
            + " channel count: " + channelListForCategory.Count, LogLevel.VERBOSE);

        foreach (ChannelName channelName in _categoryProperties.interfaceCategory.Channels)
        {
            if (channelListForCategory.Any(x => x.ChannelName == channelName))
            {
                Log.WriteLine(nameof(channelListForCategory) + " already contains channel: " +
                    channelName.ToString(), LogLevel.VERBOSE);
                continue;
            }

            Log.WriteLine("Does not contain: " + channelName.ToString() + " adding it", LogLevel.DEBUG);

            InterfaceChannel interfaceChannel = GetChannelInstance(channelName);
            if (interfaceChannel == null)
            {
                Log.WriteLine(nameof(interfaceChannel).ToString() + " was null!", LogLevel.CRITICAL);
                return;
            }

            BaseChannel baseChannel = interfaceChannel as BaseChannel;
            if (baseChannel == null)
            {
                Log.WriteLine(nameof(baseChannel).ToString() + " was null!", LogLevel.CRITICAL);
                return;
            }

            List<Overwrite> permissionsList = baseChannel.GetGuildPermissions(_guild);

            string? channelNameString = EnumExtensions.GetEnumMemberAttrValue(channelName);

            if (channelNameString == null) 
            {
                Log.WriteLine(nameof(channelNameString).ToString() + " was null!", LogLevel.CRITICAL);
                return;
            }

            Log.WriteLine("Creating a channel named: " + channelNameString + " for category: " 
                + _categoryProperties.categoryId, LogLevel.VERBOSE);
            
            interfaceChannel.ChannelId = await ChannelManager.CreateAChannelForTheCategory(
                _guild, channelNameString, _categoryProperties.categoryId, permissionsList);

            Log.WriteLine("Done creating the channel with id: " + interfaceChannel.ChannelId +
                " named:" + channelNameString + " adding it to the db.", LogLevel.DEBUG);

            channelListForCategory.Add(interfaceChannel);

            Log.WriteLine("Done adding to the db. Count is now: " + channelListForCategory.Count +
                " for the list of category: " + _categoryProperties.interfaceCategory.CategoryName.ToString() +
                " (" + _categoryProperties.categoryId + ")", LogLevel.VERBOSE);

            //LeagueChannelFeatures.ActivateFeatureOfTheChannel(channelId, channelType);

            Log.WriteLine("Done looping through: " + channelNameString, LogLevel.VERBOSE);
        }
    }

    public static InterfaceCategory GetCategoryInstance(CategoryName _categoryName)
    {
        return (InterfaceCategory)EnumExtensions.GetInstance(_categoryName.ToString());
    }

    public static InterfaceChannel GetChannelInstance(ChannelName _channelName)
    {
        return (InterfaceChannel)EnumExtensions.GetInstance(_channelName.ToString());
    }
}