using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPBar : MonoBehaviour
{
	[SerializeField] private GameObject health;

	public void SetHp(float hpNormalized)
	{
		health.transform.localScale = new Vector3(hpNormalized, 1f);
	}

	public IEnumerator SetHPSmooth(float newHp)
	{
		float curHp = health.transform.localScale.x;
		float changeAmount = curHp - newHp;

		while (curHp - newHp > Mathf.Epsilon)
		{
			curHp -= changeAmount * Time.deltaTime;
			health.transform.localScale = new Vector3 (curHp, 1f);
			yield return null;
		}
		health.transform.localScale = new Vector3(newHp, 1f);
	}
}
