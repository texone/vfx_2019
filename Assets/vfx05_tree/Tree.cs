using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using cc.creativecomputing.math.spline;
using cc.creativecomputing.math.util;



namespace CambrianExplosion
{
    public class TreeSpline
    {
        public GameObject gameObject;
        public CCBezierSpline spline;
        public float blend = 0;

        public TreeSpline(GameObject theObject, CCBezierSpline theSpline)
        {
            gameObject = theObject;
            spline = theSpline;
        }

        public bool IsActive()
        {
            return gameObject.activeInHierarchy;
        }

    }

    [ExecuteAlways]
    public class Tree : MonoBehaviour
    {

        [SerializeField] private float radius = 0.1f;
        [SerializeField] private GameObject treeElement = null;

        CCFastRandom random = new CCFastRandom();
        [SerializeField] int randomSeed = 0;

        public bool drawBezier = true;
        public bool drawHandles = true;


        public int subSplines = 5;
        public int subDivisions = 1;
        public int depth = 2;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // DrawChildrenLines(transform);
            if (!drawBezier) return;

            _myPaths.ForEach(p => p.spline.Draw(Color.red, 2, drawHandles));
        }

        private void DrawChildrenLines(Transform t)
        {
            Handles.DrawWireDisc(t.position, -SceneView.GetAllSceneCameras()[0].transform.forward, radius);

            for (var i = 0; i < t.childCount; i++)
            {
                var childTransform = t.GetChild(i);
                DrawChildrenLines(childTransform);

                Handles.DrawLine(t.position, childTransform.position);
            }

        }
#endif

        [Range(0, 1)]
        public float curveTensionA = 0.5f;
        [Range(0, 1)]
        public float curveTensionB = 0.5f;
        [Range(0, 10)]
        public float xRange = 1;
        [Range(-5, 5)]
        public float yStart = 2;
        [Range(0, 10)]
        public float yRange = 3;

        public int addPoints = 1;

        List<TreeSpline> _myPaths = new List<TreeSpline>();

        private void SubDivideSpline(GameObject theGameObject, CCBezierSpline theSpline, int theDepth) {

            int mySubdivs = random.Random(1, subDivisions);
            for (int j = 0; j < mySubdivs; j++)
            {
                CCBezierSpline myBase = theSpline.Clone();
                myBase.BeginEditSpline();
                Vector3 lastPoint = myBase.LastPoint();
                for (int c = 0; c < addPoints; c++)
                {
                    float lastY = lastPoint.y;
                    lastPoint.x += random.Random(-xRange, xRange);
                    lastPoint.z += random.Random(-xRange, xRange);
                    lastPoint.y += random.Random(yStart, yStart + yRange);
                   // myBase.AddPoint(lastPoint, curveTensionA, curveTensionB);
                    myBase.AddControlPoints(new Vector3(lastPoint.x, Mathf.Lerp(lastPoint.y, lastY, curveTensionA), lastPoint.z), lastPoint);
                }
                myBase.EndEditSpline();
                if(theDepth > 0)SubDivideSpline(theGameObject, myBase, theDepth - 1);
                _myPaths.Add(new TreeSpline(theGameObject, myBase));
            }
        }

        private CCBezierSpline BuildSpline(Transform theTransform)
        {
            CCBezierSpline mySpline = new CCBezierSpline();
            mySpline.BeginEditSpline();
            mySpline.AddPoint(theTransform.position);
            
            Transform myLastTransform = theTransform;
            while (myLastTransform.transform.parent != null)
            {
                Vector3 a = myLastTransform.position;
                Vector3 b = myLastTransform.transform.parent.position;

                mySpline.AddControlPoints(
                    new Vector3(a.x, (a.y + b.y) / 2, a.z),
                    new Vector3(b.x, (a.y + b.y) / 2, b.z),
                    b
                );
                myLastTransform = myLastTransform.transform.parent;

                
            }
            mySpline.EndEditSpline();
            mySpline.Invert();
            return mySpline;
        }

        private void BuildSplines(Transform t)
        {
            if (t.childCount == 0)
            {
                CCBezierSpline mySpline = BuildSpline(t);
                _myPaths.Add(new TreeSpline(t.gameObject, mySpline));
                return;
            }
            for (var i = 0; i < t.childCount; i++)
            {
                var childTransform = t.GetChild(i);
                BuildSplines(childTransform);
            }
        }

        public bool updateSplines = true;

        [Range(0, 1)]
        public float endRandomMin = 0.3f;
        [Range(0, 1)]
        public float endRandomMax = 1f;

        private void UpdateSplines()
        {
            //if (_myPaths != null && _myPaths.Count > 0) return;

            Debug.Log("Update Splines");

            _myPaths.Clear();

            BuildSplines(transform);

            random.RandomSeed(randomSeed);

            new List<TreeSpline>(_myPaths).ForEach(p => {
                int mySubSplines = random.Random(1, subSplines);
                for (int i = 0; i < subSplines; i++)
                {
                    CCBezierSpline s = p.spline.SubSpline(0, random.Random(endRandomMin, endRandomMax)); ;
                    SubDivideSpline(p.gameObject, s, depth);
                }
            });
        }


        [Range(0, 10)]
        public float blendTime = 1f;

        private void OnValidate()
        {
            UpdateSplines();
        }


        private void Update()
        {
            if(updateSplines) UpdateSplines();

            _myPaths.ForEach(p => {
                if (p.IsActive())
                {
                    p.blend += Time.deltaTime / blendTime;
                }
                else
                {
                    p.blend -= Time.deltaTime / blendTime;
                }
                p.blend = Mathf.Clamp01(p.blend);
               // Debug.Log(p.blend);
            });
        }

        public List<TreeSpline> Paths()
        {
            return _myPaths;
        }



    }
}