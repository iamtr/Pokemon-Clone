using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLayers : MonoBehaviour
{
    [SerializeField] private LayerMask solidObjectsLayer;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private LayerMask grassLayer;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask portalLayer;

    public static GameLayers i { get; private set; }

	private void Awake()
	{
        i = this;
	}

    public LayerMask SolidObjectsLayer => solidObjectsLayer;
    public LayerMask InteractableLayer => interactableLayer;
    public LayerMask GrassLayer => grassLayer;
    public LayerMask PlayerLayer => playerLayer;
    public LayerMask PortalLayer => portalLayer;
    public LayerMask InteractablesLayer
	{
        get { return grassLayer | portalLayer | interactableLayer; }
	}
}
