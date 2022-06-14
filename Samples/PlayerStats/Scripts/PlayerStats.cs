using System;
using System.Collections.Generic;
using Medallyon;
using UnityEngine;

public class PlayerStats : MonoBehaviour, ISaveable
{
    [Save] private Guid _playerID = Guid.Empty;
    public string ID;

    private void Start()
    {
        Debug.Log(_playerID);
        ID = _playerID.ToString();
    }

    public void OnRestore(bool isFirstLoad, Dictionary<string, object> data)
    {
        Debug.Log($"isFirstLoad: {isFirstLoad}");
        _playerID = isFirstLoad ? Guid.NewGuid() : Guid.Parse((string)data[nameof(_playerID)]);
    }
}
