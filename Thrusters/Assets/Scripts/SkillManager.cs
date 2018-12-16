using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SkillsCollection))]
public class SkillManager : NetworkBehaviour
{

    [SerializeField] private string weaponLayerName = "Weapon";

    [SerializeField] private Transform weaponHolder;

    private readonly Dictionary<KeyCode, Skill> skillLayout = new Dictionary<KeyCode, Skill>();


    private Skill _currentLeftSkill;
    private Skill _currentRightSkill;

    private Skill _currentShiftSkill;

    [SerializeField] private GameObject currentGraphicsPrefab;
    private GameObject currentWeaponGameObject;
    private WeaponGraphics currentGraphics;

    [HideInInspector]
    public Transform FirePoint;

    private SkillsCollection _skillsCollection;


    void Start()
    {
        EquipWeapon();
        _skillsCollection = GetComponent<SkillsCollection>();
        foreach (var skill in _skillsCollection.Skills)
        {
            skill.CurrentCooldownTime = skill.CooldownTime;
        }
        if (_skillsCollection.Skills.Length >= 1)
        {
            skillLayout.Add(KeyCode.Q, _skillsCollection.Skills[0]);
            EquipMainSkill(skillLayout[KeyCode.Q], true);
        }
        if (_skillsCollection.Skills.Length >= 2)
        {
            skillLayout.Add(KeyCode.E, _skillsCollection.Skills[1]);
            EquipMainSkill(skillLayout[KeyCode.E], false);
        }
        if (_skillsCollection.Skills.Length >= 3)
        {
            skillLayout.Add(KeyCode.LeftShift, _skillsCollection.Skills[2]);
            EquipShiftSkill(skillLayout[KeyCode.LeftShift]);
        }

    }


    public Skill GetCurrentLeftSkill()
    {
        return _currentLeftSkill;
    }

    public Skill GetCurrentRightSkill()
    {
        return _currentRightSkill;
    }

    public Skill GetCurrentShiftSkill()
    {
        return _currentShiftSkill;
    }

    public bool IsOnCooldown(bool isLeft)
    {
        return isLeft ? _currentLeftSkill.IsOnCooldown : _currentRightSkill.IsOnCooldown;
    }

    public bool IsShiftOnCooldown()
    {
        return _currentShiftSkill.IsOnCooldown;
    }

    public WeaponGraphics GetCurrentGraphics()
    {
        return currentGraphics;
    }

    public GameObject GetCurrentWeaponGameObject()
    {
        return currentWeaponGameObject;
    }

    void EquipWeapon()
    {
        currentWeaponGameObject = Instantiate(currentGraphicsPrefab, weaponHolder.position, weaponHolder.rotation);
        currentWeaponGameObject.transform.SetParent(weaponHolder);
        FirePoint = currentWeaponGameObject.transform.Find("FirePoint");

        currentGraphics = currentWeaponGameObject.GetComponent<WeaponGraphics>();
        if (currentGraphics == null)
        {
            Debug.LogError("No weapon graphics component: " + currentWeaponGameObject.name);
        }

        if (isLocalPlayer)
        {
            Util.SetLayerRecursively(currentWeaponGameObject, LayerMask.NameToLayer(weaponLayerName));
        }
    }

    void EquipMainSkill(Skill skill, bool toLeft)
    {
        if (toLeft)
        {
            _currentLeftSkill = skill;
        }
        else
        {
            _currentRightSkill = skill;
        }
    }

    void EquipShiftSkill(Skill skill)
    {
        _currentShiftSkill = skill;
    }

    public void ReloadLeft()
    {
        Reload(_currentLeftSkill);
    }

    public void ReloadRight()
    {
        Reload(_currentRightSkill);
    }

    public void ReloadShift()
    {
        Reload(_currentShiftSkill);
    }

    private void Reload(Skill currentSkill)
    {
        if (currentSkill.IsOnCooldown)
        {
            return;
        }

        StartCoroutine(ReloadCoroutine(currentSkill));
    }

    private IEnumerator ReloadCoroutine(Skill skill)
    {
        Debug.Log("Reloading.");

        skill.IsOnCooldown = true;

        var tick = skill.CooldownTime / 50;
        for (int i = 1; i <= 50; i++)
        {

            yield return new WaitForSeconds(tick);
            skill.CurrentCooldownTime = tick * i;
        }

        //skill.CurrentAmmo = skill.Charges;

        skill.IsOnCooldown = false;
    }

    public void Cast()
    {
        CmdOnCast();
    }

    [Command]
    void CmdOnCast()
    {
        RpcOnCast();
    }

    [ClientRpc]
    void RpcOnCast()
    {
        Animator anim = currentGraphics.GetComponent<Animator>();

        if (anim != null)
        {
            anim.SetTrigger("Reload");
        }
    }
}

