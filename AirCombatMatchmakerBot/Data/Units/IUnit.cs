﻿
using Newtonsoft.Json;

[JsonObjectAttribute]
public interface IUnit
{
    public UnitName UnitName { get; set; } // Name of the plane
    //public List<Era> UnitEras { get; set; }
}