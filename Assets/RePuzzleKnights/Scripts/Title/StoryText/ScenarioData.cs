using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class Scenario
{
    public string id;
    public string name; //キャラ名
    public string sentence; //セリフ
    public string charaImage; //キャラ画像
}

[Serializable]
public class ScenarioData
{
    public List<Scenario> scenes;
}