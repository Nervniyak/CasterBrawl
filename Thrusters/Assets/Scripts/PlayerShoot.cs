using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(SkillManager))]
public class PlayerShoot : NetworkBehaviour
{
    //private const string PLAYER_TAG = "Player";

    //[SerializeField]
    //private LayerMask _mask;
    private PlayerMotor _playerMotor;

    private Skill _currentLeftSkill;
    private Skill _currentRightSkill;
    private Skill _currentShiftSkill;
    private SkillManager _skillManager;

    private bool _isCasting;
    private Skill _castedSkill;

    private Coroutine _lastColorCoroutine;
    //[SerializeField] private Material _shardsMaterial;
    //[SerializeField] private List<GameObject> dfg; //TODO: ADASDFASF

    [SerializeField] private Camera _cam;

    void Start()
    {
        if (_cam == null)
        {
            Debug.LogError("PlayerShoot: No camera referenced");
            enabled = false;

        }

        //_shardsMaterial.SetColor("_EmissionColor", Color.red);

        //weaponManager = GetComponent<WeaponManager>();
        _playerMotor = GetComponent<PlayerMotor>();
        _skillManager = GetComponent<SkillManager>();
        //weaponGFX.layer = LayerMask.NameToLayer(weaponLayerName);
    }

    void Update()
    {
        _currentLeftSkill = _skillManager.GetCurrentLeftSkill();
        _currentRightSkill = _skillManager.GetCurrentRightSkill();
        _currentShiftSkill = _skillManager.GetCurrentShiftSkill();

        if (PauseMenu.isOn)
        {
            return;
        }

        if (!_isCasting)
        {
            if (Input.GetButtonDown("Fire1"))
            {
                Shoot("Fire1");
            }

            else if (Input.GetButtonDown("Fire2"))
            {
                Shoot("Fire2");
            }
            else if (Input.GetButtonDown("Motion1"))
            {
                Shoot("Motion1");
            }
        }
    }

    [Client]
    void Shoot(string button)
    {
        _isCasting = true;
        Skill currentSkill = null;
        bool isOnCoolDown = true;
        switch (button)
        {
            case "Fire1":
                {
                    currentSkill = _currentLeftSkill;
                    isOnCoolDown = _skillManager.IsOnCooldown(true);
                    break;
                }
            case "Fire2":
                {
                    currentSkill = _currentRightSkill;
                    isOnCoolDown = _skillManager.IsOnCooldown(false);
                    break;
                }
            case "Motion1":
                {
                    currentSkill = _currentShiftSkill;
                    isOnCoolDown = _skillManager.IsShiftOnCooldown();
                    break;
                }
        }

        if (!isLocalPlayer || currentSkill == null || isOnCoolDown)
        {
            _isCasting = false;
            return;
        }

        if (_lastColorCoroutine != null)
        {
            StopCoroutine(_lastColorCoroutine);
        }
        _lastColorCoroutine = StartCoroutine(GlowShards(Mathf.Clamp(currentSkill.CastTime - 0.275f, 0f, currentSkill.CastTime), currentSkill.Color));
        Debug.Log(currentSkill.Name);
        _skillManager.Cast();

        var delay = currentSkill.FireRate == 0 ? 0 : 1 / currentSkill.FireRate;
        StartCoroutine(ShootFewTimes(
            currentSkill.Charges, currentSkill.Slowdown, currentSkill.CastTime, delay,
            button, _skillManager.FirePoint, currentSkill.Name, currentSkill.Velocity,
            currentSkill.Damage, currentSkill.Force, currentSkill.DestroyAfterSeconds));

        switch (button)
        {
            case "Fire1":
                {
                    _skillManager.ReloadLeft();
                    break;
                }
            case "Fire2":
                {
                    _skillManager.ReloadRight();
                    break;
                }
            case "Motion1":
                {
                    _skillManager.ReloadShift();
                    break;
                }
        }

    }

    IEnumerator ShootFewTimes(int times, float slow, float castTime, float delay, string button, Transform firepoint, string skillName, float velocity, int damage, float force, float destroyAfterSeconds)
    {
        _playerMotor.SetSlow(slow, castTime);
        yield return new WaitForSeconds(castTime);
        for (var i = 0; i < times; i++)
        {
            var firePosition = Vector3.zero;
            var fireRotation = Quaternion.identity;
            try
            {
                switch (skillName)
                {
                    case "Corb":
                        firePosition = firepoint.position;
                        fireRotation = _cam.transform.rotation;
                        CmdShoot(button, firePosition, fireRotation, skillName, velocity, damage, force, destroyAfterSeconds);
                        break;
                    case "Shockwave":
                        firePosition = transform.position;
                        fireRotation = Quaternion.identity;
                        CmdShoot(button, firePosition, fireRotation, skillName, velocity, damage, force, destroyAfterSeconds);
                        break;
                    case "Dash":
                        firePosition = transform.position + transform.forward / 3;
                        fireRotation = Quaternion.Euler(0, _cam.transform.eulerAngles.y, 0);
                        CmdShoot(button, firePosition, fireRotation, skillName, 0, damage, 0, destroyAfterSeconds);
                        CmdDash(velocity);
                        break;
                }

                CmdOnShoot();
            }
            catch (Exception)
            {
                _isCasting = false;
                throw;
            }
            if (i != times - 1)
            {
                yield return new WaitForSeconds(delay);
            }
        }
        _isCasting = false;
    }

    [Command]
    void CmdShoot(string button, Vector3 firepointPosition, Quaternion firepointRotation, string skillName, float velocity, int damage, float force, float destroyAfterSeconds)
    {
        GameObject prefab = null;
        switch (button)
        {
            case "Fire1":
                {
                    prefab = _currentLeftSkill.Prefab;
                    break;
                }
            case "Fire2":
                {
                    prefab = _currentRightSkill.Prefab;
                    break;
                }
            case "Motion1":
                {
                    prefab = _currentShiftSkill.Prefab;
                    break;
                }
        }
        if (prefab != null)
        {
            var projectile = Instantiate(prefab, firepointPosition, firepointRotation);

            projectile.name = skillName + transform.name;
            projectile.GetComponent<Projectile>().owner = transform.name;

            NetworkServer.Spawn(projectile);
            StartCoroutine(DestroyAfterDelay(projectile, destroyAfterSeconds));
            RpcShoot(projectile.GetComponent<NetworkIdentity>().netId, skillName, velocity, damage, force);
        }
    }

    [Command]
    void CmdDash(float velocity)
    {
        gameObject.GetComponent<Player>().RpcGetSpeeded(transform.forward, velocity /** 0.8f*/);
    }

    [ClientRpc]
    void RpcShoot(NetworkInstanceId id, string skillName, float velocity, int damage, float force)
    {
        var projectile = ClientScene.FindLocalObject(id);
        projectile.name = skillName + transform.name;
        var projectileComponent = projectile.GetComponent<Projectile>();
        projectileComponent.owner = transform.name;

        switch (skillName)
        {
            case "Corb":
                projectileComponent.damage = damage;
                projectileComponent.force = force;
                projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * velocity;
                break;
            case "Shockwave":
                projectileComponent.damage = damage;
                projectileComponent.force = force;
                projectile.GetComponent<Rigidbody>().velocity = projectile.transform.forward * velocity;
                break;
            case "Dash":
                projectile.transform.SetParent(gameObject.transform, true);
                break;
        }



    }

    IEnumerator DestroyAfterDelay(GameObject go, float delay)
    {
        yield return new WaitForSeconds(delay);
        NetworkServer.Destroy(go);
    }

    IEnumerator GlowShards(float duration, Color endColor)
    {
        var renderers = _skillManager.GetCurrentGraphics().Shards;
        var initialColors = renderers.Select(r => r.material.GetColor("_EmissionColor")).ToList();
        var elapsedTime = 0.0f;
        while (elapsedTime <= duration)
        {
            elapsedTime += Time.deltaTime;
            for (var i = 0; i < renderers.Count; i++)
            {
                var color = Color.Lerp(initialColors[i], endColor * Mathf.LinearToGammaSpace(1.1f), (elapsedTime / duration));
                renderers[i].material.SetColor("_EmissionColor", color);
            }
            yield return null;
        }
        foreach (var render in renderers)
        {
            render.material.SetColor("_EmissionColor", render.material.GetColor("_EmissionColor") / Mathf.LinearToGammaSpace(1.1f));
        }
        yield return new WaitForSeconds(0.2f);
        foreach (var render in renderers)
        {
            render.material.SetColor("_EmissionColor", render.material.GetColor("_EmissionColor") * Mathf.LinearToGammaSpace(3.5f));
        }

        initialColors = renderers.Select(r => r.material.GetColor("_EmissionColor")).ToList();
        yield return new WaitForSeconds(0.5f);
        duration = 0.3f;
        elapsedTime = 0.0f;
        while (elapsedTime <= duration)
        {
            elapsedTime += Time.deltaTime;
            for (var i = 0; i < renderers.Count; i++)
            {
                var color = Color.Lerp(initialColors[i], (initialColors[i] / Mathf.LinearToGammaSpace(3.5f)) / 1.5f, (elapsedTime / duration));
                renderers[i].material.SetColor("_EmissionColor", color);
            }
            yield return null;
        }
    }

    [Command]
    void CmdOnShoot()
    {
        RpcDoShootEffect();
    }

    [ClientRpc]
    void RpcDoShootEffect()
    {
        _skillManager.GetCurrentGraphics().muzzleFlash.Play();
    }

    [Command]
    public void CmdPlayerShot(string playerId, int damage, string sourceId)
    {
        Debug.Log(playerId + " has been shot.");

        var player = GameManager.GetPlayer(playerId);
        player.RpcTakeDamage(damage, sourceId, "Player");
    }

    //[Command]
    //void CmdOnHit(Vector3 _pos, Vector3 _normal)
    //{
    //    RpcDoHitEffect(_pos, _normal);
    //}

    //[ClientRpc]
    //void RpcDoHitEffect(Vector3 _pos, Vector3 _normal)
    //{
    //    GameObject hitEffect = Instantiate(weaponManager.GetCurrentGraphics().hitEffectPrefab, _pos, Quaternion.LookRotation(_normal));
    //    Destroy(hitEffect, 2);
    //}

}
