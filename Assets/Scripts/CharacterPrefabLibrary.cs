using UnityEngine;

[CreateAssetMenu(fileName = "CharacterPrefabLibrary", menuName = "Custom/Character Prefab Library")]
public class CharacterPrefabLibrary : ScriptableObject
{
    public GameObject earthPrefab;
    public GameObject firePrefab;
    public GameObject waterPrefab;
    public GameObject windPrefab;
}
