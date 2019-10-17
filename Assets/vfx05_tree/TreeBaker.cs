using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using cc.creativecomputing.math.spline;

namespace CambrianExplosion
{

    
    [ExecuteAlways]
    public class TreeBaker : MonoBehaviour
    {
        #region Editable attributes

        [SerializeField] RenderTexture _positionMap = null;
        [SerializeField] RenderTexture _attributeMap = null;
        [SerializeField] ComputeShader _compute = null;

        [SerializeField] float branchXZ = 0.5f;
        [SerializeField] float branchYMin = 0.5f;
        [SerializeField] float branchYMax = 1f;

        #endregion

        #region Temporary objects

        int[] _dimensions = new int[2];

        List<Vector3> _positionList = new List<Vector3>();
        List<Vector3> _attributeList = new List<Vector3>();

        ComputeBuffer _positionBuffer;
        ComputeBuffer _attributeBuffer;

        RenderTexture _tempPositionMap;
        RenderTexture _tempAttributeMap;

        #endregion

        #region Private methods

        private static RenderTexture CreateRenderTexture(int width, int height)
        {
            var rt = new RenderTexture(width, height, 0, RenderTextureFormat.ARGBHalf);
            rt.enableRandomWrite = true;
            rt.Create();
            return rt;
        }

        private void TransferData(int theNumberOfSplines)
        {
            var mapWidth = _positionMap.width;
            var mapHeight = _positionMap.height;

            var vcount = _positionList.Count;
            var vcount_x3 = vcount * 3;

            // Release the temporary objects when the size of them don't match
            // the input.

            if (_positionBuffer != null && _positionBuffer.count != vcount_x3)
            {
                _positionBuffer.Dispose();
                _attributeBuffer.Dispose();

                _positionBuffer = null;
                _attributeBuffer = null;
            }

            if (_tempPositionMap != null &&
               (_tempPositionMap.width != mapWidth ||
                _tempPositionMap.height != mapHeight))
            {
                Destroy(_tempPositionMap);
                Destroy(_tempAttributeMap);

                _tempPositionMap = null;
                _tempAttributeMap = null;
            }

            // Lazy initialization of temporary objects

            if (_positionBuffer == null)
            {
                _positionBuffer = new ComputeBuffer(vcount_x3, sizeof(float));
                _attributeBuffer = new ComputeBuffer(vcount_x3, sizeof(float));
            }

            if (_tempPositionMap == null)
            {
                _tempPositionMap = CreateRenderTexture(mapWidth, mapHeight);
                _tempAttributeMap = CreateRenderTexture(mapWidth, mapHeight);
            }

            // Set data and execute the transfer task.
            _compute.SetInt("VertexCount", mapWidth);
            _compute.SetInt("SplineCount", theNumberOfSplines);
            _compute.SetMatrix("Transform", transform.localToWorldMatrix);

            _positionBuffer.SetData(_positionList);
            _attributeBuffer.SetData(_attributeList);

            _compute.SetBuffer(0, "PositionBuffer", _positionBuffer);
            _compute.SetBuffer(0, "AttributeBuffer", _attributeBuffer);

            _compute.SetTexture(0, "PositionMap", _tempPositionMap);
            _compute.SetTexture(0, "AttributeMap", _tempAttributeMap);

            _compute.Dispatch(0, mapWidth / 8, mapHeight / 8, 1);

            Graphics.CopyTexture(_tempPositionMap, _positionMap);
            Graphics.CopyTexture(_tempAttributeMap, _attributeMap);
        }

        bool _warned;

        bool CheckConsistency()
        {
            if (_warned) return false;

            if (_positionMap.width % 8 != 0 || _positionMap.height % 8 != 0)
            {
                Debug.LogError("Position map dimensions should be a multiple of 8.");
                _warned = true;
            }

            if (_attributeMap.width != _positionMap.width ||
               _attributeMap.height != _positionMap.height)
            {
                Debug.LogError("Position/normal map dimensions should match.");
                _warned = true;
            }

            if (_positionMap.format != RenderTextureFormat.ARGBHalf &&
               _positionMap.format != RenderTextureFormat.ARGBFloat)
            {
                Debug.LogError("Position map format should be ARGBHalf or ARGBFloat.");
                _warned = true;
            }

            if (_attributeMap.format != RenderTextureFormat.ARGBHalf &&
               _attributeMap.format != RenderTextureFormat.ARGBFloat)
            {
                Debug.LogError("Normal map format should be ARGBHalf or ARGBFloat.");
                _warned = true;
            }

            return !_warned;
        }

        

        #endregion



        #region MonoBehaviour implementation
        // Start is called before the first frame update
        void Start() { }

        public bool modActive = false;
        // Update is called once per frame
        void Update()
        {
            Random.InitState(255);
            _positionList.Clear();
            _attributeList.Clear();

            Tree myTree = GetComponent<Tree>();
            List<TreeSpline> mySplines = myTree.Paths();

            for (int y = 0; y < mySplines.Count;y++)
            {
                TreeSpline mySpline = mySplines[y];
                List<Vector3> Vertices = mySpline.spline.Discretize(_positionMap.width);
                for (int x = 0; x < _positionMap.width;x++)
                {
                    _positionList.Add(Vertices[x]);
                    _attributeList.Add(new Vector3(mySpline.IsActive() ? 1 : 0, mySpline.blend, 0));
                }
            }

            if (!CheckConsistency()) return;
            
            TransferData(mySplines.Count);
        }

        public static void TryDispose(System.IDisposable obj)
        {
            if (obj == null) return;
            obj.Dispose();
        }

        public static void TryDestroy(UnityEngine.Object obj)
        {
            if (obj == null) return;
            UnityEngine.Object.Destroy(obj);
        }

        void OnDestroy()
        {

            TryDispose(_positionBuffer);
            TryDispose(_attributeBuffer);

            TryDestroy(_tempPositionMap);
            TryDestroy(_tempAttributeMap);

            _positionBuffer = null;
            _attributeBuffer = null;

            _tempPositionMap = null;
            _tempAttributeMap = null;
        }

        #endregion
    }
}
