﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[System.Serializable]
public class CannonItem
{
    [Tooltip("Имя пушки")]
    public string name = "default_cannon";
    [Tooltip("Время перезарядки")]
    public float reloadTime = 1f;
    [Tooltip("Время переключения на эту пушку")]
    public float switchTime = 2f;
    [Tooltip("Время сведения")]
    public float aimTime = 1f;
    [Tooltip("Есть ли сведение")]
    public bool canAim = true;
    [Tooltip("Максимальный прицел")]
    public float maxAim = 0.1f;
    [Tooltip("Минимальный прицел")]
    public float minAim = 0.025f;
    [Tooltip("Выстрел в секторе или точно в цель")]
    public bool randomAim = false;
    [Tooltip("Тип снаряда из списка AmmoData")]
    public int ammoType = -1;
    [Tooltip("Наличие максимального угла отклонения")]
    public bool hasMaxAngle = true;
    [Tooltip("Максимальный угол отклонения от борта")]
    public float maxAngle = 45f;
    [Tooltip("Трекает ли прицел")]
    public bool isTrackable = true;
    [Tooltip("Тип прицела")]
    public AimType aimType = AimType.Cannon;
    [Tooltip("Анимация полета")]
    public ShootAnimation anima = ShootAnimation.Linear;
//    [Tooltip("")]
//    [Tooltip("")]
}

[System.Serializable]
public class AmmoItem
{
    [Tooltip("Название")]
    public string name = "defaul_ammo";
    [Tooltip("Дамаг снаряда")]
    public float damage = 1f;
    [Tooltip("Процентовка дамага")]
    public DamagePercentage target;
    [Tooltip("Шанс крита")]
    public float critChance = 10f;
    [Tooltip("Множитель крита")]
    public float critMultiplier = 3f;
    [Tooltip("Шанс промаха")]
    public float missChance = 15f;
    [Tooltip("Скорость полёта")]
    public float velocity = 5f;
    [Tooltip("Префаб снаряда")]
    public GameObject prefab;
}

[System.Serializable]
public class DamagePercentage
{
    public float body = 80f;
    public float team = 10f;
    public float control = 10f;
}

public enum AimType
{
    Cannon = 0,
    Mortar = 1
}

public enum ShootAnimation
{
    Linear = 0,
    Ballistic = 1,
    Momental = 2
}

public class WeaponData : MonoBehaviour {

    #region Singleton-style
    public static WeaponData Instance { private set; get; }
    void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        Instance = this;
    }
    #endregion
    public bool debug_aim = false;

    public CannonItem[] cannons;
    public AmmoItem[] ammos;

    [Header("Разное")]
    private Vector3 aim_point;
    private float timer;

    public void StartAiming(ShipData _sd, Vector3 _pos)
    {
        timer = 0f;
        switch (_sd.cannon.aimType)
        {
            case AimType.Cannon:
            {
                _sd.aim_left.enabled = true;
                _sd.aim_right.enabled = true;
                _sd.aim_left.fillAmount = _sd.cannon.maxAim;
                _sd.aim_right.fillAmount = _sd.aim_left.fillAmount;
                _pos.y = _sd.tr_canvas_aim.position.y;
                _sd.tr_canvas_aim.LookAt(_pos);
                aim_point = _pos;
                break;
            }
            case AimType.Mortar:
            {
                _sd.tr_mortar_aim.position = _pos + new Vector3(0f, _sd.mortar_aim_overhead, 0f);
                _sd.aim_mortar.enabled = true;
                _sd.tr_mortar_aim.localScale = Vector3.one * _sd.cannon.maxAim;
                break;
            }
        }
        if (!_sd.cannon.hasMaxAngle)
        {
            _sd.aim_left.color = _sd.aim_color_end;
            _sd.aim_right.color = _sd.aim_left.color;
        }
    }

    public void ProcessAiming(ShipData _sd, Vector3 _pos)
    {
        timer = Mathf.Clamp(timer + Time.deltaTime, 0f, _sd.cannon.aimTime);
        
        switch (_sd.cannon.aimType)
        {
            case AimType.Cannon:
            {
                if (_sd.cannon.hasMaxAngle)
                {
                    _sd.aim_left.fillAmount = Mathf.Lerp(_sd.cannon.maxAim, _sd.cannon.minAim, (timer / _sd.cannon.aimTime));
                    _sd.aim_right.fillAmount = _sd.aim_left.fillAmount;
                }
                _pos.y = _sd.tr_canvas_aim.position.y;
                if (_sd.cannon.isTrackable)
                {
                     _sd.tr_canvas_aim.LookAt(_pos);
                }
                else
                {
                     _sd.tr_canvas_aim.LookAt(aim_point);
                }
                if (_sd.cannon.hasMaxAngle)
                {
                     _sd.aim_left.color = Color.Lerp(_sd.aim_color_end, _sd.aim_color_start,
                        (_sd.aim_left.fillAmount - _sd.cannon.minAim) / (_sd.cannon.maxAim - _sd.cannon.minAim));
                     _sd.aim_right.color = _sd.aim_left.color;
                }
                break;
            }
            case AimType.Mortar:
            {
                if (_sd.cannon.hasMaxAngle)
                {
                    if (_sd.cannon.hasMaxAngle)
                    {
                        _sd.tr_mortar_aim.localScale = Mathf.Lerp(_sd.cannon.maxAim, _sd.cannon.minAim, (timer / _sd.cannon.aimTime)) * Vector3.one;
                        _sd.aim_mortar.color = Color.Lerp(_sd.aim_color_start, _sd.aim_color_end, timer / _sd.cannon.aimTime);
                    }
                }
                if (_sd.cannon.isTrackable)
                    {
                        _sd.tr_mortar_aim.position = _pos + new Vector3(0f, _sd.mortar_aim_overhead, 0f);
                    }
                if (_sd.canRotate)
                    {
                    _sd.tr_mortar_aim.Rotate(Vector3.up, _sd.rotateSpeed * Time.deltaTime, Space.World);
                    }
                break;
            }
        }
    }

    public void EndAiming(ShipData _sd, Vector3 _pos)
    {
        float aim_value = 0f;
        Vector3 pos;
        switch (_sd.cannon.aimType)
        {
            case AimType.Cannon:
            {
                _sd.aim_left.enabled = debug_aim;
                _sd.aim_right.enabled = debug_aim;
                aim_value = _sd.aim_right.fillAmount * 360f;
                pos = _pos;
                break;
            }
            case AimType.Mortar:
            {
                 _sd.aim_mortar.enabled = debug_aim;
                aim_value = _sd.tr_mortar_aim.localScale.x / 2f;
                pos = _pos + (Vector3) Random.insideUnitCircle * aim_value;
                break;
            }
            default:
                pos = _pos;
                break;
        }
        DamnShootEm(_sd, pos, aim_value);
    }

    public void DamnShootEm(ShipData _sd, Vector3 _pos, float _aim_value)
    {
        Vector3 cannon = _sd.tr_cannon.position;
        Vector3 cannon_dir =_sd.tr_cannon.forward;

        switch (_sd.cannon.anima)
        {
            case ShootAnimation.Linear:
                {
                    GameObject go = (GameObject)Instantiate(_sd.ammo.prefab, cannon, Quaternion.identity);
                    go.transform.position = _sd.tr_cannon.position;
                    Vector3 dir = _pos - _sd.tr_cannon.position;
                    dir.y = 0f;
                    dir.Normalize();
                    go.transform.forward = dir;
                    go.transform.Rotate(Vector3.up, Random.Range(-_aim_value/2f, _aim_value), Space.World);
                    go.GetComponent<Rigidbody>().velocity = go.transform.forward * _sd.ammo.velocity;
                    go.GetComponent<Bullet>().SetParams(Vector3.zero, 0f, 0f, 0f, _sd.tr_ship.parent); // здесь только парент нужен
                    _sd.ReloadCannon();
                    break;
                }
            case ShootAnimation.Ballistic:
                {
                    GameObject go = (GameObject)Instantiate(_sd.ammo.prefab, cannon, Quaternion.identity);
                    go.transform.position = _sd.tr_cannon.position;
                    float dist = (_pos - cannon).magnitude;
                    Vector3 dir = (_pos - cannon);
                    dir.y = 0f;
                    dir.Normalize();
                    go.GetComponent<Bullet>().SetParams(dir, dist, _sd.ammo.velocity, 2f, _sd.tr_ship.parent); // а здесь всё нужно
                    _sd.ReloadCannon();
                    break;
                }
            case ShootAnimation.Momental:
                {
                    //Whirpool.Instance.ship2.SetDamage(_sd.cannon.ammoType);
                    foreach (ShipData s in Whirpool.Instance.ships)
                        if (s != _sd) s.SetDamage(_sd.cannon.ammoType);
                    _sd.ReloadCannon();
                    break;
                }
        }
    }
}
