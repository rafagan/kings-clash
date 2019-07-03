using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] 
public class ActionBarInfo {
	[SerializeField]
	public int Atlas;
	[SerializeField]
    public string Icon;
	[SerializeField]
    public float Cooldown;
    [SerializeField]
    public KeyCode HotKey;
	[SerializeField]
    public GameObject Target;
    [SerializeField]
    public string Name;
	[SerializeField]
	public string Description;
	[SerializeField]
	public bool PlayCooldownAnimation = true;
    [SerializeField]
    public Pool _pool = null;
    [SerializeField]
    public Ability _ability = null;

    [SerializeField] public bool Disabled = true;
}
