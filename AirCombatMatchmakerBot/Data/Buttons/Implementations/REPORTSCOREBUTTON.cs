﻿using Discord;
using System.Data;
using System;
using System.Runtime.Serialization;
using Discord.Commands;
using Discord.WebSocket;
using System.Runtime.CompilerServices;

[DataContract]
public class REPORTSCOREBUTTON : BaseButton
{
    public REPORTSCOREBUTTON()
    {
        buttonName = ButtonName.REPORTSCOREBUTTON;
        buttonLabel = "0";
        buttonStyle = ButtonStyle.Primary;
    }

    public void CreateTheButton(){}

    public override Task<string> ActivateButtonFunction(
        SocketMessageComponent _component, InterfaceMessage _interfaceMessage)
    {
        InterfaceMessage reportingStatusMessage =
            Database.Instance.Categories.FindCreatedCategoryWithChannelKvpWithId(
                _interfaceMessage.MessageCategoryId).Value.FindInterfaceChannelWithIdInTheCategory(
                    _interfaceMessage.MessageChannelId).FindInterfaceMessageWithNameInTheChannel(
                        MessageName.REPORTINGSTATUSMESSAGE);

        string[] splitStrings = buttonCustomId.Split('_');

        /*
        string finalResponse = "Something went wrong with the reporting the match result of: " +
            splitStrings[1] + ". An admin has been informed.";
        */

        ulong playerId = _component.User.Id;
        int playerReportedResult = int.Parse(splitStrings[1]);

        Log.WriteLine("Pressed by: " + playerId + " in: " + reportingStatusMessage.MessageChannelId + 
            " with label int: " + playerReportedResult + " in category: " +
            buttonCategoryId, LogLevel.DEBUG);

        /*
        foreach (var item in splitStrings)
        {
            Log.WriteLine(item, LogLevel.DEBUG);
        }*/

        InterfaceChannel interfaceChannel = Database.Instance.Categories.FindCreatedCategoryWithChannelKvpWithId(
            _interfaceMessage.MessageCategoryId).Value.FindInterfaceChannelWithIdInTheCategory(
            _interfaceMessage.MessageChannelId);

        //Find the channel of the message and cast the interface to to the MATCHCHANNEL class       
        MATCHCHANNEL? matchChannel = (MATCHCHANNEL)interfaceChannel;
        if (matchChannel == null)
        {
            Log.WriteLine(nameof(matchChannel) + " was null!", LogLevel.CRITICAL);
            return Task.FromResult("Match channel was null!");
        }

        var leagueMatchTuple = 
            matchChannel.FindInterfaceLeagueAndLeagueMatchOnThePressedButtonsChannel(
                buttonCategoryId, reportingStatusMessage.MessageChannelId);

        if (leagueMatchTuple.Item1 == null || leagueMatchTuple.Item2 == null)
        {
            Log.WriteLine(nameof(leagueMatchTuple) + " was null!", LogLevel.CRITICAL);
            return Task.FromResult(leagueMatchTuple.Item3);
        }

        string finalResponse = leagueMatchTuple.Item2.MatchReporting.ProcessPlayersSentReportObject(
            leagueMatchTuple.Item1, playerId, reportingStatusMessage, playerReportedResult.ToString(),
            TypeOfTheReportingObject.REPORTEDSCORE).Result;

        Log.WriteLine("Reached end before the return with player id: " +
            playerId + " with response:" + finalResponse, LogLevel.DEBUG);

        return Task.FromResult(finalResponse);
    }
}