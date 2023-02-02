﻿using Discord;
using System.Data;
using System;
using System.Runtime.Serialization;
using Discord.WebSocket;
using System.Threading.Channels;
using System.Reflection;

[DataContract]
public class CONFIRMATIONMESSAGE : BaseMessage
{
    public CONFIRMATIONMESSAGE()
    {
        messageName = MessageName.CONFIRMATIONMESSAGE;
        messageButtonNamesWithAmount = new Dictionary<ButtonName, int>
        {
            { ButtonName.CONFIRMMATCHRESULTBUTTON, 1 },
            { ButtonName.MODIFYMATCHRESULTBUTTON, 1 },
            { ButtonName.DISPUTEMATCHRESULTBUTTON, 1 }
        };
        message = "You can either Confirm, Modify or Dispute the result below.";
    }

    public override string GenerateMessage()
    {
        string finalMessage = "Confirmed:\n;";

        InterfaceChannel interfaceChannel = Database.Instance.Categories.FindCreatedCategoryWithChannelKvpWithId(
            messageCategoryId).Value.FindInterfaceChannelWithIdInTheCategory(
            messageChannelId);

        //Find the channel of the message and cast the interface to to the MATCHCHANNEL class       
        MATCHCHANNEL? matchChannel = (MATCHCHANNEL)interfaceChannel;

        var matchTuple =
            matchChannel.FindInterfaceLeagueAndLeagueMatchOnThePressedButtonsChannel(
                messageCategoryId, messageChannelId);

        if (matchTuple.Item1 == null || matchTuple.Item2 == null)
        {
            Log.WriteLine(nameof(matchTuple) + " was null!", LogLevel.CRITICAL);
            return matchTuple.Item3;
        }

        var matchReportData = matchTuple.Item2.MatchReporting.TeamIdsWithReportData;

        foreach (var teamKvp in matchReportData)
        {
            string checkmark = EnumExtensions.GetEnumMemberAttrValue(EmojiName.REDSQUARE);

            if (teamKvp.Value.ConfirmedMatch)
            {
                checkmark = EnumExtensions.GetEnumMemberAttrValue(EmojiName.WHITECHECKMARK);
            }

            finalMessage += checkmark + " " + teamKvp.Value.TeamName + "\n";
        }

        finalMessage += "You can either Confirm, Modify or Dispute the result below.";

        Log.WriteLine("Generated: " + finalMessage, LogLevel.DEBUG);

        return finalMessage;
    }
}