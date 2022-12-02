﻿using Discord;
using Discord.WebSocket;

public static class PlayerManager
{
    public static async Task HandleUserJoin(SocketGuildUser _user)
    {
        if (!_user.IsBot)
        {
            Log.WriteLine("User: " + _user + " has joined the discord with id: " + _user.Id +
                " starting the registation process", LogLevel.DEBUG);

            if (BotReference.clientRef != null)
            {
                SocketGuild guild = BotReference.clientRef.GetGuild(BotReference.GuildID);

                // Creates a private channel for the user to proceed with the registeration 
                await PlayerRegisteration.CreateANewRegisterationChannel(_user, guild, 1047529896735428638); // category ID
            }
            else Exceptions.BotClientRefNull();
        }
        else
        {
            Log.WriteLine("A bot: " + _user.Username +
                " joined the discord, disregarding the registeration process", LogLevel.WARNING);
        }
    }

    public static async Task HandlePlayerLeave(SocketGuild _guild, SocketUser _user)
    {
        Log.WriteLine(_user.Username + " (" + _user.Id +
            ") bailed out! Handling deleting registeration channels etc.", LogLevel.DEBUG);

        await ChannelManager.DeleteUsersChannelsOnLeave(_guild, _user);
    }

    public static bool AddNewPlayerToTheDatabaseById(ulong _playerId)
    {
        Log.WriteLine("Start of the addnewplayer with: " + _playerId, LogLevel.VERBOSE);

        var nickName = CheckIfNickNameIsEmptyAndReturnUsername(_playerId);

        Log.WriteLine("Adding a new player: " + nickName + " (" + _playerId + ").", LogLevel.DEBUG);

        // Checks if the player is already in the databse, just in case
        if (!CheckIfUserIdExistsInTheDatabase(_playerId))
        {
            Database.Instance.PlayerData.PlayerIDs.Add(_playerId, new Player(_playerId, nickName));
            return true;
        }
        else
        {
            Log.WriteLine("Tried to add a player that was already in the database!", LogLevel.WARNING);
            return false;
        }
    }
    public static async Task HandleGuildMemberUpdated(Cacheable<SocketGuildUser, ulong> before, SocketGuildUser _socketGuildUserAfter)
    {
        var playerValue = Database.Instance.PlayerData.PlayerIDs.First(x => x.Key == _socketGuildUserAfter.Id).Value;

        // This should not be empty, since it's being looked up from the database
        string playerValueNickName = playerValue.playerNickName;

        string socketGuildUserAfterNickName = CheckIfNickNameIsEmptyAndReturnUsername(_socketGuildUserAfter.Id);

        Log.WriteLine("Updating user: " + _socketGuildUserAfter.Username + " (" + _socketGuildUserAfter.Id + ")" + 
            " | name: " + playerValueNickName + " -> " + socketGuildUserAfterNickName, LogLevel.DEBUG);

        if (playerValue != null)
        {
            playerValue.playerNickName = socketGuildUserAfterNickName;
            await SerializationManager.SerializeDB();
        }
        else Log.WriteLine("Trying to update " + _socketGuildUserAfter.Username + "'s profile, no valid player found (not registed?) ", LogLevel.DEBUG);
    }

    public static string CheckIfNickNameIsEmptyAndReturnUsername(ulong _id)
    {
        Log.WriteLine("Checking if nickname is empty and return username with ID: " + _id, LogLevel.VERBOSE);

        var SocketGuildUser = GetSocketGuildUserById(_id);

        if (SocketGuildUser != null)
        {
            Log.WriteLine("SocketGuildUser " + _id + " is not null", LogLevel.VERBOSE);

            string userName = SocketGuildUser.Username;
            string nickName = SocketGuildUser.Nickname;

            Log.WriteLine("Checking if " + userName + "'s (" + _id + ")" +
                " nickName: " + nickName + " | " + " is the same", LogLevel.VERBOSE);

            if (nickName == "" || nickName == userName || nickName == null)
            {
                Log.WriteLine("returning userName", LogLevel.VERBOSE);
                return userName;
            }
            else
            {
                Log.WriteLine("returning nickName", LogLevel.VERBOSE);
                return nickName;
            }
        }
        else
        {
            Log.WriteLine("SocketGuildUser by ID: " + _id + " is null!", LogLevel.ERROR);
            return null;
        }
    }

    public static bool DeletePlayerProfile(string _dataValue)
    {
        ulong id = UInt64.Parse(_dataValue);
        if (CheckIfUserIdExistsInTheDatabase(id))
        {
            Log.WriteLine("Deleting a player profile " + _dataValue, LogLevel.DEBUG);
            Database.Instance.PlayerData.PlayerIDs.Remove(id);
            return true;
        }
        else
        {
            Log.WriteLine("Did not find ID: " + id + "in the local database.", LogLevel.DEBUG);
            return false;
        }
    }

    // Just checks if the User discord ID profile exists in the database file
    public static bool CheckIfUserIdExistsInTheDatabase(ulong _id)
    {
        return Database.Instance.PlayerData.PlayerIDs.ContainsKey(_id);
    }

    // Gets the user by the discord UserId. This may not be present in the Database.
    public static SocketGuildUser? GetSocketGuildUserById(ulong _id)
    {
        Log.WriteLine("Getting SocketGuildUser by id: " + _id, LogLevel.DEBUG);

        if (BotReference.clientRef != null)
        {
            return BotReference.clientRef.GetGuild(BotReference.GuildID).GetUser(_id);
        }

        else
        {
            Exceptions.BotClientRefNull();
            return null;
        }
    }

    /*
    public static async Task MessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after, ISocketMessageChannel channel)
    {
        // If the message was not in the cache, downloading it will result in getting a copy of `after`.
        var message = await before.GetOrDownloadAsync();
        Console.WriteLine($"{message} -> {after}");
    } */
}