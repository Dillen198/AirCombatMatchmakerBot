﻿using System.Runtime.Serialization;
using System.Collections.Concurrent;
using Discord;

[DataContract]
public class CONFIRMMATCHENTRYMESSAGE : BaseMessage
{
    MatchChannelComponents mcc;

    public CONFIRMMATCHENTRYMESSAGE()
    {
        thisInterfaceMessage.MessageName = MessageName.CONFIRMMATCHENTRYMESSAGE;
        thisInterfaceMessage.MessageEmbedTitle = "Select your plane. If you do not select your plane before the timer below expires," +
            " the match will be timed out.";
    }

    protected override void GenerateButtons(ComponentBuilder _component, ulong _leagueCategoryId)
    {
        Log.WriteLine("Generating buttons with: " + _leagueCategoryId, LogLevel.VERBOSE);

        Dictionary<string, string> buttonsToGenerate = new Dictionary<string, string>();

        mcc = new MatchChannelComponents(this);
        if (mcc.interfaceLeagueCached == null || mcc.leagueMatchCached == null)
        {
            Log.WriteLine(nameof(mcc) + " was null!", LogLevel.CRITICAL);
            return;
        }
        Log.WriteLine("units count: " + mcc.interfaceLeagueCached.LeagueUnits.Count, LogLevel.VERBOSE);

        foreach (UnitName unitName in mcc.interfaceLeagueCached.LeagueUnits)
        {
            string unitNameKey = unitName.ToString();
            string unitNameEnumMemberValue = EnumExtensions.GetEnumMemberAttrValue(unitName);

            Log.WriteLine(unitNameKey + " | " + unitNameEnumMemberValue, LogLevel.DEBUG);

            buttonsToGenerate.Add(unitNameKey, unitNameEnumMemberValue);
        }

        base.GenerateButtonsWithCustomPropertiesAndIds(
            buttonsToGenerate, ButtonName.PLANESELECTIONBUTTON, _component, _leagueCategoryId);
    }

    public override Task<string> GenerateMessage()
    {
        Log.WriteLine("Starting to generate a message for the confirmation", LogLevel.DEBUG);

        mcc = new MatchChannelComponents(this);
        if (mcc.interfaceLeagueCached == null || mcc.leagueMatchCached == null)
        {
            Log.WriteLine(nameof(mcc) + " was null!", LogLevel.CRITICAL);
            return Task.FromResult(nameof(mcc) + " was null!");
        }

        string finalMessage = string.Empty;

        // Add to a method inside the match
        foreach (ScheduledEvent scheduledEvent in mcc.leagueMatchCached.MatchEventManager.ClassScheduledEvents)
        {
            if (scheduledEvent.GetType() == typeof(MatchQueueAcceptEvent))
            {
                if (scheduledEvent.LeagueCategoryIdCached == mcc.interfaceLeagueCached.LeagueCategoryId &&
                    scheduledEvent.MatchChannelIdCached == mcc.leagueMatchCached.MatchChannelId)
                {
                    // Skips the matches that have not been scheduled
                    if (mcc.leagueMatchCached.IsAScheduledMatch)
                    {
                        finalMessage += "Scheduled time: " +
                            TimeService.ConvertToDateTimeFromUnixTime(scheduledEvent.TimeToExecuteTheEventOn).ToString() + "\n";
                    }

                    finalMessage += "Time left: " +
                        TimeService.ReturnTimeLeftAsStringFromTheTimeTheActionWillTakePlace(scheduledEvent.TimeToExecuteTheEventOn) + "\n";
                }
            }
        }

        finalMessage += "Selected plane:\n";

        var matchReportData = mcc.leagueMatchCached.MatchReporting.TeamIdsWithReportData;

        int playersThatAreReady = 0;
        foreach (var teamKvp in matchReportData)
        {
            PLAYERPLANE? teamPlane = teamKvp.Value.FindBaseReportingObjectOfType(TypeOfTheReportingObject.PLAYERPLANE) as PLAYERPLANE;
            if (teamPlane == null)
            {
                Log.WriteLine(nameof(teamPlane) + " was null!", LogLevel.CRITICAL);
                return Task.FromResult(nameof(teamPlane) + " was null!");
            }

            foreach (var item in teamPlane.TeamMemberIdsWithSelectedPlanesByTheTeam)
            {
                string checkmark = EnumExtensions.GetEnumMemberAttrValue(EmojiName.REDSQUARE);

                if (item.Value != UnitName.NOTSELECTED)
                {
                    checkmark = EnumExtensions.GetEnumMemberAttrValue(EmojiName.WHITECHECKMARK);
                    playersThatAreReady++;
                }

                finalMessage += checkmark + " " + teamKvp.Value.TeamName + "\n";
            }
        }

        Log.WriteLine(playersThatAreReady + " | " +
            mcc.interfaceLeagueCached.LeaguePlayerCountPerTeam * 2, LogLevel.DEBUG);

        // Need to move this inside the class itself
        if (playersThatAreReady >= mcc.interfaceLeagueCached.LeaguePlayerCountPerTeam * 2 &&
            mcc.leagueMatchCached.MatchReporting.MatchState == MatchState.PLAYERREADYCONFIRMATIONPHASE)
        {
            // Perhaps make this an abstract method to remove each of the event type from the queue
            // with each of derived classes having their own conditions
            List<ScheduledEvent> scheduledEventsToRemove = new List<ScheduledEvent>();
            foreach (ScheduledEvent scheduledEvent in mcc.leagueMatchCached.MatchEventManager.ClassScheduledEvents)
            {
                if (scheduledEvent.GetType() == typeof(MatchQueueAcceptEvent))
                {
                    if (scheduledEvent.LeagueCategoryIdCached == mcc.interfaceLeagueCached.LeagueCategoryId &&
                        scheduledEvent.MatchChannelIdCached == mcc.leagueMatchCached.MatchChannelId)
                    {
                        scheduledEventsToRemove.Add(scheduledEvent);
                    }
                }
            }

            mcc.leagueMatchCached.MatchEventManager.RemoveEventsFromTheScheduledEventsBag(scheduledEventsToRemove);

            mcc.leagueMatchCached.MatchReporting.MatchState = MatchState.REPORTINGPHASE;

            InterfaceChannel interfaceChannel;
            try
            {
                interfaceChannel = Database.Instance.Categories.FindInterfaceCategoryWithId(
                    thisInterfaceMessage.MessageCategoryId).FindInterfaceChannelWithIdInTheCategory(
                        thisInterfaceMessage.MessageChannelId);
            }
            catch (Exception ex)
            {
                Log.WriteLine(ex.Message, LogLevel.CRITICAL);
                return Task.FromResult(ex.Message);
            }

            new Thread(() => mcc.leagueMatchCached.StartTheMatchOnSecondThread(interfaceChannel)).Start();
        }

        Log.WriteLine("Generated: " + finalMessage, LogLevel.DEBUG);

        return Task.FromResult(finalMessage);
    }
}