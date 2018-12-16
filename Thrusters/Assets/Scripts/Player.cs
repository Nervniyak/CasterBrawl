using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(PlayerSetup))]
public class Player : NetworkBehaviour
{
    [SyncVar]
    private bool _isDead;
    public bool isDead
    {
        get { return _isDead; }
        protected set { _isDead = value; }
    }

    [SerializeField] private int maxHealth = 100;

    [SyncVar]
    private int currentHealth;

    [SyncVar]
    public string username = "Loading...";

    [SyncVar]
    public string playerColor = "#FFF";

    public int wins;
    public int kills;
    public int deaths;

    [SerializeField] private Behaviour[] disableOnDeath;
    private bool[] wasEnabled;

    [SerializeField] private GameObject[] disableGameObjectsOnDeath;
    [SerializeField] private GameObject deathEffect;
    [SerializeField] private GameObject spawnEffect;

    private bool isFirstSetup = true;

    public void Start()
    {
        if (isLocalPlayer)
        {
            CmdSyncWithCurrentState();
        }
    }


    [Command]
    private void CmdSyncWithCurrentState()
    {
        GameObject.Find("BuildManager").GetComponent<LevelBuilder>().RpcSyncColor(); //TODO OOOOOOOOOOOOOOOOOOOOOOOO
        GameObject.Find("BuildManager").GetComponent<LevelBuilder>().RpcSyncPlatformsOnClient(); //TODO OOOOOOOOOOOOOOOOOOOOOOOO
        GameObject.Find("BuildManager").GetComponent<LevelBuilder>().RpcSyncColumns(); //TODO OOOOOOOOOOOOOOOOOOOOOOOO
        GameObject.Find("BuildManager").GetComponent<LevelBuilder>().RpcResync(); //TODO OOOOOOOOOOOOOOOOOOOOOOOO

    }

    public void SetupPlayer()
    {
        if (isLocalPlayer)
        {
            GameManager.Instance.SetSceneCameraActive(false);
            GetComponent<PlayerSetup>().PlayerUiInstance.GetComponent<PlayerUI>().SetPlayerDeathStatus(false);
        }

        CmdBroadcastNewPlayerSetup();
    }

    [Command]
    private void CmdBroadcastNewPlayerSetup()
    {
        //if (isLocalPlayer) //TODO: DOUBLE CHECK THIS SHIT
        //{
        RpcSetupPlayerOnAllClients();
        //}
    }

    [ClientRpc]
    private void RpcSetupPlayerOnAllClients()
    {
        if (isFirstSetup)
        {
            wasEnabled = new bool[disableOnDeath.Length];
            for (int i = 0; i < wasEnabled.Length; i++)
            {
                wasEnabled[i] = disableOnDeath[i].enabled;
            }
            isFirstSetup = false;
        }

        SetDefaults();
    }

    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            RpcTakeDamage(10000, null, "Suicide");
        }
    }

    [ClientRpc]
    public void RpcTakeDamage(int _amount, string _sourceID, string alternativeSource)
    {
        if (isDead) return;
        currentHealth -= _amount;

        Debug.Log(transform.name + " now has " + currentHealth + "hp.");

        if (currentHealth <= 0)
        {

            Die(_sourceID, alternativeSource);
        }
    }


    [ClientRpc]
    public void RpcGetPushed(Vector3 direction, float force)
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(direction * force, ForceMode.Impulse);
            rb.AddForce(direction * force);
        }
    }

    [ClientRpc]
    public void RpcGetSpeeded(Vector3 direction, float velocity)
    {
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = transform.forward * velocity;
        }
    }


    private void Die(string _sourceID, string alternativeSource)
    {
        if (isDead)
        {
            return;
        }
        if (isServer)
        {
            MatchManager.Instance.PlayerDied(this);
        }
        isDead = true;

        Player sourcePlayer = GameManager.GetPlayer(_sourceID);
        if (sourcePlayer != null)
        {
            sourcePlayer.kills++;
            GameManager.Instance.onPlayerKilledCallback.Invoke(username, playerColor, sourcePlayer.username, sourcePlayer.playerColor);
        }
        else
        {
            GameManager.Instance.onPlayerKilledCallback.Invoke(username, playerColor, alternativeSource, "#fff");
        }


        deaths++;

        Cursor.lockState = CursorLockMode.None;
        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = false;
        }

        for (int i = 0; i < disableGameObjectsOnDeath.Length; i++)
        {
            disableGameObjectsOnDeath[i].SetActive(false);
        }

        Collider _col = GetComponent<Collider>();
        if (_col != null)
        {
            _col.enabled = false;
        }

        GameObject _gfxIns = Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(_gfxIns, 3f);

        if (isLocalPlayer)
        {
            //Debug.Log(transform.name + " Camera is ON!");
            GameManager.Instance.SetSceneCameraActive(true);
            GetComponent<PlayerSetup>().PlayerUiInstance.GetComponent<PlayerUI>().SetPlayerDeathStatus(true);

        }

        Debug.Log(transform.name + " is DEAD!");

        StartCoroutine(Respawn());

    }

    [ClientRpc]
    public void RpcRespawn()
    {
        StartCoroutine(Respawn());
    }

    public IEnumerator Respawn()
    {
        var respawnTime = MatchManager.Instance.RespawnTime;
        Debug.Log(gameObject.name + " RESPAWN " + respawnTime);
        if (respawnTime != -1 && isDead)
        {
            Debug.Log(gameObject.name + " WAIT " + respawnTime);
            yield return new WaitForSeconds(respawnTime);

            if (isDead)
            {


                Transform _spawnPoint = NetworkManager.singleton.GetStartPosition();
                transform.position = _spawnPoint.position;
                transform.rotation = _spawnPoint.rotation;

                yield return new WaitForSeconds(0.2f);

                //GameManager.instance.SetSceneCameraActive(true);
                //GetComponent<PlayerSetup>().playerUIInstance.SetActive(false);
                //SetDefaults();
                Debug.Log(gameObject.name + " SETUP " + respawnTime);
                SetupPlayer();

                Debug.Log(transform.name + " respawned.");
            }
        }
    }

    public void SetDefaults()
    {
        isDead = false;
        currentHealth = maxHealth;

        for (int i = 0; i < disableOnDeath.Length; i++)
        {
            disableOnDeath[i].enabled = wasEnabled[i];
        }

        for (int i = 0; i < disableGameObjectsOnDeath.Length; i++)
        {
            disableGameObjectsOnDeath[i].SetActive(true);
        }

        Collider _col = GetComponent<Collider>();
        if (_col != null)
        {
            _col.enabled = true;
        }



        GameObject _gfxIns = Instantiate(spawnEffect, transform.position, Quaternion.identity);
        Destroy(_gfxIns, 2f);
    }

    public static Transform FindDeepChild(Transform aParent, string aName)
    {
        var result = aParent.Find(aName);
        if (result != null)
            return result;
        foreach (Transform child in aParent)
        {
            result = FindDeepChild(child, aName);
            if (result != null)
                return result;
        }
        return null;
    }

    public float GetHealthPercentage()
    {
        return (float)currentHealth / maxHealth;
    }
}
