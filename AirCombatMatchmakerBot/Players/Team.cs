﻿[Serializable]
public class Team
{
    public int skillRating { get; set; }
    public string teamName { get; set; }
    public List<Player> players { get; set; }

    public bool active { get; set; }

    public Team()
    {
        skillRating = 1600;
        teamName = string.Empty;
        players = new List<Player>();
        active = false;
    }

    public Team(List<Player> _players, string _teamName)
    {
        skillRating = 1600;
        teamName = _teamName;
        players = _players;
    }
}