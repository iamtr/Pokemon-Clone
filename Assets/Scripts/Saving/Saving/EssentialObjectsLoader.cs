using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EssentialObjectsLoader : MonoBehaviour
{
    [SerializeField] private GameObject essentialObjects;

	private void Awake()
	{
		var existingObjects = FindObjectsOfType<EssentialObjects>();
		if (existingObjects.Length == 0)
		{
			var spawnPos = new Vector3(0f, 0f, 0f);

			var grid = FindObjectOfType<Grid>();
			if (grid != null)
			{
				spawnPos = grid.transform.position;
			}
			Instantiate(essentialObjects, spawnPos, Quaternion.identity);
		}
			
	}
}
