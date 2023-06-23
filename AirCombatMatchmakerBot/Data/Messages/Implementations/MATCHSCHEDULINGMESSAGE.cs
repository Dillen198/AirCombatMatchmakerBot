﻿using System.Runtime.Serialization;
using System.Collections.Concurrent;
using Discord;

[DataContract]
public class MATCHSCHEDULINGMESSAGE : BaseMessage
{
    MatchChannelComponents mcc { get; set; }
    public MATCHSCHEDULINGMESSAGE()
    {
        thisInterfaceMessage.MessageName = MessageName.MATCHSCHEDULINGMESSAGE;

        thisInterfaceMessage.MessageButtonNamesWithAmount = new ConcurrentDictionary<ButtonName, int>(
            new ConcurrentBag<KeyValuePair<ButtonName, int>>()
            {
                new KeyValuePair<ButtonName, int>(ButtonName.ACCEPTSCHEDULEDTIME, 1),
            });

        thisInterfaceMessage.MessageEmbedTitle = "Schedule your match here using the /schedule command";
    }

    protected override void GenerateButtons(ComponentBuilder _component, ulong _leagueCategoryId)
    {
        base.GenerateRegularButtons(_component, _leagueCategoryId);
    }

    public override string GenerateMessage()
    {
        try
        {
            thisInterfaceMessage.MessageDescription = "Enter the time you would be able to play the match on in format: \n " +
                "``/schedule 27.11.2022 1030z``\n" + "Instead of using the date 27.11.2022 you can use: [today, tomorrow]," +
                " [any weekday] (will assume the next day that's available), [now] (will schedule the match 20 minutes away, 5 minutes time to accept)." +
                "\nYou can also use slashes in the date and put the time first, for example: ``/schedule 11z 27/11/2022``\n\n" +
                "Examples:\n" +
                "``/schedule 0659z 01.02.2024``\n" +
                "``/schedule 03/04/2024 07z``\n" +
                "``/schedule 1835z saturday``\n" +
                "``/schedule sunday 19z``\n" +
                "``/schedule 1920z today``\n" +
                "``/schedule tomorrow 2023z``\n" +
                "``/schedule now``\n\n" +
                "Once you agree to a certain time, use ``/schedule accept`` command, or click the button below!";

            mcc = new MatchChannelComponents(this);
            if (mcc.interfaceLeagueCached == null || mcc.leagueMatchCached == null)
            {
                string errorMsg = nameof(mcc.interfaceLeagueCached) + " or " +
                    nameof(mcc.leagueMatchCached) + " was null!";
                Log.WriteLine(errorMsg, LogLevel.CRITICAL);
                return errorMsg;
            }

            var scheduleObject = mcc.leagueMatchCached.ScheduleObject;
            var teamsInTheMatch = mcc.leagueMatchCached.TeamsInTheMatch;
            if (!teamsInTheMatch.ContainsKey(scheduleObject.TeamIdThatRequestedScheduling))
            {
                return thisInterfaceMessage.MessageDescription;
            }

            var requestedTime = TimeService.ConvertToDateTimeFromUnixTime(scheduleObject.RequestedSchedulingTimeInUnixTime);

            var teamNameThatScheduled = teamsInTheMatch.First(
                t => t.Key == scheduleObject.TeamIdThatRequestedScheduling).Value;
            /*
            string time = TimeService.ReturnTimeLeftAsStringFromTheTimeTheActionWillTakePlace(requestedTime);
            Log.WriteLine("time: " + time, LogLevel.VERBOSE);
            Log.WriteLine("teamNameThatScheduled: " + teamNameThatScheduled, LogLevel.VERBOSE);*/

            thisInterfaceMessage.MessageDescription += "\n\n" + requestedTime + " requested by team: " + teamNameThatScheduled;

            return thisInterfaceMessage.MessageDescription;
        }
        catch (Exception ex) 
        {
            Log.WriteLine(ex.Message, LogLevel.CRITICAL);
            return ex.Message;
        }

    }
}