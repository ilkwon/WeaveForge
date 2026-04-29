using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

static public class Defined
{
    //-------------------------------------------------------------------------
    [Serializable]
    public class LocalGameOption
    {
        public bool isPlayMusic = true;
        public float soundVolume = 1.0f;
        public int graphicType = 1;
        public bool isUseAlarm = true;
        public bool isUseVibrate = true;
        public int padType = 2;
        public bool isViewNoticeToday = true;
        public int lastConnectDayOfYear = 0;

        public void Reset()
        {
            isPlayMusic = true;
            soundVolume = 1.0f;
            graphicType = 1;
            isUseAlarm = true;
            isUseVibrate = true;
            padType = 2;
            isViewNoticeToday = true;
            lastConnectDayOfYear = 0;
        }
    }
    //-------------------------------------------------------------------------
    public class VersionInfo
    {
        public int state;
        public int main;
        public int server;
        public int sub;

        override public string ToString()
        {
            return string.Format("{0}.{1}.{2}.{3}", state, main, server, sub);
        }
    }
    //-------------------------------------------------------------------------
    public enum LanguageType
    {
        ENGLISH,
        CHINESE,
        KOREAN,
        JAPANESE,
        EUROPE,
        TAIPEI
    }
    //-------------------------------------------------------------------------
    public enum LoactionType
    {
        NONE = 0,
        LATINO_AMERICA,
        AFRICA,
        CHINA,
        KOREA,
        TAIPEI
    }
    //-------------------------------------------------------------------------
    public class LocationInfo
    {
        string latinoAmerica = "LatinoAmerica";
        string africa = "Africa";
        string china = "China";
    }
    //-------------------------------------------------------------------------    
    public class CurrentPage
    {
        public enum Index
        {
            Unknown = -1,
            Menu_Main = 10,
            Menu_Mission,
            Menu_Storage,
            Mission_Attack,
            Mission_Defense,
            Storage_Factory,
            Storage_Ranking,
            
            Max
        }

    }
    public enum Nation
    {
        BELGIUM,
        SOVIET,
        CHINA,
        CZECH,
        NETHERLANDS,
        EGYPT,
        UK,
        GERMANY,
        ITALY,
        POLAND,
        USA,
        FRANCE,
        JAPAN
    }
}
