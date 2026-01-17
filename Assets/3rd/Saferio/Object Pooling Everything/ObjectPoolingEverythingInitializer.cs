using UnityEngine;
using static GameEnum;

public class ObjectPoolingEverythingInitializer : MonoBehaviour
{
    public static ObjectPoolingEverythingInitializer Instance;

    [SerializeField] private GameObject vehicleEngineSoundPrefab;
    [SerializeField] private GameObject hitObstacleSoundPrefab;
    [SerializeField] private GameObject getInVehicleSoundPrefab;
    [SerializeField] private GameObject vehicleMoveOutSoundPrefab;
    [SerializeField] private GameObject screwBox;
    [SerializeField] private GameObject screwPortPrefab;
    [SerializeField] private GameObject fakeScrewPrefab;
    [SerializeField] private GameObject block1x1Prefab;
    [SerializeField] private GameObject hammerPrefab;
    [SerializeField] private GameObject vacumnPrefab;
    [SerializeField] private GameObject loosenScrewSoundPrefab;
    [SerializeField] private GameObject tightenScrewSoundPrefab;
    [SerializeField] private GameObject loosenScrewFailSoundPrefab;
    [SerializeField] private GameObject screwBoxDoneSoundPrefab;
    [SerializeField] private GameObject breakObjectSoundPrefab;
    [SerializeField] private GameObject winSoundPrefab;
    [SerializeField] private GameObject loseSoundPrefab;
    [SerializeField] private GameObject clickSoundPrefab;
    [SerializeField] private GameObject closePopupSoundPrefab;
    [SerializeField] private GameObject unlockAdsScrewBoxSoundPrefab;
    [SerializeField] private GameObject clearScrewPortsSoundPrefab;
    [SerializeField] private GameObject destroyObjectPartFxPrefab;
    [SerializeField] private GameObject clockBoosterItemPrefab;
    [SerializeField] private GameObject blockMergeFx;

    [SerializeField] private GameObject taskItemUIPrefab;


    #region TOWER
    public GameObject normalProjectile;
    public GameObject explosiveProjectile;
    public GameObject towerBalista;
    public GameObject towerCannon;
    public GameObject towerMelee;
    public GameObject towerUIItem;
    public GameObject damageText;
    public GameObject hitFx;
    public GameObject buffFx;

    public GameObject slime;
    public GameObject ork;

    public GameObject hitSound;
    public GameObject balistaSound;
    public GameObject cannonSound;
    public GameObject bladeTowerSound;

    public GameObject equipmentUIItem;
    #endregion

    private bool _isInitialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);

            _isInitialized = true;
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (!_isInitialized)
        {
            InitPool();
        }
    }

    private void InitPool()
    {
        RegisterPool<ScrewBox>(GameConstants.SCREW_BOX, screwBox, 5);
        RegisterPool<ScrewBoxSlot>(GameConstants.SCREW_PORT_SLOT, screwPortPrefab, 3);
        RegisterPool<FakeScrew>(GameConstants.FAKE_SCREW, fakeScrewPrefab, 3);
        RegisterPool<BoosterHammer>(GameConstants.HAMMER, hammerPrefab, 3);
        RegisterPool<BoosterVacumn>(GameConstants.VACUMN, vacumnPrefab, 3);
        RegisterPool<AudioSource>(GameConstants.LOOSEN_SCREW_SOUND, loosenScrewSoundPrefab, 3);
        RegisterPool<AudioSource>(GameConstants.TIGHTEN_SCREW_SOUND, tightenScrewSoundPrefab, 3);
        RegisterPool<AudioSource>(GameConstants.LOOSEN_SCREW_FAIL_SOUND, loosenScrewFailSoundPrefab, 3);
        RegisterPool<AudioSource>(GameConstants.SCREW_BOX_DONE_SOUND, screwBoxDoneSoundPrefab, 3);
        RegisterPool<AudioSource>(GameConstants.BREAK_OBJECT_SOUND, breakObjectSoundPrefab, 3);
        RegisterPool<AudioSource>(GameConstants.WIN_SOUND, winSoundPrefab, 3);
        RegisterPool<AudioSource>(GameConstants.LOSE_SOUND, loseSoundPrefab, 3);
        RegisterPool<AudioSource>(GameConstants.CLICK_SOUND, clickSoundPrefab, 2);
        RegisterPool<AudioSource>(GameConstants.CLOSE_POPUP_SOUND, closePopupSoundPrefab, 2);
        RegisterPool<AudioSource>(GameConstants.UNLOCK_ADS_SCREW_BOX_SOUND, unlockAdsScrewBoxSoundPrefab, 3);
        RegisterPool<AudioSource>(GameConstants.CLEAR_SCREW_PORTS_SOUND, clearScrewPortsSoundPrefab, 3);
        RegisterPool<TaskItemUI>(GameConstants.TASK_ITEM_UI, taskItemUIPrefab, 5);
        RegisterPool<ParticleSystem>(GameConstants.DESTROY_OBJECT_PART_FX, destroyObjectPartFxPrefab, 5);
        RegisterPool<ParticleSystem>(GameConstants.blockMergeFx, blockMergeFx, 16);
        RegisterPool<SpriteRenderer>(GameConstants.BOOSTER_CLOCK_ITEM, clockBoosterItemPrefab, 2);
        RegisterPool<BaseBlock>(GameConstants.block1x1, block1x1Prefab, 30);

        RegisterPool<BaseProjectile>(ProjectileType.Normal.ToString(), normalProjectile, 100);
        RegisterPool<BaseProjectile>(ProjectileType.Explosive.ToString(), explosiveProjectile, 10);
        RegisterPool<BaseTower>(TowerType.Arrow.ToString(), towerBalista, 20);
        RegisterPool<BaseTower>(TowerType.Cannon.ToString(), towerCannon, 20);
        RegisterPool<BaseTower>(TowerType.Spear.ToString(), towerMelee, 20);
        RegisterPool<TowerUIItem>(GameConstants.towerUIItem, towerUIItem, 20);
        RegisterPool<BaseEnemy>(EnemyType.Slime.ToString(), slime, 60);
        RegisterPool<BaseEnemy>(EnemyType.Ork.ToString(), ork, 60);
        RegisterPool<DamageText>(GameConstants.damageText, damageText, 30);
        RegisterPool<ParticleSystem>(GameConstants.hitFx, hitFx, 30);
        RegisterPool<ParticleSystem>(GameConstants.buffFx, buffFx, 12);

        RegisterPool<AudioSource>(GameConstants.hitSound, hitSound, 12);
        RegisterPool<AudioSource>(GameConstants.balistaSound, balistaSound, 12);
        RegisterPool<AudioSource>(GameConstants.cannonSound, cannonSound, 12);
        RegisterPool<AudioSource>(GameConstants.bladeTowerSound, hitSound, 12);

        RegisterPool<EquipmentUIItem>(GameConstants.equipmentUIItem, equipmentUIItem, 24);
    }

    private void RegisterPool<T>(string key, GameObject prefab, int poolSize)
    {
        ObjectPool pool = new ObjectPool();

        pool.Init<T>(prefab, poolSize);

        ObjectPoolingEverything.AddToPool(key, pool);
    }
}
