﻿using Discord;
using Discord.WebSocket;
using System.ComponentModel;
using System.Linq;

public static class ButtonComponents
{
    // Creates a button to a specific channel
    public static async Task<ulong> CreateButtonMessage(
        ITextChannel _channel,
        string _textOnTheSameMessage,
        string _label,
        string _customId)
    {
        Log.WriteLine("Creating a button on channel: " +
            "with text before the button: " + _textOnTheSameMessage + " | label: " + _label + " | custom-id:" +
            _customId, LogLevel.DEBUG);

        var builder = new ComponentBuilder()
            .WithButton(_label, _customId);


        var guild = BotReference.GetGuildRef();
        if (guild == null)
        {
            Exceptions.BotGuildRefNull();
            return 0;
        }

        var textChannel = guild.GetTextChannel(_channel.Id);

        if (textChannel == null)
        {
            Log.WriteLine(nameof(textChannel) + " was null!", LogLevel.CRITICAL);
            return 0;
        }

        var message = await textChannel.SendMessageAsync(
            _textOnTheSameMessage, components: builder.Build());

        ulong messageId = message.Id;
        Log.WriteLine("Created a button message with id:" + messageId, LogLevel.VERBOSE);
        return messageId;
    }
}