using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// 毎フレーム1度ずつX軸回転するキューブ
/// </summary>
public class ComputeCubes : MonoBehaviour
{
    /// <summary> スレッド </summary>
    [SerializeField] private int _threadBlockSize = 12;

    /// <summary> コンピュートシェーダ </summary>
    [SerializeField] private ComputeShader _computeShader;

    /// <summary> コンピュートバッファ </summary>
    [Header("配置レンジ")] private ComputeBuffer _buffer;

    /// <summary> キューブ数 </summary>
    [SerializeField] private int _cubeCount = 10000;

    /// <summary> キューブ参照 </summary>
    private GameObject[] _cubes;

    /// <summary> 各キュー部の角度を格納配列 </summary>
    private float[] _angles;

    /// <summary> カーネルID </summary>
    private int _mainKernel = 0;

    /// <summary> 配置レンジ </summary>
    private readonly float _layoutRange = 20.0f;

    void Start()
    {
        // キューブGameObject作成
        _cubes = new GameObject[_cubeCount];
        _angles = new float[_cubeCount];
        for (int i = 0; i < _cubeCount; i++)
        {
            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // キューブをランダムに初期配置
            cube.transform.localPosition =
                new Vector3(Random.Range(-_layoutRange, _layoutRange), Random.Range(-_layoutRange, _layoutRange),
                    Random.Range(-_layoutRange, _layoutRange));
            _cubes[i] = cube;
        }

        // カーネルID取得
        _mainKernel = _computeShader.FindKernel("CSMain");

        // コンピュートバッファの作成
        _buffer = new ComputeBuffer(_cubeCount, sizeof(float));
        // シェーダとバッファの関連付け
        _computeShader.SetBuffer(_mainKernel, "Result", _buffer);
        // バッファにデータをセット
        _buffer.SetData(_angles);
    }

    void Update()
    {
        // GPU並列処理実行
        int threadGroupX = (_cubeCount / _threadBlockSize) + 1;
        _computeShader.Dispatch(_mainKernel, threadGroupX, 1, 1);

        var data = new float[_cubeCount];
        // 更新結果を取得
        _buffer.GetData(data);

        for (int i = 0; i < _cubeCount; i++)
        {
            float result = data[i];
            _angles[i] = result;
            // キューブをぐるぐるさせる
            _cubes[i].transform.localEulerAngles = new Vector3(_angles[i], 0, 0);
        }
    }

    private void OnDestroy()
    {
        _buffer.Release();
    }
}