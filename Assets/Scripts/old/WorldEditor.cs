//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace Game.World
//{
//    [RequireComponent(typeof(World))]
//    [ExecuteInEditMode]
//    public class WorldEditor : MonoBehaviour
//    {
//        [SerializeField]
//        bool drawGizmo;

//        [SerializeField]
//        Color gizmoColor;

//        // Use this for initialization
//        void Start()
//        {

//        }

//        // Update is called once per frame
//        void Update()
//        {

//        }

//        private void OnDrawGizmos()
//        {
//            if (drawGizmo && Application.isEditor)
//            {
//                Gizmos.color = gizmoColor;
//                Gizmos.DrawWireCube(transform.position, GetComponent<World>().GetWorldSize());
//            }
//        }
//    }
//}