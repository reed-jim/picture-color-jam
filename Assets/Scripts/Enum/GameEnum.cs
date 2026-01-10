using System;
using UnityEngine;

public class GameEnum : MonoBehaviour
{
    public enum TowerType
    {
        Arrow,
        Explosive,
        Spear
    }

    public enum ScreenRoute
    {
        Waiting,
        Lobby,
        LobbyRoom,
        Screen1,
        Screen2,
        Screen3,
        Screen4,
        Screen5,
        Setting,
        Win,
        Debug,
        LuckyWheel,
        WeeklyTask,
        IAPShop,
        Pause,
        RemoveAd,
        Lose,
        Booster,
        Revive,
        Rate,
        Home,
        IAPShopPopup,
        NoInternet,
        Notification,
        ResourcesEarn,
        LivesShop,
        Replay,
        ReturnHome,
        None,
        ReviewIAP,
        LevelComingSoon,
        Language
    }

    public enum GameFaction
    {
        Green,
        Purple,
        Red,
        Yellow,
        Blue,
        Orange,
        Pink,
        Disabled,
        All,
        None
    }

    public enum CharacterAnimationState
    {
        Idle = 0,
        Walking = 1
    }

    public enum LevelDifficulty
    {
        Normal,
        Hard,
        VeryHard
    }

    public enum InputMode
    {
        Select,
        BreakObject,
        Vacumn,
        Disabled
    }

    public enum TaskType
    {
        Uncrew,
        CompleteLevel,
    }

    public enum BoosterType
    {
        FreezeTime,
        BreakObject,
        Vacumn,
        Teleport
    }


    public enum ButtonMode
    {
        Normal,
        Grayscale
    }

    public enum EndLevelReason
    {
        Lose,
        ReturnHome,
        Replay,
        Quit,
        Win
    }

    public static T GetRandomEnumValue<T>()
    {
        Array enumValues = Enum.GetValues(typeof(T));

        System.Random random = new System.Random();

        return (T)enumValues.GetValue(random.Next(enumValues.Length));
    }
}
