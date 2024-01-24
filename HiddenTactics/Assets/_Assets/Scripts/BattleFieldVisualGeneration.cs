using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleFieldVisualGeneration : MonoBehaviour
{

    [SerializeField] Transform elfBattleField;
    [SerializeField] Transform humanBattleField;
    [SerializeField] Transform greenskinBattleField;
    [SerializeField] Transform dwarfBattleField;

    [SerializeField] GameObject[] humanTileVisualPrefabArray;
    [SerializeField] GameObject[] elfTileVisualPrefabArray;
    [SerializeField] GameObject[] greenskinTileVisualPrefabArray;
    [SerializeField] GameObject[] dwarfTileVisualPrefabArray;

    [SerializeField] GameObject humanVillageTileVisualPrefab;
    [SerializeField] GameObject elfVillageTileVisualPrefab;
    [SerializeField] GameObject greenskinVillageTileVisualPrefab;
    [SerializeField] GameObject dwarfVillageTileVisualPrefab;

    [SerializeField] GameObject[] humanVillageVisualPrefabArray;
    [SerializeField] GameObject[] elfVillageVisualPrefabArray;
    [SerializeField] GameObject[] greenskinVillageVisualPrefabArray;
    [SerializeField] GameObject[] dwarfVillageVisualPrefabArray;

    int battleFieldSizeX = 5;
    int battleFieldSizeY = 5;

    float villageSizeX = 3.75f;
    float villageSizeY = 3.75f;
    float pixelToUnitRatio = 1 / 32;

    int tileSize = 9;

    private void Start() {
        GenerateHumanBattleField();
        GenerateElfBattleField();
        GenerateGreenSkinBattleField();
    }

    [Button]
    private void GenerateHumanBattleField() {
        GeneratePlayerBattlefield(elfBattleField, elfTileVisualPrefabArray, 2);
        GeneratePlayerVillage(elfBattleField, elfVillageTileVisualPrefab, elfVillageVisualPrefabArray, 2);
    }

    [Button]
    private void GenerateElfBattleField() {

        GeneratePlayerBattlefield(humanBattleField, humanTileVisualPrefabArray, 1);
        GeneratePlayerVillage(humanBattleField, humanVillageTileVisualPrefab, humanVillageVisualPrefabArray, 1);

    }

    [Button]
    private void GenerateGreenSkinBattleField() {
        GeneratePlayerBattlefield(greenskinBattleField, greenskinTileVisualPrefabArray, 1);
        GeneratePlayerVillage(greenskinBattleField, greenskinVillageTileVisualPrefab, greenskinVillageVisualPrefabArray, 1);
    }

    private void GeneratePlayerBattlefield(Transform battleFieldTransformParent,GameObject[] TileVisualPrefabArray, int player) {

        Vector3 origin = battleFieldTransformParent.transform.position;


        if (player == 1){
            for (int i = -1; i > -battleFieldSizeX-1; i--) {
                for (int j = 0; j < battleFieldSizeY; j++) {
                    GameObject tileVisualPrefab = TileVisualPrefabArray[Random.Range(0, TileVisualPrefabArray.Length)];
                    Vector3 position = new Vector3(tileSize * i, tileSize * j, 0) + origin;
                    Instantiate(tileVisualPrefab, position, Quaternion.identity, battleFieldTransformParent);
                }
            }
        }

        if(player == 2) {
            for (int i = 1; i < battleFieldSizeX+1; i++) {
                for (int j = 0; j < battleFieldSizeY; j++) {
                    GameObject tileVisualPrefab = TileVisualPrefabArray[Random.Range(0, TileVisualPrefabArray.Length)];
                    Vector3 position = new Vector3(tileSize * i, tileSize * j, 0) + origin;
                    Instantiate(tileVisualPrefab, position, Quaternion.identity, battleFieldTransformParent);
                }
            }
        }

    }

    private void GeneratePlayerVillage(Transform battleFieldTransformParent, GameObject villageTileVisualPrefab, GameObject[] villageVisualPrefabArray, int player) {
        Vector3 origin = battleFieldTransformParent.transform.position;

        float villagePositionX = battleFieldSizeX;
        int scaleMultiplier  = 1;

        if (player == 1) {
            villagePositionX = -battleFieldSizeX - 1;

        } else {
            villagePositionX = battleFieldSizeX + 1f;
            scaleMultiplier = -1;
        }


        for (int j = 0; j < battleFieldSizeY; j++) {
            // Battlefield Y loop

            Vector3 position = new Vector3(tileSize * (villagePositionX), tileSize * j, 0) + origin;
            Instantiate(villageTileVisualPrefab, position, Quaternion.identity, battleFieldTransformParent);

            GameObject village1VisualPrefab = villageVisualPrefabArray[Random.Range(0, villageVisualPrefabArray.Length)];
            GameObject village2VisualPrefab = villageVisualPrefabArray[Random.Range(0, villageVisualPrefabArray.Length)];
            GameObject village3VisualPrefab = villageVisualPrefabArray[Random.Range(0, villageVisualPrefabArray.Length)];
            GameObject village4VisualPrefab = villageVisualPrefabArray[Random.Range(0, villageVisualPrefabArray.Length)];

            Vector3 village1Position = new Vector3(tileSize * (villagePositionX) + (.5f * scaleMultiplier), tileSize * (j) + .5f, 0) + origin;
            Instantiate(village1VisualPrefab, village1Position, Quaternion.identity, battleFieldTransformParent);

            Vector3 village2Position = new Vector3(tileSize * (villagePositionX) + (.5f * scaleMultiplier), tileSize * (j) + 4.75f, 0) + origin;
            Instantiate(village2VisualPrefab, village2Position, Quaternion.identity, battleFieldTransformParent);

            Vector3 village3Position = new Vector3(tileSize * (villagePositionX) + (4.75f * scaleMultiplier), tileSize * (j) + .5f, 0) + origin;
            Instantiate(village3VisualPrefab, village3Position, Quaternion.identity, battleFieldTransformParent);

            Vector3 village4Position = new Vector3(tileSize * (villagePositionX) + (4.75f * scaleMultiplier), tileSize * (j) + 4.75f, 0) + origin;
            Instantiate(village4VisualPrefab, village4Position, Quaternion.identity, battleFieldTransformParent);
        }

    }

}
