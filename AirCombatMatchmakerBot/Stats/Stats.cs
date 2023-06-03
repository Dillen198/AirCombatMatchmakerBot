﻿using System.Collections.Concurrent;
using System.Runtime.Serialization;

[DataContract]
public class Stats : logClass<Stats>, InterfaceLoggableClass
{
    [IgnoreDataMember]
    public ConcurrentDictionary<UnitName, StatValues> PlaneSpecificStats
    {
        get => planeSpecificStats.GetValue();
        set => planeSpecificStats.SetValue(value);
    }

    public StatValues TotalStatValuesCached
    {
        get => totalStatValuesCached.GetValue();
        set => totalStatValuesCached.SetValue(value);
    }

    [DataMember]
    private logConcurrentDictionary<UnitName, StatValues> planeSpecificStats =
        new logConcurrentDictionary<UnitName, StatValues>();

    private logClass<StatValues> totalStatValuesCached = new logClass<StatValues>(new StatValues());

    public List<string> GetClassParameters()
    {
        return new List<string> { };
    }

    public Stats()
    {
        //CalculateAndReturnTotalStatValuesAsString();
    }

    public void CalculateStatsAfterAMatch(ReportData _thisTeamReportData, ReportData _opponentTeamReportData)
    {
        try
        {
            // Make this compatible for 2v2 etc

            PLAYERPLANE? teamPlane = _thisTeamReportData.FindBaseReportingObjectOfType(TypeOfTheReportingObject.PLAYERPLANE) as PLAYERPLANE;
            if (teamPlane == null)
            {
                Log.WriteLine(nameof(teamPlane) + " was null!", LogLevel.CRITICAL);
                throw new InvalidOperationException(nameof(teamPlane) + " was null!");
            }

            var firstUnitPlane = teamPlane.TeamMemberIdsWithSelectedPlanesByTheTeam.First().Value; // Loop through every plane here on 2v2 etc
            //Log.WriteLine("Found teamplanestring: " + teamPlaneString, LogLevel.VERBOSE);
            //UnitName unitName = (UnitName)EnumExtensions.GetInstance(teamPlaneString);
            Log.WriteLine("Found " + nameof(firstUnitPlane) + ": " + firstUnitPlane, LogLevel.VERBOSE);

            if (!PlaneSpecificStats.ContainsKey(firstUnitPlane))
            {
                PlaneSpecificStats.TryAdd(firstUnitPlane, new StatValues());
            }
            var planeSpecificStatValuesToModify = PlaneSpecificStats[firstUnitPlane];


            BaseReportingObject thisTeamScore = _thisTeamReportData.FindBaseReportingObjectOfType(TypeOfTheReportingObject.REPORTEDSCORE);
            BaseReportingObject opponentTeamScore = _opponentTeamReportData.FindBaseReportingObjectOfType(TypeOfTheReportingObject.REPORTEDSCORE);

            bool teamHasWon = _thisTeamReportData.FinalEloDelta > 0 ? true : false;
            if (teamHasWon)
            {
                planeSpecificStatValuesToModify.Wins++;

                if (planeSpecificStatValuesToModify.Streak < 0)
                {
                    planeSpecificStatValuesToModify.Streak = 0;
                }
                planeSpecificStatValuesToModify.Streak++;
            }
            else
            {
                planeSpecificStatValuesToModify.Losses++;

                if (planeSpecificStatValuesToModify.Streak > 0)
                {
                    planeSpecificStatValuesToModify.Streak = 0;
                }
                planeSpecificStatValuesToModify.Streak--;
            }

            planeSpecificStatValuesToModify.Kills += int.Parse(thisTeamScore.thisReportingObject.ObjectValue);
            planeSpecificStatValuesToModify.Deaths += int.Parse(opponentTeamScore.thisReportingObject.ObjectValue);

            // Add plane specific stats to see how the player performs against each of the enemy planes

            //CalculateTotalStatValues();
        }
        catch (Exception ex) 
        {
            Log.WriteLine(ex.Message, LogLevel.CRITICAL);
            return;
        }
    }

    public string CalculateAndReturnTotalStatValuesAsString()
    {
        // Reset so no leftovers from the last update
        TotalStatValuesCached.Wins = 0;
        TotalStatValuesCached.Losses = 0;
        TotalStatValuesCached.Kills = 0;
        TotalStatValuesCached.Deaths = 0;

        // Recalculate everything
        foreach (StatValues statValuesPerPlane in PlaneSpecificStats.Values)
        {
            // Read fields here and add to TotalStatValuesCached from each of the PlaneSpecificStats.Values
            TotalStatValuesCached.Wins += statValuesPerPlane.Wins;
            TotalStatValuesCached.Losses += statValuesPerPlane.Losses;
            TotalStatValuesCached.Kills += statValuesPerPlane.Kills;
            TotalStatValuesCached.Deaths += statValuesPerPlane.Deaths;
        }
        TotalStatValuesCached.CalculateFloats();

        string totalStatsAsString =
            "W=" + TotalStatValuesCached.Wins +
            " | L=" + TotalStatValuesCached.Losses +
            " | WR=" + TotalStatValuesCached.WinLoseRatio +
            " | K=" + TotalStatValuesCached.Kills +
            " | D=" + TotalStatValuesCached.Deaths +
            " | KDR=" + TotalStatValuesCached.KillDeathRatio;

        return totalStatsAsString;
    }
}