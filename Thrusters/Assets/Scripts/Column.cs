using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Column : NetworkBehaviour
{
    [SerializeField] private int _maxHealth = 100;

    [SerializeField] private GameObject _graphics;

    [SerializeField] private GameObject _debriScatterHolder;

    public ParticleSystem DebriScatterParticleSystem;

    [SyncVar]
    private int _currentHealth;

    void Start()
    {
        _currentHealth = _maxHealth;

    }

    [Command]
    public void CmdTakeDamage(int amount)
    {
        RpcTakeDamage(amount);
    }

    [ClientRpc]
    public void RpcTakeDamage(int amount)
    {
        _currentHealth -= amount;
        StartCoroutine(RandomJitter());

        DebriScatterParticleSystem.Play();



        if (_currentHealth <= 0)
        {
            _debriScatterHolder.transform.SetParent(null);
            Destroy(_debriScatterHolder, 5.5f);
            if (isServer)
            {
                NetworkServer.Destroy(gameObject);
            }
        }
    }

    IEnumerator RandomJitter()
    {
        float rightOffset = 0;
        float forwardOffset = 0;
        float upOffset = 0;

        for (var i = 0; i < 6; i++)
        {
            var right = Random.Range(-0.05f, 0.1f);
            rightOffset += right;
            _graphics.transform.Translate(Vector3.right * right);

            var forward = Random.Range(-0.05f, 0.1f);
            forwardOffset += forward;
            _graphics.transform.Translate(Vector3.forward * forward);

            var up = Random.Range(-0.05f, 0.1f);
            upOffset += up;
            _graphics.transform.Translate(Vector3.up * up);
            yield return new WaitForSeconds(0.011f - 0.001f * i);
        }
        for (var i = 0; i < 5; i++)
        {
            _graphics.transform.Translate(Vector3.right * -rightOffset / 5);

            _graphics.transform.Translate(Vector3.forward * -forwardOffset / 5);

            _graphics.transform.Translate(Vector3.up * -upOffset / 5);
            yield return new WaitForSeconds(0.03f);
        }
    }
}
