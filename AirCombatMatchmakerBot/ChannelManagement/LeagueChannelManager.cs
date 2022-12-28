﻿using Discord;
using Discord.WebSocket;

public static class LeagueChannelManager
{
    public static async Task<ulong> CreateALeagueJoinButton(
        ITextChannel _leagueRegistrationChannel, InterfaceLeague? _leagueInterface, string _leagueNameString)
    {
        Log.WriteLine("Starting to create a league join button for: " + _leagueNameString, LogLevel.VERBOSE);

        if (_leagueInterface == null)
        {
            Log.WriteLine("_leagueInterface was null!", LogLevel.CRITICAL);
            return 0;
        }

        Log.WriteLine(nameof(_leagueInterface) + " before creating leagueButtonRegisterationCustomId: "
            + _leagueInterface.ToString(), LogLevel.VERBOSE);

        string leagueButtonRegisterationCustomId =
           "leagueRegistration_" + _leagueInterface.DiscordLeagueReferences.LeagueCategoryId;

        Log.WriteLine(nameof(leagueButtonRegisterationCustomId) + ": " +
            leagueButtonRegisterationCustomId, LogLevel.VERBOSE);

        _leagueInterface =
            Database.Instance.Leagues.GetInterfaceLeagueCategoryFromTheDatabase(_leagueInterface);

        if (_leagueInterface == null)
        {
            Log.WriteLine("_leagueInterface was null!", LogLevel.CRITICAL);
            return 0;
        }

        _leagueInterface.DiscordLeagueReferences.LeagueRegistrationChannelMessageId =
            await ButtonComponents.CreateButtonMessage(
                _leagueRegistrationChannel.Id,
                MessageManager.GenerateALeagueJoinButtonMessage(_leagueInterface),
                "Join",
                leagueButtonRegisterationCustomId); // Maybe replace this with some other system

        Log.WriteLine("Done creating a league join button for: " + _leagueNameString, LogLevel.DEBUG);
        
        return _leagueInterface.DiscordLeagueReferences.LeagueRegistrationChannelMessageId;
    }
}