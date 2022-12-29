﻿using Discord.WebSocket;
using Discord;
using System.Runtime.Serialization;

[DataContract]
public abstract class BaseButton : InterfaceButton
{
    ButtonName InterfaceButton.ButtonName
    {
        get
        {
            Log.WriteLine("Getting " + nameof(buttonName) + ": " +
                buttonName, LogLevel.VERBOSE);
            return buttonName;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(buttonName) + buttonName
                + " to: " + value, LogLevel.VERBOSE);
            buttonName = value;
        }
    }

    string InterfaceButton.ButtonLabel
    {
        get
        {
            Log.WriteLine("Getting " + nameof(buttonLabel) + ": " +
                buttonLabel, LogLevel.VERBOSE);
            return buttonLabel;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(buttonLabel) + buttonLabel
                + " to: " + value, LogLevel.VERBOSE);
            buttonLabel = value;
        }
    }

    ButtonStyle InterfaceButton.ButtonStyle
    {
        get
        {
            Log.WriteLine("Getting " + nameof(buttonStyle) + ": " +
                buttonStyle, LogLevel.VERBOSE);
            return buttonStyle;
        }
        set
        {
            Log.WriteLine("Setting " + nameof(buttonStyle) + buttonStyle
                + " to: " + value, LogLevel.VERBOSE);
            buttonStyle = value;
        }
    }

    [DataMember] protected ButtonName buttonName;
    [DataMember] protected string buttonLabel;
    [DataMember] protected ButtonStyle buttonStyle;

    public BaseButton()
    {
    }

    public Discord.ButtonBuilder CreateTheButton(string _customId)
    {
        Log.WriteLine("Creating a button: " + buttonName + " | label: " +
            buttonLabel + " | custom-id:" + _customId + " with style: " + buttonStyle, LogLevel.VERBOSE);

        var button = new Discord.ButtonBuilder()
        {
            Label = buttonLabel,
            CustomId = buttonName.ToString() + "_" + _customId,
            Style = buttonStyle,
        };

        return button;
    }

    public void ActivateButtonFunction()
    {

    }
}