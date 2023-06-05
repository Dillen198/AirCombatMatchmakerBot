﻿using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using System.Runtime.Serialization;

// REMEMBER TO SET THE VALUES CORRECTLY HERE !!!
[JsonConverter(typeof(StringEnumConverter))]
public enum CategoryType
{
    [EnumMember(Value = "1v1 Modern BFM Guns")]
    ONEMODERNBFMGUNS = 0,

    [EnumMember(Value = "1v1 Modern BFM Fox2")]
    ONEMODERNBFMFOXTWO = 1,

    [EnumMember(Value = "bot-stuff")]
    BOTSTUFF = 0,

    [EnumMember(Value = "registration")]
    REGISTRATIONCATEGORY = 1,

    [EnumMember(Value = "league-template")]
    LEAGUETEMPLATE = 2,
}