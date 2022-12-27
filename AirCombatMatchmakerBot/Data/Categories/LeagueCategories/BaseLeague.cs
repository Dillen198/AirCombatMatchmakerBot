﻿using Discord.WebSocket;
using Discord;
using System.Runtime.Serialization;

[DataContract]
public abstract class BaseLeague : ILeague
{
    CategoryName ILeague.LeagueCategoryName
    {
        get => leagueCategoryName;
        set => leagueCategoryName = value;
    }

    Era ILeague.LeagueEra
    {
        get => leagueEra;
        set => leagueEra = value;
    }

    int ILeague.LeaguePlayerCountPerTeam
    {
        get => leaguePlayerCountPerTeam;
        set => leaguePlayerCountPerTeam = value;
    }

    List<UnitName> ILeague.LeagueUnits
    {
        get => leagueUnits;
        set => leagueUnits = value;
    }

    LeagueData ILeague.LeagueData
    {
        get => leagueData;
        set => leagueData = value;
    }

    DiscordLeagueReferences ILeague.DiscordLeagueReferences
    {
        get => discordleagueReferences;
        set => discordleagueReferences = value;
    }

    [DataMember] protected CategoryName leagueCategoryName;

    // Generated based on the implementation
    [DataMember] protected Era leagueEra;
    [DataMember] protected int leaguePlayerCountPerTeam;

    [DataMember] protected List<UnitName> leagueUnits = new List<UnitName>();

    [DataMember] protected LeagueData leagueData = new LeagueData();

    [DataMember] protected DiscordLeagueReferences discordleagueReferences = new DiscordLeagueReferences();

    public BaseLeague()
    {
    }

    public abstract List<Overwrite> GetGuildPermissions(SocketGuild _guild, SocketRole _role);

    private Team? FindActiveTeamByPlayerIdInAPredefinedLeague(ulong _playerId)
    {
        Log.WriteLine("Starting to find a active team by player id: " + _playerId +
            " in league: " + leagueCategoryName, LogLevel.VERBOSE);

        foreach (Team team in leagueData.Teams.GetListOfTeams())
        {
            Team? foundTeam = team.CheckIfTeamIsActiveAndContainsAPlayer(_playerId);

            if (foundTeam != null)
            {
                return foundTeam;
            }
        }

        Log.WriteLine("Team not found! Admin trying to access challenge" +
    " of a league that he's not registered to?", LogLevel.WARNING);

        return null;
    }

    public void PostChallengeToThisLeague(ulong _playerId)
    {
        Team? team = FindActiveTeamByPlayerIdInAPredefinedLeague(_playerId);

        if (team == null)
        {
            Log.WriteLine(nameof(team) +
                " was null! Could not find the team.", LogLevel.CRITICAL);
            return;
        }

        Log.WriteLine("Team found: " + team.GetTeamName() + " (" + team.GetTeamId() + ")" +
            " adding it to the challenge queue with count: " +
            leagueData.ChallengeStatus.GetListOfTeamsInTheQueue(),
            LogLevel.VERBOSE);

        leagueData.ChallengeStatus.AddToTeamsInTheQueue(team);

        Log.WriteLine(leagueData.ChallengeStatus.ReturnTeamsInTheQueueOfAChallenge(
            leaguePlayerCountPerTeam), LogLevel.VERBOSE);
    }
}