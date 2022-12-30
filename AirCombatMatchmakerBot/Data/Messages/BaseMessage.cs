﻿//using Discord.WebSocket;
using Discord;
using System.Runtime.Serialization;

[DataContract]
public abstract class BaseMessage : InterfaceMessage
{
    MessageName InterfaceMessage.MessageName
    {
        get
        {
            Log.WriteLine("Getting " + nameof(messageName) + 
                ": " + messageName, LogLevel.VERBOSE);
            return messageName;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(messageName) + messageName
                + " to: " + value, LogLevel.VERBOSE);
            messageName = value;
        }
    }

    bool InterfaceMessage.ShowOnChannelGeneration
    {
        get
        {
            Log.WriteLine("Getting " + nameof(showOnChannelGeneration) 
                + ": " + showOnChannelGeneration, LogLevel.VERBOSE);
            return showOnChannelGeneration;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(showOnChannelGeneration) +
                showOnChannelGeneration + " to: " + value, LogLevel.VERBOSE);
            showOnChannelGeneration = value;
        }
    }

    List<ButtonName> InterfaceMessage.MessageButtonNames
    {
        get
        {
            Log.WriteLine("Getting " + nameof(messageButtonNames) + " with count of: " +
                messageButtonNames.Count, LogLevel.VERBOSE);
            return messageButtonNames;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(messageButtonNames)
                + " to: " + value, LogLevel.VERBOSE);
            messageButtonNames = value;
        }
    }

    string InterfaceMessage.Message
    {
        get
        {
            Log.WriteLine("Getting " + nameof(message)
                + ": " + message, LogLevel.VERBOSE);
            return message;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(message) +
                message + " to: " + value, LogLevel.VERBOSE);
            message = value;
        }
    }

    ulong InterfaceMessage.MessageId
    {
        get
        {
            Log.WriteLine("Getting " + nameof(messageId)
                + ": " + messageId, LogLevel.VERBOSE);
            return messageId;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(messageId) +
                messageId + " to: " + value, LogLevel.VERBOSE);
            messageId = value;
        }
    }

    /*
    List<InterfaceButton> InterfaceMessage.InterfaceButtons
    {
        get
        {
            Log.WriteLine("Getting " + nameof(interfaceButtons) + " with count of: " +
                interfaceButtons.Count, LogLevel.VERBOSE);
            return interfaceButtons;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(interfaceButtons)
                + " to: " + value, LogLevel.VERBOSE);
            interfaceButtons = value;
        }
    }*/

    [DataMember] protected MessageName messageName;
    [DataMember] protected bool showOnChannelGeneration;
    [DataMember] protected List<ButtonName> messageButtonNames;
    [DataMember] protected string message = "";
    [DataMember] protected ulong messageId;
    //[DataMember] protected List<InterfaceButton> interfaceButtons;

    public BaseMessage()
    {
        messageButtonNames = new List<ButtonName>();
        //interfaceButtons = new List<InterfaceButton>();
    }

    public async Task<ulong> CreateTheMessageAndItsButtonsOnTheBaseClass(
        Discord.WebSocket.SocketGuild _guild, ulong _channelId, string _customIdForButton)
    {
        //List<InterfaceButton> tempInterfaceButtons = new List<InterfaceButton>();

        var component = new ComponentBuilder();

        Log.WriteLine("Creating the channel message with id: "
            + _channelId, LogLevel.VERBOSE);

        var textChannel = _guild.GetChannel(_channelId) as ITextChannel;

        if (textChannel == null)
        {
            Log.WriteLine(nameof(textChannel) + " was null!", LogLevel.CRITICAL);
            return 0;
        }

        Log.WriteLine("Found text channel: " + textChannel.Name, LogLevel.VERBOSE);

        Log.WriteLine("messageButtonNames.Count: " + messageButtonNames.Count, LogLevel.VERBOSE);

        foreach (ButtonName buttonName in messageButtonNames)
        {
            InterfaceButton interfaceButton =
                (InterfaceButton)EnumExtensions.GetInstance(buttonName.ToString());
            Log.WriteLine("button: " + interfaceButton.ButtonLabel + " name: " + 
                interfaceButton.ButtonName, LogLevel.VERBOSE);

            component.WithButton(interfaceButton.CreateTheButton(_customIdForButton));

           // tempInterfaceButtons.Add(interfaceButton);
        }

        var newMessage = await textChannel.SendMessageAsync(
            message, components: component.Build());
        ulong newMessageId = newMessage.Id;

        /*
        foreach (InterfaceButton button in tempInterfaceButtons)
        {
            button.ChannelId = _channelId;
            button.MessageId = newMessageId;

            // Temp, maybe not the best way to look it up with name?
            // cant have 2 same named buttons in same message
            if (interfaceButtons.Any(x => x.ButtonName == button.ButtonName)) continue;

            interfaceButtons.Add(button);
        }*/

        Log.WriteLine("Created a new message with id: " + newMessageId,LogLevel.VERBOSE);

        return newMessageId;
    }
}