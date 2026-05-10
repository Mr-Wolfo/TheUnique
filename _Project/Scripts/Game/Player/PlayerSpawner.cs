using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TheUnique.Core.World
{
    public class PlayerSpawner : MonoBehaviour
    {
        [Header("Настройки")]
        public MapGenerator WorldGenerator;
        public Tilemap FloorTilemap;
        public GameObject PlayerPrefab;

        [Header("Имена тайлов воды (чтобы не спавнить там)")]
        public string[] WaterTileNames = { "Water", "DeepWater" };

        public void SpawnPlayerAfterGeneration()
        {
            StartCoroutine(FindSafePointRoutine());
        }

        public void SpawnPlayerAt(Vector3 position)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Instantiate(PlayerPrefab, position, Quaternion.identity);
            }
            else
            {
                player.transform.position = position;
            }
        }

        private IEnumerator FindSafePointRoutine()
        {
            yield return new WaitForSeconds(0.1f);

            int halfW = PlayerPrefs.GetInt("WorldWidth", 50) / 2;
            int halfH = PlayerPrefs.GetInt("WorldHeight", 50) / 2;
            Vector3Int center = new Vector3Int(halfW, halfH, 0);
            
            bool found = false;
            for (int radius = 0; radius < 20; radius++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    for (int y = -radius; y <= radius; y++)
                    {
                        Vector3Int checkPos = center + new Vector3Int(x, y, 0);
                        TileBase tile = FloorTilemap.GetTile(checkPos);

                        if (tile != null && !IsWater(tile.name))
                        {
                            Vector3 spawnPos = FloorTilemap.CellToWorld(checkPos) + new Vector3(0.5f, 0.5f, 0);
                            SpawnPlayerAt(spawnPos);
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                }
                if (found) break;
            }
        }

        private bool IsWater(string tileName)
        {
            foreach (var name in WaterTileNames)
            {
                if (tileName.Contains(name)) return true;
            }
            return false;
        }
    }
}