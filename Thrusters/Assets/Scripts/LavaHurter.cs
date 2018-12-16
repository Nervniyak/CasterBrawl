using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Player))]
public class LavaHurter : NetworkBehaviour
{
    [SerializeField] private int _damagePerTick = 21;

    private Player _player;

    void Start()
    {
        _player = GetComponent<Player>();
        if (!isLocalPlayer)
        {
            return;
        }
        CmdStartChecker();
    }

    //void Update()
    //{
    //    if (!isLocalPlayer)
    //    {
    //        return;
    //    }
    //}

    [Command]
    void CmdStartChecker()
    {
        StartCoroutine(LavaChecker());
    }

    private IEnumerator LavaChecker()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            var collidersInBounds = Physics.OverlapSphere(transform.position, 0.6f);
            var isSafe = collidersInBounds.Any(col => col.isTrigger && (col.tag == "Safe" || col.tag == "Column"));
            if (!isSafe)
            {
                _player.RpcTakeDamage(_damagePerTick, null, "Lava");
            }
        }
    }
}
