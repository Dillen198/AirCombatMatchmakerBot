using Discord;
using Discord.WebSocket;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

[Serializable]
public class Database
{
    public static Database Instance
    {
        get
        {
            lock (padlock)
            {
                if (instance == null)
                {
                    instance = new Database();
                }
                return instance;
            }
        }
        set
        {
            instance = value;
        }
    }

    // Singleton stuff
    private static Database? instance = null;
    private static readonly object padlock = new object();

    // The Database components
    public PlayerData PlayerData { get; set; }
    public Admins Admins { get; set; }
    public CachedUsers CachedUsers { get; set; }
    public Categories Categories { get; set; }
    public Leagues Leagues { get; set; }
    public List<LeagueMatch> ArchivedLeagueMatches { get; set; }

    public Database()
    {
        Admins = new Admins();
        CachedUsers = new CachedUsers();
        Categories = new Categories();
        PlayerData = new PlayerData();
        Leagues = new Leagues();
        ArchivedLeagueMatches = new List<LeagueMatch>();
    }

    public async Task RemovePlayerFromTheDatabase(ulong _playerDiscordId)
    {
        Log.WriteLine("Removing player: " + _playerDiscordId + " from the database.", LogLevel.DEBUG);

        await PlayerData.DeletePlayerProfile(_playerDiscordId);
        CachedUsers.RemoveUserFromTheCachedList(_playerDiscordId);

        var interfaceChannel = Categories.FindCreatedCategoryWithChannelKvpByCategoryName(
            CategoryType.REGISTRATIONCATEGORY).Value.FindInterfaceChannelWithNameInTheCategory(
                ChannelType.LEAGUEREGISTRATION);

        Log.WriteLine("leagues count: " + Leagues.StoredLeagues.Count, LogLevel.VERBOSE);

        foreach (InterfaceLeague interfaceLeague in Leagues.StoredLeagues)
        {
            Log.WriteLine("Starting to process league: " + interfaceLeague.LeagueCategoryName, LogLevel.DEBUG);

            List<int> teamsToRemove = new List<int>();

            foreach (Team team in interfaceLeague.LeagueData.Teams.TeamsList)
            {
                Log.WriteLine("Looping through team: " + team.TeamName + "(" + team.TeamId + ")", LogLevel.VERBOSE);
                if (team.Players.Any(p => p.PlayerDiscordId == _playerDiscordId))
                {
                    Log.WriteLine("Player " + +_playerDiscordId + " is in team: " +
                        team.TeamName + "(" + team.TeamId + ")", LogLevel.VERBOSE);

                    teamsToRemove.Add(team.TeamId);

                }
                Log.WriteLine("done looping through teams: " + interfaceLeague.LeagueData.Teams.TeamsList.Count, LogLevel.VERBOSE);
            }

            foreach (int teamId in teamsToRemove)
            {
                interfaceLeague.LeagueData.ChallengeStatus.TeamsInTheQueue.Remove(teamId);

                foreach (LeagueMatch match in interfaceLeague.LeagueData.Matches.MatchesList)
                {
                    if (match.TeamsInTheMatch.ContainsKey(teamId))
                    {
                        Log.WriteLine("Match: " + match.MatchId + " contains: " + teamId +
                            " which has player: " + _playerDiscordId, LogLevel.DEBUG);

                        match.MatchReporting.EloSystem.CalculateAndSaveFinalEloDeltaForMatchForfeit(
                            match.MatchReporting.FindTeamsInTheMatch(interfaceLeague),
                            match.MatchReporting.TeamIdsWithReportData, teamId);
                        match.FinishTheMatch(interfaceLeague, true);
                    }
                }

                interfaceLeague.LeagueData.Teams.TeamsList.RemoveAll(t => t.TeamId == teamId);

                Log.WriteLine("Found and removed" + _playerDiscordId + " in team with id: " + teamId, LogLevel.DEBUG);
            }
            
            var challengeMessage = Categories.FindCreatedCategoryWithChannelKvpWithId(
                interfaceLeague.DiscordLeagueReferences.LeagueCategoryId).Value.
                    FindInterfaceChannelWithNameInTheCategory(
                        ChannelType.CHALLENGE).FindInterfaceMessageWithNameInTheChannel(
                            MessageName.CHALLENGEMESSAGE);
            if (challengeMessage == null)
            {
                Log.WriteLine(nameof(challengeMessage) + " was null!", LogLevel.ERROR);
                continue;
            }

            await challengeMessage.GenerateAndModifyTheMessage(false);

            Dictionary<string, InterfaceMessage> leagueRegistrationMessages = new Dictionary<string, InterfaceMessage>();

            foreach (var kvp in interfaceChannel.InterfaceMessagesWithIds)
            {
                if (kvp.Value.MessageName == MessageName.LEAGUEREGISTRATIONMESSAGE)
                {
                    leagueRegistrationMessages.Add(kvp.Key, kvp.Value);
                }
            }

            foreach (var messageKvp in leagueRegistrationMessages)
            {
                Log.WriteLine("ON: " + messageKvp.Key, LogLevel.DEBUG);

                LEAGUEREGISTRATIONMESSAGE? leagueRegistrationMessage = messageKvp.Value as LEAGUEREGISTRATIONMESSAGE;
                if (leagueRegistrationMessage == null)
                {
                    Log.WriteLine(nameof(leagueRegistrationMessage) + " was null!", LogLevel.CRITICAL);
                    return;
                }

                Log.WriteLine("ids: " + leagueRegistrationMessage.belongsToLeagueCategoryId + " | " +
                    interfaceLeague.DiscordLeagueReferences.LeagueCategoryId, LogLevel.DEBUG);

                if (leagueRegistrationMessage.belongsToLeagueCategoryId ==
                    interfaceLeague.DiscordLeagueReferences.LeagueCategoryId)
                {
                    Log.WriteLine("true, modifying", LogLevel.VERBOSE);

                    await messageKvp.Value.ModifyMessage(
                        leagueRegistrationMessage.GenerateMessageForSpecificCategoryLeague());

                    //await messageKvp.Value.GenerateAndModifyTheMessage();
                }

                Log.WriteLine("after if", LogLevel.VERBOSE);

                /*
                var message = messageKvp.Value;

                var leagueRegistrationMessage = message as LEAGUEREGISTRATIONMESSAGE;
                if (leagueRegistrationMessage == null)
                {
                    Log.WriteLine(nameof(leagueRegistrationMessage) + " was null!", LogLevel.ERROR);
                    continue;
                }

                if (leagueRegistrationMessage.IfLeagueRegistrationMessageIsCorrectFromCategoryId(
                    interfaceChannel, interfaceLeague.DiscordLeagueReferences.LeagueCategoryId))
                {
                    await leagueRegistrationMessage.GenerateAndModifyTheMessage();
                }*/
            }

            Log.WriteLine("Done looping through " + leagueRegistrationMessages.Count + " messages", LogLevel.VERBOSE);


            Log.WriteLine("before updating leaderboard", LogLevel.VERBOSE);

            // Updates the leaderboard after the player has been removed from the league
            interfaceLeague.UpdateLeagueLeaderboard();

            Log.WriteLine("Done processing league: " + interfaceLeague.LeagueCategoryName, LogLevel.VERBOSE);
        }

        Log.WriteLine("Done processing all leagues", LogLevel.VERBOSE);

        /*
        // Remove user's access (back to the registration...)
        await RoleManager.RevokeUserAccess(_playerDiscordId, "Member");

        foreach (InterfaceLeague interfaceLeague in Database.Instance.Leagues.StoredLeagues)
        {
            if (interfaceLeague.LeagueData.Teams.CheckIfPlayerIsAlreadyInATeamById(
                interfaceLeague.LeaguePlayerCountPerTeam, _playerDiscordId))
            {
                await RoleManager.RevokeUserAccess(_playerDiscordId, EnumExtensions.GetEnumMemberAttrValue(
                    interfaceLeague.LeagueCategoryName));
            } 
        }*/

        await SerializationManager.SerializeDB();
    }
}