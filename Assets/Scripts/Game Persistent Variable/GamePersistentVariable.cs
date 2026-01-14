using System.Collections;
using UnityEngine;

public static class GamePersistentVariable
{
    public static Vector2 canvasSize;
    public static Vector2 canvasScale;
    public static string iapInitializeFailReason;

    public static float tileSize;
    public static float tileDistance;
    public static Vector2 screenSizeWorld;

    #region COMMON
    public static int currentLevel;
    public static int levelPrefab;
    #endregion

    #region Level
    public static LevelBoard levelBoard;
    public static bool isUseHammerToBreak1x1Tile;
    #endregion

    #region GAME FLOW
    public static bool hasAppJustLaunched;
    public static bool isPendingReplay;
    public static bool isLoadingReadyToComplete;
    public static bool isLoadingComplete;
    public static bool isMenuLoading;
    public static bool isLevelLost;
    public static bool isPlayingHideIconAnimation;
    public static Queue hidingIconQueue;
    #endregion

    #region LIVES
    public static LivesData livesData;
    public static bool isLevelDirty;
    #endregion

    #region SETTING
    public static bool IsVibrate;
    public static bool isEnableSound;
    #endregion

    #region WOOD JAM
    public static bool isPreviewSnapping;
    #endregion

    #region FIREBASE
    public static bool isFirebaseAvailable;
    public static bool isFirebaseRemoteConfigLoaded;
    public static int reviveAdditionalTime;
    public static bool remoteConfigIsShowMREC;
    #endregion

    #region ADS
    public static bool isShowBanner;
    public static int bannerCooldown = 30;
    public static DoubleInterAdsConfig doubleInterAdsConfig;
    #endregion

    #region TOWER
    public static int selectedMainCharacterIndex;
    public static bool isGameStarted;
    public static bool isPlacingTower;
    public static int totalEnemy;
    #endregion
}
