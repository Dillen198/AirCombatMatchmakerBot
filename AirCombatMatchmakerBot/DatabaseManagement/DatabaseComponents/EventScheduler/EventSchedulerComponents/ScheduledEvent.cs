﻿using Discord;
using System.Collections.Concurrent;
using System.Diagnostics.Tracing;
using System.Runtime.Serialization;

[DataContract]
public abstract class ScheduledEvent : logClass<ScheduledEvent>, InterfaceEventType
{
    public ulong TimeToExecuteTheEventOn
    {
        get => timeToExecuteTheEventOn.GetValue();
        set => timeToExecuteTheEventOn.SetValue(value);
    }

    public int EventId
    {
        get => eventId.GetValue();
        set => eventId.SetValue(value);
    }

    public bool EventIsBeingExecuted
    {
        get => eventIsBeingExecuted.GetValue();
        set => eventIsBeingExecuted.SetValue(value);
    }

    public ulong LeagueCategoryIdCached
    {
        get => leagueCategoryIdCached.GetValue();
        set => leagueCategoryIdCached.SetValue(value);
    }

    public ulong MatchChannelIdCached
    {
        get => matchChannelIdCached.GetValue();
        set => matchChannelIdCached.SetValue(value);
    }

    [DataMember] protected logClass<ulong> timeToExecuteTheEventOn = new logClass<ulong>();
    [DataMember] protected logClass<int> eventId = new logClass<int>();
    [DataMember] protected logClass<bool> eventIsBeingExecuted = new logClass<bool>();
    [DataMember] protected logClass<ulong> leagueCategoryIdCached = new logClass<ulong>();
    [DataMember] protected logClass<ulong> matchChannelIdCached = new logClass<ulong>();

    public ScheduledEvent() { }

    public bool CheckIfTheEventCanBeExecuted(
        ulong _currentUnixTime, bool _clearEventOnTheStartup = false)
    {
        Log.WriteLine("Loop on event: " + EventId + " type: " + GetType() + " with times: " +
            _currentUnixTime + " >= " + TimeToExecuteTheEventOn);

        if (_currentUnixTime >= TimeToExecuteTheEventOn)
        {
            Log.WriteLine("Attempting to execute event: " + EventId);

            if (EventIsBeingExecuted && !_clearEventOnTheStartup)
            {
                Log.WriteLine("Event: " + EventId + " was being executed already, continuing.");
                return false;
            }

            EventIsBeingExecuted = true;

            Log.WriteLine("Executing event: " + EventId, LogLevel.DEBUG);

            //InterfaceEventType interfaceEventType = (InterfaceEventType)scheduledEvent;
            //Log.WriteLine("event: " + EventId + " cast");
            ExecuteTheScheduledEvent();
            Log.WriteLine("event: " + EventId + " after execute await");

            return true;
        }
        else if (_currentUnixTime % 5 == 0 && _currentUnixTime <= TimeToExecuteTheEventOn)
        {
            Log.WriteLine("event: " + EventId + " going to check the event status");
            CheckTheScheduledEventStatus();
        }
        else
        {
            Log.WriteLine("event: " + EventId + " ended up in else");
        }

        Log.WriteLine("Done with if statement on event: " + EventId + " type: " + GetType() + " with times: " +
            _currentUnixTime + " >= " + TimeToExecuteTheEventOn);

        return false;
    }

    protected void SetupScheduledEvent(
        ulong _timeFromNowToExecuteOn, ConcurrentBag<ScheduledEvent> _scheduledEvents)
    {
        Log.WriteLine("Setting " + typeof(ScheduledEvent) + "' TimeToExecuteTheEventOn: " +
            _timeFromNowToExecuteOn + " seconds from now");

        ulong currentUnixTime = (ulong)DateTimeOffset.Now.ToUnixTimeSeconds();
        TimeToExecuteTheEventOn = currentUnixTime + (ulong)_timeFromNowToExecuteOn;
        EventId = ++Database.Instance.EventScheduler.EventCounter;

        // Replace this with league of match specific ScheduledEvents list
        _scheduledEvents.Add(this);

        Log.WriteLine(typeof(ScheduledEvent) + "' TimeToExecuteTheEventOn is now: " +
            TimeToExecuteTheEventOn + " with id event: " + EventId);
    }

    public abstract Task ExecuteTheScheduledEvent(bool _serialize = true);
    public abstract void CheckTheScheduledEventStatus();
}