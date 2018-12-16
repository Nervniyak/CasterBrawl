using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{

    [SerializeField] private RectTransform thrusterFuelFill;

    [SerializeField] private RectTransform healthBarFill;

    //[SerializeField] private Text ammoText;

    [SerializeField] private Image leftIcon;
    [SerializeField] private Image rightIcon;
    [SerializeField] private Image bottomIcon;

    private Player player;

    private PlayerController controller;

    private SkillManager skillManager;

    public bool PlayerIsDead;

    public void SetPlayer(Player _player)
    {
        player = _player;
        controller = player.GetComponent<PlayerController>();
        skillManager = player.GetComponent<SkillManager>();
    }

    void Start()
    {
    }

    void Update()
    {
        SetFuelAmount(controller.GetThrusterFuelAmount());
        SetHealthAmount(player.GetHealthPercentage());
        SetIconsState();
    }


    void SetFuelAmount(float _amount)
    {
        thrusterFuelFill.localScale = new Vector3(1f, _amount, 1f);
    }

    void SetHealthAmount(float _amount)
    {
        healthBarFill.localScale = new Vector3(1f, _amount, 1f);
    }

    void SetIconsState()
    {
        var leftSkill = skillManager.GetCurrentLeftSkill();

        leftIcon.fillAmount = leftSkill.CurrentCooldownTime / leftSkill.CooldownTime;

        leftIcon.color = leftSkill.IsOnCooldown ? new Color(leftIcon.color.r, leftIcon.color.g, leftIcon.color.b, 0.3f) : new Color(leftIcon.color.r, leftIcon.color.g, leftIcon.color.b, 0.8f);


        var rightSkill = skillManager.GetCurrentRightSkill();

        rightIcon.fillAmount = rightSkill.CurrentCooldownTime / rightSkill.CooldownTime;

        rightIcon.color = rightSkill.IsOnCooldown ? new Color(rightIcon.color.r, rightIcon.color.g, rightIcon.color.b, 0.3f) : new Color(rightIcon.color.r, rightIcon.color.g, rightIcon.color.b, 0.8f);


        var bottomSkill = skillManager.GetCurrentShiftSkill();

        bottomIcon.fillAmount = bottomSkill.CurrentCooldownTime / bottomSkill.CooldownTime;

        bottomIcon.color = bottomSkill.IsOnCooldown ? new Color(bottomIcon.color.r, bottomIcon.color.g, bottomIcon.color.b, 0.3f) : new Color(bottomIcon.color.r, bottomIcon.color.g, bottomIcon.color.b, 0.8f);
    }

    public void SetPlayerDeathStatus(bool playerIsDead)
    {
        PlayerIsDead = playerIsDead;
        if (!PauseMenu.isOn)
        {
            gameObject.SetActive(!playerIsDead);
        }
    }
}
